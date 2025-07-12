#pragma once

#include <Keypad.h>
#include <U8g2lib.h>

#include "Keymap.h"

class Macropad {
public:
  Macropad(Layer* layerData, Keypad *keypad, U8G2_SSD1306_128X64_NONAME_1_HW_I2C* display);

  void begin();
  void updateDisplay();
  void update();
  void switchToNextLayer();

private:
  void executeAction(int buttonIndex);
  void changeLayer(int newLayer); 
  int findNextEnabledLayer();
  
  Layer* layers;
  Keypad* keypad;
  U8G2_SSD1306_128X64_NONAME_1_HW_I2C* display;

  // Misc variables
  int currentLayer;
  unsigned long keyHeldTime[LIST_MAX]; // Keep a list of how long each key is held

  // Display invert color timer
  bool invertOled = false;
  unsigned long invertOledTimestamp;
  const unsigned long invertOledDuration = 5 * 60 * 1000; // Should work out to 5 min :-)
  // const unsigned long invertOledDuration = 1000; // Should work out to 5 min :-)
};