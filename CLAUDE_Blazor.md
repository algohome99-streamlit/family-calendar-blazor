# CLAUDE_Blazor.md

## Project Overview

Family Calendar Blazor WebAssembly 應用程式，用於協調家庭活動和目標。專為國小高年級家庭設計，所有 UI 文字使用繁體中文。

## Tech Stack

- **Framework**: Blazor WebAssembly (.NET 8)
- **Language**: C# 12
- **Data**: Google Sheets (via Google Apps Script Web App)
- **Hosting**: GitHub Pages
- **Auth**: 簡單密碼驗證 (localStorage)

## How to Run

```bash
cd FamilyCalendar.Blazor
dotnet watch run
```

Production build:
```bash
dotnet publish -c Release -o publish
```

## Project Structure

```
FamilyCalendar.Blazor/
├── wwwroot/
│   ├── index.html          # 入口 HTML
│   ├── css/
│   │   └── app.css         # 樣式
│   └── appsettings.json    # GAS Web App URL 設定
├── Models/
│   ├── Event.cs            # 行程資料模型
│   ├── Goal.cs             # 目標資料模型
│   ├── HistoryRecord.cs    # 歷史紀錄模型
│   └── Settings.cs         # 設定模型
├── Services/
│   ├── IDataService.cs     # 資料服務介面
│   ├── DataService.cs      # Google Sheets 資料服務
│   └── AuthService.cs      # 認證服務
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor
│   ├── GoalsWall.razor     # 目標牆
│   ├── EventCard.razor     # 行程卡片
│   ├── EventForm.razor     # 行程表單
│   ├── WeeklyView.razor    # 週視圖
│   ├── HistoryView.razor   # 歷史紀錄
│   └── PointsLeaderboard.razor  # 積分排行榜
├── Pages/
│   ├── Index.razor         # 主頁面
│   └── Login.razor         # 登入頁面
├── Program.cs              # 應用程式入口
├── _Imports.razor          # 全域 using
├── App.razor               # 根元件
└── FamilyCalendar.Blazor.csproj
```

## Google Sheets Schema

### Sheet1 (Events)
| 欄位 | 類型 | 說明 |
|------|------|------|
| date | YYYY-MM-DD | 行程日期 |
| member | string | 哥哥/妹妹/全家 |
| content | string | 行程內容 |
| type | string | 行程類型 |
| is_done | boolean | 是否完成 |
| recurring | string | "是" = 每週重複 |

### Goals
| 欄位 | 類型 | 說明 |
|------|------|------|
| member | string | 成員 |
| goal | string | 目標名稱 |
| deadline | YYYY-MM-DD | 截止日期 |
| sub1~sub3 | string | 子項目名稱 |
| sub1_pct~sub3_pct | int | 進度 0-100 |

### History
| 欄位 | 類型 | 說明 |
|------|------|------|
| date | YYYY-MM-DD | 原始日期 |
| member | string | 成員 |
| content | string | 內容 |
| type | string | 類型 |
| completed_date | YYYY-MM-DD | 完成日期 |
| points | int | 積分（可手動修改） |

### Settings
| 欄位 | 類型 | 說明 |
|------|------|------|
| key | string | 設定名稱 |
| value | string | 設定值 |

預設設定：
- `points_日常行程` = 1
- `points_重要提醒` = 2
- `points_功課` = 2
- `points_考試` = 3
- `points_社團` = 2
- `points_才藝` = 2
- `points_家庭活動` = 3
- `points_重大事件` = 5
- `points_goal` = 50
- `password` = 1234

## 行程類型
| 類型 | 圖示 | 預設積分 |
|------|------|----------|
| 日常行程 | 📅 | 1 |
| 重要提醒 | ⚠️ | 2 |
| 功課 | 📝 | 2 |
| 考試 | 📖 | 3 |
| 社團 | 🏃 | 2 |
| 才藝 | 🎨 | 2 |
| 家庭活動 | 👨‍👩‍👧‍👦 | 3 |
| 重大事件(期末考/校外教學) | 🚩 | 5 |

## 成員顏色
| 成員 | 背景色 | Emoji |
|------|--------|-------|
| 哥哥 | #e3f2fd (藍) | 👦 |
| 妹妹 | #fce4ec (粉) | 👧 |
| 全家 | #fff9c4 (黃) | 👨‍👩‍👧‍👦 |

## 每週重複邏輯（簡化版）

**原則**：不需要虛擬實例，直接操作原始資料

**完成流程**：
```
1. 用戶點擊「完成」(today >= date)
2. 複製資料到 History（含 points）
3. 更新 Sheet1 的 date = date + 7 天
4. 原始資料保留，日期往後推一週
```

## Google Apps Script API

### 端點
Base URL: `{GAS_WEB_APP_URL}`

| 方法 | action | 功能 |
|------|--------|------|
| GET | getEvents | 取得所有行程 |
| GET | getGoals | 取得所有目標 |
| GET | getHistory | 取得歷史紀錄 |
| GET | getSettings | 取得設定 |
| POST | addEvent | 新增行程 |
| POST | updateEvent | 更新行程 |
| POST | deleteEvent | 刪除行程 |
| POST | completeEvent | 完成行程 |
| POST | addGoal | 新增目標 |
| POST | updateGoal | 更新目標 |
| POST | deleteGoal | 刪除目標 |
| POST | completeGoal | 完成目標 |

## GitHub Pages 部署

1. GitHub Actions 自動建置
2. 輸出至 `gh-pages` branch
3. 需要 `.nojekyll` 檔案
4. `<base href="/family-calendar-blazor/" />`

## 開發注意事項

1. **Blazor WASM 限制**：無法直接存取檔案系統
2. **CORS**：GAS Web App 需設定為「任何人都可存取」
3. **localStorage**：用於儲存登入狀態
4. **HttpClient**：所有 API 呼叫使用 HttpClient
