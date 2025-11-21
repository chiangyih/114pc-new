# 114 工科賽電腦修護職種 - PC 監控系統

Last Update 2025-11-21 12:41
<img width="802" height="482" alt="image" src="https://github.com/user-attachments/assets/06379281-4c39-4cbe-93ac-410c227938c7" />

## 📋 專案簡介

本專案為 **113 學年度工業類科學生技藝競賽電腦修護職種第二站** 的應用程式，用於監控電腦的 CPU 和 RAM 使用率，並透過藍芽（COM Port）將資料傳送至 Arduino/MCU 進行顯示。同時提供 EEPROM 資料寫入功能。

### 功能特色

✅ **即時 CPU 監控** - 顯示系統 CPU 使用率並以顏色區分負載狀態  
✅ **即時 RAM 監控** - 顯示系統記憶體使用率並以顏色區分負載狀態  
✅ **藍芽通訊** - 透過 COM Port 與 Arduino/MCU 進行通訊  
✅ **EEPROM 寫入** - 支援二進位資料轉換並寫入 Arduino EEPROM  
✅ **熱插拔支援** - 自動偵測 COM Port 的插入與移除  
✅ **簡化資料格式** - 使用 "LOAD" + 數值 + 換行符號的文字格式傳送資料

---

## 🖥️ 系統需求

### 開發環境
- **作業系統**: Windows 10/11
- **開發工具**: Visual Studio 2022 (或更高版本)
- **框架**: .NET Framework 4.8 或 .NET 6.0+
- **語言**: Visual Basic .NET

### 執行環境
- **作業系統**: Windows 10/11
- **硬體**: 支援藍芽或 USB 轉 Serial 的 COM Port
- **權限**: 需要系統效能計數器讀取權限

---

## 🏗️ 專案結構

```
114pc-new/
│
├── 📦 方案與專案設定
│   ├── 114pc-new.sln                 # Visual Studio 方案檔
│   ├── 114pc-new.vbproj              # Visual Basic 專案檔 (.NET 8.0)
│   ├── 114pc-new.vbproj.user         # 專案使用者設定
│
├── 💻 核心程式碼
│   ├── Form1.vb                      # 主應用程式類別 (~450 行)
│   │   ├─ COM Port 管理邏輯
│   │   ├─ CPU/RAM 效能監控
│   │   ├─ EEPROM 寫入功能
│   │   ├─ 藍芽通訊協議
│   │   └─ 事件處理與清理
│   │
│   ├── Form1.Designer.vb             # Windows Form UI 設計檔
│   │   └─ 控件配置、佈局定義
│   │
│   └── ApplicationEvents.vb          # 應用程式生命週期事件
│       └─ 程式啟動/關閉時的事件處理
│
├── ⚙️ 設定與資源
│   ├── My Project/
│   │   └── Application.Designer.vb   # 應用程式全域設定
│   │       ├─ 應用程式名稱與版本
│   │       ├─ 啟動表單設定
│   │       └─ 應用程式事件
│   │
│   └── (Form1.resx)                  # 資源檔案
│
├── 📚 文件
│   ├── README.md                     # 專案說明文件（本檔案）
│   ├── DEVELOPMENT_PLAN.md           # 開發計畫書
│   ├── TECH_SPEC.md                  # 技術規格書
│   ├── DOCUMENTATION_INDEX.md        # 文檔索引
│   ├── GitHub_Spec_Kit_Compliance.md # GitHub 規範檢查
│   ├── CHANGELOG.md                  # 版本歷史
│   └── CPU_RAM_Loading_Data_Format.md # 通訊協議詳細說明
│
├── 🔒 版本控制
│   ├── .gitignore                    # Git 忽略規則
│   │   ├─ bin/, obj/ 目錄
│   │   ├─ *.user 檔案
│   │   ├─ 暫存檔
│   │   └─ IDE 特定檔案
│   │
│   └── .gitattributes                # Git 屬性設定
│       ├─ 換行符號處理
│       └─ 文件編碼設定
│
└── 🔨 編譯輸出 (自動生成)
    └── obj/
        └── Debug/
            └── net8.0-windows/
                ├── 114pc-new.AssemblyInfo.vb
                ├── .NETCoreApp,Version=v8.0.AssemblyAttributes.vb
                └── (編譯中間檔)
```

### 重要檔案說明

