#pragma once
#include "Keymap.h"
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include <Keypad.h>
#include <EncoderButton.h>

class Macropad {
public:
  Macropad(Layer* layerData, EncoderButton *encoderButton, Keypad *keypad, Adafruit_SSD1306* oled);
  void begin();
  void update();
  // void scanButtons();
  // void updateDisplay();

private:
  void startupAnimation();

  // void executeAction(int buttonIndex);
  // void changeLayer(int newLayer);
  // int findNextEnabledLayer();

  Layer* layers;
  EncoderButton* eb;
  Keypad *kp;
  Adafruit_SSD1306* display;

  int currentLayer;
  unsigned long keyHeldTime[LIST_MAX]; // Keep a list of how long each key is held
  // uint8_t buttonStates[NUM_BUTTONS];
  // unsigned long lastDebounceTime[NUM_BUTTONS];
  // const unsigned long debounceDelay = 50;
};