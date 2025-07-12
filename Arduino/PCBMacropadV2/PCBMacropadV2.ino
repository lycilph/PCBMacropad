#include <Wire.h>
#include <Keypad.h>
#include <EncoderButton.h>

#include "Keymap.h"
#include "ConfigManager.h"
#include "Macropad.h"

TinyOLED display;

// Rotary Encoder setup
const int buttonPin = 7;
const int encoderPin1 = 6;
const int encoderPin2 = 5;
EncoderButton encoderButton(encoderPin1, encoderPin2, buttonPin);

// Keypad stuff
const byte ROWS = 3; // 3 rows
const byte COLS = 3; // 3 columns
char keys[ROWS][COLS] = {
  {'1','4','7'},
  {'2','5','8'},
  {'3','6','9'},
};
byte rowPins[ROWS] = {19, 18, 15}; //connect to the row pinouts of the keypad
byte colPins[COLS] = {14, 16, 10}; //connect to the column pinouts of the keypad
Keypad keypad = Keypad( makeKeymap(keys), colPins, rowPins, ROWS, COLS );

// --- Global Objects ---
Layer allLayers[NUM_LAYERS];
ConfigManager configManager;
Macropad macropad(allLayers, &keypad, &display);

void setup() {
  Serial.begin(9600);
  // Wait a moment for serial to connect if needed
  while (!Serial); 
  
  // SSD1306_SWITCHCAPVCC = generate display voltage from 3.3V internally
  // if(!display.begin(SSD1306_SWITCHCAPVCC, SCREEN_ADDRESS)) {
  //   Serial.println(F("SSD1306 allocation failed"));
  //   for(;;); // Don't proceed, loop forever
  // }
  display.begin(true)
  
  // Show initial display buffer contents on the screen --
  // the library initializes this with an Adafruit splash screen.
  display.display();
  delay(500);

  // Initialize and load configuration from EEPROM
  configManager.begin();
  configManager.loadConfig(allLayers);
  configManager.dumpCurrentConfig(allLayers); // Debug - comment out

  // Setup for encoder button
  encoderButton.setEncoderHandler(onEncoderEvent);
  // eb->setClickHandler(onEncoderClick);
  // eb->setLongPressHandler(onEncoderLongClick);

  // Initialize the macropad logic
  macropad.begin();
}

void loop() {
  // Call 'update' for every EncoderButton
  encoderButton.update();

  macropad.update();
}


void onEncoderEvent(EncoderButton& eb) {
  int incr = eb.increment();
  if (incr > 0)
  {
    Consumer.write(MEDIA_VOLUME_DOWN);
  }
  else if (incr < 0)
  {
    Consumer.write(MEDIA_VOLUME_UP);
  }
}

void onEncoderClick(EncoderButton& eb) {
  Consumer.write(MEDIA_PLAY_PAUSE);
}

void onEncoderLongClick(EncoderButton& eb) {
  macropad.switchToNextLayer();
}