| 檔案/目錄 | 行數 | 說明 | 重要性 |
|----------|------|------|--------|
| **Form1.vb** | ~450 | 主應用程式邏輯，包含所有功能實現。包括：COM Port 管理、CPU/RAM 監控、EEPROM 寫入、藍芽通訊、事件處理 | ⭐⭐⭐⭐⭐ |
| **Form1.Designer.vb** | - | Windows Form 設計器自動生成的控件定義、UI 配置與事件綁定 | ⭐⭐⭐ |
| **ApplicationEvents.vb** | - | 應用程式生命週期事件（Startup、Shutdown等） | ⭐⭐ |
| **114pc-new.vbproj** | - | 專案配置檔，定義編譯參數、NuGet 依賴、目標框架（.NET 8.0）等 | ⭐⭐⭐⭐ |
| **My Project/Application.Designer.vb** | - | 應用程式全域設定、啟動表單、應用程式事件處理器 | ⭐⭐ |
| **README.md** | ~426 | 專案使用者手冊與快速開始指南 | ⭐⭐⭐⭐ |
| **.gitignore** | - | Git 版本控制忽略規則（排除編譯輸出、暫存檔等） | ⭐⭐⭐ |
| **.gitattributes** | - | Git 換行符號和編碼設定（確保跨平台相容性） | ⭐⭐ |

---

## 📊 程式架構

### 核心方法列表

#### 初始化模組

```visualbasic
' 程式啟動
Form1_Load(sender As Object, e As EventArgs)
    └─ InitializeUI()                   ' UI 初始化
    └─ InitializePerformanceCounters() ' 效能計數器初始化
    └─ InitializePortWatcher()         ' COM Port 熱插拔監控初始化
    └─ UpdateConnectionStatus(False)   ' 設定初始連線狀態為 Disconnect
```

#### 連線管理模組

```visualbasic
' COM Port 偵測與管理
LoadComPorts()                  ' 載入系統可用 COM Port
ButtonRefresh_Click()           ' 重新整理 COM Port 清單

' COM Port 開啟
ButtonOpen_Click()              ' 開啟 COM Port
  └─ ConfigureSerialPort()      ' 設定序列埠參數 (9600, 8, N, 1)
  └─ SerialPort1.Open()         ' 開啟序列埠
  └─ SerialPort1.Write("c")     ' 發送連線確認字元
  └─ UpdateConnectionStatus(True)  ' 更新 UI 狀態

' COM Port 關閉
ButtonClose_Click()             ' 關閉 COM Port
  └─ StopAllMonitoring()        ' 停止所有監控
  └─ SerialPort1.Close()        ' 關閉序列埠
  └─ UpdateConnectionStatus(False) ' 更新 UI 狀態
  └─ ResetDisplayValues()       ' 重置顯示值

' 連線狀態管理
UpdateConnectionStatus(isConnected As Boolean)
  ├─ 更新 LabelConnectionStatus 顯示
  ├─ 更新按鈕啟用/停用狀態
  └─ 停止監控（如果斷開連線）

' COM Port 熱插拔偵測
InitializePortWatcher()         ' 初始化 WMI 事件監聽
OnPortChanged()                 ' COM Port 變更時自動重新載入清單
```

#### CPU 監控模組

```visualbasic
' CPU 監控控制
ButtonStartCPU_Click()          ' 啟動 CPU 監控
  └─ ToggleMonitoring(TimerCPU, ButtonStartCPU, ButtonStopCPU, True)

ButtonStopCPU_Click()           ' 停止 CPU 監控
  └─ ToggleMonitoring(TimerCPU, ButtonStartCPU, ButtonStopCPU, False)
  └─ PanelCPUColor.BackColor = Color.Gray ' 重置顏色

' CPU 監控計時器（每秒觸發）
TimerCPU_Tick(sender As Object, e As EventArgs)
  ├─ 讀取 cpuCounter.NextValue() 取得 CPU 使用率
  ├─ 四捨五入為整數百分比
  ├─ 更新 LabelCPUValue 顯示
  ├─ 計算顏色：GetLoadingColor(cpuPercent)
  ├─ 更新 PanelCPUColor 背景色
  └─ 若已連線，則傳送 "LOAD " + cpuPercent + vbLf
```

#### RAM 監控模組

```visualbasic
' RAM 監控控制
ButtonStartRAM_Click()          ' 啟動 RAM 監控
  └─ ToggleMonitoring(TimerRAM, ButtonStartRAM, ButtonStopRAM, True)

ButtonStopRAM_Click()           ' 停止 RAM 監控
  └─ ToggleMonitoring(TimerRAM, ButtonStartRAM, ButtonStopRAM, False)
  └─ PanelRAMColor.BackColor = Color.Gray ' 重置顏色

' RAM 監控計時器（每秒觸發）
TimerRAM_Tick(sender As Object, e As EventArgs)
  ├─ 呼叫 GetRAMUsage() 取得 RAM 使用率
  ├─ 四捨五入為整數百分比
  ├─ 更新 LabelRAMValue 顯示
  ├─ 計算顏色：GetLoadingColor(ramPercent)
  ├─ 更新 PanelRAMColor 背景色
  └─ 若已連線，則傳送 "LOAD " + ramPercent + vbLf

' 取得 RAM 使用率
GetRAMUsage() As Single         ' 從效能計數器讀取 RAM %
```

#### EEPROM 寫入模組

