# ESG Report Verification API

## Overview
This project is an ASP.NET Core 9 Web API designed for **automatic verification of ESG reports**. It evaluates uploaded documents based on predefined environmental criteria, ensuring data quality and compliance with industry standards. The system supports both **rule-based** and **machine learning** approaches for document analysis.

## Features
✅ File Upload via API (`.pdf`, `.docx` only)  
✅ ESG Criteria Validation 
✅ Rule-based and Machine Learning Analysis  

## Technologies Used
- **ASP.NET Core 9** (Minimal APIs & Controllers)
- **C# CLI Frontend** for interacting with the API

## API Endpoints
### Upload File
**Endpoint:** `POST /api/upload`  
**Request:** Multipart form-data with `file` field  

## Installation & Setup
1. **Clone the repository**
```sh
git clone https://github.com/adinzlotyint/ESGanalyzer
cd ESGanalyzer
```
2. **Install dependencies** (Ensure .NET 9 SDK is installed)
```sh
dotnet restore
```
3. **Run the API**
```sh
dotnet run
```
4. **Test using Postman or Curl**
```sh
curl -X POST -F "file=@sample.pdf" http://localhost:5000/api/upload
```

## ESG Criteria Evaluation
The system checks for the following ESG reporting criteria:
- **C1 (GHG Emissions)** – Detects Scope 1, 2, and 3 mentions, and whether "location-based" or "market-based" methods are referenced.
- **C2 (Calculation Standards)** – Checks for mentions of `ISO 14064`, `GHG Protocol`, `IPCC Guidelines`, etc.
- **C3 (Emission Factors Sources)** – Validates references to `IPCC`, `KOBiZE`, `DEFRA`, `EU ETS`.
- **C4 (Emission Intensity Metrics)** – Detects metrics like `emissions per revenue`, `per product`, `per employee`.
- **C5 (Proper CO2e Units)** – Ensures emissions are reported in `CO2e`, not just `CO2`.
- **C6 (Climate Terminology Presence)** – Confirms the presence of keywords like `climate`, `greenhouse gases`, `carbon footprint`.

