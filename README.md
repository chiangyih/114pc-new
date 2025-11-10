# 114 工科賽電腦修護職種 - PC 監控系統

## ?? 專案簡介

本專案為 **113 學年度工業類科學生技藝競賽電腦修護職種第二站** 的應用程式，用於監控電腦的 CPU 和 RAM 使用率，並透過藍芽（COM Port）將資料傳送至 Arduino/MCU 進行顯示。同時提供 EEPROM 資料寫入功能。

### 功能特色

? **即時 CPU 監控** - 顯示系統 CPU 使用率並以顏色區分負載狀態  
? **即時 RAM 監控** - 顯示系統記憶體使用率並以顏色區分負載狀態  
? **藍芽通訊** - 透過 COM Port 與 Arduino/MCU 進行雙向通訊  
? **EEPROM 寫入** - 支援二進位資料轉換並寫入 Arduino EEPROM  
? **熱插拔支援** - 自動偵測 COM Port 的插入與移除  
? **獨立運作** - CPU/RAM 監控可在未連線時獨立執行  

---

## ??? 系統需求

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

## ??? 專案結構

```
114pc-new/
│
├── Form1.vb                      # 主程式邏輯
├── Form1.Designer.vb             # UI 設計檔案（自動生成）
├── Form1.resx                    # 資源檔案
├── My Project/
│   └── Application.Designer.vb   # 應用程式設定
└── README.md                     # 專案說明文件
```

---

## ?? 功能說明

### 1. COM Port 連線管理

#### 功能描述
- 自動偵測系統可用的 COM Port
- 手動選擇 COM Port 並建立連線
- 支援 COM Port 熱插拔偵測
- 顯示即時連線狀態

#### 使用方式
1. 啟動程式後，系統會自動載入可用的 COM Port
2. 從下拉選單選擇目標 COM Port（通常為藍芽裝置）
3. 點擊 **Open** 按鈕建立連線
4. 連線成功後，狀態顯示為綠色 **Connected**
5. 點擊 **Close** 按鈕中斷連線
6. 點擊 **重新整理** 按鈕可重新掃描 COM Port

#### 技術細節
```visualbasic
' 連線參數
BaudRate: 9600
DataBits: 8
Parity: None
StopBits: One
Encoding: ASCII (預設)
```

#### 連線時自動動作
- 發送字元 `'c'` 至 Arduino，作為連線確認訊號

---

### 2. CPU Loading 監控

#### 功能描述
- 即時監控系統 CPU 使用率（百分比）
- 根據負載程度顯示不同顏色
- 每秒更新一次數據
- 可獨立於藍芽連線運作

#### 顏色指示規則
| 使用率範圍 | 顏色 | 說明 |
|-----------|------|------|
| 0% - 50% | ?? 綠色 | 正常負載 |
| 51% - 84% | ?? 黃色 | 中度負載 |
| ? 85% | ?? 紅色 | 高負載 |

#### 使用方式
1. 點擊 **Start CPU** 按鈕開始監控
2. 系統會每秒更新 CPU 使用率
3. 右側色塊會根據負載程度變色
4. 點擊 **Stop CPU** 按鈕停止監控

#### 資料傳送格式（連線時）
```
[SOF][CMD][LEN][Data][CHK][EOF]
0xFF  'C'   4   %,R,G,B  SUM  0xFE
```

#### 技術實作
```visualbasic
' 使用 Windows Performance Counter
cpuCounter = New PerformanceCounter("Processor", "% Processor Time", "_Total")
Dim cpuUsage As Single = cpuCounter.NextValue()
```

---

### 3. RAM Loading 監控

#### 功能描述
- 即時監控系統記憶體使用率（百分比）
- 根據負載程度顯示不同顏色
- 每秒更新一次數據
- 可獨立於藍芽連線運作

#### 顏色指示規則
與 CPU 監控相同（參考上方表格）

#### 使用方式
1. 點擊 **Start RAM** 按鈕開始監控
2. 系統會每秒更新 RAM 使用率
3. 右側色塊會根據負載程度變色
4. 點擊 **Stop RAM** 按鈕停止監控

