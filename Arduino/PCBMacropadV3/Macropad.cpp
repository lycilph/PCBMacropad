#include "Macropad.h"

Macropad::Macropad(Layer* layerData, Keypad* keypad, U8G2_SSD1306_128X64_NONAME_1_HW_I2C* display)
  : layers(layerData), keypad(keypad), display(display), currentLayer(0) {}

void Macropad::begin() {
  updateDisplay();

  // Sends a clean report to the host. This is important on any Arduino type.
  Keyboard.begin();
  Consumer.begin();
}

void Macropad::updateDisplay() {
  char layerStrBuffer[25];

  display->firstPage();
  do {
    // --- START OF DRAWING COMMANDS ---

    // Define the height of our header bar in pixels
    int headerHeight = 14;

    // 1. Draw the white bar at the top
    display->setDrawColor((invertOled) ? 0 : 1); // Set drawing color to 1 (white/foreground)
    display->drawBox(0, 0, 128, headerHeight); // drawBox(x, y, width, height)

    // 2. Draw the black text on top of the white bar
    display->setDrawColor((invertOled) ? 1 : 0); // Set drawing color to 0 (black/background)
    display->setFont(u8g2_font_7x14B_tr); // Set the font for the header
    
    // The y-coordinate for drawStr is the text baseline, not the top.
    // Placing it at y=11 looks good for this font in a 12px high box.
    sprintf(layerStrBuffer, "Layer: %s", layers[currentLayer].name);
    display->drawStr(2, 11, layerStrBuffer); 

    // 3. Draw the white text on the black background
    display->setDrawColor((invertOled) ? 1 : 0); // IMPORTANT: Set drawing color back to 1 (white)
    display->drawBox(0, headerHeight, 128, 64-headerHeight);

    display->setDrawColor((invertOled) ? 0 : 1); // IMPORTANT: Set drawing color back to 1 (white)
    display->setFont(u8g2_font_profont12_tr); // Use a different font for the body

    const int startX = 0, startY = 28, colWidth = 42, rowHeight = 14;
    for (int i = 0; i < NUM_BUTTONS; i++) {
      int row = i / 3, col = i % 3;
      display->drawStr(col*colWidth, row*rowHeight+startY, layers[currentLayer].actions[i].text);
    }
    // --- END OF DRAWING COMMANDS ---

  } while (display->nextPage()); // Sends the completed "page" to the display and loops
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
        Serial.println(keypad->key[i].kchar);
        executeAction(keypad->key[i].kchar - 49);
      }
    }
  }
  // Check for held keys
  for (int i=0; i < LIST_MAX; i++)
  {
    if (keypad->key[i].kstate == HOLD && (millis() - keyHeldTime[i]) > 100)
    {
      Serial.print(keypad->key[i].kchar);
      Serial.println(" - held");
      executeAction(keypad->key[i].kchar - 49);

      keyHeldTime[i] = millis();
    }
  }
}

void Macropad::executeAction(int buttonIndex) {
  KeyAction action = layers[currentLayer].actions[buttonIndex];

  Serial.print("Button: "); Serial.print(action.key); Serial.print(" - "); Serial.print(action.modifier); Serial.print(" - "); Serial.println(action.text);
  delay(250);
  
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
  updateDisplay();
}

void Macropad::switchToNextLayer() {
  changeLayer(findNextEnabledLayer());
}

int Macropad::findNextEnabledLayer() {
  for (int i = 1; i <= NUM_LAYERS; i++) {
    int nextIndex = (currentLayer + i) % NUM_LAYERS;
    if (layers[nextIndex].isEnabled) return nextIndex;
  }
  return currentLayer;
}