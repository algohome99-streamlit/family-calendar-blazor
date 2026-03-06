/**
 * Family Calendar - Google Apps Script Web App
 *
 * 部署方式：
 * 1. 在 Google Sheets 中，點選「擴充功能」→「Apps Script」
 * 2. 貼上此程式碼
 * 3. 點選「部署」→「新增部署作業」
 * 4. 選擇「網路應用程式」
 * 5. 設定「誰可以存取」為「任何人」
 * 6. 點選「部署」，複製 Web App URL
 */

// 取得試算表
function getSpreadsheet() {
  return SpreadsheetApp.getActiveSpreadsheet();
}

// === GET 請求處理 ===
function doGet(e) {
  const action = e.parameter.action;
  let result;

  try {
    switch (action) {
      // 讀取操作
      case 'getEvents':
        result = getEvents();
        break;
      case 'getGoals':
        result = getGoals();
        break;
      case 'getHistory':
        result = getHistory();
        break;
      case 'getSettings':
        result = getSettings();
        break;
      // 寫入操作 (透過 GET 傳遞參數，避免 CORS 問題)
      case 'addEvent':
        result = addEvent(e.parameter);
        break;
      case 'updateEvent':
        result = updateEvent(e.parameter);
        break;
      case 'deleteEvent':
        result = deleteEvent(e.parameter);
        break;
      case 'completeEvent':
        result = completeEvent(e.parameter);
        break;
      case 'addGoal':
        result = addGoal(e.parameter);
        break;
      case 'updateGoal':
        result = updateGoal(e.parameter);
        break;
      case 'deleteGoal':
        result = deleteGoal(e.parameter);
        break;
      case 'completeGoal':
        result = completeGoal(e.parameter);
        break;
      case 'cleanupExpiredEvents':
        result = cleanupExpiredEvents();
        break;
      default:
        result = { success: false, error: 'Unknown action' };
    }
  } catch (error) {
    result = { success: false, error: error.message };
  }

  return ContentService
    .createTextOutput(JSON.stringify(result))
    .setMimeType(ContentService.MimeType.JSON);
}

// === POST 請求處理 ===
function doPost(e) {
  const data = JSON.parse(e.postData.contents);
  const action = data.action;
  let result;

  try {
    switch (action) {
      case 'addEvent':
        result = addEvent(data);
        break;
      case 'updateEvent':
        result = updateEvent(data);
        break;
      case 'deleteEvent':
        result = deleteEvent(data);
        break;
      case 'completeEvent':
        result = completeEvent(data);
        break;
      case 'addGoal':
        result = addGoal(data);
        break;
      case 'updateGoal':
        result = updateGoal(data);
        break;
      case 'deleteGoal':
        result = deleteGoal(data);
        break;
      case 'completeGoal':
        result = completeGoal(data);
        break;
      default:
        result = { success: false, error: 'Unknown action' };
    }
  } catch (error) {
    result = { success: false, error: error.message };
  }

  return ContentService
    .createTextOutput(JSON.stringify(result))
    .setMimeType(ContentService.MimeType.JSON);
}

// === Events ===
function getEvents() {
  const sheet = getSpreadsheet().getSheetByName('Sheet1');
  const data = sheet.getDataRange().getValues();
  const headers = data[0];
  const events = [];

  for (let i = 1; i < data.length; i++) {
    const row = data[i];
    if (!row[0]) continue; // 跳過空行

    // 欄位順序：date, member, content, type, is_done, recurring
    events.push({
      rowIndex: i + 1, // Google Sheets 行號從 1 開始
      date: formatDate(row[0]),
      member: row[1] || '',
      content: row[2] || '',
      type: row[3] || '',
      isDone: row[4] === true || row[4] === 'TRUE',
      recurring: row[5] || ''
    });
  }

  return { success: true, data: events };
}

function addEvent(data) {
  const sheet = getSpreadsheet().getSheetByName('Sheet1');
  // 欄位順序：date, member, content, type, is_done, recurring
  sheet.appendRow([
    data.date,
    data.member,
    data.content,
    data.type,
    false,
    data.recurring || ''
  ]);
  return { success: true };
}

function updateEvent(data) {
  const sheet = getSpreadsheet().getSheetByName('Sheet1');
  const row = data.rowIndex;

  // 欄位順序：date, member, content, type, is_done, recurring
  sheet.getRange(row, 1).setValue(data.date);
  sheet.getRange(row, 2).setValue(data.member);
  sheet.getRange(row, 3).setValue(data.content);
  sheet.getRange(row, 4).setValue(data.type);
  sheet.getRange(row, 6).setValue(data.recurring || '');

  return { success: true };
}

function deleteEvent(data) {
  const sheet = getSpreadsheet().getSheetByName('Sheet1');
  sheet.deleteRow(data.rowIndex);
  return { success: true };
}

