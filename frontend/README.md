# Hacker News Story Feed - Frontend

Minimal Angular 18 application with just the essentials.

## Getting Started

### Prerequisites
- Node.js 18+ and npm

### Setup & Run

```bash
cd frontend
npm install
npm start
```

The application will be available at `http://localhost:4200`.

## Project Structure

- **src/app/**: Main application component and services
- **src/app/services/**: API communication (StoryService)
- **src/assets/**: Static assets

## Features

- Fetch and display stories from the backend API
- Responsive design with minimal CSS
- Error handling and loading states
- Type-safe Angular with strict mode

## Development Server

Run `npm start` for a dev server. Navigate to `http://localhost:4200/`.

## Build

Run `npm run build` to build the project for production.

## API Integration

The frontend communicates with the .NET backend at `http://localhost:5000/api/stories`.
Make sure the backend is running before starting the frontend.
