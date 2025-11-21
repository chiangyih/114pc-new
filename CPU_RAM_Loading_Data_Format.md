# CPU/RAM Loading 資料格式說明

## ?? 資料格式

### 文字格式（推薦使用）

**格式：** `"LOAD" + 數值 + "\n"`

CPU 與 RAM 監控採用相同的文字格式傳輸，前置 "LOAD" 字串 + 使用率百分比 + 換行符號（`\n`），易於 Arduino 端使用 `Serial.readStringUntil('\n')` 解析。

**說明：** 
- 每次傳輸包含一個資料集
- 結尾必須使用換行符號 `\n` (ASCII 10, 0x0A)
- Arduino 應使用 `Serial.readStringUntil('\n')` 讀取資料

---

## ?? 資料範例

### CPU Loading 範例

| CPU 使用率 | 傳送字串 | Hex 數值 | 說明 |
|-----------|---------|----------|------|
| 0% | `LOAD0\n` | 4C 4F 41 44 30 0A | CPU 閒置 |
| 30% | `LOAD30\n` | 4C 4F 41 44 33 30 0A | CPU 中度使用 |
| 65% | `LOAD65\n` | 4C 4F 41 44 36 35 0A | CPU 較高使用率 |
| 90% | `LOAD90\n` | 4C 4F 41 44 39 30 0A | CPU 高度使用 |
| 100% | `LOAD100\n` | 4C 4F 41 44 31 30 30 0A | CPU 滿載 |

### RAM Loading 範例

| RAM 使用率 | 傳送字串 | Hex 數值 | 說明 |
|-----------|---------|----------|------|
| 0% | `LOAD0\n` | 4C 4F 41 44 30 0A | RAM 可用 |
| 45% | `LOAD45\n` | 4C 4F 41 44 34 35 0A | RAM 中度使用 |
| 70% | `LOAD70\n` | 4C 4F 41 44 37 30 0A | RAM 較高使用率 |
| 88% | `LOAD88\n` | 4C 4F 41 44 38 38 0A | RAM 高度使用 |
| 100% | `LOAD100\n` | 4C 4F 41 44 31 30 30 0A | RAM 滿載 |

---

## ?? 顏色對應表

| 使用率範圍 | 顏色 | 說明 |
|-----------|------|------|
| 0% - 50% | ?? 綠色 (Green) | 正常負載 |
| 51% - 84% | ?? 黃色 (Yellow) | 中度負載 |
| 85% - 100% | ?? 紅色 (Red) | 高度負載 |

**注意：** 顏色對應在 PC 端顯示，Arduino 只接收數值

---

## ?? 資料傳輸週期

- **CPU Loading**: 每 1 秒傳輸一次
- **RAM Loading**: 每 1 秒傳輸一次
- **預設傳輸間隔**: 1000 毫秒

---

## ?? Arduino 接收程式範例

### 範例 1: 使用 readStringUntil() - **推薦**

```cpp
void loop() {
    if (Serial.available() > 0) {
        // 讀取直到換行符號 '\n'
        String data = Serial.readStringUntil('\n');
        
        // 檢查是否以 "LOAD" 開頭
        if (data.startsWith("LOAD")) {
            // 提取 "LOAD" 後面的數值
            String valueStr = data.substring(4);
            int loadValue = valueStr.toInt();
            
            // 顯示接收值
            Serial.print("當前 Loading 值: ");
            Serial.println(loadValue);
            
            // 根據數值控制 LED 或顯示
            if (loadValue <= 50) {
                // 綠色 LED 亮起
                digitalWrite(LED_GREEN, HIGH);
                digitalWrite(LED_YELLOW, LOW);
                digitalWrite(LED_RED, LOW);
            } else if (loadValue <= 84) {
                // 黃色 LED 亮起
                digitalWrite(LED_GREEN, LOW);
                digitalWrite(LED_YELLOW, HIGH);
                digitalWrite(LED_RED, LOW);
            } else {
                // 紅色 LED 亮起
                digitalWrite(LED_GREEN, LOW);
                digitalWrite(LED_YELLOW, LOW);
                digitalWrite(LED_RED, HIGH);
            }
        }
    }
}
```

### 範例 2: 使用 parseInt() 搭配 find()

