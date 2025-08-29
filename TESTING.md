# Backend Test Commands Reference

This document provides commands for running backend tests with various configurations.

## Quick Commands

### Run All Tests (Simple)
```bash
dotnet test backend/Nextech.Hn.Api.Tests
```

### Run Tests with Detailed Output
```bash
dotnet test backend/Nextech.Hn.Api.Tests --verbosity normal
```

### Run Tests with Code Coverage
```bash
dotnet test backend/Nextech.Hn.Api.Tests \
    /p:CollectCoverage=true \
    /p:CoverletOutput=./TestResults/coverage \
    /p:CoverletOutputFormat=cobertura \
    /p:Exclude="[xunit*]*,[*.Tests]*" \
    /p:Include="[Nextech.Hn.Api]*"
```

## Test Categories

### Run Only Unit Tests
```bash
dotnet test backend/Nextech.Hn.Api.Tests --filter "FullyQualifiedName!~Integration"
```

### Run Only Integration Tests
```bash
dotnet test backend/Nextech.Hn.Api.Tests --filter "FullyQualifiedName~Integration"
```

### Run Specific Test Class
```bash
# StoriesService tests
dotnet test backend/Nextech.Hn.Api.Tests --filter "FullyQualifiedName~StoriesServiceTests"

# StoriesController tests  
dotnet test backend/Nextech.Hn.Api.Tests --filter "FullyQualifiedName~StoriesControllerTests"

# HackerNewsClient tests
dotnet test backend/Nextech.Hn.Api.Tests --filter "FullyQualifiedName~HackerNewsClientTests"

# Integration tests
dotnet test backend/Nextech.Hn.Api.Tests --filter "FullyQualifiedName~StoriesEndpointTests"
```

## Development Workflow

### 1. Quick Test Run (During Development)
```bash
dotnet test backend/Nextech.Hn.Api.Tests --verbosity minimal
```

### 2. Full Test Run with Coverage (Before Commit)
```bash
# Windows
.\test-backend.bat

# Linux/Mac
./test-backend.sh
```

### 3. Continuous Integration
```bash
dotnet test backend/Nextech.Hn.Api.Tests \
    --logger trx \
    --logger "console;verbosity=normal" \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=cobertura \
    /p:CoverletOutput=./TestResults/coverage.cobertura.xml
```

## Coverage Analysis

### Generate HTML Coverage Report
```bash
# Install reportgenerator (if not already installed)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate report
reportgenerator \
    -reports:"./TestResults/coverage.cobertura.xml" \
    -targetdir:"./TestResults/html" \
    -reporttypes:Html

# View report
# Open ./TestResults/html/index.html in browser
```

### Coverage Thresholds
The project aims for:
- **Line Coverage**: > 90%
- **Branch Coverage**: > 85%
- **Method Coverage**: > 95%

## Test Organization

```
backend/Nextech.Hn.Api.Tests/
├── Adapters/           # HTTP client tests
├── Controllers/        # API controller tests  
├── Services/          # Business logic tests
├── Integration/       # End-to-end API tests
└── Utilities/         # Test helpers and utilities
```

## Performance Guidelines

- **Unit Tests**: < 100ms each
- **Integration Tests**: < 1000ms each
- **Total Test Suite**: < 30 seconds

## Troubleshooting

### Tests Failing Randomly
- Check for `Thread.Sleep` usage (should be avoided)
- Verify deterministic test data
- Review cancellation token timeouts

### Coverage Not Collecting
- Ensure `coverlet.msbuild` package is installed
- Check exclude/include patterns
- Verify project references

### Slow Test Execution
- Run tests in parallel: `dotnet test --parallel`
- Profile individual test methods
- Check for unnecessary async delays
