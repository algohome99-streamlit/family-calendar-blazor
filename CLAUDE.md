# CLAUDE.md

## Project Overview

Family calendar/whiteboard app for coordinating family activities and goals. Designed for elementary school upper grades (國小高年級) families. All UI text is in Traditional Chinese.

**Architecture**: Blazor WASM (frontend) → HTTP GET → Google Apps Script (backend) → Google Sheets (data)

## Tech Stack

- **Frontend**: .NET 8.0 Blazor WebAssembly (C# 12)
- **Backend**: Google Apps Script (deployed as Web App)
- **Data**: Google Sheets (Sheet1, Goals, History, Settings worksheets)
- **Dependencies**: `Blazored.LocalStorage` 4.5.0, `Microsoft.AspNetCore.Components.WebAssembly` 8.0.0

## Project Structure

```
FamilyCalendar.Blazor/          # Blazor WASM frontend
├── Models/
│   ├── Event.cs                # Event model (date, member, content, type, recurring)
│   ├── Goal.cs                 # Goal model (member, goal, deadline, sub-items)
│   ├── HistoryRecord.cs        # History record (date, member, content, type, points)
│   └── AppSettings.cs          # Settings model (points map, password)
├── Services/
│   ├── IDataService.cs         # Data service interface
│   ├── DataService.cs          # HTTP calls to GAS Web App
│   └── AuthService.cs          # Password auth with LocalStorage
├── Components/
│   ├── EventCard.razor         # Event card with complete/edit/delete
│   ├── EventForm.razor         # Add/edit event form
│   ├── GoalsWall.razor         # Goals display + CRUD
│   ├── HistoryView.razor       # Completed items archive
│   ├── PointsLeaderboard.razor # Points per member
│   ├── WeeklyView.razor        # Mon-Sun grid view
│   ├── Login.razor             # Password login
│   └── Layout/MainLayout.razor # Root layout with auth gate
├── Pages/Index.razor           # Main page (assembles components)
├── Program.cs                  # DI setup
├── wwwroot/                    # Static assets + appsettings.json
└── CLAUDE_Blazor.md            # Blazor-specific docs
GoogleAppsScript/Code.gs        # Backend API (all CRUD + cleanup)
CLAUDE.md
.gitignore
```

## Google Sheets Schema

**Sheet1** — events:
`date | member | content | type | is_done | recurring`
- `recurring`: `"是"` for weekly, empty otherwise

**Goals** — long-term goals:
`member | goal | deadline | sub1 | sub1_pct | sub2 | sub2_pct | sub3 | sub3_pct`

**History** — completed events/goals:
`date | member | content | type | completed_date | points`
- `points`: integer (0 for expired events, normal points for completed)

**Settings** — key/value pairs:
`key | value` (e.g. `points_功課 | 2`, `password | 1234`, `points_goal | 50`)

## Key Business Logic

### Recurring Events
- Stored in Sheet1 with `recurring="是"`
- On manual completion (`completeEvent`): copies to History with points, updates date +7 days
- On expiration (`cleanupExpiredEvents`): copies to History with points=0, updates date to this week's matching weekday

### Expired Events Cleanup
- Runs automatically when user opens the app (`Index.razor` → `OnInitializedAsync`)
- Calls GAS `cleanupExpiredEvents` which processes all `date < today` and `is_done == false` rows:
  - Expired + recurring: History (0 pts) → update date to this week's matching weekday
  - Expired + non-recurring: History (0 pts) → delete from Sheet1
- Note: `is_done == true` rows won't exist in Sheet1 because `completeEvent` immediately moves/deletes them

### Event Completion
- Complete button only enabled when `DateTime.Today >= Event.Date`
- Non-recurring: copies to History → deletes from Sheet1
- Recurring: copies to History → updates date +7 days (stays in Sheet1)

### Points System

| Type | Points |
|------|--------|
| 日常行程 | 1 |
| 重要提醒 | 2 |
| 功課 | 2 |
| 考試 | 3 |
| 社團 | 2 |
| 才藝 | 2 |
| 家庭活動 | 3 |
| 重大事件(期末考/校外教學) | 5 |
| 長期目標 | 10 |

Points stored in History records. Expired events get 0 points. Leaderboard sums from History.

### Authentication
- Password gate (default: `1234`, configurable via Settings sheet)
- Persisted in `Blazored.LocalStorage` (key: `family_calendar_auth`)

## GAS API Actions (Code.gs)

All via GET with query parameters (to avoid CORS):
- `getEvents`, `getGoals`, `getHistory`, `getSettings` — read
- `addEvent`, `updateEvent`, `deleteEvent`, `completeEvent` — event CRUD
- `addGoal`, `updateGoal`, `deleteGoal`, `completeGoal` — goal CRUD
- `cleanupExpiredEvents` — batch cleanup of expired events

## Deployment

**GitHub repo**: `family-calendar-blazor` — GitHub Actions deploys to GitHub Pages on push to main.
- URL: `https://algohome99-streamlit.github.io/family-calendar-blazor/`
- Workflow: `.github/workflows/deploy.yml` (dotnet publish → sed base href → upload pages artifact)
- Local clone: `C:\Users\algoK\Desktop\family-calendar-blazor`

**GAS deployment**: after editing `Code.gs`, manually update in Google Apps Script editor → Deploy → Manage deployments → New version.

## Not Present

No tests or linting.
