/*
 * Arduino WS2812 - 最簡化測試版
 * 用於快速驗證連線和 LED 顯示
 */

#include <Adafruit_NeoPixel.h>

#define LED_PIN 6
#define NUM_LEDS 8

Adafruit_NeoPixel strip(NUM_LEDS, LED_PIN, NEO_GRB + NEO_KHZ800);

void setup() {
    Serial.begin(9600);
    strip.begin();
    strip.setBrightness(50);
    strip.show();
    
    // 啟動測試
    Serial.println("Arduino Ready!");
    testLEDs();
}

void loop() {
    // 等待封包（至少 9 bytes）
    if (Serial.available() >= 9) {
        byte sof = Serial.read();
        
        if (sof == 0xFF) {  // 檢查 SOF
            byte cmd = Serial.read();
            byte len = Serial.read();
            
            if (cmd == 'C' && len == 4) {  // CPU 封包
                byte percent = Serial.read();
                byte r = Serial.read();
                byte g = Serial.read();
                byte b = Serial.read();
                byte chk = Serial.read();
                byte eof = Serial.read();
                
                // 驗證
                if (eof == 0xFE && chk == ((percent + r + g + b) & 0xFF)) {
                    // 顯示在 LED
                    for (int i = 0; i < NUM_LEDS; i++) {
                        strip.setPixelColor(i, strip.Color(r, g, b));
                    }
                    strip.show();
                    
                    // 顯示資訊
                    Serial.print("CPU: ");
                    Serial.print(percent);
                    Serial.print("% RGB(");
                    Serial.print(r);
                    Serial.print(",");
                    Serial.print(g);
                    Serial.print(",");
                    Serial.print(b);
                    Serial.println(")");
                } else {
                    Serial.println("Checksum Error!");
                }
            }
        }
    }
}

// LED 測試函式
void testLEDs() {
    Serial.println("Testing LEDs...");
    
    // 紅色
    for (int i = 0; i < NUM_LEDS; i++) {
        strip.setPixelColor(i, strip.Color(255, 0, 0));
    }
    strip.show();
    delay(500);
    
    // 綠色
    for (int i = 0; i < NUM_LEDS; i++) {
        strip.setPixelColor(i, strip.Color(0, 255, 0));
    }
    strip.show();
    delay(500);
    
    // 藍色
    for (int i = 0; i < NUM_LEDS; i++) {
        strip.setPixelColor(i, strip.Color(0, 0, 255));
    }
    strip.show();
    delay(500);
    
    // 關閉
    for (int i = 0; i < NUM_LEDS; i++) {
        strip.setPixelColor(i, 0);
    }
    strip.show();
    
    Serial.println("LED Test Complete!");
}