function completeEvent(data) {
  const sheet = getSpreadsheet().getSheetByName('Sheet1');
  const historySheet = getSpreadsheet().getSheetByName('History');
  const settingsSheet = getSpreadsheet().getSheetByName('Settings');

  // 取得積分設定
  const points = getPointsForType(data.type);

  // 新增到 History
  historySheet.appendRow([
    data.date,
    data.member,
    data.content,
    data.type,
    formatDate(new Date()),
    points
  ]);

  // 如果是每週重複，更新日期 +7 天
  if (data.recurring === '是') {
    const currentDate = new Date(data.date);
    currentDate.setDate(currentDate.getDate() + 7);
    sheet.getRange(data.rowIndex, 1).setValue(formatDate(currentDate));
  } else {
    // 否則刪除該行
    sheet.deleteRow(data.rowIndex);
  }

  return { success: true };
}

// === Cleanup Expired Events ===
function cleanupExpiredEvents() {
  const sheet = getSpreadsheet().getSheetByName('Sheet1');
  const historySheet = getSpreadsheet().getSheetByName('History');
  const data = sheet.getDataRange().getValues();
  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const todayStr = formatDate(today);
  const rowsToDelete = []; // 收集要刪除的行號（從大到小刪）
  let cleaned = 0;

  // 從第 2 行開始（跳過 header），從最後一行往前遍歷
  for (let i = data.length - 1; i >= 1; i--) {
    const row = data[i];
    if (!row[0]) continue; // 跳過空行

    const rowDate = new Date(row[0]);
    rowDate.setHours(0, 0, 0, 0);

    if (rowDate >= today) continue; // 未過期，跳過

    const isDone = row[4] === true || row[4] === 'TRUE';
    const recurring = row[5] || '';
    const sheetRow = i + 1; // Google Sheets 行號

    if (isDone) continue; // 已完成的不會留在 Sheet1（completeEvent 會立刻搬走）

    // 過期未完成：寫入 History（points = 0）
    historySheet.appendRow([
      formatDate(rowDate),
      row[1] || '',  // member
      row[2] || '',  // content
      row[3] || '',  // type
      todayStr,       // completed_date
      0               // points = 0
    ]);

    if (recurring === '是') {
      // 重複行程：更新 date 為本週對應星期幾
      const originalWeekday = rowDate.getDay(); // 0=Sun, 1=Mon, ...
      const todayWeekday = today.getDay();
      const daysAhead = (originalWeekday - todayWeekday + 7) % 7;
      const nextDate = new Date(today);
      nextDate.setDate(nextDate.getDate() + daysAhead);
      sheet.getRange(sheetRow, 1).setValue(formatDate(nextDate));
    } else {
      // 一般行程：標記刪除
      rowsToDelete.push(sheetRow);
    }
    cleaned++;
  }

  // 從大到小刪除行（避免 index 偏移）
  rowsToDelete.sort((a, b) => b - a);
  for (const rowNum of rowsToDelete) {
    sheet.deleteRow(rowNum);
  }

  return { success: true, data: { cleaned: cleaned } };
}

// === Goals ===
function getGoals() {
  const sheet = getSpreadsheet().getSheetByName('Goals');
  if (!sheet) return { success: true, data: [] };

  const data = sheet.getDataRange().getValues();
  const goals = [];

  for (let i = 1; i < data.length; i++) {
    const row = data[i];
    if (!row[0]) continue;

    goals.push({
      rowIndex: i + 1,
      member: row[0] || '',
      goalName: row[1] || '',
      deadline: formatDate(row[2]),
      sub1: row[3] || '',
      sub1Pct: parseInt(row[4]) || 0,
      sub2: row[5] || '',
      sub2Pct: parseInt(row[6]) || 0,
      sub3: row[7] || '',
      sub3Pct: parseInt(row[8]) || 0
    });
  }

  return { success: true, data: goals };
}

function addGoal(data) {
  const sheet = getSpreadsheet().getSheetByName('Goals');
  sheet.appendRow([
    data.member,
    data.goal,
    data.deadline,
    data.sub1 || '',
    data.sub1_pct || 0,
    data.sub2 || '',
    data.sub2_pct || 0,
    data.sub3 || '',
    data.sub3_pct || 0
  ]);
  return { success: true };
}

function updateGoal(data) {
  const sheet = getSpreadsheet().getSheetByName('Goals');
  const row = data.rowIndex;

  sheet.getRange(row, 1).setValue(data.member);
  sheet.getRange(row, 2).setValue(data.goal);
  sheet.getRange(row, 3).setValue(data.deadline);
  sheet.getRange(row, 4).setValue(data.sub1 || '');
  sheet.getRange(row, 5).setValue(data.sub1_pct || 0);
  sheet.getRange(row, 6).setValue(data.sub2 || '');
  sheet.getRange(row, 7).setValue(data.sub2_pct || 0);
  sheet.getRange(row, 8).setValue(data.sub3 || '');
  sheet.getRange(row, 9).setValue(data.sub3_pct || 0);

  return { success: true };
}

