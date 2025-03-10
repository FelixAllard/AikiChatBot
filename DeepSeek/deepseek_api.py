from fastapi import FastAPI, HTTPException, BackgroundTasks
from transformers import AutoTokenizer, AutoModelForCausalLM, BitsAndBytesConfig
import torch
import logging
import os
import psutil
import gc
from pydantic import BaseModel
from fastapi.middleware.cors import CORSMiddleware
import sys
from contextlib import asynccontextmanager
import asyncio
import time
import concurrent.futures
from functools import partial
import threading

# Initialize logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[logging.StreamHandler(sys.stdout)]
)
logger = logging.getLogger(__name__)

# Global variables
model = None
tokenizer = None
model_name = "microsoft/Phi-2"#deepseek-ai/deepseek-llm-7b-base"
offload_folder = "/app/offload_folder"
os.makedirs(offload_folder, exist_ok=True)

# Thread pool for parallel processing
# Using 6 threads to leave 1 for the main process
executor = concurrent.futures.ThreadPoolExecutor(max_workers=6)

# Semaphore to limit concurrent model inferences
# This prevents memory overload while still allowing parallel requests
MAX_CONCURRENT_INFERENCES = 3
inference_semaphore = asyncio.Semaphore(MAX_CONCURRENT_INFERENCES)

# Check if GPU is available
device = "cuda" if torch.cuda.is_available() else "cpu"
logger.info(f"Using device: {device}")

# Enable TF32 for faster computation on GPUs
if device == "cuda":
    torch.backends.cuda.matmul.allow_tf32 = True
    torch.backends.cudnn.benchmark = True
    torch.backends.cudnn.deterministic = False

# Function to log system resources
def log_system_resources():
    cpu_percent = psutil.cpu_percent()
    memory = psutil.virtual_memory()
    logger.info(f"CPU Usage: {cpu_percent}%")
    logger.info(f"RAM Usage: {memory.percent}% ({memory.used / (1024**3):.2f} GB / {memory.total / (1024**3):.2f} GB)")

    if torch.cuda.is_available():
        gpu_memory_allocated = torch.cuda.memory_allocated() / (1024**3)
        gpu_memory_reserved = torch.cuda.memory_reserved() / (1024**3)
        logger.info(f"GPU Memory Allocated: {gpu_memory_allocated:.2f} GB")
        logger.info(f"GPU Memory Reserved: {gpu_memory_reserved:.2f} GB")

def load_model():
    global model, tokenizer

    try:
        logger.info(f"Starting to load model: {model_name}")
        log_system_resources()

        # Load tokenizer
        logger.info("Loading tokenizer...")
        tokenizer = AutoTokenizer.from_pretrained(
            model_name,
            use_fast=True
        )
        tokenizer.pad_token = tokenizer.eos_token
        logger.info("Tokenizer loaded successfully")

        # Force garbage collection before loading model
        gc.collect()
        if torch.cuda.is_available():
            torch.cuda.empty_cache()

        # Load model with appropriate quantization based on device
        if device == "cuda":
            logger.info("Loading model with 4-bit quantization for CUDA...")
            quantization_config = BitsAndBytesConfig(
                load_in_4bit=True,
                bnb_4bit_compute_dtype=torch.float16,
                bnb_4bit_use_double_quant=True,
                bnb_4bit_quant_type="nf4"
            )
            model = AutoModelForCausalLM.from_pretrained(
                model_name,
                device_map="auto",
                torch_dtype=torch.float16,
                offload_folder=offload_folder,
                quantization_config=quantization_config,
                low_cpu_mem_usage=True,
            )
        else:
            logger.info("Loading model without 8-bit quantization (CPU only)...")
            model = AutoModelForCausalLM.from_pretrained(
                model_name,
                device_map="auto",
                torch_dtype=torch.float32,
                offload_folder=offload_folder,
                low_cpu_mem_usage=True
            )

        model.config.pad_token_id = model.config.eos_token_id
        model.eval()

        logger.info("Model loaded successfully!")
        log_system_resources()

        return True
    except Exception as e:
        logger.error(f"Error loading model: {str(e)}")
        import traceback
        logger.error(traceback.format_exc())
        return False

# Tokenization can be done in parallel threads
def tokenize_prompt(tokenizer, prompt, max_input_length=1024):
    return tokenizer(prompt, return_tensors="pt", truncation=True, max_length=max_input_length)