#### 資料傳送格式（連線時）
```
[SOF][CMD][LEN][Data][CHK][EOF]
0xFF  'R'   4   %,R,G,B  SUM  0xFE
```

#### 技術實作
```visualbasic
' 使用 Windows Performance Counter
ramCounter = New PerformanceCounter("Memory", "% Committed Bytes In Use")
Dim ramUsage As Single = ramCounter.NextValue()
```

---

### 4. EEPROM 資料寫入

#### 功能描述
- 輸入 4 位元二進位值（0000-1111）
- 即時轉換為十進位顯示（0-15）
- 格式驗證與錯誤提示
- 透過藍芽將資料寫入 Arduino EEPROM

#### 使用方式
1. 在文字框中輸入 4 位二進位值（例如：`1010`）
2. 系統會即時顯示對應的十進位值（例如：`10`）
3. 確認藍芽已連線
4. 點擊 **Write to EEPROM** 按鈕寫入資料

#### 輸入驗證
- ? 僅接受 `0` 和 `1` 字元
- ? 必須輸入完整 4 位元
- ? 其他字元會顯示「格式錯誤」
- ? 未滿 4 位元會顯示「Not BIN Format」

#### 資料傳送格式
```
[SOF][CMD][LEN][Data][CHK][EOF]
0xFF  'E'   1    值     值   0xFE

範例：輸入 1010 (十進位 10)
0xFF 0x45 0x01 0x0A 0x0A 0xFE
```

#### 即時轉換功能
```visualbasic
' 二進位 → 十進位
輸入: "1010"
顯示: "10"

輸入: "1111"
顯示: "15"
```

---

### 5. 藍芽模組資訊

#### 功能描述
- 根據崗位編號自動產生藍芽模組名稱
- 顯示格式：`ODD/EVEN-編號-二進位後4位`

#### 命名規則
```visualbasic
崗位編號: 1 (STATION_NUMBER = 1)
二進位: 00000001
後4位: 0001
奇偶: 奇數 (ODD)
結果: ODD-01-0001
```

| 崗位編號 | 奇偶 | 二進位 | 模組名稱 |
|---------|------|--------|---------|
| 1 | 奇數 | 0001 | ODD-01-0001 |
| 2 | 偶數 | 0010 | EVEN-02-0010 |
| 3 | 奇數 | 0011 | ODD-03-0011 |
| 10 | 偶數 | 1010 | EVEN-10-1010 |

#### 自訂崗位編號
```visualbasic
' 在 Form1.vb 中修改常數
Private Const STATION_NUMBER As Integer = 1  ' 修改此值
```

---

## ?? 通訊協定

### 封包格式

所有傳送至 Arduino/MCU 的資料都使用以下統一格式：

```
[SOF] [CMD] [LEN] [Data...] [CHK] [EOF]
```

#### 欄位說明

| 欄位 | 長度 | 說明 | 值 |
|-----|------|------|-----|
| SOF | 1 byte | 封包起始標記 | 0xFF |
| CMD | 1 byte | 命令類型 | 'C', 'R', 'E' |
| LEN | 1 byte | 資料長度 | 1 或 4 |
| Data | 1-4 bytes | 實際資料 | 依命令而定 |
| CHK | 1 byte | 檢查碼（資料總和 & 0xFF）| 計算值 |
| EOF | 1 byte | 封包結束標記 | 0xFE |

### 命令類型

#### 1. CPU 資料封包 (CMD = 'C' / 0x43)

```
封包結構:
0xFF 0x43 0x04 [%] [R] [G] [B] [CHK] 0xFE

範例：CPU 65%, RGB(255, 255, 0) 黃色
0xFF 0x43 0x04 0x41 0xFF 0xFF 0x00 0x3F 0xFE
       ↓    ↓    ↓    ↓    ↓    ↓    ↓
      CMD  LEN   %    R    G    B   CHK
```

- **%**: CPU 使用率百分比 (0-100)
- **R**: 紅色分量 (0-255)
- **G**: 綠色分量 (0-255)
- **B**: 藍色分量 (0-255)
- **CHK**: (% + R + G + B) & 0xFF

#### 2. RAM 資料封包 (CMD = 'R' / 0x52)

