# Hacker News Viewer

A full-stack web application displaying the newest stories from Hacker News using Angular frontend and ASP.NET Core backend.

## Live Demo

- **Frontend:** [https://your-app-name.azurewebsites.net](https://your-app-name.azurewebsites.net)
- **API:** [https://your-api-name.azurewebsites.net](https://your-api-name.azurewebsites.net)

## Features

### Frontend (Angular)

- Story list with newest Hacker News stories
- Smart link handling for stories with/without URLs
- Real-time search across story titles
- Pagination for large datasets
- Responsive design
- Loading states and error handling

### Backend (ASP.NET Core)

- RESTful API endpoints
- Dependency injection configuration
- Intelligent caching of Hacker News data
- Comprehensive exception handling
- Structured logging
- Swagger/OpenAPI documentation

### Testing

- Unit tests with high coverage
- Integration tests for API endpoints
- Angular component and service tests

## Architecture

```
┌─────────────────────────────────────┐
│           Angular Frontend          │
│  ┌─────────────┐  ┌─────────────┐   │    HTTP/REST
│  │ Components  │  │  Services   │   │◄──────────────┐
│  │ • Story List│  │ • HTTP      │   │               │
│  │ • Search    │  │ • Caching   │   │               │
│  │ • Pagination│  │ • State Mgmt│   │               │
│  └─────────────┘  └─────────────┘   │               │
└─────────────────────────────────────┘               │
                                                      │
┌─────────────────────────────────────┐               │
│         ASP.NET Core API            │               │
│  ┌─────────────┐  ┌─────────────┐   │               │
│  │Controllers  │  │  Services   │   │◄──────────────┘
│  │ • Stories   │  │ • Business  │   │
│  │ • Caching   │  │ • Mapping   │   │
│  │ • Swagger   │  │ • Logging   │   │
│  └─────────────┘  └─────────────┘   │
│  ┌─────────────┐  ┌─────────────┐   │
│  │Memory Cache │  │Dependency   │   │    HTTP/JSON
│  │ • 5min TTL  │  │Injection    │   │◄───────────────┐
│  │ • Story Data│  │Container    │   │                │
│  └─────────────┘  └─────────────┘   │                │
└─────────────────────────────────────┘                │
                                                       │
┌─────────────────────────────────────┐                │
│         Hacker News API             │                │
│  • /v0/newstories.json              │                │
│  • /v0/item/{id}.json               │◄───────────────┘
│  • Firebase Realtime Database       │
└─────────────────────────────────────┘
```

## Technology Stack

**Frontend:**

- Angular 17+ with TypeScript
- RxJS for reactive programming
- Angular Material UI components
- Jasmine/Karma testing

**Backend:**

- ASP.NET Core 8.0 with C# 12
- Entity Framework Core
- AutoMapper for object mapping
- Serilog structured logging
- xUnit and Moq for testing

**Infrastructure:**

- Azure App Service hosting
- Azure Application Insights monitoring
- GitHub Actions CI/CD

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli)
- Git

### Installation

1. **Clone the repository**

```bash
git clone https://github.com/yourusername/hacker-news-viewer.git
cd hacker-news-viewer
```

2. **Backend setup**

```bash
cd backend/HackerNewsViewer.Api
dotnet restore
dotnet build
```

3. **Frontend setup**

```bash
cd frontend/hacker-news-viewer
npm install
```

### Running the Application

**Start the backend API:**

```bash
cd backend/HackerNewsViewer.Api
dotnet run
```

API available at: `https://localhost:5001`

**Start the frontend:**

```bash
cd frontend/hacker-news-viewer
ng serve
```

Application available at: `http://localhost:4200`

### Production Build

**Backend:**

```bash
cd backend/HackerNewsViewer.Api
dotnet publish -c Release -o publish
```

**Frontend:**

```bash
cd frontend/hacker-news-viewer
ng build --configuration production
```

## Testing

### Run All Tests

**Backend tests:**

```bash
cd backend
dotnet test --logger "console;verbosity=detailed"
```

**Frontend tests:**

```bash
cd frontend/hacker-news-viewer
npm run test
npm run e2e
```

### Test Coverage

**Backend coverage:**

```bash
cd backend
dotnet test --collect:"XPlat Code Coverage"
```

**Frontend coverage:**

```bash
cd frontend/hacker-news-viewer
ng test --code-coverage
```

## API Documentation

### Base URLs

- **Development:** `https://localhost:5001/api`
- **Production:** `https://your-api-name.azurewebsites.net/api`

### Endpoints

#### GET /api/stories

Retrieve paginated list of newest stories

**Query Parameters:**

- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 20, max: 50)
- `search` (optional): Search term for filtering stories

