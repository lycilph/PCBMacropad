#pragma once

#include <Keypad.h>
#include "SSD1306Ascii.h"
#include "SSD1306AsciiAvrI2c.h"

#include "Keymap.h"

class Macropad {
public:
  Macropad(Layer* layerData, Keypad *keypad, SSD1306AsciiAvrI2c* display);

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
  SSD1306AsciiAvrI2c* display;

  // Misc variables
  int currentLayer;
  unsigned long keyHeldTime[LIST_MAX]; // Keep a list of how long each key is held

  // Display invert color timer
  bool invertOled = false;
  unsigned long invertOledTimestamp;
  const unsigned long invertOledDuration = 5 * 60 * 1000; // Should work out to 5 min :-)
  //const unsigned long invertOledDuration = 10 * 1000; // debug - invert every 10 sec
};