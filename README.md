# 114 工科賽電腦修護職種 - PC 監控系統

Last Update 2025-11-21 12:41

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
├── 114pc-new.sln                     # Visual Studio 方案檔
├── 114pc-new.vbproj                  # Visual Basic 專案檔
├── 114pc-new.vbproj.user             # 專案使用者設定
│
├── Form1.vb                          # 主程式邏輯
├── Form1.Designer.vb                 # UI 設計檔案（自動生成）
├── Form1.resx                        # 資源檔案（控件、圖片等）
├── ApplicationEvents.vb              # 應用程式事件定義
│
├── README.md                         # 專案說明文件（本檔案）
│
├── My Project/
│   └── Application.Designer.vb       # 應用程式設定
│
├── .gitignore                        # Git 忽略檔案清單
├── .gitattributes                    # Git 屬性設定
│
└── obj/                              # 編譯輸出目錄（自動生成）
    └── Debug/
        └── net8.0-windows/           # .NET 8 Windows 目標框架輸出
```

### 重要檔案說明

| 檔案/目錄 | 說明 |
|----------|------|
| **Form1.vb** | 主要程式邏輯，包含所有功能實現 |
| **Form1.Designer.vb** | Windows Form 設計器自動生成的控件定義 |
| **Form1.resx** | 資源檔案，存放嵌入式資源（字串、圖片等） |
| **ApplicationEvents.vb** | 應用程式生命週期事件（Startup、Shutdown等） |
| **114pc-new.vbproj** | 專案配置檔，定義編譯參數、依賴等 |
| **My Project/Application.Designer.vb** | 應用程式全域設定 |
| **.gitignore** | Git 版本控制忽略規則 |
| **.gitattributes** | Git 換行符號和編碼設定 |

---

## 🎯 功能說明

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

#### 連線參數
```visualbasic
BaudRate: 9600
DataBits: 8
Parity: None
StopBits: One
```

#### 連線確認
連線成功後會自動發送字元 `'c'` 至 Arduino，作為連線確認訊號。

---

### 2. CPU Loading 監控

#### 功能描述
- 即時監控系統 CPU 使用率（百分比）
- 根據負載程度顯示不同顏色
- 每秒更新一次數據
- **需要先建立連線才能啟動監控**

#### 顏色指示規則
| 使用率範圍 | 顏色 | 說明 |
|-----------|------|------|
| 0% - 50% | 🟢 綠色 | 正常負載 |
| 51% - 84% | 🟡 黃色 | 中度負載 |
| ≥ 85% | 🔴 紅色 | 高負載 |

#### 使用方式
1. 先建立 COM Port 連線
2. 點擊 **Start CPU** 按鈕開始監控
3. 系統會每秒更新 CPU 使用率並傳送至 Arduino
4. 右側色塊會根據負載程度變色
5. 點擊 **Stop CPU** 按鈕停止監控

#### 資料傳送格式
```
格式: "LOAD" + 數值 + "\n"

範例:
CPU 30% → "LOAD30\n"
CPU 65% → "LOAD65\n"
CPU 90% → "LOAD90\n"
```

#### 技術實作
```visualbasic
' 使用 Windows Performance Counter
cpuCounter = New PerformanceCounter("Processor", "% Processor Time", "_Total")
Dim cpuUsage As Single = cpuCounter.NextValue()

' 傳送資料
SerialPort1.Write($"LOAD{cpuPercent}" & vbLf)
```

---

### 3. RAM Loading 監控

#### 功能描述
- 即時監控系統記憶體使用率（百分比）
- 根據負載程度顯示不同顏色
- 每秒更新一次數據
- **需要先建立連線才能啟動監控**

#### 顏色指示規則
與 CPU 監控相同（參考上方表格）

#### 使用方式
1. 先建立 COM Port 連線
2. 點擊 **Start RAM** 按鈕開始監控
3. 系統會每秒更新 RAM 使用率並傳送至 Arduino
4. 右側色塊會根據負載程度變色
5. 點擊 **Stop RAM** 按鈕停止監控

#### 資料傳送格式
```
格式: "LOAD" + 數值 + "\n"