```visualbasic
' 二進位輸入實時轉換
TextBoxBinary_TextChanged()     ' 使用者輸入二進位數值時觸發
  ├─ 驗證輸入格式 (僅 0 和 1)
  ├─ 即時轉換為十進位顯示
  └─ 顯示顏色反饋（正確=藍色，錯誤=紅色）

' EEPROM 寫入
ButtonWrite_Click()             ' 寫入按鈕
  ├─ 驗證輸入：Regex.IsMatch(input, "^[01]{4}$")
  ├─ 若格式錯誤 → 顯示 "Not BIN Format" 訊息
  ├─ 轉換二進位為十進位：Convert.ToInt32(input, 2)
  ├─ 驗證連線狀態
  └─ 傳送 "WRITE " + decimalValue + vbLf
```

#### 效能計數器模組

```visualbasic
' 初始化效能計數器
InitializePerformanceCounters()
  ├─ 建立 CPU Counter："Processor", "% Processor Time", "_Total"
  ├─ 建立 RAM Counter："Memory", "% Committed Bytes In Use"
  └─ 預熱計數器：呼叫 NextValue() 一次
```

#### 輔助與清理模組

```visualbasic
' 顏色決定邏輯
GetLoadingColor(percent As Integer) As Color
  ├─ 0-50% → Color.Green
  ├─ 51-84% → Color.Yellow
  └─ ≥85% → Color.Red

' 藍芽模組名稱設定
SetBluetoothName()
  ├─ 將 STATION_NUMBER 轉為 8 位二進位
  ├─ 取後 4 位
  ├─ 判斷奇偶：ODD / EVEN
  └─ 組合格式："{ODD/EVEN}-{編號:D2}-{後4位}"

' 重置顯示值
ResetDisplayValues()
  ├─ LabelCPUValue = "0 %"
  ├─ LabelRAMValue = "0 %"
  ├─ PanelCPUColor = Color.Gray
  └─ PanelRAMColor = Color.Gray

' 監控切換
ToggleMonitoring(timer, startBtn, stopBtn, isStart)
  ├─ 若 isStart=True：啟動計時器，禁用 Start，啟用 Stop
  └─ 若 isStart=False：停止計時器，啟用 Start，禁用 Stop

' 資源清理
CleanupResources()
  ├─ 關閉序列埠 (if open)
  ├─ 釋放 cpuCounter
  ├─ 釋放 ramCounter
  └─ 停止 portWatcher

' 程式結束
ButtonExit_Click()              ' Exit 按鈕
  └─ CleanupResources() + Application.Exit()

' 視窗關閉事件
OnFormClosing()                 ' 視窗關閉時
  └─ CleanupResources()
```

#### 錯誤處理與日誌模組

```visualbasic
' 錯誤處理
HandleError(context, ex)        ' 統一錯誤處理
  ├─ 記錄至 Debug 輸出
  └─ 顯示訊息對話框

' 日誌輸出
LogDebug(message, portName, baudRate)
  └─ 格式化輸出至 Debug

' 訊息提示
ShowMessage(message, title, icon)
  └─ 顯示 MessageBox
```

### 關鍵常數與變數

```visualbasic
' 常數定義
Private Const STATION_NUMBER As Integer = 1
Private Const BAUD_RATE As Integer = 9600
Private Const PACKET_SOF As Byte = &HFF
Private Const PACKET_EOF As Byte = &HFE
Private Const CMD_CONNECT As String = "c"
Private Const THRESHOLD_LOW As Integer = 50
Private Const THRESHOLD_MEDIUM As Integer = 84

' 類別級變數
Private cpuCounter As PerformanceCounter           ' CPU 計數器
Private ramCounter As PerformanceCounter           ' RAM 計數器
Private WithEvents portWatcher As ManagementEventWatcher ' COM Port 監聽
```

### 控件狀態流程圖

```
未連線狀態：
┌─────────────────────────┐
│ ComboBoxCOM 啟用        │
│ ButtonOpen 啟用         │
│ ButtonClose 停用        │
│ ButtonStartCPU 停用     │
│ ButtonStartRAM 停用     │
│ ButtonWrite 停用        │
│ LabelConnectionStatus   │
│ = "Disconnect" (紅色)  │
└──────┬──────────────────┘
       │ 點擊 Open
       ▼
已連線狀態：
┌─────────────────────────┐
│ ComboBoxCOM 停用        │
│ ButtonOpen 停用         │
│ ButtonClose 啟用        │
│ ButtonStartCPU 啟用     │
│ ButtonStartRAM 啟用     │
│ ButtonWrite 啟用        │
│ LabelConnectionStatus   │
│ = "Connected" (綠色)   │
└──────┬──────────────────┘
       │ 點擊 StartCPU/StartRAM
       ▼
監控進行中狀態：
┌─────────────────────────┐
│ ButtonStopCPU/RAM 啟用  │
│ LabelCPUValue/RAMValue  │
│ 每秒更新                │
│ PanelCPU/RAMColor       │
│ 根據使用率顯示色彩      │
└─────────────────────────┘
```
---
