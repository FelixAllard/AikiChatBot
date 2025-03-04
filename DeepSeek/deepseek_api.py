from fastapi import FastAPI, HTTPException
from transformers import AutoTokenizer, AutoModelForCausalLM, BitsAndBytesConfig
import torch
import logging
import os
from pydantic import BaseModel
from fastapi.middleware.cors import CORSMiddleware
from tqdm import tqdm  # For progress bars
import sys

# Initialize FastAPI app
app = FastAPI()

# Enable CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"], # TODO We will need to make sure only people on the same network can have access to this endpoint.
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Set up logging with more detailed format
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.StreamHandler(sys.stdout)
    ]
)
logger = logging.getLogger(__name__)

# Initialize model name
model_name = "deepseek-ai/deepseek-llm-7b-base"

# Define offload folder path
offload_folder = "/app/offload_folder"
os.makedirs(offload_folder, exist_ok=True)

# Check if GPU is available
device = "cuda" if torch.cuda.is_available() else "cpu"
logger.info(f"Using device: {device}")

# Custom progress callback for Hugging Face
class ProgressCallback:
    def __init__(self, desc="Downloading"):
        self.desc = desc
        self.pbar = None

    def __call__(self, current, total):
        if self.pbar is None:
            self.pbar = tqdm(total=total, desc=self.desc, unit='B', unit_scale=True)
        self.pbar.update(current - self.pbar.n)
        if current >= total:
            self.pbar.close()

def load_model():
    logger.info(f"Starting to load model: {model_name}")
    logger.info(f"Offload folder path: {offload_folder}")

    # Check offload folder permissions
    try:
        test_file = os.path.join(offload_folder, "test_write.txt")
        with open(test_file, 'w') as f:
            f.write("Test write permission")
        os.remove(test_file)
        logger.info("Offload folder is writable")
    except Exception as e:
        logger.warning(f"Offload folder permission issue: {str(e)}")

    # Configure quantization based on device
    if device == "cuda":
        logger.info("Using 8-bit quantization for GPU")
        quantization_config = BitsAndBytesConfig(load_in_8bit=True)
    else:
        logger.info("Running on CPU without quantization")
        quantization_config = None

    # Load tokenizer with progress
    logger.info("Loading tokenizer...")
    tokenizer = AutoTokenizer.from_pretrained(
        model_name,
        use_fast=True
    )
    logger.info("Tokenizer loaded successfully")

    # Load model with progress
    logger.info("Loading model (this may take several minutes)...")
    model = AutoModelForCausalLM.from_pretrained(
        model_name,
        device_map="auto" if torch.cuda.is_available() else "cpu",
        torch_dtype=torch.float16 if torch.cuda.is_available() else torch.float32,
        offload_folder=offload_folder,
        quantization_config=quantization_config,
        offload_state_dict=True,  # Explicitly enable offloading
        low_cpu_mem_usage=True,
    )

    logger.info("Model loaded successfully!")
    logger.info(f"Model is using device: {next(model.parameters()).device}")

    # Log memory usage
    if torch.cuda.is_available():
        logger.info(f"GPU memory allocated: {torch.cuda.memory_allocated() / 1024**2:.2f} MB")
        logger.info(f"GPU memory reserved: {torch.cuda.memory_reserved() / 1024**2:.2f} MB")

    return model, tokenizer

# Add a startup event to load the model
@app.on_event("startup")
async def startup_event():
    global model, tokenizer
    model, tokenizer = load_model()
    logger.info("API is ready to accept requests")

# Pydantic model for request body
class GenerateRequest(BaseModel):
    prompt: str
    max_length: int = 200

@app.get("/health")
async def health_check():
    return {"status": "healthy", "model": model_name}

@app.post("/generate")
async def generate(request: GenerateRequest):
    try:
        prompt = request.prompt
        max_length = request.max_length
        logger.info(f"Received prompt: {prompt[:50]}..." if len(prompt) > 50 else f"Received prompt: {prompt}")

        # Tokenize input
        inputs = tokenizer(prompt, return_tensors="pt")
        inputs = {k: v.to(model.device) for k, v in inputs.items()}

        # Generate response
        logger.info("Generating response...")
        generation_start = torch.cuda.Event(enable_timing=True) if torch.cuda.is_available() else None
        generation_end = torch.cuda.Event(enable_timing=True) if torch.cuda.is_available() else None

        if generation_start:
            generation_start.record()

        outputs = model.generate(
            **inputs,
            max_length=max_length,
            num_return_sequences=1,
            temperature=0.7,
            do_sample=True,
        )

        if generation_end:
            generation_end.record()
            torch.cuda.synchronize()
            generation_time = generation_start.elapsed_time(generation_end) / 1000  # Convert to seconds
            logger.info(f"Generation took {generation_time:.2f} seconds")

        # Decode response
        response = tokenizer.decode(outputs[0], skip_special_tokens=True)
        logger.info(f"Generated response length: {len(response)} characters")

        return {"response": response}

    except Exception as e:
        logger.error(f"Error occurred: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Internal server error: {str(e)}")