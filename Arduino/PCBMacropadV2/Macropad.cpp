#include "Macropad.h"

Macropad::Macropad(Layer* layerData, EncoderButton *encoderButton, Keypad *keypad, Adafruit_SSD1306* oled)
  : layers(layerData), eb(encoderButton), kp(keypad), display(oled), currentLayer(0) {}

void Macropad::begin() {
  startupAnimation();

  // Sends a clean report to the host. This is important on any Arduino type.
  Keyboard.begin();
}

void Macropad::update() {
  // Check if the display should be inverted (to prevent burn in)
  // if (millis() - invertOledTimestamp >= invertOledDuration)
  // {
  //   invertOled = !invertOled;
  //   invertOledTimestamp = millis();
  //   ShowCurrentLayer();
  // }

  // Call 'update' for every EncoderButton
  eb->update();

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