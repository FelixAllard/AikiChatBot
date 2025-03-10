FROM python:3.9-bookworm

WORKDIR /app

# Install system dependencies and update libstdc++ to support GLIBCXX_3.4.32
RUN apt-get update && apt-get install -y --no-install-recommends \
    gcc-12 \
    g++-12 \
    build-essential \
    software-properties-common \
    ninja-build \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

# Update alternatives to use gcc-12 and g++-12
RUN update-alternatives --install /usr/bin/gcc gcc /usr/bin/gcc-12 12 \
    && update-alternatives --install /usr/bin/g++ g++ /usr/bin/g++-12 12

# Install Python dependencies
RUN pip install --no-cache-dir \
    fastapi \
    uvicorn[standard] \
    transformers>=4.33.0 \
    torch \
    torchvision \
    torchaudio \
    accelerate \
    tqdm \
    psutil \
    ninja \
    --extra-index-url https://download.pytorch.org/whl/cpu

# Install bitsandbytes (latest version)
RUN pip uninstall -y bitsandbytes && \
    pip install --no-cache-dir bitsandbytes>=0.41.1

# Create offload folder for model weights with proper permissions
RUN mkdir -p /app/offload_folder && chmod 777 /app/offload_folder

# Copy the API script
COPY deepseek_api.py /app/deepseek_api.py

# Expose the API port
EXPOSE 8000

# Set environment variables for better memory management
ENV MALLOC_TRIM_THRESHOLD_=0
ENV PYTHONUNBUFFERED=1
ENV OMP_NUM_THREADS=6
ENV MKL_NUM_THREADS=6
ENV TOKENIZERS_PARALLELISM=true
ENV TORCH_CUDNN_V8_API_ENABLED=1
ENV CUDA_MODULE_LOADING=LAZY

# Run the FastAPI server with optimized workers
CMD ["uvicorn", "deepseek_api:app", "--host", "0.0.0.0", "--port", "8000", "--log-level", "info", "--timeout-keep-alive", "120", "--workers", "1", "--loop", "uvloop", "--http", "httptools"]