```cpp
void loop() {
    if (Serial.available() > 0) {
        // 尋找 "LOAD" 字串
        if (Serial.find("LOAD")) {
            // 讀取隨後的數值（結尾在 '\n'）
            int loadValue = Serial.parseInt();
            
            // 執行處理邏輯
            processLoading(loadValue);
        }
    }
}

void processLoading(int value) {
    Serial.print("Loading: ");
    Serial.print(value);
    Serial.println("%");
    
    // 根據數值控制顯示
    displayLoading(value);
}
```

### 範例 3: 緩衝區累積與驗證

```cpp
String buffer = "";
bool receiving = false;

void loop() {
    while (Serial.available() > 0) {
        char c = Serial.read();
        
        // 檢測換行符號（作為結束標記）
        if (c == '\n') {
            if (buffer.startsWith("LOAD") && buffer.length() > 4) {
                // 提取數值
                String valueStr = buffer.substring(4);
                int loadValue = valueStr.toInt();
                
                // 驗證數值範圍（0-100）
                if (loadValue >= 0 && loadValue <= 100) {
                    processLoading(loadValue);
                } else {
                    Serial.println("錯誤：數值超出範圍");
                }
            }
            // 清空緩衝區
            buffer = "";
        } else {
            // 累積字元
            buffer += c;
            
            // 防止緩衝區過大
            if (buffer.length() > 20) {
                buffer = "";
                Serial.println("錯誤：緩衝區溢滿");
            }
        }
    }
}

void processLoading(int value) {
    Serial.print("Loading: ");
    Serial.print(value);
    Serial.println("%");
    
    // 更新顯示
    displayLoading(value);
}
```

### 範例 4: 狀態機解析（進階用法）

```cpp
enum ParseState {
    WAIT_L,
    WAIT_O,
    WAIT_A,
    WAIT_D,
    READ_VALUE
};

ParseState state = WAIT_L;
String valueBuffer = "";

void loop() {
    while (Serial.available() > 0) {
        char c = Serial.read();
        
        switch (state) {
            case WAIT_L:
                if (c == 'L') {
                    state = WAIT_O;
                }
                break;
                
            case WAIT_O:
                if (c == 'O') {
                    state = WAIT_A;
                } else {
                    state = WAIT_L;
                }
                break;
                
            case WAIT_A:
                if (c == 'A') {
                    state = WAIT_D;
                } else {
                    state = WAIT_L;
                }
                break;
                
            case WAIT_D:
                if (c == 'D') {
                    state = READ_VALUE;
                    valueBuffer = "";
                } else {
                    state = WAIT_L;
                }
                break;
                
            case READ_VALUE:
                if (c == '\n') {
                    // 完整解析數值
                    int loadValue = valueBuffer.toInt();
                    if (loadValue >= 0 && loadValue <= 100) {
                        processLoading(loadValue);
                    }
                    state = WAIT_L;
                } else if (isDigit(c)) {
                    valueBuffer += c;
                } else {
                    // 非數字字元，復位狀態機
                    state = WAIT_L;
                }
                break;
        }
    }
}

void processLoading(int value) {
    Serial.print("Loading: ");
    Serial.print(value);
    Serial.println("%");
}
```

---

## ??? PC 端發送程式碼

### CPU 計時器 (TimerCPU_Tick)

```visualbasic
Private Sub TimerCPU_Tick(sender As Object, e As EventArgs) Handles TimerCPU.Tick
    Try
        ' 讀取 CPU 使用率
        Dim cpuUsage As Single = cpuCounter.NextValue()
        Dim cpuPercent As Integer = CInt(Math.Round(cpuUsage))
        
        ' 更新 UI
        LabelCPUValue.Text = $"{cpuPercent} %"
        PanelCPUColor.BackColor = GetLoadingColor(cpuPercent)
        
        ' 傳送格式: "LOAD" + 數值 + 換行符號
        ' 使用 vbLf (Chr(10)) 作為換行符號
        If SerialPort1.IsOpen Then
            SerialPort1.Write($"LOAD{cpuPercent}" & vbLf)
        End If
    Catch ex As Exception
        MessageBox.Show($"CPU 計時器錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Try
End Sub
```

### RAM 計時器 (TimerRAM_Tick)