```
封包結構:
0xFF 0x52 0x04 [%] [R] [G] [B] [CHK] 0xFE

範例：RAM 30%, RGB(0, 255, 0) 綠色
0xFF 0x52 0x04 0x1E 0x00 0xFF 0x00 0x1D 0xFE
       ↓    ↓    ↓    ↓    ↓    ↓    ↓
      CMD  LEN   %    R    G    B   CHK
```

- 格式與 CPU 封包相同，僅命令碼不同

#### 3. EEPROM 寫入封包 (CMD = 'E' / 0x45)

```
封包結構:
0xFF 0x45 0x01 [Value] [CHK] 0xFE

範例：寫入數值 10 (二進位 1010)
0xFF 0x45 0x01 0x0A 0x0A 0xFE
       ↓    ↓    ↓    ↓
      CMD  LEN  值   CHK
```

- **Value**: 0-15 (4 位元二進位值)
- **CHK**: Value & 0xFF

#### 4. 連線確認 (純字元)

```
連線時發送: 'c' (0x63)
```

### Checksum 計算方式

```visualbasic
' 將所有 Data 欄位的 byte 相加，並與 0xFF 進行 AND 運算
Dim checksum As Byte = CByte((data1 + data2 + ... + dataN) And &HFF)
```

### Arduino 接收範例

```cpp
// 封包緩衝區
byte packetBuffer[10];
int packetIndex = 0;
bool receiving = false;

void loop() {
    if (Serial.available() > 0) {
        byte inByte = Serial.read();
        
        // 檢測 SOF
        if (inByte == 0xFF && !receiving) {
            receiving = true;
            packetIndex = 0;
            packetBuffer[packetIndex++] = inByte;
        }
        // 接收資料
        else if (receiving) {
            packetBuffer[packetIndex++] = inByte;
            
            // 檢測 EOF
            if (inByte == 0xFE) {
                processPacket(packetBuffer, packetIndex);
                receiving = false;
            }
        }
    }
}

void processPacket(byte* packet, int length) {
    byte cmd = packet[1];
    byte len = packet[2];
    
    switch(cmd) {
        case 'C':  // CPU 資料
            handleCPU(packet);
            break;
        case 'R':  // RAM 資料
            handleRAM(packet);
            break;
        case 'E':  // EEPROM 寫入
            handleEEPROM(packet);
            break;
    }
}
```

---

## ?? 安裝與執行

### 1. 從原始碼編譯

#### 步驟
1. 開啟 Visual Studio 2022
2. 開啟專案檔案 `114pc-new.vbproj`
3. 選擇建置配置（Debug 或 Release）
4. 按下 `F5` 或點擊「開始」執行

#### 建置設定
```
目標框架: .NET Framework 4.8 / .NET 6.0
平台目標: Any CPU
輸出類型: Windows Application
```

### 2. 執行程式

#### 首次執行
1. 確保已安裝藍芽裝置驅動程式
2. 將藍芽模組與電腦配對（產生 COM Port）
3. 執行程式，選擇對應的 COM Port

#### 常見問題排除

**問題 1: 找不到 COM Port**
```
解決方案:
1. 檢查裝置管理員中藍芽裝置是否正常
2. 重新配對藍芽裝置
3. 點擊「重新整理」按鈕
```

**問題 2: 無法開啟 COM Port**
```
解決方案:
1. 確認 COM Port 未被其他程式佔用
2. 檢查使用者權限
3. 重新啟動程式
```

**問題 3: CPU/RAM 監控無數據**
```
解決方案:
1. 以系統管理員身分執行程式
2. 檢查效能計數器服務是否啟動
3. 重新啟動 Windows Management Instrumentation 服務
```

**問題 4: 藍芽傳輸失敗**
```
解決方案:
1. 檢查 Arduino 是否正確接收資料
2. 確認 Baud Rate 設定一致（9600）
3. 檢查藍芽模組電源
```

---

## ?? 使用者介面說明

### 主視窗配置