範例:
RAM 45% → "LOAD45\n"
RAM 70% → "LOAD70\n"
RAM 88% → "LOAD88\n"
```

#### 技術實作
```visualbasic
' 使用 Windows Performance Counter
ramCounter = New PerformanceCounter("Memory", "% Committed Bytes In Use")
Dim ramUsage As Single = ramCounter.NextValue()

' 傳送資料
SerialPort1.Write($"LOAD{ramPercent}" & vbLf)
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
- ✅ 僅接受 `0` 和 `1` 字元
- ✅ 必須輸入完整 4 位元
- ❌ 其他字元會顯示「格式錯誤」
- ❌ 未滿 4 位元會顯示「Not BIN Format」

#### 資料傳送格式（封包格式）
```
[SOF][CMD][LEN][Data][CHK][EOF]
0xFF  'E'   1    值     值   0xFE

範例：輸入 1010 (十進位 10)
0xFF 0x45 0x01 0x0A 0x0A 0xFE
```

**注意**: EEPROM 寫入使用封包格式，與 CPU/RAM 監控的文字格式不同。

---

### 5. 藍芽模組資訊

#### 功能描述
根據崗位編號自動產生並顯示藍芽模組名稱。

#### 命名規則
```
格式: ODD/EVEN-編號(2位數)-二進位後4位

範例:
崗位編號 1: 00000001 → 後4位: 0001 → 奇數 → ODD-01-0001
崗位編號 2: 00000010 → 後4位: 0010 → 偶數 → EVEN-02-0010
```

#### 自訂崗位編號
```visualbasic
' 在 Form1.vb 中修改常數
Private Const STATION_NUMBER As Integer = 1  ' 修改此值
```

---

## 📡 通訊協定

### CPU/RAM 監控資料格式（文字格式）

**格式**: `"LOAD" + 數值 + "\n"`

```
範例:
LOAD30\n  (CPU 或 RAM 使用率 30%)
LOAD65\n  (CPU 或 RAM 使用率 65%)
LOAD90\n  (CPU 或 RAM 使用率 90%)
```

**特點**:
- 使用文字格式，易於除錯
- 換行符號 `\n` (0x0A) 作為資料分隔
- Arduino 可使用 `Serial.readStringUntil('\n')` 讀取

### EEPROM 寫入封包格式

**新格式**: `"WRITE" + 數值 + "\n"`

```
範例:
WRITE0\n   (EEPROM 寫入十進位 0)
WRITE5\n   (EEPROM 寫入十進位 5)
WRITE12\n  (EEPROM 寫入十進位 12)
WRITE15\n  (EEPROM 寫入十進位 15)
```

**特點**:
- 與 CPU/RAM 監控格式一致（都是文字 + 換行符號）
- Arduino 可用相同的 `Serial.readStringUntil('\n')` 讀取
- 判斷字串開頭是 "LOAD" 還是 "WRITE" 執行對應功能
- 簡化通訊協定，提高可讀性

### 連線確認

連線時發送: `'c'` (ASCII 99, 0x63)

---

## 🔧 Arduino 端接收範例

### CPU/RAM 資料接收（推薦方法）

```cpp
void loop() {
    if (Serial.available() > 0) {
        // 讀取直到換行符號
        String data = Serial.readStringUntil('\n');
        
        // 檢查是否以 "LOAD" 開頭
        if (data.startsWith("LOAD")) {
            // 提取數值部分
            String valueStr = data.substring(4);
            int loadValue = valueStr.toInt();
            
            Serial.print("Loading: ");
            Serial.print(loadValue);
            Serial.println("%");
            
            // 根據數值控制 LED 或顯示
            if (loadValue <= 50) {
                setColor(GREEN);
            } else if (loadValue <= 84) {
                setColor(YELLOW);
            } else {
                setColor(RED);
            }
        }
    }
}
```

