# YouScan Test Task – Full-Stack Developer

## Goal
Build a simple full-stack dashboard app: **.NET Core backend + React frontend**.  
The app should look like a dashboard that can display three types of widgets in a grid-like way: **line chart, bar chart, and text**.

- Initially, the dashboard should be empty and each widget is added to a grid with a button.  
- The grid should allow three widgets per row (unlimited rows).  
- All charts should use randomized data, while the text widget should be editable.

---

## Backend
- **.NET Core**  
- Widgets with their positions and data must be stored and restored after page refresh.  
- Store data in either **SQLite** or **JSON files**.  
- Validate input and return proper error codes.  

---

## Frontend
- **React 18 + TypeScript**  
- Additional tooling allowed (Vite, AntD, Material Design, etc).  
- Charts: **Recharts** or **Chart.js** recommended, but any library is fine.  
- Fetch data from backend, render charts and text.  
- Text widget must be editable: **Edit → Save → Persisted after reload**.  
- All widgets should be deletable.
- Show **loading** and **error states** per widget.  

---

## Acceptance
- The app is deployed and available for testing (e.g., Cloudflare, Vercel or any platform of your choice).  
- Editing text updates backend and persists.  
- TypeScript passes with no errors.  
- Errors are handled gracefully.  
