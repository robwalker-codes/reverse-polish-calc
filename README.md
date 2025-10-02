# RpnCalc

A multi-project solution delivering a Reverse Polish Notation (RPN) and standard infix calculator across domain, application, infrastructure, web API, and Angular SPA layers.

## Prerequisites

- .NET SDK 8.0+
- Node.js 18+
- Angular CLI (`npm install -g @angular/cli`)

## Build

```bash
dotnet build RpnCalc.sln
```

## Test

```bash
dotnet test RpnCalc.sln
cd src/RpnCalc.Web
npm install
npm test
```

## Run API

```bash
dotnet run --project src/RpnCalc.Api
```

Swagger UI: http://localhost:5000/swagger

Health checks:
- http://localhost:5000/healthz
- http://localhost:5000/healthz/ready

## Run SPA (Development)

```bash
cd src/RpnCalc.Web
npm install
ng serve --proxy-config proxy.conf.json
```

Configure `proxy.conf.json` if required to forward `/api` to the API host.

## Production build

```bash
cd src/RpnCalc.Web
npm run build
```

Outputs to `dist/rpn-calc-web/`. Serve via any static host or integrate with the API `wwwroot` directory.

## Project Structure

- `RpnCalc.Domain` – Pure domain logic, tokenisation, conversion, evaluation.
- `RpnCalc.Application` – Command handlers orchestrating domain services and memory store.
- `RpnCalc.Infrastructure` – In-memory memory store and system clock.
- `RpnCalc.Api` – ASP.NET Core minimal API with versioning, OpenAPI, rate limiting, Serilog logging.
- `RpnCalc.Web` – Angular SPA with calculator UI.
- `RpnCalc.Tests` – xUnit + FluentAssertions covering core behaviours.
