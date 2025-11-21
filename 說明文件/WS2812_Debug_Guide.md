# WS2812 沒有燈號顯示 - 完整除錯指南

## 問題分析

如果 PC 端啟動 CPU 監控後，WS2812 沒有燈號對應顯示，可能的原因包括：

### 1. Arduino 端問題
- 沒有燒錄程式
- 序列埠參數不一致
- 封包解析錯誤
- WS2812 函式庫未安裝

### 2. 硬體連接問題
- WS2812 接線錯誤
- 電源供應不足
- LED 腳位定義錯誤

### 3. 通訊問題
- 藍芽未配對
- COM Port 選擇錯誤
- 封包格式不正確

---

## ?? 系統性除錯步驟

### 步驟 1：驗證 PC 端是否正確傳送資料

#### 1.1 檢查 Visual Studio 輸出視窗

執行 PC 端程式後，檢查「輸出」視窗：

```
預期看到的訊息：

[14:30:25.123] COM Port 連線成功
  - Port: COM5
  - Baud Rate: 9600
  - 連線確認字元 'c' 已發送

[14:30:26.456] 發送 CPU 封包：
  - 使用率: 35%
  - RGB: (0, 255, 0)
  - Checksum: 0x23
  - 封包內容: 0xFF 0x43 0x04 0x23 0x00 0xFF 0x00 0x23 0xFE
```

**如果沒有看到這些訊息：**
- 確認已連線藍芽（狀態顯示 "Connected" 綠色）
- 確認已啟動 CPU 監控（點擊 Start CPU）
- 檢查 `SerialPort1.IsOpen` 是否為 `True`

#### 1.2 使用序列埠監視工具

下載並使用「Serial Port Monitor」或「HTerm」等工具：

1. 設定與 Arduino 相同的 COM Port
2. Baud Rate: 9600
3. 觀察是否有資料傳送

**預期看到的 Hex 資料：**
```
FF 43 04 23 00 FF 00 23 FE  (綠色)
FF 43 04 41 FF FF 00 3F FE  (黃色)
FF 43 04 5A FF 00 00 59 FE  (紅色)
```

---

### 步驟 2：驗證 Arduino 端是否正常運作

#### 2.1 上傳測試程式

**使用簡化測試版：`Arduino_WS2812_Simple_Test.ino`**

1. 開啟 Arduino IDE
2. 載入 `Arduino_WS2812_Simple_Test.ino`
3. 選擇正確的開發板和 COM Port
4. 上傳程式

#### 2.2 觀察啟動測試

上傳成功後，Arduino 會自動執行 LED 測試：
- ? 所有 LED 顯示**紅色** 0.5 秒
- ? 所有 LED 顯示**綠色** 0.5 秒
- ? 所有 LED 顯示**藍色** 0.5 秒
- ? LED 熄滅

**如果 LED 測試失敗：**

| 問題 | 可能原因 | 解決方案 |
|------|---------|---------|
| LED 完全不亮 | 接線錯誤 | 檢查接線（DIN, VCC, GND） |
| LED 顏色錯誤 | RGB 順序錯誤 | 改用 `NEO_RGB` 或 `NEO_BRG` |
| LED 閃爍不穩 | 電源不足 | 使用外部 5V 電源 |
| 部分 LED 不亮 | LED 損壞 | 更換 LED 燈條 |

#### 2.3 開啟序列埠監視器

Arduino IDE → 工具 → 序列埠監視器
- Baud Rate 設定為 **9600**

**預期看到的訊息：**
```
Arduino Ready!
Testing LEDs...
LED Test Complete!
```

---

### 步驟 3：測試 PC 與 Arduino 通訊

#### 3.1 建立連線

1. **PC 端**：選擇藍芽 COM Port → 點擊 Open
2. **Arduino 端**：序列埠監視器應顯示（如果使用除錯版）：
```
? PC 連線成功！
```

#### 3.2 啟動監控並觀察

1. **PC 端**：點擊 Start CPU
2. **Arduino 端**：序列埠監視器應顯示：

```
--- 收到封包 ---
類型: CPU
使用率: 35 %
RGB: (0, 255, 0)
Checksum: 計算值=0x23, 接收值=0x23
封包總數: 1
? 封包驗證成功
顏色: 綠色 (0-50% 正常負載)
```

3. **WS2812**：應同步顯示對應顏色

---

### 步驟 4：常見問題排除

#### 問題 A：Arduino 收不到資料

**現象：**
- PC 端輸出視窗顯示「發送封包」
- Arduino 序列埠監視器沒有任何訊息

**可能原因與解決方案：**

