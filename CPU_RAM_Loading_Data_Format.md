# CPU 和 RAM Loading 資料傳送格式說明

## ?? 資料格式

### 新格式（目前使用）

**格式：** `"LOAD" + 數值 + "\n"`


所有 CPU 和 RAM 的監控數據都使用統一的文字格式傳送，由 "LOAD" 前綴 + 使用率數值 + 換行符號組成。

**重要：** 每筆資料結尾都包含換行符號 `\n` (ASCII 10, 0x0A)，方便 Arduino 使用 `Serial.readStringUntil('\n')` 解析。

---

## ?? 傳送範例

### CPU Loading 資料

| CPU 使用率 | 傳送內容 | Hex 表示 | 說明 |
|-----------|---------|----------|------|
| 0% | `LOAD0\n` | 4C 4F 41 44 30 0A | CPU 閒置 |
| 30% | `LOAD30\n` | 4C 4F 41 44 33 30 0A | CPU 低負載（綠色） |
| 65% | `LOAD65\n` | 4C 4F 41 44 36 35 0A | CPU 中度負載（黃色） |
| 90% | `LOAD90\n` | 4C 4F 41 44 39 30 0A | CPU 高負載（紅色） |
| 100% | `LOAD100\n` | 4C 4F 41 44 31 30 30 0A | CPU 滿載 |

### RAM Loading 資料

| RAM 使用率 | 傳送內容 | Hex 表示 | 說明 |
|-----------|---------|----------|------|
| 0% | `LOAD0\n` | 4C 4F 41 44 30 0A | RAM 未使用 |
| 45% | `LOAD45\n` | 4C 4F 41 44 34 35 0A | RAM 低負載（綠色） |
| 70% | `LOAD70\n` | 4C 4F 41 44 37 30 0A | RAM 中度負載（黃色） |
| 88% | `LOAD88\n` | 4C 4F 41 44 38 38 0A | RAM 高負載（紅色） |
| 100% | `LOAD100\n` | 4C 4F 41 44 31 30 30 0A | RAM 滿載 |

---

## ?? 顏色判斷規則

| 使用率範圍 | 顏色 | 狀態 |
|-----------|------|------|
| 0% - 50% | ?? 綠色 (Green) | 正常負載 |
| 51% - 84% | ?? 黃色 (Yellow) | 中度負載 |
| 85% - 100% | ?? 紅色 (Red) | 高負載 |

**注意：** 顏色判斷在 PC 端完成，Arduino 只接收數值。

---

## ?? 傳送頻率

- **CPU Loading**: 每 1 秒傳送一次
- **RAM Loading**: 每 1 秒傳送一次
- **定時器間隔**: 1000 毫秒

---

## ?? Arduino 端接收程式範例

### 方法 1: 使用 readStringUntil() - **推薦**

```cpp
void loop() {
    if (Serial.available() > 0) {
        // 讀取直到遇到換行符號 '\n'
        String data = Serial.readStringUntil('\n');
        
        // 檢查是否以 "LOAD" 開頭
        if (data.startsWith("LOAD")) {
            // 移除 "LOAD" 前綴，取得數值
            String valueStr = data.substring(4);
            int loadValue = valueStr.toInt();
            
            // 處理數值
            Serial.print("收到 Loading 值: ");
            Serial.println(loadValue);
            
            // 根據數值控制 LED 或顯示器
            if (loadValue <= 50) {
                // 綠色 LED 或顯示
                digitalWrite(LED_GREEN, HIGH);
                digitalWrite(LED_YELLOW, LOW);
                digitalWrite(LED_RED, LOW);
            } else if (loadValue <= 84) {
                // 黃色 LED 或顯示
                digitalWrite(LED_GREEN, LOW);
                digitalWrite(LED_YELLOW, HIGH);
                digitalWrite(LED_RED, LOW);
            } else {
                // 紅色 LED 或顯示
                digitalWrite(LED_GREEN, LOW);
                digitalWrite(LED_YELLOW, LOW);
                digitalWrite(LED_RED, HIGH);
            }
        }
    }
}
```

### 方法 2: 使用 parseInt() 配合 find()

```cpp
void loop() {
    if (Serial.available() > 0) {
        // 尋找 "LOAD" 字串
        if (Serial.find("LOAD")) {
            // 讀取接下來的整數（自動停在 '\n'）
            int loadValue = Serial.parseInt();
            
            // 處理數值
            processLoading(loadValue);
        }
    }
}

void processLoading(int value) {
    Serial.print("Loading: ");
    Serial.print(value);
    Serial.println("%");
    
    // 顯示或控制邏輯
    displayLoading(value);
}
```

### 方法 3: 完整的接收與驗證

```cpp
String buffer = "";
bool receiving = false;

void loop() {
    while (Serial.available() > 0) {
        char c = Serial.read();
        
        // 檢查換行符號（資料結束）
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
            
            // 防止緩衝區溢位
            if (buffer.length() > 20) {
                buffer = "";
                Serial.println("錯誤：緩衝區溢位");
            }
        }
    }
}

void processLoading(int value) {
    Serial.print("Loading: ");
    Serial.print(value);
    Serial.println("%");
    
    // 顯示到 LCD 或 LED
    displayLoading(value);
}
```