### EEPROM 寫入接收

```cpp
byte packetBuffer[10];
int packetIndex = 0;
bool receiving = false;

void loop() {
    if (Serial.available() > 0) {
        byte inByte = Serial.read();
        
        if (inByte == 0xFF && !receiving) {
            receiving = true;
            packetIndex = 0;
            packetBuffer[packetIndex++] = inByte;
        } else if (receiving) {
            packetBuffer[packetIndex++] = inByte;
            
            if (inByte == 0xFE) {
                processPacket(packetBuffer, packetIndex);
                receiving = false;
            }
        }
    }
}

void processPacket(byte* packet, int length) {
    if (packet[1] == 'E') {  // EEPROM 命令
        byte value = packet[3];
        EEPROM.write(0, value);
        Serial.print("EEPROM 寫入: ");
        Serial.println(value);
    }
}
```

詳細的接收範例請參考 `CPU_RAM_Loading_Data_Format.md`。

---

## 💻 安裝與執行

### 從原始碼編譯

1. 開啟 Visual Studio 2022
2. 開啟專案檔案 `114pc-new.vbproj`
3. 選擇建置配置（Debug 或 Release）
4. 按下 `F5` 或點擊「開始」執行

### 首次執行

1. 確保已安裝藍芽裝置驅動程式
2. 將藍芽模組與電腦配對（產生 COM Port）
3. 執行程式，選擇對應的 COM Port
4. 點擊 Open 建立連線
5. 點擊 Start CPU/RAM 開始監控

---

## 🐛 常見問題排除

### 問題 1: 找不到 COM Port

**解決方案**:
1. 檢查裝置管理員中藍芽裝置是否正常
2. 重新配對藍芽裝置
3. 點擊「重新整理」按鈕

### 問題 2: 無法開啟 COM Port

**解決方案**:
1. 確認 COM Port 未被其他程式佔用
2. 檢查使用者權限
3. 重新啟動程式

### 問題 3: CPU/RAM 監控按鈕無法點擊

**原因**: 需要先建立 COM Port 連線

**解決方案**:
1. 選擇 COM Port
2. 點擊 Open 按鈕建立連線
3. 連線成功後才能啟動監控

### 問題 4: 監控數值顯示為 0

**解決方案**:
1. 以系統管理員身分執行程式
2. 檢查效能計數器服務是否啟動
3. 重新啟動 Windows Management Instrumentation 服務

### 問題 5: Arduino 收不到資料

**解決方案**:
1. 確認 Arduino Baud Rate 設定為 9600
2. 檢查序列埠是否正確開啟
3. 確認使用 `Serial.readStringUntil('\n')` 讀取資料

---

## 🎨 使用者介面

### 按鈕狀態邏輯

| 按鈕 | 未連線 | 已連線 | 監控中 |
|-----|--------|--------|--------|
| Open | ✅ 啟用 | ❌ 停用 | ❌ 停用 |
| Close | ❌ 停用 | ✅ 啟用 | ✅ 啟用 |
| Start CPU | ❌ 停用 | ✅ 啟用 | ❌ 停用 |
| Stop CPU | ❌ 停用 | ❌ 停用 | ✅ 啟用 |
| Start RAM | ❌ 停用 | ✅ 啟用 | ❌ 停用 |
| Stop RAM | ❌ 停用 | ❌ 停用 | ✅ 啟用 |
| Write EEPROM | ❌ 停用 | ✅ 啟用 | ✅ 啟用 |

---

## 📊 程式架構

### 核心方法