1. **藍芽未配對**
   ```
   解決：
   - 進入 Windows 藍芽設定
   - 配對 HC-05 藍芽模組
   - 記下對應的 COM Port (例如：COM5)
   ```

2. **選擇錯誤的 COM Port**
   ```
   解決：
   - 在 PC 程式中重新整理 COM Port
   - 選擇正確的藍芽 COM Port
   - 重新連線
   ```

3. **Baud Rate 不一致**
   ```
   解決：
   - 確認 Arduino 程式：Serial.begin(9600);
   - 確認 PC 程式：BAUD_RATE = 9600
   - 兩者必須一致
   ```

#### 問題 B：Arduino 收到資料但 LED 不亮

**現象：**
- Arduino 序列埠監視器顯示「收到封包」
- WS2812 沒有任何反應

**可能原因與解決方案：**

1. **LED_PIN 定義錯誤**
   ```cpp
   // 確認接線和定義一致
   #define LED_PIN 6  // 必須與實際接線相符
   ```

2. **WS2812 型號錯誤**
   ```cpp
   // 嘗試不同的顏色順序
   // 原本：
   Adafruit_NeoPixel strip(NUM_LEDS, LED_PIN, NEO_GRB + NEO_KHZ800);
   
   // 改為：
   Adafruit_NeoPixel strip(NUM_LEDS, LED_PIN, NEO_RGB + NEO_KHZ800);
   // 或：
   Adafruit_NeoPixel strip(NUM_LEDS, LED_PIN, NEO_BRG + NEO_KHZ800);
   ```

3. **電源供應不足**
   ```
   解決：
   - 使用外部 5V 2A 電源供應器
   - 確保 GND 共地
   - 在電源端加裝 1000μF 電容
   ```

4. **strip.show() 未呼叫**
   ```cpp
   // 必須呼叫 strip.show() 才會顯示
   strip.setPixelColor(i, strip.Color(r, g, b));
   strip.show();  // 這行很重要！
   ```

#### 問題 C：封包驗證失敗

**現象：**
- Arduino 顯示「Checksum Error!」或「封包驗證失敗」

**解決方案：**

1. **檢查 PC 端 Checksum 計算**
   ```visualbasic
   ' 確認計算方式正確
   Dim checksum As Byte = CByte((percent + color.R + color.G + color.B) And &HFF)
   ```

2. **檢查 Arduino 端 Checksum 驗證**
   ```cpp
   // 確認計算方式一致
   byte calculatedChk = (percent + r + g + b) & 0xFF;
   ```

3. **檢查資料讀取順序**
   ```cpp
   // 必須按照正確順序讀取
   byte percent = Serial.read();  // 第 1 個資料
   byte r = Serial.read();        // 第 2 個資料
   byte g = Serial.read();        // 第 3 個資料
   byte b = Serial.read();        // 第 4 個資料
   byte chk = Serial.read();      // 檢查碼
   byte eof = Serial.read();      // 結束標記
   ```

---

## ??? 快速診斷工具

### 診斷程式（PC 端）

在 PC 端加入測試封包發送功能：

```visualbasic
' 在 Form1.vb 中加入測試方法
Private Sub SendTestPacket()
    Try
        ' 測試封包：CPU 50%, 綠色 (0, 255, 0)
        Dim testPacket As Byte() = {
            &HFF,  ' SOF
            Asc("C"c),  ' CMD
            4,     ' LEN
            50,    ' Percent
            0,     ' R
            255,   ' G
            0,     ' B
            49,    ' CHK (50+0+255+0=305, 305&0xFF=49)
            &HFE   ' EOF
        }
        
        If SerialPort1.IsOpen Then
            SerialPort1.Write(testPacket, 0, testPacket.Length)
            Debug.WriteLine("測試封包已發送")
        End If
    Catch ex As Exception
        Debug.WriteLine($"測試封包發送失敗：{ex.Message}")
    End Try
End Sub
```

### 診斷程式（Arduino 端）

```cpp
// 在 loop() 中加入手動測試
void loop() {
    // ... 原有程式碼 ...
    
    // 按下 Arduino 板上的按鈕時測試
    // 或每 10 秒自動測試
    static unsigned long lastTest = 0;
    if (millis() - lastTest > 10000) {
        testManualColor();
        lastTest = millis();
    }
}

void testManualColor() {
    Serial.println("手動測試：顯示綠色");
    for (int i = 0; i < NUM_LEDS; i++) {
        strip.setPixelColor(i, strip.Color(0, 255, 0));
    }
    strip.show();
}
```

---

## ?? 完整檢查清單

### PC 端檢查

