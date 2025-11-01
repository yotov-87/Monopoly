# Monopoly E2E Tests

End-to-end tests for the Monopoly application using Playwright.

## Prerequisites

- Node.js installed
- Backend API running on `http://localhost:5262`
- Frontend application running on `http://localhost:4200`

## Installation

```bash
npm install
```

## Running Tests

Run all tests:
```bash
npm test
```

Run tests in headed mode (see browser):
```bash
npm run test:headed
```

Run tests in debug mode:
```bash
npm run test:debug
```

Run tests in UI mode:
```bash
npm run test:ui
```

View test report:
```bash
npm run report
```

## Test Coverage

Current tests cover:
- User registration with auto-login
- User login
- Logout functionality
- Password validation during registration
- Auth guard redirects for authenticated users
- UI state changes based on authentication status
- Token storage in localStorage
