#!/bin/bash
set -e

echo "Building Angular application for Render.com..."

# Navigate to the Angular client directory
cd Monopoly.Client

# Install dependencies
echo "Installing npm dependencies..."
npm ci --legacy-peer-deps

# Replace API URL in environment file if RENDER_API_URL is set
if [ ! -z "$RENDER_API_URL" ]; then
  echo "Configuring API URL: $RENDER_API_URL"
  sed -i "s|apiUrl: ''|apiUrl: '$RENDER_API_URL'|g" src/environments/environment.ts
  sed -i "s|hubUrl: ''|hubUrl: '$RENDER_API_URL/hubs/game'|g" src/environments/environment.ts
fi

# Build for production
echo "Building production bundle..."
npm run build -- --configuration production

echo "Build completed successfully!"
echo "Output directory: dist/monopoly.client/browser"
