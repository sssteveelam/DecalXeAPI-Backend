#!/bin/bash

echo "Starting DecalXe Development Environment..."

# Function to cleanup background processes
cleanup() {
    echo "Stopping services..."
    pkill -f "dotnet run"
    pkill -f "npm start"
    exit 0
}

# Set up signal handlers
trap cleanup SIGINT SIGTERM

# Start backend API
echo "Starting .NET Core API..."
cd /workspace
dotnet run --urls=http://localhost:5000 &
API_PID=$!

# Wait a moment for API to start
sleep 5

# Start frontend
echo "Starting React frontend..."
cd /workspace/decal-xe-frontend
npm start &
FRONTEND_PID=$!

echo "Services started:"
echo "- API: http://localhost:5000"
echo "- Frontend: http://localhost:3000"
echo "- API Documentation: http://localhost:5000/swagger"
echo ""
echo "Press Ctrl+C to stop all services"

# Wait for both processes
wait $API_PID $FRONTEND_PID