```visualbasic
Private Sub TimerRAM_Tick(sender As Object, e As EventArgs) Handles TimerRAM.Tick
    Try
        ' 讀取 RAM 使用率
        Dim ramUsage As Single = GetRAMUsage()
        Dim ramPercent As Integer = CInt(Math.Round(ramUsage))
        
        ' 更新 UI
        LabelRAMValue.Text = $"{ramPercent} %"
        PanelRAMColor.BackColor = GetLoadingColor(ramPercent)
        
        ' 傳送格式: "LOAD" + 數值 + 換行符號
        If SerialPort1.IsOpen Then
            SerialPort1.Write($"LOAD{ramPercent}" & vbLf)
        End If
    Catch ex As Exception
        MessageBox.Show($"RAM 計時器錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Try
End Sub
```

---

## ?? EEPROM 寫入格式

### 新文字格式（推薦使用）

```
"WRITE" + 數值 + "\n"

範例:
WRITE0\n   (寫入十進位 0)
WRITE5\n   (寫入十進位 5)
WRITE12\n  (寫入十進位 12)
WRITE15\n  (寫入十進位 15)
```

**優點**:
- 與 CPU/RAM 監控格式一致
- Arduino 可使用相同的解析邏輯
- 提高代碼重用率

### Arduino EEPROM 接收範例

```cpp
void loop() {
    if (Serial.available() > 0) {
        String data = Serial.readStringUntil('\n');
        
        // 檢查是否為 WRITE 命令
        if (data.startsWith("WRITE")) {
            String valueStr = data.substring(5);
            int eepromValue = valueStr.toInt();
            
            // 驗證範圍（0-15）
            if (eepromValue >= 0 && eepromValue <= 15) {
                // 寫入 EEPROM
                EEPROM.write(0, eepromValue);
                Serial.print("EEPROM 寫入: ");
                Serial.println(eepromValue);
            } else {
                Serial.println("錯誤：值超出範圍 (0-15)");
            }
        }
    }
}
```

---

## ?? 常見注意事項

1. **換行符號** - 使用 `\n` (LF, 0x0A)，不是 `\r\n` (CRLF)
2. **VB.NET 方法** - 使用 `vbLf` 或 `Chr(10)` 表示換行符號
3. **Arduino 讀取** - 使用 `Serial.readStringUntil('\n')` 是最可靠的方法
4. **新資料格式** - CPU 與 RAM 每秒傳輸一次，都以 `LOAD` 開頭
5. **Baud Rate** - 確保 PC 端與 Arduino 端設置相同（預設 9600）
6. **EEPROM 新格式** - 現已改用文字格式（新增 "WRITE" 字首）

---

## ?? 除錯技巧

### 在 Arduino 串列埠監視器查看

1. 開啟 Arduino IDE 串列埠監視器
2. 設置 Baud Rate 為 9600
3. 設置「換行符號」選項為「僅換行」
4. 連接 PC 端程式並執行
5. 點擊 "Start CPU" 和 "Start RAM"
6. 應該看到類似的連續輸出：
   ```
   LOAD25
   LOAD26
   LOAD24
   LOAD27
   ...
   ```

### 預期看到的資料流

```
LOAD30
LOAD31
LOAD29
LOAD32
LOAD55
LOAD54
LOAD56
...
```

每行是一個資料集，會在約一秒內更新一次

---

## ? 常見問題排除

### 問題 1: Arduino 收不到資料

**可能原因**
- Baud Rate 不一致
- 序列埠未正確開啟
- 未使用換行符號

**解決辦法**
```cpp
void setup() {
    Serial.begin(9600);  // 確保 Baud Rate 為 9600
    while (!Serial);     // 等待序列埠就緒
}
```

### 問題 2: 資料顯示異常

**可能原因**
- 緩衝區過小
- 讀取時機有誤

**解決辦法**
使用 `Serial.readStringUntil('\n')` 並檢驗 `\n`

### 問題 3: 數據在一秒內

**情況**
沒有接收到換行符號

**解決辦法**
確保使用 `Serial.readStringUntil('\n')` 方式讀取資料

---

## ?? 參考資料

- **README.md** - 完整功能說明
- **Form1.vb** - PC 端原始碼
- **Arduino 官方文件** - (待補充)

---

**最後更新：** 2025-01-11  
**版本：** 2.2 (新增 EEPROM 文字格式)  
**開發單位：** 新化高工
