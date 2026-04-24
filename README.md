# Hacker News Story Feed

A minimal, full-stack boilerplate with **minimal dependencies**:
- **Backend**: .NET 8 Core API (C#)
- **Frontend**: Angular 18 (TypeScript)

Built with only essential packages — no bloat, just the core functionality you need.

## Implementation Journey

Since this README.md holds some basic setup info, I didn't want to mix it with the implementation steps I took to complete the assessment. Please refer to the file IMPLEMENTATION-JOURNEY.md to view my work process with different LLMs

## Quick Start (Local Development)

### 1. Start the Backend

```bash
cd backend
dotnet restore
dotnet run
```

Backend runs on `http://localhost:5000`.

### 2. Start the Frontend

In another terminal:

```bash
cd frontend
npm install
npm start
```

Frontend runs on `http://localhost:4200`.

## Deployment to Azure (Free Tier)

Ready to deploy? The project includes a **GitHub Actions workflow** that automatically builds and deploys your app on every push to `main`.

**Quick start:**
1. Create an Azure App Service (Free tier)
2. Download the publish profile from Azure
3. Add two GitHub secrets: `AZURE_APPSERVICE_NAME` and `AZURE_PUBLISH_PROFILE`
4. Push to GitHub → **automatic deployment!** 🚀

See [AZURE_FREE_DEPLOYMENT.md](AZURE_FREE_DEPLOYMENT.md) for detailed step-by-step instructions.

The workflow (`.github/workflows/deploy.yml`) handles:
- Building your Angular frontend
- Copying files to backend `wwwroot/`
- Publishing the .NET backend
- Deploying to Azure App Service

**No manual build/deploy steps needed!** Just `git push`.

## Project Structure

```
hackernews-story-feed/
├── backend/                    # .NET 8 Core API
│   ├── src/
│   │   ├── Controllers/        # API endpoints
│   │   ├── Models/             # Data models
│   │   ├── Services/           # Business logic
│   │   ├── Program.cs          # Startup configuration
│   │   └── appsettings*.json
│   ├── hackernews-api.csproj
│   └── README.md
│
└── frontend/                   # Angular 18 App
    ├── src/
    │   ├── app/                # Components & services
    │   ├── environments/        # Dev & prod configs
    │   ├── index.html
    │   ├── main.ts
    │   └── styles.css
    ├── package.json
    ├── angular.json
    ├── tsconfig.json
    └── README.md
```

## Features

- **Backend API**: RESTful endpoints for story management
- **Frontend UI**: Displays stories with metadata
- **Environment-based Configuration**: 
  - Dev: Uses `http://localhost:5000`
  - Production: Uses relative URLs for same-domain hosting
- **CORS Configured**: Supports local development and Azure deployment
- **Error Handling**: Both frontend and backend handle errors gracefully
- **Type Safety**: Strict TypeScript in Angular, nullable types in C#
- **Single App Service**: Frontend and backend in one Azure Free-tier app

## Backend Dependencies

**Zero external dependencies!** Pure .NET 8 Core.

## Frontend Dependencies (Core Angular Only)

- **@angular/core, @angular/common, @angular/forms**: Core Angular
- **@angular/platform-browser**: Browser platform
- **@angular/router**: Routing (ready to expand)
- **rxjs**: Reactive utilities
- **zone.js**: Angular zone

## License

MIT
