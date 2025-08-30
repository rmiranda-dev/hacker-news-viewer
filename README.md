# Hacker News Viewer

A full-stack web application displaying the newest stories from Hacker News using Angular frontend and ASP.NET Core backend.

## Live Demo

🚀 **Production Deployment:**

- **Frontend:** [https://rmiranda-dev.github.io/hacker-news-viewer/](https://rmiranda-dev.github.io/hacker-news-viewer/)
- **Backend API:** [https://app-hn-sjcdpkxpaunjm.azurewebsites.net](https://app-hn-sjcdpkxpaunjm.azurewebsites.net)
- **API Test Endpoint:** [https://app-hn-sjcdpkxpaunjm.azurewebsites.net/api/stories/new?offset=0&limit=20](https://app-hn-sjcdpkxpaunjm.azurewebsites.net/api/stories/new?offset=0&limit=20)

### 🎯 Try It Now

1. **Visit the Live App**: [https://rmiranda-dev.github.io/hacker-news-viewer/](https://rmiranda-dev.github.io/hacker-news-viewer/)
2. **Test API Directly**: [https://app-hn-sjcdpkxpaunjm.azurewebsites.net/api/stories/new?offset=0&limit=20](https://app-hn-sjcdpkxpaunjm.azurewebsites.net/api/stories/new?offset=0&limit=20)
3. **Search Stories**: Try searching for "AI", "JavaScript", or "Python"
4. **Navigate Pages**: Use pagination to browse through hundreds of stories

### API Endpoints

**Base URL:** `https://app-hn-sjcdpkxpaunjm.azurewebsites.net`

#### Get New Stories
```http
GET /api/stories/new?offset={offset}&limit={limit}&search={search}
```

**Parameters:**
- `offset` (optional): Number of stories to skip (default: 0)
- `limit` (optional): Number of stories to return (default: 20, max: 100)
- `search` (optional): Filter stories by title (case-insensitive)

**Example Requests:**
```bash
# Get first 20 stories
curl "https://app-hn-sjcdpkxpaunjm.azurewebsites.net/api/stories/new"

# Get stories 20-39 (pagination)
curl "https://app-hn-sjcdpkxpaunjm.azurewebsites.net/api/stories/new?offset=20&limit=20"

# Search for stories containing "AI"
curl "https://app-hn-sjcdpkxpaunjm.azurewebsites.net/api/stories/new?search=AI"

# Get 50 stories with search
curl "https://app-hn-sjcdpkxpaunjm.azurewebsites.net/api/stories/new?offset=0&limit=50&search=javascript"
```

**Or test directly in your browser:**
- [Get 10 latest stories](https://app-hn-sjcdpkxpaunjm.azurewebsites.net/api/stories/new?limit=10)
- [Search for "Python"](https://app-hn-sjcdpkxpaunjm.azurewebsites.net/api/stories/new?search=Python&limit=10)
- [Get stories 10-19](https://app-hn-sjcdpkxpaunjm.azurewebsites.net/api/stories/new?offset=10&limit=10)
- [Search for "React"](https://app-hn-sjcdpkxpaunjm.azurewebsites.net/api/stories/new?search=React&limit=5)

**Response Format:**
```json
{
  "items": [
    {
      "id": 45071989,
      "title": "Show HN: SJT- A lightweight structured JSON table format for APIs",
      "url": "https://github.com/yukiakai/sjt",
      "by": "yukiakai",
      "time": 1725032651,
      "score": 1,
      "descendants": 0
    }
  ],
  "totalCount": 500,
  "offset": 0,
  "limit": 20,
  "hasMore": true
}
```

**Status Codes:**
- `200 OK`: Success
- `400 Bad Request`: Invalid parameters (limit > 100, negative offset/limit)
- `500 Internal Server Error`: Server error

#### Health Check
```http
GET /health
```

**Response:**
```json
{
  "status": "Healthy"
}
```

## Features

### Frontend (Angular 20)

- **Modern Material Design UI** with enhanced styling and animations
- **Real-time search** with debounced input and instant filtering
- **Responsive design** optimized for desktop and mobile devices
- **Staggering animations** for smooth story list transitions
- **Pagination** with customizable page sizes (10, 20, 50, 100 items)
- **State management** using RxJS observables and reactive patterns
- **Error handling** with user-friendly error messages
- **Loading states** with Material Design progress indicators
- **Clean search experience** with proper placeholder behavior
- **Accessibility** features following WCAG guidelines

### Backend (ASP.NET Core)

- **RESTful API** with comprehensive Swagger/OpenAPI documentation
- **CORS support** for cross-origin requests from Angular frontend
- **Memory caching** with configurable TTL for optimal performance
- **Retry policies** using Polly for resilient HTTP operations
- **Comprehensive validation** with detailed error responses
- **Structured logging** throughout the application
- **Dependency injection** with clean architecture patterns
- **Exception handling** with consistent ProblemDetails responses
- **Search functionality** with case-insensitive title filtering
- **Pagination** with offset/limit parameters and total count

### Testing & Quality

- **Frontend**: 24 comprehensive tests covering components and services
- **Backend**: 50 unit and integration tests with high coverage
- **Animation testing** with proper provider configuration
- **Error scenario testing** for robust error handling
- **API integration testing** for end-to-end functionality

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

- Angular 20 with TypeScript (strict mode)
- Standalone components architecture
- RxJS for reactive state management
- Angular Material UI components
- Server-Side Rendering (SSR) support
- CSS animations and modern styling
- Jasmine/Karma testing framework

**Backend:**

- ASP.NET Core 8.0 with C# 12
- RESTful API with Swagger/OpenAPI documentation
- HttpClient with Polly retry policies
- Memory caching with configurable TTL
- CORS support for cross-origin requests
- Comprehensive error handling and logging
- xUnit testing framework with FluentAssertions

**Production Deployment:**

- **Frontend Hosting:** GitHub Pages (FREE)
- **Backend Hosting:** Azure App Service F1 (FREE tier)
- **CI/CD:** GitHub Actions for automated deployment
- **Total Cost:** $0.00 (completely free deployment)

**Development Tools:**

- TypeScript with strict compilation
- ESLint for code quality
- Angular CLI for project scaffolding
- Hot Module Replacement (HMR) for development
- GitHub for version control

**Architecture Patterns:**

- Clean architecture with separation of concerns
- Dependency injection throughout the application
- Reactive programming with observables
- Component-based UI architecture
- Repository pattern for data access

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli)
- Git

### Installation

1. **Clone the repository**

```bash
git clone https://github.com/rmiranda-dev/hacker-news-viewer.git
cd hacker-news-viewer
```

2. **Backend setup**

```bash
cd backend/Nextech.Hn.Api
dotnet restore
dotnet build
```

3. **Frontend setup**

```bash
cd frontend/hn-app
npm install
```

### Running the Application

**Start the backend API:**

```bash
cd backend/Nextech.Hn.Api
dotnet run
```

API available at: `http://localhost:5098`

**Start the frontend:**

```bash
cd frontend/hn-app
ng serve
```

Application available at: `http://localhost:4200`

### Production Build

**Backend:**

```bash
cd backend/Nextech.Hn.Api
dotnet publish -c Release -o publish
```

**Frontend:**

```bash
cd frontend/hn-app
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
cd frontend/hn-app
npm run test
ng test --watch=false
```

### Test Coverage

**Backend coverage:**

```bash
cd backend
dotnet test --collect:"XPlat Code Coverage"
```

**Frontend coverage:**

```bash
cd frontend/hn-app
ng test --code-coverage --watch=false
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

## Production Deployment

### Architecture Overview

The application is deployed using a **completely FREE** cloud infrastructure:

- **Frontend**: Deployed to GitHub Pages (FREE) via GitHub Actions
- **Backend**: Deployed to Azure App Service F1 Free tier (FREE) via GitHub Actions
- **Total Monthly Cost**: $0.00

### Deployment URLs

| Service | URL | Purpose |
|---------|-----|---------|
| **Live Application** | [https://rmiranda-dev.github.io/hacker-news-viewer/](https://rmiranda-dev.github.io/hacker-news-viewer/) | Production frontend |
| **API Backend** | [https://app-hn-sjcdpkxpaunjm.azurewebsites.net](https://app-hn-sjcdpkxpaunjm.azurewebsites.net) | Production API |
| **API Test Endpoint** | [https://app-hn-sjcdpkxpaunjm.azurewebsites.net/api/stories/new?offset=0&limit=20](https://app-hn-sjcdpkxpaunjm.azurewebsites.net/api/stories/new?offset=0&limit=20) | Live API test |

### CI/CD Pipeline

**Frontend Deployment (GitHub Pages):**
```yaml
# .github/workflows/frontend-github-pages.yml
name: Deploy Frontend to GitHub Pages
on:
  push:
    branches: [ main ]
    paths: [ 'frontend/**' ]
  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Build Angular app for production
        run: npm run build -- --configuration=production
      - name: Deploy to GitHub Pages
        uses: actions/deploy-pages@v4
```

**Backend Deployment (Azure App Service):**
```yaml
# .github/workflows/backend-appservice.yml
name: Deploy Backend to Azure App Service
on:
  push:
    branches: [ main ]
    paths: [ 'backend/**' ]
  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Build .NET application
        run: dotnet publish --configuration Release
      - name: Deploy to Azure App Service
        uses: azure/webapps-deploy@v2
```

### Environment Configuration

**Production Frontend Environment:**
```typescript
// src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiBaseUrl: 'https://app-hn-sjcdpkxpaunjm.azurewebsites.net'
};
```

**Azure App Service Configuration:**
- **Runtime**: .NET 8.0
- **OS**: Linux
- **Pricing Tier**: F1 (Free)
- **Region**: West US 2
- **Always On**: Disabled (Free tier limitation)

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
