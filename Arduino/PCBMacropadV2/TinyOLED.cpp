#include "TinyOLED.h"

#include <Arduino.h>
#include <Wire.h>

void TinyOLED::sendCommand(uint8_t cmd) {
  Wire.beginTransmission(_i2c_addr);
  Wire.write(0x00); // Co = 0, D/C# = 0 -> Command mode
  Wire.write(cmd);
  Wire.endTransmission();
}

void TinyOLED::begin(bool rotate180 = false) {
  _is_rotated = rotate180;
  Wire.begin();
  // Simplified init sequence for SSD1306
  sendCommand(0xAE); // Display OFF
  sendCommand(0xD5); // Set Display Clock Div
  sendCommand(0x80); //   Suggested ratio
  sendCommand(0xA8); // Set MUX Ratio
  sendCommand(0x3F); //   1/64
  sendCommand(0xD3); // Set Display Offset
  sendCommand(0x00); //   No offset
  sendCommand(0x40); // Set Start Line

  if (_is_rotated) {
    sendCommand(0xA0); // Seg remap 0 to 127 -> 127 to 0
    sendCommand(0xC0); // COM scan inc -> dec
  } else {
    sendCommand(0xA1); // Seg remap 127 to 0 -> 0 to 127
    sendCommand(0xC8); // COM scan dec -> inc
  }

  sendCommand(0xDA); // Set COM Pins
  sendCommand(0x12); //   Hardware config
  sendCommand(0x81); // Contrast
  sendCommand(0xCF); //   Default
  sendCommand(0x8D); // Charge Pump
  sendCommand(0x14); //   Enable
  sendCommand(0xD9); // Pre-charge
  sendCommand(0xF1); //   Default
  sendCommand(0xDB); // VCOMH
  sendCommand(0x40); //   Default
  sendCommand(0xA4); // Display all on resume
  sendCommand(0xA6); // Normal display (non-inverted)
  sendCommand(0x2E); // Deactivate scroll
  sendCommand(0xAF); // Display ON
}

void TinyOLED::clear() {
  sendCommand(0x21); // Set column address
  sendCommand(0);    //   Start at 0
  sendCommand(127);  //   End at 127
  sendCommand(0x22); // Set page address
  sendCommand(0);    //   Start at 0
  sendCommand(7);    //   End at 7
  for (uint16_t i = 0; i < (128 * 64 / 8); i++) {
    Wire.beginTransmission(_i2c_addr);
    Wire.write(0x40); // Data mode
    for (uint8_t j = 0; j < 16; j++) {
      Wire.write(0x00);
    }
    Wire.endTransmission();
  }
  setCursor(0, 0);
}

void TinyOLED::setCursor(uint8_t x, uint8_t y) {
  _cursor_x = x;
  _cursor_y = y;
  sendCommand(0xB0 + y);             // Set page
  sendCommand(0x00 + (x & 0x0F));    // Set lower col
  sendCommand(0x10 + (x >> 4));      // Set upper col
}

void TinyOLED::print(const char* s) {
  Wire.beginTransmission(_i2c_addr);
  Wire.write(0x40); // Data mode
  while (*s) {
    char c = *s++;
    if (c >= ' ' && c <= 'Z') {
      for (uint8_t i = 0; i < 5; i++) {
        Wire.write(pgm_read_byte(&tiny_font[(c - ' ') * 5 + i]));
      }
      Wire.write(0x00); // Spacing
    }
  }
  Wire.endTransmission();
}

void TinyOLED::invert(bool inverted) {
  if (_is_inverted != inverted) {
    _is_inverted = inverted;
    sendCommand(inverted ? 0xA7 : 0xA6); // 0xA7=INVERT, 0xA6=NORMAL
  }
}