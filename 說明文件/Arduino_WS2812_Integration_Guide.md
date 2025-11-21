# Arduino WS2812 整合指南

## 113 學年度工業類科學生技藝競賽 - 電腦修護職種
### PC 端 CPU Loading 同步顯示於 WS2812-8bits 8 LEDs

---

## ?? 目錄

1. [專案概述](#專案概述)
2. [硬體需求](#硬體需求)
3. [通訊協定說明](#通訊協定說明)
4. [封包格式詳解](#封包格式詳解)
5. [Arduino 程式架構](#arduino-程式架構)
6. [完整程式碼範例](#完整程式碼範例)
7. [測試與除錯](#測試與除錯)
8. [常見問題排除](#常見問題排除)

---

## 專案概述

### 功能需求

本專案需要實作 Arduino 端接收來自 PC 的 CPU Loading 資料，並同步顯示於 WS2812 LED 燈條（8 個 LEDs）。

**主要功能：**
- 接收 PC 透過序列埠（藍芽/USB）傳送的 CPU 使用率資料
- 根據 CPU Loading 百分比顯示對應顏色
- 8 個 LEDs 同時顯示相同顏色
- 每秒更新一次顯示

### 顏色對應規則

| CPU 使用率 | 顏色 | RGB 值 | 說明 |
|-----------|------|--------|------|
| 0% - 50% | ?? 綠色 | (0, 255, 0) | 正常負載 |
| 51% - 84% | ?? 黃色 | (255, 255, 0) | 中度負載 |
| 85% - 100% | ?? 紅色 | (255, 0, 0) | 高負載 |

### 系統架構

```
┌─────────────────┐          ┌──────────────────┐
│   PC (VB.NET)   │          │  Arduino + WS2812│
│                 │          │                  │
│ 1. 讀取 CPU %   │          │ 1. 接收封包      │
│ 2. 判斷顏色     │  藍芽/   │ 2. 解析資料      │
│ 3. 封裝封包     │  ══════> │ 3. 驗證 Checksum │
│ 4. 序列埠傳送   │   USB    │ 4. 控制 WS2812   │
│                 │          │ 5. 顯示顏色      │
└─────────────────┘          └──────────────────┘
       每秒更新 1 次                8 LEDs 同步
```

---

## 硬體需求

### Arduino 開發板
- **推薦型號**: Arduino Uno / Nano / Mega
- **微控制器**: ATmega328P 或更高階
- **工作電壓**: 5V
- **序列埠**: 支援 UART (TX/RX)

### WS2812 LED 燈條
- **型號**: WS2812B (8 LEDs)
- **工作電壓**: 5V DC
- **通訊協定**: 單線通訊
- **顏色**: 全彩 RGB (每顆 LED 可獨立控制)

### 藍芽模組（選用）
- **型號**: HC-05 或 HC-06
- **Baud Rate**: 9600 bps
- **工作電壓**: 3.3V - 6V
- **通訊協定**: UART

### 接線說明

#### WS2812 接線
```
Arduino Pin 6 ──→ WS2812 DIN (Data Input)
Arduino 5V    ──→ WS2812 VCC
Arduino GND   ──→ WS2812 GND
```

#### HC-05 藍芽模組接線
```
Arduino RX (Pin 0) ──→ HC-05 TX
Arduino TX (Pin 1) ──→ HC-05 RX
Arduino 5V         ──→ HC-05 VCC
Arduino GND        ──→ HC-05 GND
```

**?? 注意事項：**
- HC-05 的 RX 腳位最好加裝分壓電路（使用兩個電阻將 5V 降至 3.3V）
- WS2812 建議在電源端加裝 1000μF 電容以穩定電壓
- Data 訊號線建議加裝 330Ω 電阻保護

---

## 通訊協定說明

### 序列埠參數

```cpp
// Arduino Serial 初始化設定
Serial.begin(9600);  // Baud Rate: 9600 bps
// Data Bits: 8
// Parity: None
// Stop Bits: 1
```

**重要參數：**
- **Baud Rate**: 9600 bps（必須與 PC 端一致）
- **資料位元**: 8 bits
- **同位檢查**: 無 (None)
- **停止位元**: 1 bit
- **更新頻率**: 每秒 1 次

### PC 端連線確認

PC 端開啟連線時會發送字元 `'c'` (0x63) 作為連線確認訊號。

```cpp
// Arduino 可接收此訊號並執行相應動作
if (Serial.read() == 'c') {
    // 連線成功，可執行初始化動作
    // 例如：LED 閃爍、蜂鳴器提示等
}
```

---

## 封包格式詳解

### 標準封包結構

所有資料封包都遵循以下統一格式：

```
[SOF] [CMD] [LEN] [Data...] [CHK] [EOF]
  1B    1B    1B    N bytes   1B    1B
```

### 欄位說明

| 欄位名稱 | 長度 | 說明 | 數值範圍 |
|---------|------|------|---------|
| **SOF** (Start of Frame) | 1 byte | 封包起始標記 | 固定 `0xFF` |
| **CMD** (Command) | 1 byte | 命令類型 | 'C', 'R', 'E' |
| **LEN** (Length) | 1 byte | 資料長度 | 1 或 4 |
| **Data** | N bytes | 實際資料內容 | 依命令而定 |
| **CHK** (Checksum) | 1 byte | 檢查碼 | 資料總和 & 0xFF |
| **EOF** (End of Frame) | 1 byte | 封包結束標記 | 固定 `0xFE` |

### CPU Loading 封包（重點）

#### 命令碼 (CMD)
```
'C' = 0x43 (ASCII 67)
```

#### 封包結構
```
+------+------+------+------+------+------+------+------+------+
| 0xFF | 0x43 | 0x04 |  %   |  R   |  G   |  B   | CHK  | 0xFE |
+------+------+------+------+------+------+------+------+------+
  SOF    CMD    LEN   Data1  Data2  Data3  Data4  CHK    EOF
```

#### 資料欄位詳解

| 欄位 | 說明 | 數值範圍 | 範例 |
|-----|------|---------|------|
| **%** (Data1) | CPU 使用率百分比 | 0-100 | 65 (0x41) |
| **R** (Data2) | 紅色分量 | 0-255 | 255 (0xFF) |
| **G** (Data3) | 綠色分量 | 0-255 | 255 (0xFF) |
| **B** (Data4) | 藍色分量 | 0-255 | 0 (0x00) |

#### Checksum 計算方式

```cpp
// Checksum = (% + R + G + B) & 0xFF
uint8_t checksum = (percent + r + g + b) & 0xFF;
```

### 封包範例

#### 範例 1：CPU 30%（綠色）
```
封包內容:
0xFF 0x43 0x04 0x1E 0x00 0xFF 0x00 0x1D 0xFE

解析:
- SOF:  0xFF
- CMD:  0x43 ('C')
- LEN:  0x04 (4 bytes)
- %:    0x1E (30)
- R:    0x00 (0)
- G:    0xFF (255)
- B:    0x00 (0)
- CHK:  0x1D (30+0+255+0 = 285, 285 & 0xFF = 29 = 0x1D)
- EOF:  0xFE

顯示: 8 LEDs 全綠色 (0, 255, 0)
```

#### 範例 2：CPU 65%（黃色）
```
封包內容:
0xFF 0x43 0x04 0x41 0xFF 0xFF 0x00 0x3F 0xFE

解析:
- SOF:  0xFF
- CMD:  0x43 ('C')
- LEN:  0x04 (4 bytes)
- %:    0x41 (65)
- R:    0xFF (255)
- G:    0xFF (255)
- B:    0x00 (0)
- CHK:  0x3F (65+255+255+0 = 575, 575 & 0xFF = 63 = 0x3F)
- EOF:  0xFE

顯示: 8 LEDs 全黃色 (255, 255, 0)
```

#### 範例 3：CPU 90%（紅色）
```
封包內容:
0xFF 0x43 0x04 0x5A 0xFF 0x00 0x00 0x59 0xFE

解析:
- SOF:  0xFF
- CMD:  0x43 ('C')
- LEN:  0x04 (4 bytes)
- %:    0x5A (90)
- R:    0xFF (255)
- G:    0x00 (0)
- B:    0x00 (0)
- CHK:  0x59 (90+255+0+0 = 345, 345 & 0xFF = 89 = 0x59)
- EOF:  0xFE

顯示: 8 LEDs 全紅色 (255, 0, 0)
```

### RAM Loading 封包（擴充功能）

#### 命令碼 (CMD)
```
'R' = 0x52 (ASCII 82)
```

#### 封包結構
與 CPU 封包完全相同，僅命令碼不同：
```
0xFF 0x52 0x04 [%] [R] [G] [B] [CHK] 0xFE
```

### EEPROM 寫入封包（選用）

#### 命令碼 (CMD)
```
'E' = 0x45 (ASCII 69)
```

#### 封包結構
```
+------+------+------+-------+-------+------+
| 0xFF | 0x45 | 0x01 | Value | CHK   | 0xFE |
+------+------+------+-------+-------+------+
  SOF    CMD    LEN   Data    CHK     EOF
```

#### 範例：寫入數值 10 (二進位 1010)
```
0xFF 0x45 0x01 0x0A 0x0A 0xFE
```

---

## Arduino 程式架構

### 程式流程圖

```
         ┌─────────────┐
         │   開始      │
         └──────┬──────┘
                │
         ┌──────▼──────┐
         │ Serial 初始化│
         │ WS2812 初始化│
         └──────┬──────┘
                │
         ┌──────▼──────┐
         │  主迴圈 Loop │
         └──────┬──────┘
                │
         ┌──────▼──────┐
         │ 檢查序列埠   │
    ┌────┤ 是否有資料？├────┐
    │ 否 └─────────────┘ 是 │
    │                       │
    │               ┌───────▼──────┐
    │               │ 讀取 SOF (0xFF)│
    │               └───────┬──────┘
    │                       │
    │               ┌───────▼──────┐
    │               │ 讀取 CMD     │
    │               │ 判斷命令類型 │
    │               └───────┬──────┘
    │                       │
    │               ┌───────▼──────┐
    │               │ 讀取資料欄位 │
    │               │ (%, R, G, B) │
    │               └───────┬──────┘
    │                       │
    │               ┌───────▼──────┐
    │               │ 驗證 Checksum│
    │               └───────┬──────┘
    │                       │
    │               ┌───────▼──────┐
    │               │ 更新 WS2812  │
    │               │ 顯示顏色     │
    │               └───────┬──────┘
    │                       │
    └───────────────────────┘
                │
         ┌──────▼──────┐
         │   等待下次   │
         │   資料接收   │
         └──────────────┘
```

### 主要函式說明

#### 1. `setup()` - 初始化函式
```cpp
void setup() {
    // 初始化序列埠通訊
    Serial.begin(9600);
    
    // 初始化 WS2812
    strip.begin();
    strip.show();  // 關閉所有 LED
    
    // 可選：顯示啟動動畫
    startupAnimation();
}
```

#### 2. `loop()` - 主迴圈
```cpp
void loop() {
    // 檢查是否有足夠的資料
    if (Serial.available() >= 9) {
        // 讀取並處理封包
        processPacket();
    }
}
```

#### 3. `processPacket()` - 封包處理
```cpp
void processPacket() {
    // 1. 讀取 SOF
    // 2. 驗證命令碼
    // 3. 讀取資料
    // 4. 驗證 Checksum
    // 5. 更新 LED
}
```

#### 4. `handleCPU()` - CPU 資料處理
```cpp
void handleCPU(byte* packet) {
    // 提取百分比和 RGB 值
    // 驗證資料有效性
    // 更新 WS2812 顯示
}
```

#### 5. `setAllLEDs()` - 設定所有 LED
```cpp
void setAllLEDs(byte r, byte g, byte b) {
    // 將所有 8 個 LED 設為相同顏色
    for (int i = 0; i < NUM_LEDS; i++) {
        strip.setPixelColor(i, strip.Color(r, g, b));
    }
    strip.show();
}
```

---

## 完整程式碼範例

### 方案 1：基礎版本（推薦入門）

```cpp
/*
 * Arduino WS2812 CPU Monitor
 * 基礎版本 - 適合初學者
 * 
 * 硬體需求:
 * - Arduino Uno/Nano/Mega
 * - WS2812B LED 燈條 (8 LEDs)
 * - HC-05 藍芽模組 (選用)
 * 
 * 接線:
 * - WS2812 DIN -> Arduino Pin 6
 * - WS2812 VCC -> Arduino 5V
 * - WS2812 GND -> Arduino GND
 */

#include <Adafruit_NeoPixel.h>

// ===== 硬體設定 =====
#define LED_PIN 6        // WS2812 資料腳位
#define NUM_LEDS 8       // LED 數量
#define BAUD_RATE 9600   // 序列埠速率

// ===== 封包格式定義 =====
#define SOF 0xFF         // 封包起始標記
#define EOF_MARKER 0xFE  // 封包結束標記
#define CMD_CPU 'C'      // CPU 資料命令
#define CMD_RAM 'R'      // RAM 資料命令
#define PACKET_SIZE 9    // 完整封包大小

// ===== WS2812 物件 =====
Adafruit_NeoPixel strip(NUM_LEDS, LED_PIN, NEO_GRB + NEO_KHZ800);

// ===== 初始化函式 =====
void setup() {
    // 啟動序列埠通訊
    Serial.begin(BAUD_RATE);
    
    // 初始化 WS2812
    strip.begin();
    strip.setBrightness(50);  // 設定亮度 (0-255)
    strip.show();  // 初始化為關閉狀態
    
    // 啟動動畫（選用）
    startupAnimation();
}

// ===== 主迴圈 =====
void loop() {
    // 檢查是否有完整封包（9 bytes）
    if (Serial.available() >= PACKET_SIZE) {
        // 讀取第一個位元組
        byte sof = Serial.read();
        
        // 驗證 SOF
        if (sof == SOF) {
            processPacket();
        }
    }
}

// ===== 封包處理函式 =====
void processPacket() {
    // 讀取命令碼
    byte cmd = Serial.read();
    
    // 讀取資料長度
    byte len = Serial.read();
    
    // 根據命令類型處理
    if (cmd == CMD_CPU && len == 4) {
        handleCPUData();
    } else if (cmd == CMD_RAM && len == 4) {
        handleRAMData();
    } else {
        // 清除無效資料
        clearSerialBuffer();
    }
}

// ===== CPU 資料處理 =====
void handleCPUData() {
    // 讀取資料欄位
    byte percent = Serial.read();
    byte r = Serial.read();
    byte g = Serial.read();
    byte b = Serial.read();
    byte chk = Serial.read();
    byte eof = Serial.read();
    
    // 驗證 Checksum
    byte calculatedChecksum = (percent + r + g + b) & 0xFF;
    
    // 驗證 EOF
    if (eof == EOF_MARKER && chk == calculatedChecksum) {
        // 資料正確，更新 LED
        setAllLEDs(r, g, b);
        
        // 可選：輸出除錯訊息
        debugPrint(percent, r, g, b);
    } else {
        // 封包錯誤，顯示錯誤狀態（閃爍紅色）
        errorBlink();
    }
}

// ===== RAM 資料處理（擴充功能）=====
void handleRAMData() {
    // 與 CPU 處理方式相同
    handleCPUData();
}

// ===== 設定所有 LED 顏色 =====
void setAllLEDs(byte r, byte g, byte b) {
    for (int i = 0; i < NUM_LEDS; i++) {
        strip.setPixelColor(i, strip.Color(r, g, b));
    }
    strip.show();
}

// ===== 啟動動畫 =====
void startupAnimation() {
    // 彩虹效果
    for (int j = 0; j < 256; j++) {
        for (int i = 0; i < NUM_LEDS; i++) {
            strip.setPixelColor(i, Wheel((i * 256 / NUM_LEDS + j) & 255));
        }
        strip.show();
        delay(10);
    }
    
    // 清除顯示
    setAllLEDs(0, 0, 0);
}

// ===== 錯誤閃爍 =====
void errorBlink() {
    for (int i = 0; i < 3; i++) {
        setAllLEDs(255, 0, 0);  // 紅色
        delay(100);
        setAllLEDs(0, 0, 0);    // 關閉
        delay(100);
    }
}

// ===== 清除序列埠緩衝區 =====
void clearSerialBuffer() {
    while (Serial.available() > 0) {
        Serial.read();
    }
}

// ===== 除錯輸出 =====
void debugPrint(byte percent, byte r, byte g, byte b) {
    Serial.print("CPU: ");
    Serial.print(percent);
    Serial.print("% | RGB(");
    Serial.print(r);
    Serial.print(", ");
    Serial.print(g);
    Serial.print(", ");
    Serial.print(b);
    Serial.println(")");
}

// ===== 彩虹色輪函式 =====
uint32_t Wheel(byte WheelPos) {
    WheelPos = 255 - WheelPos;
    if (WheelPos < 85) {
        return strip.Color(255 - WheelPos * 3, 0, WheelPos * 3);
    }
    if (WheelPos < 170) {
        WheelPos -= 85;
        return strip.Color(0, WheelPos * 3, 255 - WheelPos * 3);
    }
    WheelPos -= 170;
    return strip.Color(WheelPos * 3, 255 - WheelPos * 3, 0);
}
```

### 方案 2：進階版本（狀態機架構）

```cpp
/*
 * Arduino WS2812 CPU Monitor
 * 進階版本 - 使用狀態機架構
 * 
 * 特色:
 * - 更穩定的封包接收
 * - 逾時處理機制
 * - 詳細除錯資訊
 * - 支援多種命令
 */

#include <Adafruit_NeoPixel.h>

// ===== 硬體設定 =====
#define LED_PIN 6
#define NUM_LEDS 8
#define BAUD_RATE 9600

// ===== 封包格式 =====
#define SOF 0xFF
#define EOF_MARKER 0xFE
#define CMD_CPU 'C'
#define CMD_RAM 'R'
#define CMD_EEPROM 'E'
#define CMD_CONNECT 'c'

// ===== 狀態機定義 =====
enum PacketState {
    WAIT_SOF,      // 等待起始標記
    READ_CMD,      // 讀取命令
    READ_LEN,      // 讀取長度
    READ_DATA,     // 讀取資料
    READ_CHK,      // 讀取檢查碼
    READ_EOF       // 讀取結束標記
};

// ===== 全域變數 =====
Adafruit_NeoPixel strip(NUM_LEDS, LED_PIN, NEO_GRB + NEO_KHZ800);
PacketState currentState = WAIT_SOF;
byte packetBuffer[10];
int bufferIndex = 0;
byte currentCmd = 0;
byte dataLength = 0;
unsigned long lastReceiveTime = 0;
const unsigned long TIMEOUT = 1000;  // 逾時 1 秒

// ===== 初始化 =====
void setup() {
    Serial.begin(BAUD_RATE);
    
    strip.begin();
    strip.setBrightness(50);
    strip.show();
    
    // 顯示就緒訊號（藍色閃爍）
    readySignal();
    
    Serial.println("Arduino WS2812 CPU Monitor Ready");
}

// ===== 主迴圈 =====
void loop() {
    // 檢查逾時
    if (millis() - lastReceiveTime > TIMEOUT && currentState != WAIT_SOF) {
        resetStateMachine();
    }
    
    // 處理序列埠資料
    if (Serial.available() > 0) {
        byte inByte = Serial.read();
        lastReceiveTime = millis();
        
        processStateMachine(inByte);
    }
}

// ===== 狀態機處理 =====
void processStateMachine(byte inByte) {
    switch (currentState) {
        case WAIT_SOF:
            if (inByte == SOF) {
                packetBuffer[0] = inByte;
                bufferIndex = 1;
                currentState = READ_CMD;
            }
            break;
            
        case READ_CMD:
            currentCmd = inByte;
            packetBuffer[bufferIndex++] = inByte;
            currentState = READ_LEN;
            break;
            
        case READ_LEN:
            dataLength = inByte;
            packetBuffer[bufferIndex++] = inByte;
            currentState = READ_DATA;
            break;
            
        case READ_DATA:
            packetBuffer[bufferIndex++] = inByte;
            if (bufferIndex >= (3 + dataLength)) {
                currentState = READ_CHK;
            }
            break;
            
        case READ_CHK:
            packetBuffer[bufferIndex++] = inByte;
            currentState = READ_EOF;
            break;
            
        case READ_EOF:
            if (inByte == EOF_MARKER) {
                packetBuffer[bufferIndex++] = inByte;
                processCompletePacket();
            }
            resetStateMachine();
            break;
    }
}

// ===== 處理完整封包 =====
void processCompletePacket() {
    // 驗證 Checksum
    if (!validateChecksum()) {
        Serial.println("Checksum Error!");
        errorBlink();
        return;
    }
    
    // 根據命令處理
    switch (currentCmd) {
        case CMD_CPU:
            handleCPUPacket();
            break;
        case CMD_RAM:
            handleRAMPacket();
            break;
        case CMD_CONNECT:
            handleConnectPacket();
            break;
        default:
            Serial.println("Unknown Command");
            break;
    }
}

// ===== 驗證 Checksum =====
bool validateChecksum() {
    byte sum = 0;
    for (int i = 3; i < 3 + dataLength; i++) {
        sum += packetBuffer[i];
    }
    sum &= 0xFF;
    
    return (sum == packetBuffer[3 + dataLength]);
}

// ===== 處理 CPU 封包 =====
void handleCPUPacket() {
    byte percent = packetBuffer[3];
    byte r = packetBuffer[4];
    byte g = packetBuffer[5];
    byte b = packetBuffer[6];
    
    setAllLEDs(r, g, b);
    
    Serial.print("CPU: ");
    Serial.print(percent);
    Serial.print("% -> RGB(");
    Serial.print(r); Serial.print(",");
    Serial.print(g); Serial.print(",");
    Serial.print(b); Serial.println(")");
}

// ===== 處理 RAM 封包 =====
void handleRAMPacket() {
    // 與 CPU 處理相同
    handleCPUPacket();
}

// ===== 處理連線確認 =====
void handleConnectPacket() {
    Serial.println("PC Connected!");
    
    // 顯示連線成功（綠色閃爍）
    for (int i = 0; i < 3; i++) {
        setAllLEDs(0, 255, 0);
        delay(200);
        setAllLEDs(0, 0, 0);
        delay(200);
    }
}

// ===== 重置狀態機 =====
void resetStateMachine() {
    currentState = WAIT_SOF;
    bufferIndex = 0;
    currentCmd = 0;
    dataLength = 0;
}

// ===== 設定所有 LED =====
void setAllLEDs(byte r, byte g, byte b) {
    for (int i = 0; i < NUM_LEDS; i++) {
        strip.setPixelColor(i, strip.Color(r, g, b));
    }
    strip.show();
}

// ===== 就緒訊號 =====
void readySignal() {
    for (int i = 0; i < 2; i++) {
        setAllLEDs(0, 0, 255);  // 藍色
        delay(300);
        setAllLEDs(0, 0, 0);
        delay(300);
    }
}

// ===== 錯誤閃爍 =====
void errorBlink() {
    for (int i = 0; i < 3; i++) {
        setAllLEDs(255, 0, 0);
        delay(100);
        setAllLEDs(0, 0, 0);
        delay(100);
    }
}

// ===== 彩虹色輪 =====
uint32_t Wheel(byte WheelPos) {
    WheelPos = 255 - WheelPos;
    if (WheelPos < 85) {
        return strip.Color(255 - WheelPos * 3, 0, WheelPos * 3);
    }
    if (WheelPos < 170) {
        WheelPos -= 85;
        return strip.Color(0, WheelPos * 3, 255 - WheelPos * 3);
    }
    WheelPos -= 170;
    return strip.Color(WheelPos * 3, 255 - WheelPos * 3, 0);
}
```

### 方案 3：簡化版本（最小程式碼）

```cpp
/*
 * Arduino WS2812 CPU Monitor
 * 簡化版本 - 最小程式碼實作
 */

#include <Adafruit_NeoPixel.h>

#define LED_PIN 6
#define NUM_LEDS 8

Adafruit_NeoPixel strip(NUM_LEDS, LED_PIN, NEO_GRB + NEO_KHZ800);

void setup() {
    Serial.begin(9600);
    strip.begin();
    strip.show();
}

void loop() {
    if (Serial.available() >= 9) {
        if (Serial.read() == 0xFF && Serial.read() == 'C') {
            Serial.read();  // 跳過 LEN
            byte p = Serial.read();
            byte r = Serial.read();
            byte g = Serial.read();
            byte b = Serial.read();
            byte chk = Serial.read();
            byte eof = Serial.read();
            
            if (eof == 0xFE && chk == ((p + r + g + b) & 0xFF)) {
                for (int i = 0; i < NUM_LEDS; i++) {
                    strip.setPixelColor(i, strip.Color(r, g, b));
                }
                strip.show();
            }
        }
    }
}
```

---

## 測試與除錯

### 測試步驟

#### 1. 硬體連接測試
```cpp
// 測試程式：LED 順序點亮
void testLEDs() {
    for (int i = 0; i < NUM_LEDS; i++) {
        strip.setPixelColor(i, strip.Color(255, 0, 0));  // 紅色
        strip.show();
        delay(200);
    }
}
```

#### 2. 序列埠通訊測試
```cpp
// 測試程式：回傳接收到的資料
void testSerial() {
    if (Serial.available() > 0) {
        byte inByte = Serial.read();
        Serial.print("Received: 0x");
        Serial.println(inByte, HEX);
    }
}
```

#### 3. 封包接收測試
```cpp
// 手動發送測試封包
// 在序列埠監視器（Hex 模式）發送：
// FF 43 04 1E 00 FF 00 1D FE
// 應顯示綠色 LED
```

### 除錯技巧

#### 1. 序列埠監視器除錯
```cpp
void debugPacket() {
    Serial.println("=== Packet Debug ===");
    Serial.print("SOF: 0x"); Serial.println(packetBuffer[0], HEX);
    Serial.print("CMD: "); Serial.println((char)packetBuffer[1]);
    Serial.print("LEN: "); Serial.println(packetBuffer[2]);
    Serial.print("Data: ");
    for (int i = 3; i < 3 + dataLength; i++) {
        Serial.print(packetBuffer[i]); Serial.print(" ");
    }
    Serial.println();
}
```

#### 2. LED 狀態指示
```cpp
// 綠色：正常運作
// 黃色：等待資料
// 紅色：錯誤狀態
// 藍色：連線成功
```

#### 3. 常見問題檢查清單

**問題 1：LED 不亮**
```
檢查項目：
? 電源供應是否正常（5V）
? 接線是否正確
? LED_PIN 定義是否正確
? strip.begin() 是否被呼叫
? strip.show() 是否被呼叫
```

**問題 2：收不到資料**
```
檢查項目：
? Baud Rate 是否一致（9600）
? 藍芽模組是否配對成功
? RX/TX 接線是否正確
? Serial.begin() 是否被呼叫
```

**問題 3：顏色不正確**
```
檢查項目：
? Checksum 驗證是否通過
? RGB 順序是否正確（NEO_GRB）
? 亮度設定是否合適
? 電源供應是否足夠
```

### 測試封包產生器

```cpp
// 用於 Arduino 自我測試的封包產生器
void generateTestPacket(byte percent) {
    byte r, g, b;
    
    // 模擬 PC 端顏色判斷邏輯
    if (percent <= 50) {
        r = 0; g = 255; b = 0;  // 綠色
    } else if (percent <= 84) {
        r = 255; g = 255; b = 0;  // 黃色
    } else {
        r = 255; g = 0; b = 0;  // 紅色
    }
    
    // 顯示測試結果
    setAllLEDs(r, g, b);
    
    Serial.print("Test: CPU ");
    Serial.print(percent);
    Serial.print("% -> RGB(");
    Serial.print(r); Serial.print(",");
    Serial.print(g); Serial.print(",");
    Serial.print(b); Serial.println(")");
}

// 自動測試迴圈
void autoTest() {
    for (int i = 0; i <= 100; i += 10) {
        generateTestPacket(i);
        delay(1000);
    }
}
```

---

## 常見問題排除

### Q1: WS2812 顯示顏色錯亂

**原因：**
- RGB 順序設定錯誤
- 電源不穩定
- 資料線太長或受干擾

**解決方案：**
```cpp
// 嘗試不同的顏色順序
Adafruit_NeoPixel strip(NUM_LEDS, LED_PIN, NEO_GRB + NEO_KHZ800);  // GRB
Adafruit_NeoPixel strip(NUM_LEDS, LED_PIN, NEO_RGB + NEO_KHZ800);  // RGB

// 加裝濾波電容
// 在 WS2812 電源端加裝 1000μF 電容

// 加裝訊號保護電阻
// 在 Data 腳位串聯 330Ω 電阻
```

### Q2: 序列埠接收不到資料

**檢查步驟：**
```cpp
void setup() {
    Serial.begin(9600);
    
    // 等待序列埠就緒
    while (!Serial) {
        ; // 等待序列埠連接
    }
    
    Serial.println("Serial Ready!");
}
```

### Q3: Checksum 驗證失敗

**除錯程式：**
```cpp
void debugChecksum() {
    byte sum = 0;
    Serial.print("Data: ");
    for (int i = 3; i < 3 + dataLength; i++) {
        Serial.print(packetBuffer[i]); Serial.print(" ");
        sum += packetBuffer[i];
    }
    Serial.println();
    
    sum &= 0xFF;
    byte receivedChk = packetBuffer[3 + dataLength];
    
    Serial.print("Calculated CHK: 0x");
    Serial.println(sum, HEX);
    Serial.print("Received CHK: 0x");
    Serial.println(receivedChk, HEX);
    
    if (sum == receivedChk) {
        Serial.println("? Checksum OK");
    } else {
        Serial.println("? Checksum Error!");
    }
}
```

### Q4: LED 閃爍或不穩定

**原因與解決：**
```cpp
// 1. 降低亮度
strip.setBrightness(50);  // 0-255，建議 30-80

// 2. 加入延遲避免更新過快
void setAllLEDs(byte r, byte g, byte b) {
    static unsigned long lastUpdate = 0;
    if (millis() - lastUpdate > 50) {  // 最少間隔 50ms
        for (int i = 0; i < NUM_LEDS; i++) {
            strip.setPixelColor(i, strip.Color(r, g, b));
        }
        strip.show();
        lastUpdate = millis();
    }
}

// 3. 檢查電源供應
// 每顆 LED 最大消耗 60mA
// 8 顆 LED 最大需求：8 × 60mA = 480mA
// 建議使用外部 5V 2A 電源供應器
```

### Q5: 藍芽連線不穩定

**設定檢查：**
```cpp
// HC-05 設定指令（AT 模式）
AT+NAME=CPU_Monitor    // 設定名稱
AT+PSWD=1234           // 設定密碼
AT+UART=9600,0,0       // 設定 Baud Rate
AT+ROLE=0              // 設定為從機模式
```

---

## 進階功能擴充

### 1. 支援亮度調整

```cpp
// 根據 CPU 使用率調整亮度
void setLEDsWithBrightness(byte percent, byte r, byte g, byte b) {
    // 計算亮度（0-100% 對應 10-255）
    byte brightness = map(percent, 0, 100, 10, 255);
    strip.setBrightness(brightness);
    
    for (int i = 0; i < NUM_LEDS; i++) {
        strip.setPixelColor(i, strip.Color(r, g, b));
    }
    strip.show();
}
```

### 2. 漸變效果

```cpp
// 平滑色彩過渡
void smoothTransition(byte targetR, byte targetG, byte targetB) {
    static byte currentR = 0, currentG = 0, currentB = 0;
    
    // 逐步接近目標顏色
    if (currentR < targetR) currentR++;
    else if (currentR > targetR) currentR--;
    
    if (currentG < targetG) currentG++;
    else if (currentG > targetG) currentG--;
    
    if (currentB < targetB) currentB++;
    else if (currentB > targetB) currentB--;
    
    setAllLEDs(currentR, currentG, currentB);
}
```

### 3. LED 圖案顯示

```cpp
// 根據 CPU 百分比顯示 LED 數量
void showPercentageBars(byte percent) {
    int numLit = map(percent, 0, 100, 0, NUM_LEDS);
    byte r, g, b;
    
    // 判斷顏色
    if (percent <= 50) {
        r = 0; g = 255; b = 0;
    } else if (percent <= 84) {
        r = 255; g = 255; b = 0;
    } else {
        r = 255; g = 0; b = 0;
    }
    
    // 顯示對應數量的 LED
    for (int i = 0; i < NUM_LEDS; i++) {
        if (i < numLit) {
            strip.setPixelColor(i, strip.Color(r, g, b));
        } else {
            strip.setPixelColor(i, 0);  // 關閉
        }
    }
    strip.show();
}
```

### 4. EEPROM 資料儲存

```cpp
#include <EEPROM.h>

// 儲存設定值
void saveSettings(byte brightness, byte mode) {
    EEPROM.write(0, brightness);
    EEPROM.write(1, mode);
}

// 讀取設定值
void loadSettings() {
    byte brightness = EEPROM.read(0);
    byte mode = EEPROM.read(1);
    
    if (brightness == 255) brightness = 50;  // 預設值
    strip.setBrightness(brightness);
}
```

---

## 附錄

### A. 函式庫安裝

#### Adafruit NeoPixel 函式庫
```
Arduino IDE 步驟：
1. 開啟 Arduino IDE
2. 工具 → 管理程式庫
3. 搜尋 "Adafruit NeoPixel"
4. 點擊安裝

或手動下載：
https://github.com/adafruit/Adafruit_NeoPixel
```

### B. 效能優化建議

```cpp
// 1. 使用常數代替變數
const byte NUM_LEDS = 8;  // 編譯時期常數

// 2. 避免重複計算
byte checksum = calculateChecksum();  // 只計算一次

// 3. 使用位元運算
checksum = sum & 0xFF;  // 比 checksum = sum % 256 快

// 4. 減少序列埠輸出
#ifdef DEBUG
    Serial.println(debugInfo);
#endif
```

### C. 記憶體優化

```cpp
// 使用 PROGMEM 儲存常數資料
const char startMsg[] PROGMEM = "Arduino Ready!";

// 使用 F() 巨集減少 RAM 使用
Serial.println(F("Arduino Ready!"));

// 釋放未使用的緩衝區
Serial.flush();
```

### D. 參考資源

**官方文件：**
- [Adafruit NeoPixel Guide](https://learn.adafruit.com/adafruit-neopixel-uberguide)
- [Arduino Serial Reference](https://www.arduino.cc/reference/en/language/functions/communication/serial/)

**相關教學：**
- WS2812B 規格書
- HC-05 藍芽模組設定指南
- Arduino 序列埠通訊教學

---

## 版本資訊

- **文件版本**: 1.0.0
- **最後更新**: 2025-01-XX
- **適用對象**: Arduino 初學者至進階使用者
- **測試平台**: Arduino Uno R3, WS2812B
- **競賽資訊**: 113 學年度工業類科學生技藝競賽

---

**?? 注意事項：**
- 本文件僅供教育與學習使用
- 實際競賽時請遵守相關規則
- 建議在實作前先進行完整測試

---

**? 祝您開發順利！?**
