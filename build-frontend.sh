#!/bin/bash
set -e

echo "Building Angular application..."

# Navigate to the Angular client directory
cd Monopoly.Client

# Install dependencies
echo "Installing npm dependencies..."
npm ci

# Build for production
echo "Building production bundle..."
npm run build -- --configuration production

echo "Build completed successfully!"