- [ ] ? 程式已啟動
- [ ] ? 藍芽已配對並顯示 COM Port
- [ ] ? 已選擇正確的 COM Port
- [ ] ? 點擊 Open 按鈕，狀態顯示 "Connected"（綠色）
- [ ] ? Start CPU 按鈕已啟用
- [ ] ? 點擊 Start CPU，開始監控
- [ ] ? 輸出視窗顯示「發送 CPU 封包」訊息
- [ ] ? 封包內容格式正確（以 0xFF 開頭，0xFE 結尾）

### Arduino 端檢查

- [ ] ? 已安裝 Adafruit_NeoPixel 函式庫
- [ ] ? 程式已成功上傳
- [ ] ? 序列埠監視器 Baud Rate 設定為 9600
- [ ] ? 啟動測試（紅綠藍）正常顯示
- [ ] ? 序列埠監視器顯示「Arduino Ready!」
- [ ] ? 收到 PC 連線確認訊息
- [ ] ? 收到 CPU 封包並顯示詳細資訊
- [ ] ? 封包驗證成功（Checksum 正確）

### 硬體檢查

- [ ] ? WS2812 DIN 接到 Arduino Pin 6
- [ ] ? WS2812 VCC 接到 5V（或外部電源）
- [ ] ? WS2812 GND 接到 GND（共地）
- [ ] ? 電源供應充足（建議 5V 2A）
- [ ] ? 接線穩固，沒有鬆脫
- [ ] ? LED 燈條沒有損壞

---

## ?? 推薦除錯流程

### 第一階段：硬體驗證

1. 上傳 `Arduino_WS2812_Simple_Test.ino`
2. 觀察 LED 啟動測試
3. 如果 LED 測試失敗 → 檢查硬體接線

### 第二階段：序列埠驗證

1. 上傳 `Arduino_WS2812_Debug_Test.ino`
2. 開啟序列埠監視器
3. 連線 PC 程式
4. 觀察是否收到連線確認訊息

### 第三階段：封包驗證

1. 啟動 CPU 監控
2. 觀察序列埠監視器的封包資訊
3. 檢查 Checksum 是否正確
4. 確認 LED 顯示對應顏色

---

## ?? 技術支援資訊

### 常見錯誤訊息

| 錯誤訊息 | 原因 | 解決方案 |
|---------|------|---------|
| "Checksum Error!" | 封包資料損壞 | 檢查序列埠連線品質 |
| "EOF Error" | 封包不完整 | 降低傳送頻率或增加緩衝 |
| "Unknown Command" | 命令碼錯誤 | 確認 PC 端命令碼正確 |
| "超過 5 秒未收到資料" | 連線中斷 | 重新連線或檢查藍芽 |

### 除錯工具推薦

1. **Serial Port Monitor** - 監視序列埠通訊
2. **HTerm** - Hex 格式顯示序列埠資料
3. **Arduino IDE 序列埠監視器** - 即時觀察 Arduino 輸出

---

## ?? 成功案例參考

### 正常運作時的完整流程

```
1. [PC] 啟動程式
2. [PC] 選擇 COM5 (HC-05)
3. [PC] 點擊 Open
4. [PC] 輸出：「COM Port 連線成功」
5. [PC] 輸出：「連線確認字元 'c' 已發送」

6. [Arduino] 接收：'c'
7. [Arduino] 顯示：「? PC 連線成功！」
8. [Arduino] LED 綠色閃爍 3 次

9. [PC] 點擊 Start CPU
10. [PC] 每秒發送 CPU 封包

11. [Arduino] 每秒收到封包
12. [Arduino] 顯示：「收到封包 - CPU: 35% RGB(0,255,0)」
13. [Arduino] WS2812 顯示綠色

14. [PC] CPU 使用率上升到 65%
15. [PC] 發送：RGB(255,255,0) 黃色

16. [Arduino] 收到封包
17. [Arduino] WS2812 顯示黃色
```

---

## ?? 最佳化建議

### 1. 降低傳送頻率（如果資料遺失）

```visualbasic
' 將 Timer 間隔從 1000ms 改為 2000ms
TimerCPU.Interval = 2000
```

### 2. 增加亮度（如果 LED 太暗）

```cpp
strip.setBrightness(100);  // 0-255，預設 50
```

### 3. 加入資料緩衝（避免封包遺失）

```cpp
// Arduino 端加入延遲
delay(10);  // 在 strip.show() 之後
```

---

**最後更新：2025-01-11**  
**版本：1.0**

如有其他問題，請參考：
- Arduino_WS2812_Integration_Guide.md
- README.md
