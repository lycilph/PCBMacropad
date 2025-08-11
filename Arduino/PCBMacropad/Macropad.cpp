#include "Macropad.h"
#include "config.h"

Macropad::Macropad(Layer* layerData, Keypad* keypad, SSD1306AsciiAvrI2c* display)
  : layers(layerData), keypad(keypad), display(display), currentLayer(0) {}

void Macropad::begin() {
  updateDisplay();

  // Sends a clean report to the host. This is important on any Arduino type.
  Keyboard.begin();
  Consumer.begin();
}

void Macropad::updateDisplay() {
  display->invertDisplay(invertOled);
  display->clear();

  display->setFont(Verdana12);
  display->print(F("Layer: ")); display->println(layers[currentLayer].name);
  display->setFont(Adafruit5x7);
  display->println(F("--------------------------------------"));

  const int startX = 0, startY = 4, colWidth = 42;
  for (int i = 0; i < NUM_BUTTONS; i++) {
    int row = i / 3, col = i % 3;
    display->setCursor(startX+col*colWidth, startY+row);
    display->print(layers[currentLayer].actions[i].text);
  }
}

void Macropad::update() {
  // Check if the display should be inverted (to prevent burn in)
  if (millis() - invertOledTimestamp >= invertOledDuration)
  {
    invertOled = !invertOled;
    invertOledTimestamp = millis();
    updateDisplay();
  }

  // 'Update' the keypad (and check for pressed keys)
  if (keypad->getKeys())
  {
    for (int i=0;  i < LIST_MAX; i++)
    {
      if (keypad->key[i].stateChanged && keypad->key[i].kstate == PRESSED)
      {
        // DEBUG_PRINTLN(keypad->key[i].kchar);
        executeAction(keypad->key[i].kchar - 49);
      }
    }
  }
  // Check for held keys
  for (int i=0; i < LIST_MAX; i++)
  {
    if (keypad->key[i].kstate == HOLD && (millis() - keyHeldTime[i]) > 100)
    {
      // DEBUG_PRINT(keypad->key[i].kchar);
      // DEBUG_PRINTLN(F(" - held"));
      executeAction(keypad->key[i].kchar - 49);

      keyHeldTime[i] = millis();
    }
  }
}

void Macropad::executeAction(int buttonIndex) {
  KeyAction action = layers[currentLayer].actions[buttonIndex];

  DEBUG_PRINTLN(action.text);
  
  if (action.modifier & MOD_LEFT_CTRL) Keyboard.press(KEY_LEFT_CTRL);
  if (action.modifier & MOD_LEFT_SHIFT) Keyboard.press(KEY_LEFT_SHIFT);
  if (action.modifier & MOD_LEFT_ALT) Keyboard.press(KEY_LEFT_ALT);
  if (action.modifier & MOD_LEFT_GUI) Keyboard.press(KEY_LEFT_GUI);
  if (action.modifier & MOD_RIGHT_CTRL) Keyboard.press(KEY_RIGHT_CTRL);
  if (action.modifier & MOD_RIGHT_SHIFT) Keyboard.press(KEY_RIGHT_SHIFT);
  if (action.modifier & MOD_RIGHT_ALT) Keyboard.press(KEY_RIGHT_ALT);
  if (action.modifier & MOD_RIGHT_GUI) Keyboard.press(KEY_RIGHT_GUI);

  Keyboard.press(action.key);
  Keyboard.releaseAll();
}

void Macropad::changeLayer(int newLayer) {
  currentLayer = newLayer;
  DEBUG_PRINT(F("New layer: ")); DEBUG_PRINTLN(newLayer);
  updateDisplay();
}

void Macropad::switchToNextLayer() {
  DEBUG_PRINTLN(F("Switching layer"));
  changeLayer(findNextEnabledLayer());
}

int Macropad::findNextEnabledLayer() {
  for (int i = 1; i <= NUM_LAYERS; i++) {
    int nextIndex = (currentLayer + i) % NUM_LAYERS;
    if (layers[nextIndex].isEnabled) return nextIndex;
  }
  return currentLayer;
}