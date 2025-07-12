#pragma once

#include <Keypad.h>
#include <EncoderButton.h>

#include "Keymap.h"
#include "TinyOLED.h"

class Macropad {
public:
  Macropad(Layer* layerData, Keypad *keypad, TinyOLED* oled);

  void begin();
  void update();  
  void updateDisplay();
  void switchToNextLayer();

private:
  void startupAnimation();

  // void executeAction(int buttonIndex);
  void changeLayer(int newLayer);
  int findNextEnabledLayer();

  Layer* layers;
  Keypad *kp;
  TinyOLED* display;

  int currentLayer;
  unsigned long keyHeldTime[LIST_MAX]; // Keep a list of how long each key is held
  // uint8_t buttonStates[NUM_BUTTONS];
  // unsigned long lastDebounceTime[NUM_BUTTONS];
  // const unsigned long debounceDelay = 50;

  // buffer for the display routine
  char layerStrBuffer[20];

  // Display invert color timer
  bool invertOled = false;
  unsigned long invertOledTimestamp;
  const unsigned long invertOledDuration = 5 * 60 * 1000; // Should work out to 5 min :-)
};