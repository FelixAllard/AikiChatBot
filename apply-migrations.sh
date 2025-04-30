#!/bin/bash

echo "🔍 Looking for EF Core projects with migrations..."

# Build the entire solution
echo "🛠️  Building the solution..."
dotnet build
if [ $? -ne 0 ]; then
    echo "❌ Build failed. Aborting migration application."
    exit 1
fi

# Find all .csproj files that have a Migrations folder next to them
for csproj in $(find . -name '*.csproj'); do
    proj_dir=$(dirname "$csproj")
    
    if [ -d "$proj_dir/Migrations" ]; then
        echo "📦 Found migrations in: $csproj"
        pushd "$proj_dir" > /dev/null

        # Apply migrations
        echo "⚙️  Applying migrations in $proj_dir..."
        dotnet ef database update

        popd > /dev/null
    else
        echo "❌ No migrations found in: $csproj"
    fi
done

echo "✅ Done applying all migrations."
read -p "Press enter to continue..."
