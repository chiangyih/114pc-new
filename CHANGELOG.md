# 更新日誌

## ?? 最新更新
2025-01-11

## ?? 更新內容

### README.md - 文檔與功能說明

#### ? 已修改項目

1. **EEPROM 寫入格式修改**
   - 舊格式: 二進位封包 `[SOF][CMD][LEN][Data][CHK][EOF]`
   - 新格式: 文字格式 `"WRITE" + 數值 + "\n"`

2. **資料傳送格式統一**
   - CPU Loading: `"LOAD" + 數值 + "\n"` (已確認)
   - RAM Loading: `"LOAD" + 數值 + "\n"` (已確認)
   - EEPROM 寫入: `"WRITE" + 數值 + "\n"` (新增)

3. **通訊協定簡化**
   - 從複雜的二進位封包改為簡單的文字協議
   - Arduino 可統一使用 `Serial.readStringUntil('\n')` 解析
   - 減少固件複雜度，提高可讀性

4. **Arduino 接收範例更新**
   - 新增 WRITE 命令接收範例
   - 提供統一的狀態機解析邏輯
   - 完整的錯誤處理機制

5. **文檔組織改進**
   - 統一使用文字格式說明
   - 修正舊有的二進位格式描述
   - 新增 EEPROM 新格式說明

#### ?? 已更正項目

1. **CPU/RAM 監控傳送格式** - 確認為文字格式（無空格）
   - 舊描述: `"LOAD " + 數值 + "\n"` (有空格)
   - 新描述: `"LOAD" + 數值 + "\n"` (無空格)
   - 例: `LOAD30\n`, `LOAD65\n`

2. **EEPROM 封包格式** - 已改用文字格式
   - 舊格式: 複雜的二進位封包
   - 新格式: 簡單的文字格式

3. **程式碼標註更新** - 更新程式註解對應新格式

---

## ?? 文件清單

### 修改檔案

| 檔案名稱 | 狀態 | 修改內容 |
|---------|------|---------|
| `README.md` | ? 已更新 | 通訊協定說明 |
| `CPU_RAM_Loading_Data_Format.md` | ? 已更新 | 完全改寫新格式 |
| `Form1.vb` | ? 已修改 | 代碼實現新格式 |

### 已淘汰檔案

| 檔案名稱 | 狀態 | 原因 |
|---------|------|------|
| `CPU_Loading_Bluetooth_Data_Format.md` | ?? 已淘汰 | 被 CPU_RAM_Loading_Data_Format.md 整合 |

---

## ? 版本紀錄

### Version 2.2 (2025-01-11)
- ? EEPROM 寫入改用文字格式 "WRITE{數值}\n"
- ? 更新所有相關文檔說明
- ? CPU/RAM 格式確認為 "LOAD{數值}\n" (無空格)
- ? 統一通訊協定為文字格式

### Version 2.1 (2025-01-11)
- ? 修改 CPU/RAM 資料格式為 "LOAD+數值+\n"
- ? 簡化通訊協定，提高可讀性
- ? 修正監控功能需要連線才能啟動的邏輯
- ? 清理冗餘程式碼

### Version 2.0 (2025-01-11)
- ? 修改資料傳送格式為文字格式
- ? 修正 RAM 監控無數據問題
- ? 完整的中文註解

### Version 1.0.0 (2025-01-XX)
- ? 初始版本發布
- ? 基本功能實作

---

## ?? 功能實現狀態

### 已完成功能

- [x] COM Port 選擇與連線管理
- [x] CPU 監控 - 每秒更新一次
- [x] RAM 監控 - 每秒更新一次
- [x] 顏色指示 (綠/黃/紅)
- [x] EEPROM 寫入 - 新文字格式
- [x] COM Port 熱插拔偵測
- [x] 藍牙模組名稱生成
- [x] 資料統一為文字格式

### 待完成功能

- [ ] MCU 端韌體開發
- [ ] 完整的除錯工具
- [ ] 效能最佳化

---

## ?? 主要改變說明

### 1. EEPROM 寫入格式變更

**舊格式** (已淘汰):
```
[SOF][CMD][LEN][Data][CHK][EOF]
0xFF  'E'   1   0x0A  0x0A  0xFE
```

**新格式** (現行):
```
"WRITE" + 數值 + "\n"
例: "WRITE10\n", "WRITE15\n"
```

**變更原因**:
- 統一通訊協定
- 簡化 MCU 接收邏輯
- 降低複雜度
- 提高代碼可維護性

### 2. 資料格式統一

**之前**: 混合使用二進位封包和文字格式
**現在**: 全面採用文字格式 + 換行符號

**好處**:
- Arduino 可統一使用 `Serial.readStringUntil('\n')`
- 減少狀態機複雜度
- 易於除錯和測試
- 符合業界標準做法

### 3. 程式碼更新

**ButtonWrite_Click() 函數**:
- 移除二進位封包構造
- 直接發送文字字符串
- 保持驗證邏輯不變

---

## ?? 待辦事項

1. ? 更新 README.md
2. ? 更新 CPU_RAM_Loading_Data_Format.md
3. ? 修改 Form1.vb 代碼
4. ? 編寫 MCU 接收程式
5. ? 測試功能完整性
6. ? 性能優化

---

## ?? 開發建議

### 對 MCU 開發者的建議

```cpp
// 簡化的接收邏輯
void setup() {
    Serial.begin(9600);
}

void loop() {
    if (Serial.available()) {
        String cmd = Serial.readStringUntil('\n');
        
        if (cmd.startsWith("LOAD")) {
            // CPU/RAM 資料
            int val = cmd.substring(4).toInt();
            handleLoading(val);
        } 
        else if (cmd.startsWith("WRITE")) {
            // EEPROM 寫入
            int val = cmd.substring(5).toInt();
            EEPROM.write(0, val);
        }
    }
}
```

---

## ?? 相關文件

- **README.md** - 完整功能說明
- **CPU_RAM_Loading_Data_Format.md** - 詳細格式說明
- **Form1.vb** - PC 端原始碼
- **PC_FunctionSpec.md** - 功能規格書

---

## ?? 重要提醒

1. 所有格式已改為文字 + 換行符號
2. EEPROM 新格式為 `WRITE{數值}\n`
3. CPU/RAM 格式為 `LOAD{數值}\n`
4. Arduino 需要更新接收邏輯以支持新格式

---

*最後更新者：GitHub Copilot*  
*更新時間：2025-01-11*  
*版本：2.2*