```visualbasic
' 初始化
Form1_Load()                    ' 程式啟動初始化
InitializeUI()                  ' UI 初始化
InitializePerformanceCounters() ' 效能計數器初始化
InitializePortWatcher()         ' COM Port 熱插拔監控初始化

' 連線管理
ButtonOpen_Click()              ' 開啟 COM Port
ButtonClose_Click()             ' 關閉 COM Port
ConfigureSerialPort()           ' 設定序列埠參數
UpdateConnectionStatus()        ' 更新連線狀態

' 監控功能
ButtonStartCPU_Click()          ' 啟動 CPU 監控
ButtonStopCPU_Click()           ' 停止 CPU 監控
TimerCPU_Tick()                 ' CPU 監控計時器（每秒）
ButtonStartRAM_Click()          ' 啟動 RAM 監控
ButtonStopRAM_Click()           ' 停止 RAM 監控
TimerRAM_Tick()                 ' RAM 監控計時器（每秒）

' EEPROM 寫入
TextBoxBinary_TextChanged()     ' 二進位輸入即時轉換
ButtonWrite_Click()             ' 寫入 EEPROM

' 輔助功能
GetRAMUsage()                   ' 取得 RAM 使用率
GetLoadingColor()               ' 根據百分比取得顏色
SetBluetoothName()              ' 設定藍芽模組名稱
LoadComPorts()                  ' 載入 COM Port 清單
OnPortChanged()                 ' COM Port 變更事件
CleanupResources()              ' 資源清理
```

### 關鍵常數

```visualbasic
STATION_NUMBER = 1              ' 崗位編號
BAUD_RATE = 9600                ' 通訊速率
THRESHOLD_LOW = 50              ' 低負載閾值
THRESHOLD_MEDIUM = 84           ' 中負載閾值
```

---

## 🔄 版本歷史

### Version 2.1 (2025-01-11)
- ✅ 修改 CPU/RAM 資料格式為 "LOAD+數值+\n"
- ✅ 簡化通訊協定，提高可讀性
- ✅ 修正監控功能需要連線才能啟動的邏輯
- ✅ 清理冗餘程式碼

### Version 2.0 (2025-01-11)
- ✅ 修改資料傳送格式為文字格式
- ✅ 修正 RAM 監控無數據問題
- ✅ 完整的中文註解

### Version 1.0.0 (2025-01-XX)
- ✨ 初始版本發布
- ✅ 基本功能實作

---

## 📖 參考資料

### 技術文件
- [System.Diagnostics.PerformanceCounter](https://learn.microsoft.com/zh-tw/dotnet/api/system.diagnostics.performancecounter)
- [System.IO.Ports.SerialPort](https://learn.microsoft.com/zh-tw/dotnet/api/system.io.ports.serialport)
- [System.Management](https://learn.microsoft.com/zh-tw/dotnet/api/system.management)

### Arduino 相關
- [Arduino Serial Communication](https://www.arduino.cc/reference/en/language/functions/communication/serial/)
- [Arduino EEPROM Library](https://www.arduino.cc/en/Reference/EEPROM)

### 專案文件
- `CPU_RAM_Loading_Data_Format.md` - 詳細的資料格式說明
- `Form1.vb` - 主程式原始碼（含完整註解）

---

## 👥 專案資訊

### 競賽資訊
- **學年度**: 113 學年度
- **競賽**: 工業類科學生技藝競賽
- **職種**: 電腦修護職種
- **站別**: 第二站
- **學校**: 新化高工
- **崗位**: 01

### 開發單位
新化高工

---

## ⚡ 快速開始

```
1. 開啟 Visual Studio → 開啟 114pc-new.vbproj
2. 按 F5 執行程式
3. 連接藍芽裝置，選擇 COM Port → 點擊 Open
4. 點擊 Start CPU 和 Start RAM 開始監控
5. 在 Arduino 序列埠監視器查看: LOAD30, LOAD45 等資料
6. 輸入 4 位二進位 → 點擊 Write to EEPROM 寫入資料
```

*最後更新: 2025-11-21*  
*文件版本: 2.1*  
*開發單位: 新化高工 *