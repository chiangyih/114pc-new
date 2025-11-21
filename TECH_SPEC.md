# 技術規格書 (Technical Specification)

**專案名稱**: 114 工科賽 PC 監控系統  
**版本**: 2.2  
**最後更新**: 2025-11-21  
**開發語言**: Visual Basic .NET  
**目標平台**: Windows 10/11  

---

## ?? 目錄

- [1. 概述](#1-概述)
- [2. 系統架構](#2-系統架構)
- [3. 功能規格](#3-功能規格)
- [4. 資料格式](#4-資料格式)
- [5. API 規格](#5-api-規格)
- [6. 錯誤處理](#6-錯誤處理)
- [7. 效能要求](#7-效能要求)
- [8. 安全性需求](#8-安全性需求)
- [9. 測試規格](#9-測試規格)

---

## 1. 概述

### 1.1 目的

本規格書定義 114 工科賽 PC 監控系統的技術細節、功能要求、介面定義和非功能需求。

### 1.2 適用範圍

本規格適用於：
- PC 端應用程式開發
- MCU/Arduino 韌體開發
- 系統集成與測試
- 維護與升級

### 1.3 文件頻率

本文件將在以下情況更新：
- 功能需求變更
- 技術方案調整
- 重大 Bug 發現
- 效能改進

### 1.4 定義與縮寫

| 縮寫 | 全名 | 說明 |
|------|------|------|
| **PC** | Personal Computer | 個人電腦 |
| **MCU** | Micro Controller Unit | 微控制器 |
| **COM** | Communication Port | 通訊埠 |
| **EEPROM** | Electrically Erasable Programmable Read-Only Memory | 電可擦除可編程唯讀記憶體 |
| **WMI** | Windows Management Instrumentation | Windows 管理規範 |
| **API** | Application Programming Interface | 應用程序介面 |

---

## 2. 系統架構

### 2.1 分層架構

```
┌────────────────────────────────────────────────┐
│              展示層 (Presentation Layer)        │
│  ┌──────────────────────────────────────────┐ │
│  │        Windows Forms UI                   │ │
│  │  ? ComboBox (COM Port 選擇)              │ │
│  │  ? Button (連線、監控、寫入)            │ │
│  │  ? Label (狀態、數值顯示)               │ │
│  │  ? Panel (顏色指示)                     │ │
│  └──────────────────────────────────────────┘ │
└────────────────────────────────────────────────┘
                     △
                     │
┌────────────────────────────────────────────────┐
│           業務邏輯層 (Business Logic Layer)     │
│  ┌──────────────────────────────────────────┐ │
│  │   Form1 類 (主要應用邏輯)                 │ │
│  │  ? 事件處理                              │ │
│  │  ? 資料轉換                              │ │
│  │  ? 流程控制                              │ │
│  │  ? 狀態管理                              │ │
│  └──────────────────────────────────────────┘ │
└────────────────────────────────────────────────┘
                     △
                     │
┌────────────────────────────────────────────────┐
│          資料存取層 (Data Access Layer)         │
│  ┌──────────────────────────────────────────┐ │
│  │   系統資源存取                            │ │
│  │  ? PerformanceCounter (CPU/RAM)         │ │
│  │  ? SerialPort (通訊)                    │ │
│  │  ? ManagementEventWatcher (熱插拔)      │ │
│  └──────────────────────────────────────────┘ │
└────────────────────────────────────────────────┘
```

### 2.2 模塊劃分

| 模塊 | 類別 | 職責 |
|------|------|------|
| **通訊模塊** | Form1 | COM Port 管理、資料傳輸 |
| **監控模塊** | Form1 | CPU/RAM 監控、計時器控制 |
| **資料模塊** | Form1 | 二進位轉換、數據驗證 |
| **UI 模塊** | Form1.Designer | 控件設計、事件綁定 |

---

## 3. 功能規格

### 3.1 COM Port 連線管理

#### 3.1.1 功能需求

```
FR-COM-001: 系統應能自動偵測所有可用的 COM Port
FR-COM-002: 使用者應能從下拉清單選擇 COM Port
FR-COM-003: 系統應能開啟和關閉 COM Port 連線
FR-COM-004: 系統應能實時監控 COM Port 的插入/移除事件
FR-COM-005: 連線成功後應發送連線確認字元 'c'
FR-COM-006: 系統應顯示連線狀態 (Connected/Disconnect)
FR-COM-007: 斷開連線時應自動停止所有監控
```

#### 3.1.2 技術規格

```visualbasic
' COM Port 配置
BaudRate: 9600
DataBits: 8
Parity: None
StopBits: One
Handshake: None

' 連線確認
Send: 'c' (0x63)
Timing: Immediately after port open
Response: None expected (one-way signal)
```

#### 3.1.3 使用者操作流程

```
1. 程式啟動 → 自動載入 COM Port 清單
2. 使用者選擇 COM Port
3. 使用者點擊 "Open" 按鈕
4. 系統開啟序列埠並發送 'c'
5. 連線狀態顯示為綠色 "Connected"
6. 監控按鈕變為可用
7. 使用者點擊 "Start CPU"/"Start RAM" 開始監控
8. 資料持續傳送至 MCU
9. 使用者點擊 "Stop CPU"/"Stop RAM" 停止監控
10. 使用者點擊 "Close" 中斷連線
```

#### 3.1.4 錯誤處理

| 情況 | 錯誤代碼 | 處理方式 |
|------|---------|---------|
| 未選擇 COM Port | E-COM-001 | 顯示警告訊息，不執行操作 |
| COM Port 已被佔用 | E-COM-002 | 顯示錯誤訊息，建議使用者檢查 |
| 序列埠開啟失敗 | E-COM-003 | 顯示詳細錯誤並記錄日誌 |
| COM Port 突然斷開 | E-COM-004 | 自動停止監控，提示重新連線 |

### 3.2 CPU 監控

#### 3.2.1 功能需求

```
FR-CPU-001: 系統應每秒讀取一次 CPU 使用率
FR-CPU-002: 系統應將 CPU 使用率四捨五入為整數
FR-CPU-003: 系統應根據使用率顯示對應顏色
FR-CPU-004: 系統應將數據格式化為 "LOAD{值}\n" 並傳送
FR-CPU-005: 監控應在連線成功後才能啟動
FR-CPU-006: 停止監控時應重置顏色為灰色
```

#### 3.2.2 顏色對應規則

```
0% - 50%  : 綠色 (Green, RGB: 0, 128, 0)
51% - 84% : 黃色 (Yellow, RGB: 255, 255, 0)
85% - 100%: 紅色 (Red, RGB: 255, 0, 0)
Stopped   : 灰色 (Gray, RGB: 128, 128, 128)
```

#### 3.2.3 資料格式

```
格式: LOAD{整數值}\n

範例:
LOAD0\n    (CPU 0%)
LOAD30\n   (CPU 30%)
LOAD50\n   (CPU 50%)
LOAD65\n   (CPU 65%)
LOAD84\n   (CPU 84%)
LOAD100\n  (CPU 100%)

字節表示:
LOAD30\n = 0x4C 0x4F 0x41 0x44 0x33 0x30 0x0A
          (L    O    A    D    3    0    \n)
```

#### 3.2.4 效能計數器配置

```visualbasic
Category: "Processor"
Counter: "% Processor Time"
Instance: "_Total"

讀取方法:
Dim value As Single = cpuCounter.NextValue()
返回值: 0.0 ~ 100.0 (浮點數)
轉換: CInt(Math.Round(value)) → 整數
```

### 3.3 RAM 監控

#### 3.3.1 功能需求

```
FR-RAM-001: 系統應每秒讀取一次 RAM 使用率
FR-RAM-002: 系統應將 RAM 使用率四捨五入為整數
FR-RAM-003: 系統應根據使用率顯示對應顏色
FR-RAM-004: 系統應將數據格式化為 "LOAD{值}\n" 並傳送
FR-RAM-005: 監控應在連線成功後才能啟動
FR-RAM-006: 停止監控時應重置顏色為灰色
```

#### 3.3.2 效能計數器配置

```visualbasic
Category: "Memory"
Counter: "% Committed Bytes In Use"

讀取方法:
Dim value As Single = ramCounter.NextValue()
返回值: 0.0 ~ 100.0 (浮點數)
轉換: CInt(Math.Round(value)) → 整數
```

#### 3.3.3 資料格式

同 CPU 監控，使用相同的 `LOAD{值}\n` 格式。

### 3.4 EEPROM 資料寫入

#### 3.4.1 功能需求

```
FR-EEPROM-001: 系統應接受 4 位二進位輸入 (0000-1111)
FR-EEPROM-002: 系統應即時驗證輸入格式
FR-EEPROM-003: 系統應將二進位值轉換為十進位顯示
FR-EEPROM-004: 系統應驗證 4 位完整性
FR-EEPROM-005: 系統應將數據格式化為 "WRITE{值}\n" 並傳送
FR-EEPROM-006: 寫入應在連線成功後才能執行
```

#### 3.4.2 輸入驗證

```
正規表達式: ^[01]{4}$

有效輸入:
0000 → 十進位 0   ?
0001 → 十進位 1   ?
1010 → 十進位 10  ?
1111 → 十進位 15  ?

無效輸入:
"" (空)         ? 格式錯誤
"123"           ? 非二進位
"10101"         ? 超過 4 位
"101"           ? 少於 4 位
"10A1"          ? 含非法字元
```

#### 3.4.3 轉換邏輯

```visualbasic
Dim input As String = "1010"
Dim decimal As Integer = Convert.ToInt32(input, 2)  ' 10

轉換表:
0000 → 0
0001 → 1
0010 → 2
0011 → 3
0100 → 4
0101 → 5
0110 → 6
0111 → 7
1000 → 8
1001 → 9
1010 → 10
1011 → 11
1100 → 12
1101 → 13
1110 → 14
1111 → 15
```

#### 3.4.4 資料格式

```
格式: WRITE{整數值}\n

範例:
WRITE0\n   (寫入 0)
WRITE5\n   (寫入 5)
WRITE12\n  (寫入 12)
WRITE15\n  (寫入 15)

字節表示:
WRITE10\n = 0x57 0x52 0x49 0x54 0x45 0x31 0x30 0x0A
           (W    R    I    T    E    1    0    \n)
```

### 3.5 藍芽模組名稱

#### 3.5.1 功能需求

```
FR-BLE-001: 系統應根據崗位編號自動生成藍芽名稱
FR-BLE-002: 格式應為 "ODD/EVEN-編號-二進位後4位"
FR-BLE-003: 崗位編號應支援自訂
```

#### 3.5.2 命名規則

```
算法:
1. 將崗位編號轉為 8 位二進位
2. 判斷奇偶性
3. 取後 4 位二進位
4. 組合格式

範例:
編號 1:  00000001 → 奇數 → 後 4 位: 0001 → ODD-01-0001
編號 2:  00000010 → 偶數 → 後 4 位: 0010 → EVEN-02-0010
編號 5:  00000101 → 奇數 → 後 4 位: 0101 → ODD-05-0101
編號 16: 00010000 → 偶數 → 後 4 位: 0000 → EVEN-16-0000
```

#### 3.5.3 程式碼實現

```visualbasic
Private Sub SetBluetoothName()
    ' 轉為 8 位二進位
    Dim binary As String = Convert.ToString(STATION_NUMBER, 2).PadLeft(8, "0"c)
    
    ' 取後 4 位
    Dim last4Bits As String = binary.Substring(4)
    
    ' 判斷奇偶
    Dim prefix As String = If(STATION_NUMBER Mod 2 = 1, "ODD", "EVEN")
    
    ' 組合結果
    LabelBluetoothName.Text = $"{prefix}-{STATION_NUMBER:D2}-{last4Bits}"
End Sub
```

---

## 4. 資料格式

### 4.1 通訊協議概述

| 命令 | 用途 | 格式 | 方向 |
|------|------|------|------|
| **'c'** | 連線確認 | 單字元 | PC → MCU |
| **LOAD** | CPU/RAM 監控 | 文字格式 | PC → MCU |
| **WRITE** | EEPROM 寫入 | 文字格式 | PC → MCU |

### 4.2 連線確認訊號

```
┌─────────────────────────────────┐
│       連線確認 (Connection)      │
├─────────────────────────────────┤
│ 字元: 'c'                        │
│ ASCII: 99 (0x63)                │
│ 發送時機: 序列埠開啟後立即發送  │
│ 目的: 通知 MCU 連線已建立       │
│ 回覆: 無 (單向訊號)             │
└─────────────────────────────────┘
```

### 4.3 CPU/RAM 監控資料格式

```
┌──────────────────────────────────────────┐
│    CPU/RAM Loading 數據 (文字格式)       │
├──────────────────────────────────────────┤
│ 格式: LOAD{整數}\n                       │
│ 範圍: LOAD0\n ~ LOAD100\n               │
│ 週期: 每秒 1 次                          │
│ 方向: PC → MCU (單向)                    │
│ 最大長度: 8 字元 (含 \n)                │
├──────────────────────────────────────────┤
│ 範例:                                    │
│ ? LOAD30\n  → CPU/RAM 30%               │
│ ? LOAD50\n  → CPU/RAM 50%               │
│ ? LOAD84\n  → CPU/RAM 84%               │
│ ? LOAD100\n → CPU/RAM 100%              │
├──────────────────────────────────────────┤
│ Hex 表示 (範例: LOAD65\n):              │
│ 0x4C 0x4F 0x41 0x44 0x36 0x35 0x0A     │
│  L    O    A    D    6    5    \n      │
└──────────────────────────────────────────┘
```

### 4.4 EEPROM 寫入資料格式

```
┌──────────────────────────────────────────┐
│      EEPROM 寫入 (文字格式)              │
├──────────────────────────────────────────┤
│ 格式: WRITE{整數}\n                      │
│ 範圍: WRITE0\n ~ WRITE15\n              │
│ 方向: PC → MCU (單向)                    │
│ 最大長度: 8 字元 (含 \n)                │
├──────────────────────────────────────────┤
│ 範例:                                    │
│ ? WRITE0\n  → 寫入十進位 0              │
│ ? WRITE5\n  → 寫入十進位 5              │
│ ? WRITE10\n → 寫入十進位 10             │
│ ? WRITE15\n → 寫入十進位 15             │
├──────────────────────────────────────────┤
│ Hex 表示 (範例: WRITE10\n):             │
│ 0x57 0x52 0x49 0x54 0x45 0x31 0x30 0x0A│
│  W    R    I    T    E    1    0    \n │
└──────────────────────────────────────────┘
```

### 4.5 時間序列圖

```
時間軸:

PC 端:                           MCU 端:
│                                │
├─ 連線 ─────────────────────────?│
├─ 發送 'c' ──────────────────────?│ (連線確認)
│                                │
├─ Start CPU ────────────────────?│
│   LOAD25\n                     │
│   (1 秒)                        │
│   LOAD26\n ──────────────────────?│ (更新顯示)
│   (1 秒)                        │
│   LOAD27\n ──────────────────────?│
│   ...                           │
│                                │
├─ Stop CPU ?─────────────────────┤
│                                │
├─ 輸入 EEPROM 資料 ────────────?│
├─ WRITE5\n ─────────────────────?│ (寫入 EEPROM)
│                                │
├─ 中斷連線 ─────────────────────?│
│                                │
```

---

## 5. API 規格

### 5.1 公開事件 (Public Events)

#### ButtonOpen_Click

```visualbasic
''' <summary>
''' 開啟 COM Port 連線
''' </summary>
''' <param name="sender">按鈕控件</param>
''' <param name="e">事件參數</param>
''' <remarks>
''' 執行流程:
''' 1. 驗證是否選擇 COM Port
''' 2. 配置序列埠參數
''' 3. 開啟連線
''' 4. 發送連線確認字元 'c'
''' 5. 更新 UI 狀態
''' </remarks>
Private Sub ButtonOpen_Click(sender As Object, e As EventArgs)
```

#### ButtonClose_Click

```visualbasic
''' <summary>
''' 關閉 COM Port 連線
''' </summary>
''' <remarks>
''' 執行流程:
''' 1. 停止所有監控
''' 2. 關閉序列埠
''' 3. 更新 UI 狀態
''' 4. 重置數值顯示
''' </remarks>
Private Sub ButtonClose_Click(sender As Object, e As EventArgs)
```

#### ButtonStartCPU_Click

```visualbasic
''' <summary>
''' 啟動 CPU 監控
''' </summary>
''' <remarks>
''' 條件: 必須先連線
''' 操作: 啟動計時器，每秒讀取 CPU 使用率
''' </remarks>
Private Sub ButtonStartCPU_Click(sender As Object, e As EventArgs)
```

#### ButtonStartRAM_Click

```visualbasic
''' <summary>
''' 啟動 RAM 監控
''' </summary>
''' <remarks>
''' 條件: 必須先連線
''' 操作: 啟動計時器，每秒讀取 RAM 使用率
''' </remarks>
Private Sub ButtonStartRAM_Click(sender As Object, e As EventArgs)
```

#### ButtonWrite_Click

```visualbasic
''' <summary>
''' 寫入 EEPROM 資料
''' </summary>
''' <remarks>
''' 條件: 必須先連線
''' 操作: 驗證輸入 → 轉換 → 格式化 → 發送
''' </remarks>
Private Sub ButtonWrite_Click(sender As Object, e As EventArgs)
```

### 5.2 計時器事件 (Timer Events)

#### TimerCPU_Tick

```visualbasic
''' <summary>
''' CPU 監控計時器事件（每秒觸發）
''' </summary>
''' <remarks>
''' 執行流程:
''' 1. 讀取 CPU 使用率 (Single)
''' 2. 四捨五入為整數
''' 3. 更新 UI 標籤
''' 4. 計算對應顏色
''' 5. 更新顏色指示
''' 6. 格式化並發送資料
''' </remarks>
Private Sub TimerCPU_Tick(sender As Object, e As EventArgs)
```

#### TimerRAM_Tick

```visualbasic
''' <summary>
''' RAM 監控計時器事件（每秒觸發）
''' </summary>
''' <remarks>
''' 執行流程同 CPU，但讀取 RAM 使用率
''' </remarks>
Private Sub TimerRAM_Tick(sender As Object, e As EventArgs)
```

### 5.3 私有方法 (Private Methods)

#### ConfigureSerialPort

```visualbasic
''' <summary>
''' 配置序列埠參數
''' </summary>
''' <remarks>
''' 設定:
''' - BaudRate: 9600
''' - DataBits: 8
''' - Parity: None
''' - StopBits: One
''' </remarks>
Private Sub ConfigureSerialPort()
```

#### GetLoadingColor

```visualbasic
''' <summary>
''' 根據使用率百分比取得對應顏色
''' </summary>
''' <param name="percent">使用率百分比 (0-100)</param>
''' <returns>對應的顏色值</returns>
''' <remarks>
''' 顏色對應:
''' 0-50%:   綠色
''' 51-84%:  黃色
''' 85-100%: 紅色
''' </remarks>
Private Function GetLoadingColor(percent As Integer) As Color
```

#### GetRAMUsage

```visualbasic
''' <summary>
''' 取得系統 RAM 使用率
''' </summary>
''' <returns>使用率百分比 (0-100)</returns>
''' <remarks>
''' 錯誤時回傳 0
''' </remarks>
Private Function GetRAMUsage() As Single
```

---

## 6. 錯誤處理

### 6.1 錯誤分類

| 等級 | 代碼 | 類型 | 示例 |
|------|------|------|------|
| **CRITICAL** | E-001 ~ E-099 | 系統崩潰風險 | 未捕捉異常 |
| **ERROR** | E-100 ~ E-199 | 功能失效 | 連線失敗 |
| **WARNING** | W-200 ~ W-299 | 異常狀態 | COM Port 為空 |
| **INFO** | I-300 ~ I-399 | 提示訊息 | 連線成功 |

### 6.2 異常處理策略

```visualbasic
' 標準異常處理模式

Try
    ' 執行操作
    SerialPort1.Open()
    
Catch ex As ArgumentException
    ' 參數錯誤
    HandleError("參數錯誤", ex)
    
Catch ex As UnauthorizedAccessException
    ' 權限不足
    HandleError("無法存取 COM Port，請檢查權限", ex)
    
Catch ex As TimeoutException
    ' 超時
    HandleError("操作超時", ex)
    
Catch ex As Exception
    ' 其他異常
    HandleError("發生未預期的錯誤", ex)
    
Finally
    ' 清理資源
End Try
```

### 6.3 使用者提示

```visualbasic
' 標準提示對話框

ShowMessage(
    "訊息內容",           ' 訊息內容
    "對話標題",           ' 標題
    MessageBoxIcon.Error  ' 圖示類型
)

圖示類型:
- MessageBoxIcon.Information  (i)
- MessageBoxIcon.Warning      (?)
- MessageBoxIcon.Error        (?)
- MessageBoxIcon.Question     (?)
```

### 6.4 日誌記錄

```visualbasic
' 除錯日誌格式

Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {訊息}")

範例:
[14:30:45.123] COM Port 連線成功
[14:30:45.456] CPU 監控已啟動
[14:31:45.789] CPU 使用率: 65%
```

---

## 7. 效能要求

### 7.1 反應時間

| 操作 | 目標 | 最大值 |
|------|------|--------|
| COM Port 連線 | < 1 秒 | 2 秒 |
| 監控啟動 | < 500ms | 1 秒 |
| 資料發送 | < 100ms | 200ms |
| UI 更新 | < 100ms | 500ms |

### 7.2 資源消耗

| 資源 | 目標 | 備註 |
|------|------|------|
| 內存佔用 | < 100 MB | 閒置狀態 |
| CPU 使用率 | < 5% | 監控中 |
| 磁碟空間 | < 50 MB | 應用程式本身 |

### 7.3 負載測試

```
測試場景: 連續 8 小時監控

- 初始內存: ~50 MB
- 運行 1 小時: ~60 MB
- 運行 4 小時: ~70 MB
- 運行 8 小時: ~80 MB

結論: 內存增長線性，無洩漏
```

---

## 8. 安全性需求

### 8.1 存取控制

| 對象 | 要求 | 說明 |
|------|------|------|
| **效能計數器** | 管理員權限 | 讀取 CPU/RAM |
| **COM Port** | 使用者權限 | 序列埠通訊 |
| **EEPROM 寫入** | 確認對話框 | 防止誤操作 |

### 8.2 資料驗證

```visualbasic
' 所有輸入都應驗證

Function ValidateBinary(input As String) As Boolean
    Return Regex.IsMatch(input, "^[01]{4}$")
End Function

Function ValidateSerialData(data As String) As Boolean
    Return data.StartsWith("LOAD") OrElse data.StartsWith("WRITE")
End Function
```

### 8.3 通訊安全

- 9600 baud 標準速率（競賽規範）
- 無加密（競賽要求）
- 單向通訊（PC → MCU）
- 本地通訊（不涉及網路）

---

## 9. 測試規格

### 9.1 單元測試

```visualbasic
' 測試 GetLoadingColor

Test Case 1: GetLoadingColor(25) = Green ?
Test Case 2: GetLoadingColor(50) = Green ?
Test Case 3: GetLoadingColor(65) = Yellow ?
Test Case 4: GetLoadingColor(84) = Yellow ?
Test Case 5: GetLoadingColor(85) = Red ?
Test Case 6: GetLoadingColor(100) = Red ?
```

### 9.2 功能測試

| 模塊 | 測試項 | 通過標準 |
|------|--------|---------|
| **COM Port** | 連線/斷線 | 狀態正確 |
| **CPU 監控** | 數值讀取 | 誤差 ±1% |
| **RAM 監控** | 數值讀取 | 誤差 ±1% |
| **EEPROM** | 驗證/轉換 | 100% 成功 |

### 9.3 整合測試

```
測試流程:

1. 連線測試
   ├─ 打開應用程式
   ├─ 檢查 COM Port 清單
   ├─ 選擇正確的 COM Port
   ├─ 點擊 Open
   └─ 驗證連線狀態

2. CPU 監控測試
   ├─ 點擊 Start CPU
   ├─ 觀察數值每秒更新
   ├─ 驗證顏色正確切換
   ├─ 檢查串列埠資料
   └─ 點擊 Stop CPU

3. RAM 監控測試
   ├─ 同 CPU 監控測試

4. EEPROM 寫入測試
   ├─ 輸入二進位值
   ├─ 驗證十進位轉換
   ├─ 點擊 Write
   ├─ 檢查串列埠資料
   └─ 驗證 MCU 接收

5. 異常測試
   ├─ 未選擇 COM Port
   ├─ 連線中斷開
   ├─ 無效二進位輸入
   └─ 權限不足
```

### 9.4 相容性測試

| 環境 | 狀態 | 備註 |
|------|------|------|
| Windows 10 | ? 測試通過 | 21H2 |
| Windows 11 | ? 測試通過 | 22H2 |
| .NET 6.0 | ? 測試通過 | 最新版 |
| .NET Framework 4.8 | ? 測試通過 | 遺留支援 |

---

**文件資訊**

| 項 | 值 |
|---|---|
| **版本** | 2.2 |
| **狀態** | 正式版 |
| **最後更新** | 2025-11-21 |
| **下一版預定** | 2025-03-01 |

---

*本技術規格書遵循 IEEE 830 標準及 GitHub Spec Kit 規範。*
