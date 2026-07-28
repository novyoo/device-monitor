# Pulsle

A device lifecycle management platform for a laptop-rental business. It tracks a rented laptop through its entire life  while it's out with a customer, when it comes back, and what its extended lifespan means environmentally.

**Live demo:** https://pulsle-c8dfeedphahmbxdg.westus3-01.azurewebsites.net

## What it does

### The Doctor: device health monitoring
Every device (simulated fleet + real registered laptops) reports periodic hardware check-ins: battery health, disk wear, disk errors, crash count, sudden shutdowns, temperature, RAM usage, active-use hours, and days since last OS update. A background worker turns those into a 0–100 health score using a fixed, explainable weighted formula (no ML)
🟢 healthy / 🟡 watch / 🔴 act now with plain-language reasons behind every score. A predictive model also estimates "weeks until red" for declining devices, and server-side alerts fire when a device crosses into the red zone.

### Returns & Decision Maker
Devices move through a real lifecycle: `InStock → Rented → Returned → InStock / InRepair / Resale / Retired`. When a device is returned, a rules engine recommends **Rent again / Repair first / Resale / Retire** based on health score, age, usage, and repair history. A human always makes the final call - the system advises, and every accept/override is logged to an audit trail with a running "agreement rate."

### Green Report
Estimates CO₂ emissions avoided by keeping a laptop in service beyond the typical 3-year replacement cycle, using published manufacturer footprint data. Shows per-tenant and fleet-wide savings with relatable equivalents, plus a downloadable PDF report.

### Real device agent
A lightweight C# console agent (`DeviceOptimizer.Agent`) reads real hardware vitals via WMI and `powercfg` and reports in through the same API endpoint the simulated fleet uses, so a handful of real laptops sit alongside ~100 simulated devices, indistinguishable to the server.

## Tech stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core Web API (C#) |
| Database | Azure SQL Database, via EF Core |
| Frontend | React + Fluent UI (Vite) |
| Auth | ASP.NET Core Identity — cookie-based, role-based (Admin/Customer), tenant-isolated |
| PDF generation | QuestPDF |
| Hosting / CI-CD | Azure App Service, GitHub Actions |
| Device agent | C# console app, System.Management (WMI) |

## Security & privacy by design

- Only hardware vitals are ever collected - no file contents, browsing history, keystrokes, or anything identifying the person using the device.
- Every query is scoped server-side to the logged-in user's tenant; customers can never see another company's devices.
- No auth tokens in `localStorage`, sessions live in an httpOnly, Secure cookie.
- Passwords checked against the HaveIBeenPwned breach database (k-anonymity, only a hash prefix leaves the server) and rate-limited login attempts.

## Running locally

```bash
# backend
cd backend/DeviceOptimizer.Api
dotnet run --launch-profile https

# frontend
cd frontend/device-optimizer-ui
npm install
npm run dev