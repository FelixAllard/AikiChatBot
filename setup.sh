#!/bin/bash

set -e

LOGS_DIR="logs"
mkdir -p "$LOGS_DIR"
rm -f "$LOGS_DIR"/*.log

echo "📦 Restoring packages..."
dotnet restore

echo "🔨 Building all projects..."
dotnet build --no-restore

echo "📥 Applying EF Core migrations..."
for PROJ in $(find . -name "*.csproj"); do
  if grep -q "Microsoft.EntityFrameworkCore" "$PROJ"; then
    echo "🔁 Applying migrations for project: $PROJ"
    dotnet ef database update --project "$PROJ" || echo "⚠️  Skipped migration for $PROJ"
  fi
done

echo "🚀 Launching executable projects..."

# Collecting all processes in the background
PIDS=()

for PROJ in $(find . -name "*.csproj"); do
  if grep -q "<OutputType>Exe</OutputType>" "$PROJ"; then
    NAME=$(basename "$PROJ" .csproj)
    LOG_FILE="$LOGS_DIR/$NAME.log"

    echo "▶️ Building and launching $NAME, logging to $LOG_FILE"

    (
      # Build, publish, and run the project asynchronously
      cd "$(dirname "$PROJ")" 
      dotnet build && dotnet publish -c Release && dotnet run
    ) > "$LOG_FILE" 2>&1 &

    PIDS+=($!)
  fi
done

# Handle running React frontend (if exists)
FRONT_END_DIR="front-end"
if [ -d "$FRONT_END_DIR" ] && [ -f "$FRONT_END_DIR/package.json" ]; then
    echo "🌐 Running React frontend..."
    (
        cd "$FRONT_END_DIR"
        npm install
        npm run build
        npx serve -s build
    ) > "$LOGS_DIR/frontend.log" 2>&1 &
    PIDS+=($!)
    echo "📄 Frontend log is at: $LOGS_DIR/frontend.log"
else
    echo "⚠️ No front-end project found in 'front-end'."
fi

echo "✅ All services started in background."
echo "📜 Logs are available in the '$LOGS_DIR' folder."

# Wait for all background processes
trap 'kill ${PIDS[*]}' SIGINT

wait "${PIDS[@]}"
