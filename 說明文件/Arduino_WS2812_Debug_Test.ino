/*
 * Arduino WS2812 CPU Monitor - 完整測試版
 * 用於除錯 PC 端連線問題
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
#define LED_PIN 6
#define NUM_LEDS 8
#define BAUD_RATE 9600

// ===== 封包格式定義 =====
#define SOF 0xFF
#define EOF_MARKER 0xFE
#define CMD_CPU 'C'
#define CMD_RAM 'R'
#define CMD_CONNECT 'c'

// ===== WS2812 物件 =====
Adafruit_NeoPixel strip(NUM_LEDS, LED_PIN, NEO_GRB + NEO_KHZ800);

// ===== 狀態變數 =====
bool connected = false;
unsigned long lastPacketTime = 0;
int packetCount = 0;

void setup() {
    // 啟動序列埠
    Serial.begin(BAUD_RATE);
    
    // 初始化 WS2812
    strip.begin();
    strip.setBrightness(50);
    strip.show();
    
    // 啟動動畫
    startupAnimation();
    
    // 顯示就緒訊息
    Serial.println("=================================");
    Serial.println("Arduino WS2812 CPU Monitor");
    Serial.println("版本: 1.0 (Debug Mode)");
    Serial.println("Baud Rate: 9600");
    Serial.println("等待 PC 連線...");
    Serial.println("=================================");
}

void loop() {
    // 檢查序列埠資料
    if (Serial.available() > 0) {
        byte inByte = Serial.read();
        
        // 檢查連線確認字元
        if (inByte == CMD_CONNECT) {
            handleConnect();
            return;
        }
        
        // 檢查封包起始
        if (inByte == SOF) {
            if (Serial.available() >= 8) {
                processPacket();
            }
        }
    }
    
    // 連線逾時檢查
    if (connected && (millis() - lastPacketTime > 5000)) {
        Serial.println("[WARNING] 超過 5 秒未收到資料");
        connected = false;
        setAllLEDs(0, 0, 0);  // 關閉所有 LED
    }
}

// ===== 處理連線確認 =====
void handleConnect() {
    connected = true;
    Serial.println("");
    Serial.println("=================================");
    Serial.println("? PC 連線成功！");
    Serial.println("=================================");
    
    // 連線成功動畫（綠色閃爍 3 次）
    for (int i = 0; i < 3; i++) {
        setAllLEDs(0, 255, 0);
        delay(200);
        setAllLEDs(0, 0, 0);
        delay(200);
    }
    
    lastPacketTime = millis();
}

// ===== 處理封包 =====
void processPacket() {
    // 讀取命令碼
    byte cmd = Serial.read();
    
    // 讀取資料長度
    byte len = Serial.read();
    
    // 檢查是否為 CPU/RAM 封包
    if ((cmd == CMD_CPU || cmd == CMD_RAM) && len == 4) {
        // 讀取資料
        byte percent = Serial.read();
        byte r = Serial.read();
        byte g = Serial.read();
        byte b = Serial.read();
        byte chk = Serial.read();
        byte eof = Serial.read();
        
        // 計算檢查碼
        byte calculatedChk = (percent + r + g + b) & 0xFF;
        
        // 驗證封包
        if (eof == EOF_MARKER && chk == calculatedChk) {
            // 封包正確
            setAllLEDs(r, g, b);
            
            // 更新計數和時間
            packetCount++;
            lastPacketTime = millis();
            
            // 顯示詳細資訊
            Serial.println("");
            Serial.println("--- 收到封包 ---");
            Serial.print("類型: ");
            Serial.println(cmd == CMD_CPU ? "CPU" : "RAM");
            Serial.print("使用率: ");
            Serial.print(percent);
            Serial.println(" %");
            Serial.print("RGB: (");
            Serial.print(r);
            Serial.print(", ");
            Serial.print(g);
            Serial.print(", ");
            Serial.print(b);
            Serial.println(")");
            Serial.print("Checksum: 計算值=0x");
            Serial.print(calculatedChk, HEX);
            Serial.print(", 接收值=0x");
            Serial.println(chk, HEX);
            Serial.print("封包總數: ");
            Serial.println(packetCount);
            Serial.println("? 封包驗證成功");
            
            // 顯示顏色名稱
            printColorName(r, g, b);
            
        } else {
            // 封包錯誤
            Serial.println("");
            Serial.println("? 封包驗證失敗！");
            Serial.print("EOF: 預期=0x");
            Serial.print(EOF_MARKER, HEX);
            Serial.print(", 實際=0x");
            Serial.println(eof, HEX);
            Serial.print("Checksum: 計算值=0x");
            Serial.print(calculatedChk, HEX);
            Serial.print(", 接收值=0x");
            Serial.println(chk, HEX);
            
            errorBlink();
        }
    } else {
        // 不支援的命令或長度錯誤
        Serial.println("");
        Serial.println("? 不支援的命令或長度錯誤");
        Serial.print("CMD: 0x");
        Serial.println(cmd, HEX);
        Serial.print("LEN: ");
        Serial.println(len);
        
        clearSerialBuffer();
    }
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
    // 測試所有 LED（紅、綠、藍）
    Serial.println("正在測試 LED...");
    
    // 紅色
    Serial.println("  紅色測試");
    setAllLEDs(255, 0, 0);
    delay(500);
    
    // 綠色
    Serial.println("  綠色測試");
    setAllLEDs(0, 255, 0);
    delay(500);
    
    // 藍色
    Serial.println("  藍色測試");
    setAllLEDs(0, 0, 255);
    delay(500);
    
    // 關閉
    Serial.println("  LED 測試完成");
    setAllLEDs(0, 0, 0);
    delay(200);
    
    // 彩虹效果
    Serial.println("彩虹效果測試...");
    for (int j = 0; j < 256; j += 4) {
        for (int i = 0; i < NUM_LEDS; i++) {
            strip.setPixelColor(i, Wheel((i * 256 / NUM_LEDS + j) & 255));
        }
        strip.show();
        delay(5);
    }
    
    setAllLEDs(0, 0, 0);
    Serial.println("啟動動畫完成");
    Serial.println("");
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

// ===== 清除序列埠緩衝區 =====
void clearSerialBuffer() {
    while (Serial.available() > 0) {
        Serial.read();
    }
}

// ===== 顯示顏色名稱 =====
void printColorName(byte r, byte g, byte b) {
    Serial.print("顏色: ");
    
    if (r == 0 && g == 255 && b == 0) {
        Serial.println("綠色 (0-50% 正常負載)");
    } else if (r == 255 && g == 255 && b == 0) {
        Serial.println("黃色 (51-84% 中度負載)");
    } else if (r == 255 && g == 0 && b == 0) {
        Serial.println("紅色 (85-100% 高負載)");
    } else {
        Serial.println("自訂顏色");
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