```
┌─────────────────────────────────────────────────────────────┐
│  113 學年度 工業類科學生技藝競賽 電腦修護職種 台中高工...      │
├─────────────────────────────────────────────────────────────┤
│ ┌─ COM Port 連線管理 ─────┐  ┌─ EEPROM 資料寫入 ──────┐  │
│ │ COM Port: [下拉選單▼]    │  │ 輸入二進位值:           │  │
│ │ [重新整理]               │  │     [____] → 十進位: 0  │  │
│ │                          │  │                         │  │
│ │ [Open]  [Close]          │  │ [Write to EEPROM]       │  │
│ │ 連線狀態: Disconnect ??   │  └─────────────────────────┘  │
│ └──────────────────────────┘                              │
│                               ┌─ 藍牙模組資訊 ──────────┐  │
│ ┌─ CPU Loading 監控 ───────┐  │ 模組名稱命名:           │  │
│ │ CPU 使用率: 0 %          │  │ ODD-01-0001            │  │
│ │ [Start CPU] [Stop CPU] ? │  └─────────────────────────┘  │
│ └──────────────────────────┘                              │
│                               ┌─────────┐                 │
│ ┌─ RAM Loading 監控 ───────┐  │  [Exit] │                 │
│ │ RAM 使用率: 0 %          │  └─────────┘                 │
│ │ [Start RAM] [Stop RAM] ? │                              │
│ └──────────────────────────┘                              │
└─────────────────────────────────────────────────────────────┘
```

### 按鈕狀態說明

| 按鈕 | 初始狀態 | 連線後 | 監控中 |
|-----|---------|--------|--------|
| Open | 啟用 | 停用 | - |
| Close | 停用 | 啟用 | - |
| Start CPU | 啟用 | 啟用 | 停用 |
| Stop CPU | 停用 | 停用 | 啟用 |
| Start RAM | 啟用 | 啟用 | 停用 |
| Stop RAM | 停用 | 停用 | 啟用 |
| Write to EEPROM | 停用 | 啟用 | - |

---

## ?? 程式碼架構

### 主要類別與方法

#### Form1.vb 核心方法

```visualbasic
' ========== 初始化 ==========
Form1_Load()                      ' 程式啟動初始化
InitializePerformanceCounters()   ' 初始化效能計數器
InitializePortWatcher()           ' 初始化 COM Port 監控

' ========== UI 事件處理 ==========
ButtonOpen_Click()                ' 開啟 COM Port
ButtonClose_Click()               ' 關閉 COM Port
ButtonRefresh_Click()             ' 重新整理 COM Port
ButtonStartCPU_Click()            ' 開始 CPU 監控
ButtonStopCPU_Click()             ' 停止 CPU 監控
ButtonStartRAM_Click()            ' 開始 RAM 監控
ButtonStopRAM_Click()             ' 停止 RAM 監控
ButtonWrite_Click()               ' EEPROM 寫入
ButtonExit_Click()                ' 離開程式
TextBoxBinary_TextChanged()       ' 二進位輸入即時轉換

' ========== 計時器事件 ==========
TimerCPU_Tick()                   ' CPU 監控更新 (1秒)
TimerRAM_Tick()                   ' RAM 監控更新 (1秒)

' ========== 資料處理 ==========
GetRAMUsage()                     ' 取得 RAM 使用率
GetLoadingColor()                 ' 根據百分比取得顏色
SendLoadingData()                 ' 傳送監控資料至 MCU

' ========== 輔助功能 ==========
SetBluetoothName()                ' 設定藍牙模組名稱
LoadComPorts()                    ' 載入可用 COM Port
UpdateConnectionStatus()          ' 更新連線狀態
ResetDisplayValues()              ' 重置顯示數值
OnPortChanged()                   ' COM Port 熱插拔事件
OnFormClosing()                   ' 程式關閉事件
```

### 關鍵常數定義

```visualbasic
Private Const STATION_NUMBER As Integer = 1   ' 崗位編號
Private Const BAUD_RATE As Integer = 9600     ' 通訊速率
```

### 效能計數器變數

```visualbasic
Private cpuCounter As PerformanceCounter      ' CPU 計數器
Private ramCounter As PerformanceCounter      ' RAM 計數器
Private WithEvents portWatcher As ManagementEventWatcher  ' COM Port 監控
```

---

## ?? 測試建議

### 功能測試清單