# Function to generate text
def generate_text(prompt, max_length):
    try:
        max_input_length = 1024
        
        # Tokenize in a separate thread
        inputs_future = executor.submit(tokenize_prompt, tokenizer, prompt, max_input_length)
        inputs = inputs_future.result()
        
        inputs = {k: v.to(model.device) for k, v in inputs.items()}

        logger.info(f"Starting text generation for prompt: {prompt[:50]}...")
        start_time = time.time()

        with torch.inference_mode():
            # Use more efficient generation settings
            outputs = model.generate(
                **inputs,
                max_length=max_length,
                num_return_sequences=1,
                temperature=0.7,
                do_sample=True,
                pad_token_id=tokenizer.eos_token_id,
                max_time=1000.0,  # 1-minute timeout
                use_cache=True,
                num_beams=1,  # Disable beam search for speed
            )

        generation_time = time.time() - start_time
        logger.info(f"Generation completed in {generation_time:.2f} seconds")

        # Decode in a separate thread
        response_future = executor.submit(tokenizer.decode, outputs[0], skip_special_tokens=True)
        response = response_future.result()
        
        return response
    except Exception as e:
        logger.error(f"Error in generate_text: {str(e)}")
        raise

# Lifespan context manager for FastAPI
@asynccontextmanager
async def lifespan(app: FastAPI):
    # Load model in a separate thread to not block the main thread
    with concurrent.futures.ThreadPoolExecutor() as exec:
        success = await asyncio.get_event_loop().run_in_executor(exec, load_model)
    
    if success:
        logger.info("API is ready to accept requests")
    else:
        logger.error("Failed to load model. API will not function correctly.")
    yield
    
    global model, tokenizer
    model = None
    tokenizer = None
    gc.collect()
    if torch.cuda.is_available():
        torch.cuda.empty_cache()

# Initialize FastAPI app
app = FastAPI(lifespan=lifespan)

# Enable CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Pydantic model for request body
class GenerateRequest(BaseModel):
    prompt: str
    max_length: int = 200

@app.get("/health")
async def health_check():
    status = "healthy" if model is not None and tokenizer is not None else "unhealthy"
    return {
        "status": status, 
        "model": model_name, 
        "device": device,
        "concurrent_inferences": MAX_CONCURRENT_INFERENCES - inference_semaphore._value,
        "max_concurrent_inferences": MAX_CONCURRENT_INFERENCES
    }

@app.post("/generate")
async def generate(request: GenerateRequest, background_tasks: BackgroundTasks):
    if model is None or tokenizer is None:
        raise HTTPException(status_code=503, detail="Model not loaded. Please check server logs.")

    try:
        # Use semaphore to limit concurrent inferences
        async with inference_semaphore:
            # Run the generation in a thread pool to not block the event loop
            response_text = await asyncio.get_event_loop().run_in_executor(
                executor, 
                partial(generate_text, request.prompt, request.max_length)
            )
            
        # Clean up in background
        background_tasks.add_task(gc.collect)
        if torch.cuda.is_available():
            background_tasks.add_task(torch.cuda.empty_cache)
            
        return {"response": response_text}
    except Exception as e:
        logger.error(f"Error processing request: {str(e)}")
        import traceback
        logger.error(traceback.format_exc())
        raise HTTPException(status_code=500, detail=f"Internal server error: {str(e)}")

# Add a batch processing endpoint for multiple prompts
class BatchGenerateRequest(BaseModel):
    prompts: list[str]
    max_length: int = 200

@app.post("/generate_batch")
async def generate_batch(request: BatchGenerateRequest, background_tasks: BackgroundTasks):
    if model is None or tokenizer is None:
        raise HTTPException(status_code=503, detail="Model not loaded. Please check server logs.")

    try:
        results = []
        # Process each prompt in parallel using asyncio.gather
        async def process_prompt(prompt):
            async with inference_semaphore:
                return await asyncio.get_event_loop().run_in_executor(
                    executor, 
                    partial(generate_text, prompt, request.max_length)
                )
                
        # Start all tasks
        tasks = [process_prompt(prompt) for prompt in request.prompts]
        responses = await asyncio.gather(*tasks)
        
        # Clean up in background
        background_tasks.add_task(gc.collect)
        if torch.cuda.is_available():
            background_tasks.add_task(torch.cuda.empty_cache)
            
        return {"responses": responses}
    except Exception as e:
        logger.error(f"Error processing batch request: {str(e)}")
        import traceback
        logger.error(traceback.format_exc())
        raise HTTPException(status_code=500, detail=f"Internal server error: {str(e)}")