**Response:**

```json
{
  "stories": [
    {
      "id": 12345,
      "title": "Story Title",
      "url": "https://example.com",
      "author": "username",
      "score": 100,
      "time": "2024-01-01T12:00:00Z",
      "descendants": 25
    }
  ],
  "totalCount": 500,
  "page": 1,
  "pageSize": 20,
  "totalPages": 25
}
```

#### GET /api/stories/{id}

Retrieve specific story by ID

**Response:**

```json
{
  "id": 12345,
  "title": "Story Title",
  "url": "https://example.com",
  "author": "username",
  "score": 100,
  "time": "2024-01-01T12:00:00Z",
  "descendants": 25,
  "text": "Story content if available"
}
```

**Swagger Documentation:** Available at `/swagger` endpoint

## Project Structure

```
hacker-news-viewer/
├── backend/
│   ├── HackerNewsViewer.Api/          # Web API project
│   ├── HackerNewsViewer.Core/         # Business logic
│   ├── HackerNewsViewer.Infrastructure/ # External services
│   └── HackerNewsViewer.Tests/        # Test projects
├── frontend/
│   └── hacker-news-viewer/            # Angular application
│       ├── src/app/
│       │   ├── components/            # Angular components
│       │   ├── services/              # Angular services
│       │   ├── models/                # TypeScript models
│       │   └── shared/                # Shared utilities
│       └── e2e/                       # End-to-end tests
└── .github/workflows/                 # CI/CD pipelines
```

## Configuration

### Backend Configuration (`appsettings.json`)

```json
{
  "HackerNewsApi": {
    "BaseUrl": "https://hacker-news.firebaseio.com/v0",
    "CacheExpirationMinutes": 5,
    "RequestTimeoutSeconds": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Frontend Configuration (`src/environments/environment.ts`)

```typescript
export const environment = {
  production: false,
  apiUrl: "https://localhost:5001/api",
};
```

## Deployment

### Azure App Service

**Create Azure resources:**

```bash
az group create --name rg-hackernews --location eastus
az appservice plan create --name plan-hackernews --resource-group rg-hackernews --sku B1
az webapp create --name hackernews-api --resource-group rg-hackernews --plan plan-hackernews
az webapp create --name hackernews-app --resource-group rg-hackernews --plan plan-hackernews
```

**Environment Variables (Azure App Service Configuration):**

- `ASPNETCORE_ENVIRONMENT`: `Production`
- `HackerNewsApi__BaseUrl`: `https://hacker-news.firebaseio.com/v0`
- `HackerNewsApi__CacheExpirationMinutes`: `5`

**CI/CD:** GitHub Actions workflows automatically deploy on push to main branch

## Key Implementation Details

### Caching Strategy

- In-memory caching with 5-minute expiration
- Reduces Hacker News API calls
- Improves response times significantly

### Search Implementation

- Client-side real-time filtering
- 300ms debounce to optimize performance
- Case-insensitive search

### Error Handling

- Global exception handler for centralized error management
- User-friendly error messages
- Graceful fallback states

### Testing Strategy

- Unit tests with 80%+ code coverage
- Integration tests for API endpoints
- Angular component and service testing
- End-to-end testing for critical user journeys

## Known Issues & Limitations

- Hacker News API rate limits may affect performance during high traffic
- Search functionality is client-side only
- Real-time updates require manual refresh
