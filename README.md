# ABC Pharmacy - Medicine Tracker

A Single-Page Application (SPA) dashboard backed by an ASP.NET Core Web API. This solution persists inventory and sales data directly as JSON files, using a thread-safe and atomic repository layer to prepare for an easy swap to EF Core or SQL Server in the future.

---

## Key Features & Business Rules
- **Color-Coded Stock Status Grid**:
  - **Red Row Highlight**: Near Expiry (Expiry date is less than 30 days from today).
  - **Yellow Row Highlight**: Low Stock (Quantity is less than 10 units).
  - *Note*: If both conditions apply, expiry status (Red) takes visual precedence.
- **Search Filtering**: Real-time debounced inventory searching by medicine name and brand.
- **Stock Validation & Decrementing**: Core validation logic checks stock levels and records transaction totals atomically to protect from concurrency anomalies.
- **Swagger Documentation**: Fully integrated OpenAPI specification for straightforward manual backend testing.

---

## Project Structure
```
PharmacyTracker/
├── PharmacyTracker.sln
├── PharmacyTracker.Api/
│   ├── Controllers/             # MedicinesController & SalesController
│   ├── Models/                  # Core Domain Models & Data Transfer Objects (DTOs)
│   ├── Services/                # Business logic validation & expiry calculation
│   ├── Repositories/            # Thread-safe JSON generic file repository
│   ├── Data/                    # Seed medicines.json & transaction logs
│   ├── Middleware/              # Global HTTP Exception Handling Middleware
│   ├── Program.cs               # Host Setup & SPA static server routes
│   └── wwwroot/                 # Vanilla JavaScript Dashboard SPA
└── PharmacyTracker.Tests/       # xUnit Unit tests for Domain and Stock rules
```

---

## System Requirements
- **.NET 8.0 SDK** (Installed locally on this workspace at `~/.dotnet/`).

### Setup Environment PATH
If `dotnet` is not globally available in your terminal, add it to your path with the command:
```bash
export PATH=$PATH:$HOME/.dotnet
```

---

## Getting Started

### 1. Build the Solution
To compile code and restore dependencies for all projects, execute:
```bash
dotnet build
```

### 2. Run the Application
Navigate to the root of the project `/home/veerm/abc-pharmacy/PharmacyTracker` and start the server host:
```bash
dotnet run --project PharmacyTracker.Api --urls "http://localhost:5000"
```
The server host will boot and serve both the API backend and the static SPA frontend.

### 3. Open in Browser
Open your browser and navigate to:
- **Frontend Dashboard**: [http://localhost:5000](http://localhost:5000)
- **Interactive Swagger Documentation**: [http://localhost:5000/swagger](http://localhost:5000/swagger)

### 4. Execute Unit Tests
To run the automated xUnit unit test suite (covering color logic computation and stock checks):
```bash
dotnet test
```