function deleteGoal(data) {
  const sheet = getSpreadsheet().getSheetByName('Goals');
  sheet.deleteRow(data.rowIndex);
  return { success: true };
}

function completeGoal(data) {
  const sheet = getSpreadsheet().getSheetByName('Goals');
  const historySheet = getSpreadsheet().getSheetByName('History');

  // 取得目標完成積分
  const points = getPointsForType('長期目標');

  // 新增到 History
  historySheet.appendRow([
    formatDate(new Date()),
    data.member,
    '[目標] ' + data.goal,
    '長期目標',
    formatDate(new Date()),
    points
  ]);

  // 刪除目標
  sheet.deleteRow(data.rowIndex);

  return { success: true };
}

// === History ===
function getHistory() {
  const sheet = getSpreadsheet().getSheetByName('History');
  if (!sheet) return { success: true, data: [] };

  const data = sheet.getDataRange().getValues();
  const history = [];

  for (let i = 1; i < data.length; i++) {
    const row = data[i];
    if (!row[0]) continue;

    history.push({
      rowIndex: i + 1,
      date: formatDate(row[0]),
      member: row[1] || '',
      content: row[2] || '',
      type: row[3] || '',
      completedDate: formatDate(row[4]),
      points: parseInt(row[5]) || 0
    });
  }

  return { success: true, data: history };
}

// === Settings ===
function getSettings() {
  const sheet = getSpreadsheet().getSheetByName('Settings');
  if (!sheet) {
    // 如果沒有 Settings 工作表，回傳預設值
    return {
      success: true,
      data: {
        'points_日常行程': '1',
        'points_重要提醒': '2',
        'points_功課': '2',
        'points_考試': '3',
        'points_社團': '2',
        'points_才藝': '2',
        'points_家庭活動': '3',
        'points_重大事件': '5',
        'points_長期目標': '10',
        'points_goal': '50',
        'password': '1234'
      }
    };
  }

  const data = sheet.getDataRange().getValues();
  const settings = {};

  for (let i = 1; i < data.length; i++) {
    const row = data[i];
    if (row[0]) {
      settings[row[0]] = row[1] ? row[1].toString() : '';
    }
  }

  return { success: true, data: settings };
}

function getPointsForType(type) {
  const settings = getSettings().data;

  // 嘗試從設定取得積分
  const key = 'points_' + type.replace('(期末考/校外教學)', '').replace('(會考/旅遊)', '');
  if (settings[key]) {
    return parseInt(settings[key]) || 1;
  }

  // 預設積分
  const defaultPoints = {
    '日常行程': 1,
    '重要提醒': 2,
    '功課': 2,
    '考試': 3,
    '社團': 2,
    '才藝': 2,
    '家庭活動': 3,
    '重大事件(期末考/校外教學)': 5,
    '重大事件(會考/旅遊)': 5,
    '長期目標': 10
  };

  return defaultPoints[type] || 1;
}

// === 工具函數 ===
function formatDate(date) {
  if (!date) return '';
  if (typeof date === 'string') return date;

  const d = new Date(date);
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');

  return `${year}-${month}-${day}`;
}

// === 初始化設定表（如果需要） ===
function initSettingsSheet() {
  const ss = getSpreadsheet();
  let sheet = ss.getSheetByName('Settings');

  if (!sheet) {
    sheet = ss.insertSheet('Settings');

    // 設定標題
    sheet.getRange(1, 1).setValue('key');
    sheet.getRange(1, 2).setValue('value');

    // 預設設定
    const defaultSettings = [
      ['points_日常行程', 1],
      ['points_重要提醒', 2],
      ['points_功課', 2],
      ['points_考試', 3],
      ['points_社團', 2],
      ['points_才藝', 2],
      ['points_家庭活動', 3],
      ['points_重大事件', 5],
      ['points_長期目標', 10],
      ['points_goal', 50],
      ['password', '1234']
    ];

    for (let i = 0; i < defaultSettings.length; i++) {
      sheet.getRange(i + 2, 1).setValue(defaultSettings[i][0]);
      sheet.getRange(i + 2, 2).setValue(defaultSettings[i][1]);
    }
  }

  return { success: true, message: 'Settings sheet initialized' };
}

// === 初始化 History 表（如果需要） ===
function initHistorySheet() {
  const ss = getSpreadsheet();
  let sheet = ss.getSheetByName('History');

  if (!sheet) {
    sheet = ss.insertSheet('History');

    // 設定標題
    sheet.getRange(1, 1).setValue('date');
    sheet.getRange(1, 2).setValue('member');
    sheet.getRange(1, 3).setValue('content');
    sheet.getRange(1, 4).setValue('type');
    sheet.getRange(1, 5).setValue('completed_date');
    sheet.getRange(1, 6).setValue('points');
  }

  return { success: true, message: 'History sheet initialized' };
}
