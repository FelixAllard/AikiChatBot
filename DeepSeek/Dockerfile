FROM python:3.9-slim

WORKDIR /app

# Install dependencies with better progress indication
RUN pip install --no-cache-dir fastapi uvicorn transformers torch torchvision torchaudio accelerate tqdm \
    --index-url https://download.pytorch.org/whl/cpu \
    --extra-index-url https://pypi.org/simple

# Create offload folder for model weights with proper permissions
RUN mkdir -p /app/offload_folder && chmod 777 /app/offload_folder

# Copy the API script
COPY deepseek_api.py /app/deepseek_api.py

# Expose the API port
EXPOSE 8000

# Run the FastAPI server with reload for development
CMD ["uvicorn", "deepseek_api:app", "--host", "0.0.0.0", "--port", "8000", "--log-level", "info"]