### 方法 4: 使用狀態機解析（最穩定）

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
                    // 收到完整數值
                    int loadValue = valueBuffer.toInt();
                    if (loadValue >= 0 && loadValue <= 100) {
                        processLoading(loadValue);
                    }
                    state = WAIT_L;
                } else if (isDigit(c)) {
                    valueBuffer += c;
                } else {
                    // 非法字元，重置狀態
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

## ?? PC 端實作說明

### CPU 監控 (TimerCPU_Tick)

```visualbasic
Private Sub TimerCPU_Tick(sender As Object, e As EventArgs) Handles TimerCPU.Tick
    Try
        ' 讀取 CPU 使用率
        Dim cpuUsage As Single = cpuCounter.NextValue()
        Dim cpuPercent As Integer = CInt(Math.Round(cpuUsage))
        
        ' 更新 UI
        LabelCPUValue.Text = $"{cpuPercent} %"
        PanelCPUColor.BackColor = GetLoadingColor(cpuPercent)
        
        ' 傳送資料: "LOAD" + 數值 + 換行符號
        ' 使用 vbLf (Chr(10)) 作為換行符號
        If SerialPort1.IsOpen Then
            SerialPort1.Write($"LOAD{cpuPercent}" & vbLf)
        End If
    Catch ex As Exception
        MessageBox.Show($"CPU 監控錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Try
End Sub
```

### RAM 監控 (TimerRAM_Tick)

```visualbasic
Private Sub TimerRAM_Tick(sender As Object, e As EventArgs) Handles TimerRAM.Tick
    Try
        ' 讀取 RAM 使用率
        Dim ramUsage As Single = GetRAMUsage()
        Dim ramPercent As Integer = CInt(Math.Round(ramUsage))
        
        ' 更新 UI
        LabelRAMValue.Text = $"{ramPercent} %"
        PanelRAMColor.BackColor = GetLoadingColor(ramPercent)
        
        ' 傳送資料: "LOAD" + 數值 + 換行符號
        If SerialPort1.IsOpen Then
            SerialPort1.Write($"LOAD{ramPercent}" & vbLf)
        End If
    Catch ex As Exception
        MessageBox.Show($"RAM 監控錯誤：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Try
End Sub
```

---

## ?? 格式比較

### 舊格式（封包格式 - 已停用）

```
[SOF][CMD][LEN][Data][CHK][EOF]
0xFF  'C'   4   %,R,G,B  SUM  0xFE

範例: CPU 65%, Yellow
0xFF 0x43 0x04 0x41 0xFF 0xFF 0x00 0x3F 0xFE
總共 9 bytes
```

### 新格式（文字格式 + 換行符號 - 目前使用）

```
"LOAD" + 數值 + "\n"

範例: CPU 65%
"LOAD65\n"
總共 7 bytes (7 字元)

Hex: 4C 4F 41 44 36 35 0A
     L  O  A  D  6  5  \n
```

---

## ? 優點

1. **明確結束符號** - 換行符號 `\n` 提供明確的資料邊界
2. **易於解析** - Arduino 可使用 `Serial.readStringUntil('\n')` 一次讀取完整資料
3. **減少錯誤** - 不會發生資料黏連或截斷問題
4. **除錯方便** - 序列埠監視器會自動換行顯示
5. **標準格式** - 符合常見的序列通訊協定慣例
6. **避免緩衝區問題** - 清楚的資料分隔降低解析複雜度

---

## ?? 注意事項

1. **換行符號** - 使用 `\n` (LF, 0x0A)，不是 `\r\n` (CRLF)
2. **VB.NET 寫法** - 使用 `vbLf` 或 `Chr(10)` 表示換行符號
3. **Arduino 讀取** - 使用 `Serial.readStringUntil('\n')` 是最可靠的方法
4. **連續傳送** - CPU 和 RAM 每秒各傳送一次，會看到連續的 `LOAD` 訊息
5. **Baud Rate** - 確保 PC 端和 Arduino 端都設定為 9600
6. **EEPROM 功能** - 仍使用原本的封包格式（不受影響）

---

## ?? 其他功能

### EEPROM 寫入（保持原封包格式）

```
[SOF][CMD][LEN][Data][CHK][EOF]
0xFF  'E'   1    值     值   0xFE

範例: 寫入 10 (二進位 1010)
0xFF 0x45 0x01 0x0A 0x0A 0xFE
```

### 連線確認

```
連線時發送: 'c' (0x63)
```

---

## ?? 測試方法

### 使用 Arduino 序列埠監視器測試

1. 開啟 Arduino IDE 序列埠監視器
2. 設定 Baud Rate 為 9600
3. 確保「行結尾」設定為「換行」或「兩者皆是」
4. 啟動 PC 端程式並連線
5. 點擊 "Start CPU" 或 "Start RAM"
6. 應該會看到清楚分行的輸出：
   ```
   LOAD25
   LOAD26
   LOAD24
   LOAD27
   ...
   ```

### 預期的序列埠輸出

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

每行都是一筆完整的資料，不會黏在一起。

---

## ?? 常見問題排除

### 問題 1: Arduino 收不到資料

**可能原因：**
- Baud Rate 不一致
- 序列埠未正確開啟
- 接線錯誤

**解決方法：**
```cpp
void setup() {
    Serial.begin(9600);  // 確保 Baud Rate 為 9600
    while (!Serial);     // 等待序列埠就緒
}
```

### 問題 2: 收到的資料不完整

**可能原因：**
- 緩衝區太小
- 讀取速度太慢

**解決方法：**
使用 `Serial.readStringUntil('\n')` 確保讀取完整資料。

### 問題 3: 資料黏在一起

**原因：**
沒有正確處理換行符號。

**解決方法：**
確保使用 `Serial.readStringUntil('\n')` 或在狀態機中檢查 `\n`。

---

## ?? 相關文件

- **README.md** - 專案完整說明
- **Form1.vb** - PC 端主程式
- **Arduino 範例程式** - (需另外提供)

---

**最後更新：** 2025-01-11  
**版本：** 2.1 (加入換行符號)  
**狀態：** ? 使用中
