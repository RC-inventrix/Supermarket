# Setup Guide for Supermarket Project

## Setup Details
This guide explains how to set up and run the Supermarket project locally.

## Technologies and Versions
- **Node.js**: v14.17.0
- **Docker**: v20.10.8
- **MongoDB**: v5.0.3

## Prerequisites
Before you begin, ensure you have the following installed:
- Docker
- Docker Compose
- Git

## Quick Start Guide with Docker
1. Clone the repository:
   ```bash
   git clone https://github.com/RC-inventrix/Supermarket.git
   cd Supermarket
   ```
2. Build and run the application with Docker:
   ```bash
   docker-compose up --build
   ```
3. Access the application at `http://localhost:3000`.

## Local Development Instructions
1. Set up your environment variables in a `.env` file based on the `.env.example` provided.
2. Run the application locally without Docker:
   ```bash
   npm install
   npm start
   ```
3. The application will be accessible at `http://localhost:3000`.

## Project Structure
```
/Supermarket
    /src                # Source files
    /tests              # Test files
    /docker             # Docker files
    .env.example        # Sample environment variables
    Dockerfile          # Docker configuration
    docker-compose.yml  # Docker orchestration file
    README.md          # Project documentation
    SETUP_GUIDE.md     # Setup guide
```

## Verification Steps
To verify the correct setup:
1. Ensure the application runs without errors.
2. Run the test suite:
   ```bash
   npm test
   ```

## Database Initialization
1. After starting the application, the database will be automatically initialized via MongoDB.
2. You can use the provided scripts to seed your database if needed.
   ```bash
   npm run seed
   ```

For any issues, refer to the project documentation or reach out to the maintainers.