#### ? COM Port 連線測試
- [ ] 可正確列出系統所有 COM Port
- [ ] 可成功開啟選定的 COM Port
- [ ] 可正常關閉已開啟的 COM Port
- [ ] 熱插拔時能自動更新清單
- [ ] 連線狀態顯示正確（紅色/綠色）

#### ? CPU 監控測試
- [ ] 啟動監控後每秒更新數值
- [ ] 數值範圍在 0-100% 之間
- [ ] 低負載顯示綠色 (?50%)
- [ ] 中負載顯示黃色 (51-84%)
- [ ] 高負載顯示紅色 (?85%)
- [ ] 停止監控後數值不再更新
- [ ] 未連線時仍可正常監控

#### ? RAM 監控測試
- [ ] 啟動監控後每秒更新數值
- [ ] 數值範圍在 0-100% 之間
- [ ] 顏色指示與負載對應正確
- [ ] 停止監控後數值不再更新
- [ ] 未連線時仍可正常監控

#### ? EEPROM 寫入測試
- [ ] 僅接受 0 和 1 字元
- [ ] 即時顯示十進位轉換值
- [ ] 未滿 4 位元時顯示錯誤
- [ ] 連線後才能寫入
- [ ] 寫入成功後顯示確認訊息

#### ? 藍芽通訊測試
- [ ] Arduino 能正確接收 CPU 資料
- [ ] Arduino 能正確接收 RAM 資料
- [ ] Arduino 能正確接收 EEPROM 資料
- [ ] Checksum 驗證正確
- [ ] 封包格式符合協定

### 壓力測試

```visualbasic
' 建議測試場景
1. 長時間連續監控 (1小時以上)
2. 快速切換監控啟動/停止
3. 頻繁開關 COM Port 連線
4. 大量 EEPROM 寫入操作
5. 多次熱插拔 COM Port 裝置
```

---

## ?? 效能與最佳化

### 系統資源使用

| 項目 | 典型值 | 備註 |
|-----|--------|------|
| CPU 使用率 | < 5% | 閒置時 |
| 記憶體使用 | ~50 MB | 監控啟動後 |
| 更新頻率 | 1 Hz | 每秒更新一次 |
| 封包大小 | 7-8 bytes | 依命令而定 |

### 最佳化建議

1. **減少 UI 更新頻率**
   ```visualbasic
   ' 如需降低 CPU 使用率，可調整計時器間隔
   TimerCPU.Interval = 2000  ' 改為 2 秒更新一次
   ```

2. **批次傳送資料**
   ```visualbasic
   ' 可累積多筆資料後一次傳送，減少通訊次數
   ```

3. **錯誤處理靜默化**
   ```visualbasic
   ' 監控過程中的錯誤採用靜默處理，避免中斷
   Catch ex As Exception
       ' 靜默失敗，避免中斷監控
   End Try
   ```

---

## ?? 安全性考量

### 資料驗證

1. **COM Port 選擇**
   - 檢查 COM Port 是否存在
   - 驗證連線狀態

2. **二進位輸入**
   - 正規表達式驗證：`^[01]{4}$`
   - 長度限制：4 位元

3. **封包驗證**
   - Checksum 確保資料完整性
   - SOF/EOF 標記確保封包邊界

### 權限需求

```
必要權限:
? 讀取系統效能計數器
? 存取 COM Port 裝置
? WMI 查詢 (熱插拔偵測)

選用權限:
○ 系統管理員 (更準確的效能數據)
```

---

## ?? 已知問題與限制

### 已知問題

1. **Performance Counter 初始化延遲**
   - **現象**: 第一次讀取 CPU 使用率可能為 0
   - **解決**: 在 `InitializePerformanceCounters()` 中預先呼叫 `NextValue()`

2. **COM Port 熱插拔延遲**
   - **現象**: 拔除裝置後可能需要 1-2 秒才更新清單
   - **影響**: 不影響正常使用

3. **高 DPI 顯示器縮放**
   - **現象**: 部分 UI 元素可能顯示異常
   - **解決**: 設定應用程式 DPI 感知

### 限制

