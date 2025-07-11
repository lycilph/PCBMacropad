#include "Macropad.h"

Macropad::Macropad(Layer* layerData, Keypad *keypad, Adafruit_SSD1306* oled)
  : layers(layerData), kp(keypad), display(oled), currentLayer(0) {}

void Macropad::begin() {
  startupAnimation();

  updateDisplay();

  // Sends a clean report to the host. This is important on any Arduino type.
  Keyboard.begin();
}

void Macropad::updateDisplay() {
  // Clear the buffer
  display->clearDisplay();
  display->setTextSize(1); //height=9 pixels, width=5 pixels at textsize 1
  display->setRotation(2); //rotates text on OLED 1=90 degrees, 2=180 degrees

  // Draw top bar with layer # and name
  display->fillRect(0, 0, 128, 13, WHITE);
  display->setCursor(5, 2);
  display->setTextColor(BLACK);
  sprintf(layerStrBuffer, "Layer: %d [%s]", currentLayer, layers[currentLayer].name);
  display->print(layerStrBuffer);

  // Update the buttons text
  display->setTextColor(WHITE);

  // const int startX = 0, startY = 16;
  // const int cellWidth = 42, cellHeight = 16;

  // for (int i = 0; i < NUM_BUTTONS; i++) {
  //   int row = i / 3;
  //   int col = i % 3;
  //   int x = startX + col * cellWidth;
  //   int y = startY + row * cellHeight;
    
  //   oled->setCursor(x, y);
  //   oled->print(layers[currentLayer].actions[i].text);

  // Invert display to prevent burn in (if needed)
  display->invertDisplay(invertOled);

  display->display();
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
  if (kp->getKeys())
  {
    for (int i=0; i<LIST_MAX; i++)
    {
      if (kp->key[i].stateChanged && kp->key[i].kstate == PRESSED)
      {
        Serial.println(kp->key[i].kchar);
        //HandleSingleKey(keypad.key[i].kchar);
      }
    }
  }
  // Check for held keys
  for (int i=0; i<LIST_MAX; i++)
  {
    if (kp->key[i].kstate == HOLD && (millis() - keyHeldTime[i]) > 100)
    {
      //Serial.print(keypad.key[i].kchar);
      //Serial.println(" - held");
      //HandleSingleKey(keypad.key[i].kchar);
      keyHeldTime[i] = millis();
    }
  }
}

void Macropad::switchToNextLayer() {
  int nextLayer = findNextEnabledLayer();
  changeLayer(nextLayer);
}

void Macropad::startupAnimation() {
  display->clearDisplay();

  int16_t width = display->width();
  int16_t height = display->height();

  for(int16_t i=0; i < max(width,height)/2; i += 2) {
    display->drawCircle(width/2, height/2, i, SSD1306_WHITE);
    display->display();
    delay(1);
  }

  delay(500);
}

int Macropad::findNextEnabledLayer() {
  for (int i = 1; i <= NUM_LAYERS; i++) {
    int nextIndex = (currentLayer + i) % NUM_LAYERS;
    if (layers[nextIndex].isEnabled) {
      return nextIndex;
    }
  }
  return currentLayer; // Fallback if no other layers are enabled
}

void Macropad::changeLayer(int newLayer) {
  currentLayer = newLayer;
  updateDisplay();
}