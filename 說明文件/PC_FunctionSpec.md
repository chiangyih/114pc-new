# 💻 PC 端功能需求總表（崗位編號：01）
## 113 學年度 全國高級中等學校 工業類科學生技藝競賽
### 電腦修護職類 第二站 — 個人電腦 USB / 藍牙介面卡製作及控制
### PC 端開發語言：Visual Basic .NET 2022

---

## 1️⃣ 功能需求總覽（依據題目原文 B-5～B-8 條）

| 編號 | 功能名稱 | 功能描述（依競賽題目） | 是否需與 MCU 通訊 | 備註 / 技術要點 |
|:--:|:--|:--|:--:|:--|
| **P1** | **程式標題列顯示** | 視窗標題列須顯示：<br>`113 學年度 工業類科學生技藝競賽 電腦修護職種 台中高工 第二站 崗位號碼：01` | ❌ 否 | 在 Form_Load 時設定固定文字 |
| **P2** | **藍牙模組名稱處理邏輯** | 將崗位號碼 `01` 轉為二進位（00000001），取最右位判斷奇偶：<br>奇數 → 顯示 `ODD-01-0001` | ✅ 是 | 命名邏輯：ODD-01-BBBB |
| **P3** | **COM Port 清單偵測與即時更新** | 顯示系統內所有可用 COM Port（USB / BLE 虛擬埠），可即時更新 | ❌ 否 | 使用 `SerialPort.GetPortNames()` 或 `ManagementEventWatcher` |
| **P4** | **COM Port Open 自動連線** | 開啟序列埠時自動顯示「Connected」，不需額外操作 ，並發送一個c字元給arduino| ✅ 是 | 成功開啟 SerialPort 後自動更新狀態 |
| **P5** | **COM Port Close 處理** | 關閉埠時顯示「Disconnect」，並停用：`Write`、`Now Time`、`CPU Loading` | ✅ 是 | Close 事件需同步更新 UI 狀態 |
| **P6** | **連線狀態顯示** | 顯示目前藍牙連線狀態：Connected / Disconnect | ✅ 是 | 以 Label 或狀態列顯示 |
| **P7** | **CPU Loading 取得與顏色同步** | 取得 PC CPU 使用率 → 顯示數值與顏色：<br>0~50% 綠、51~84% 黃、≥85% 紅<br>並以藍牙封包傳送至 MCU 控制 WS2812 | ✅ 是 | 使用 `PerformanceCounter("Processor", "% Processor Time", "_Total")` |
| **補充**| **RAM Loading 取得**| 取得 PC RAM 使用率|
| **P8** | **資料傳輸：二進位輸入與 EEPROM 寫入** | 輸入四位二進位數值（0/1），若正確 → 轉十進位 → 傳送至 MCU 寫入 EEPROM；<br>若格式錯誤 → 清空欄位並彈出 `Not BIN Format` | ✅ 是 | 寫入前需正規表示式驗證 `^[01]{4}$` |
| **P9** | **Write 按鈕行為** | 按下後立即封裝封包並傳送至 MCU | ✅ 是 | 與 P8 結合實現 |
| **P10** | **Exit 按鈕行為** | 關閉程式、釋放序列埠資源 | ❌ 否 | 使用 `Application.Exit()` |
| **P11** | **畫面資訊同步顯示** | 即時顯示 CPU Loading 數值、顏色、COM Port 狀態、RAM Loading 數值等 | ❌ 否 | 更新頻率建議每秒 1 次 |

---

## 2️⃣ 操作行為邏輯

| 行為情境 | 系統應執行動作 |
|-----------|----------------|
| 程式啟動 | 顯示標題列與崗位號碼 `01`，列出所有 COM Port，狀態為 Disconnect |
| 選擇 Port 並開啟 | 自動顯示 Connected，啟用 Write / Start 按鈕 |
| 關閉 Port | 顯示 Disconnect，停用所有操作按鈕 |
| 按下 StartCPU | 啟動 CPU Loading 監測計時器（每秒更新一次） |
| 按下 StartRAM | 啟動 RAM Loading 監測計時器（每秒更新一次） |
| 傳送 Loading 至 MCU | 封裝格式 `[SOF][CMD][LEN][Data][CHK][EOF]`，內容包含 Loading% 與 RGB 顏色 |
| 寫入 EEPROM | 驗證輸入後轉十進位，傳送封包給 MCU 寫入 EEPROM |
| 結束程式 | 關閉所有連線與埠，釋放資源 |

---

## 3️⃣ 模組分層架構建議

| 模組名稱 | 功能 |
|-----------|------|
| `SerialPortManager` | 控制藍牙 / USB 序列埠開關與傳輸 |
| `ComPortWatcher` | 偵測 COM Port 熱插拔與更新清單 |
| `CpuLoadProvider` | 取得 CPU Loading 值並傳送至 MCU |
| `EepromService` | 驗證輸入、轉換十進位與資料封包 |
| `UiController` | 控制畫面更新、狀態切換、按鈕管理 |
| `TitleService` | 設定標題列文字與崗位號碼 |
| `BleProtocol` | 負責封包打包與 Checksum 驗證 |

---

## 4️⃣ 額外系統需求

| 項目 | 說明 |
|------|------|
| 更新頻率 | CPU Loading 每 1 秒更新一次 |
| 更新頻率 | RAM Loading 每 1 秒更新一次 |
| 封包完整性 | 建議使用簡易校驗 `(SUM of data) & 0xFF` |
| 錯誤處理 | 連線中斷 → 自動顯示 Disconnect；輸入錯誤 → 彈出警告 |
| 顏色對應表 | 0~50% 綠 / 51~84% 黃 / ≥85% 紅 |
| 預設 Baud Rate | 9600 bps（HC-05 標準速率） |

---

## 5️⃣ 整體通訊對應關係

| PC 端動作 | 傳送資料 | MCU 響應 | 備註 |
|------------|-----------|-----------|------|
| 開啟連線 | `CMD_OPEN` | 回傳 `ACK` | BLE 連線成功 |
| 傳送 CPU Loading | `CMD_LOAD <VAL>` | 顯示顏色於 WS2812 | 實時更新 |
| 寫入 EEPROM | `CMD_EEPROM <DEC>` | EEPROM 寫入成功 | 需回 ACK |
| 關閉連線 | `CMD_CLOSE` | 無 | 結束通訊 |
| 程式結束 | 自動關閉 SerialPort | 無 | 釋放資源 |

---

### EEPROM 寫入封包格式

**新格式（推薦）**: `"WRITE" + 數值 + "\n"`

```
範例:
WRITE0\n   (寫入十進位 0)
WRITE5\n   (寫入十進位 5)
WRITE12\n  (寫入十進位 12)
WRITE15\n  (寫入十進位 15)
```

**特點**:
- 與 CPU/RAM 監控格式一致（都是文字 + 換行符號）
- Arduino 可使用相同的 `Serial.readStringUntil('\n')` 解析
- 簡化通訊協定，提高可讀性

**舊格式（已淘汰）**: 二進位封包 `[SOF][CMD][LEN][Data][CHK][EOF]`

**版本：** v1.0  
**崗位號碼：** 01  
**撰寫者：** Visual Basic .NET 2022 專案規格書  
**對應題目：** 113 年全國高級中等學校工業類科技藝競賽 — 電腦修護職類 第二站