- **COM Port 數量**: 僅支援 COM1-COM256
- **監控頻率**: 最小間隔 1 秒（受效能計數器限制）
- **EEPROM 寫入**: 僅支援 0-15 (4 位元)
- **同時監控**: 最多 2 項（CPU + RAM）

---

## ?? 版本歷史

### Version 1.0.0 (2025-01-XX)
- ? 初始版本發布
- ? 實作 CPU 監控功能
- ? 實作 RAM 監控功能
- ? 實作藍芽通訊功能
- ? 實作 EEPROM 寫入功能
- ? 實作 COM Port 熱插拔偵測
- ? 實作獨立監控模式
- ?? 修正 ASCIIEncoding 唯讀錯誤
- ?? 修正 RAM 監控無數據問題

---

## ?? 參考資料

### 技術文件
- [System.Diagnostics.PerformanceCounter](https://learn.microsoft.com/zh-tw/dotnet/api/system.diagnostics.performancecounter)
- [System.IO.Ports.SerialPort](https://learn.microsoft.com/zh-tw/dotnet/api/system.io.ports.serialport)
- [System.Management](https://learn.microsoft.com/zh-tw/dotnet/api/system.management)

### Arduino 相關
- [Arduino Serial Communication](https://www.arduino.cc/reference/en/language/functions/communication/serial/)
- [Arduino EEPROM Library](https://www.arduino.cc/en/Reference/EEPROM)

### 競賽相關
- 113 學年度工業類科學生技藝競賽競賽規則
- 電腦修護職種競賽說明

---

## ?? 貢獻者

### 開發團隊
- **競賽崗位**: 台中高工 第二站 崗位號碼 01
- **學年度**: 113 學年度
- **競賽類別**: 工業類科學生技藝競賽 - 電腦修護職種

---

## ?? 授權條款

本專案為競賽作品，僅供教育與學習使用。

```
Copyright (c) 2025 台中高工
版權所有，保留所有權利
```

---

## ?? 聯絡資訊

### 技術支援
如有任何問題或建議，請聯絡：
- **競賽單位**: 台中高工
- **專案目錄**: `D:\OneDrive\21上課用資料夾\學生競賽或校外研習\2025_分區與工科賽\114工科賽\114pc-new\`

---

## ?? 學習資源

### 建議學習順序

1. **Visual Basic .NET 基礎**
   - 變數與資料型別
   - 控制流程
   - 事件驅動程式設計

2. **Windows Forms 應用程式**
   - UI 元件使用
   - 事件處理
   - Timer 控制項

3. **序列埠通訊**
   - SerialPort 類別
   - 資料傳送與接收
   - 封包設計

4. **系統監控**
   - PerformanceCounter 使用
   - WMI 查詢
   - 系統資源存取

5. **Arduino 程式設計**
   - Serial 通訊
   - EEPROM 操作
   - 資料解析

---

## ?? 未來發展方向

### 可能的擴充功能

- [ ] 新增磁碟 I/O 監控
- [ ] 新增網路流量監控
- [ ] 新增溫度監控（需硬體支援）
- [ ] 資料記錄與歷史曲線圖
- [ ] 支援多個 Arduino 裝置
- [ ] 自動連線功能
- [ ] 設定檔案存取
- [ ] 多語言介面

### 改進計畫

- [ ] 重構通訊協定為可擴充架構
- [ ] 實作單元測試
- [ ] 加入日誌記錄功能
- [ ] UI/UX 優化
- [ ] 效能優化

---

## ? 快速開始

### 5 分鐘快速上手

```
1. 開啟 Visual Studio → 開啟 114pc-new.vbproj
2. 按 F5 執行程式
3. 連接藍芽裝置，選擇 COM Port → 點擊 Open
4. 點擊 Start CPU 和 Start RAM 開始監控
5. 在 Arduino 序列埠監視器查看接收到的資料
```

### 常用快捷鍵

| 功能 | 快捷鍵 |
|-----|--------|
| 執行程式 | F5 |
| 停止偵錯 | Shift + F5 |
| 建置專案 | Ctrl + Shift + B |
| 儲存全部 | Ctrl + Shift + S |

---

**? 祝您競賽順利！?**

---

*最後更新: 2025-01-XX*  
*文件版本: 1.0.0*