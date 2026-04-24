# Hacker News Story Feed - Backend API

Minimal .NET 8 Core API with just the essentials.

## Getting Started

### Prerequisites
- .NET 8 SDK

### Setup & Run

```bash
cd backend
dotnet restore
dotnet run
```

The API will be available at `http://localhost:5000`.

## Project Structure

- **Controllers/**: API endpoints
- **Models/**: Data models
- **Services/**: Business logic (add as needed)

## Dependencies

No external dependencies! Pure .NET 8 Core.

## API Endpoints

- `GET /api/stories` - Get all stories
- `GET /api/stories/{id}` - Get a specific story
- `POST /api/stories` - Create a new story

## CORS Configuration

Angular frontend on `http://localhost:4200` is pre-configured for local development.
