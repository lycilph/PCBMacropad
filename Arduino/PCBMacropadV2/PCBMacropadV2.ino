#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include <Keypad.h>
#include <EncoderButton.h>

#include "Keymap.h"
#include "ConfigManager.h"
#include "Macropad.h"

#define SCREEN_WIDTH 128 // OLED display width, in pixels
#define SCREEN_HEIGHT 64 // OLED display height, in pixels
#define OLED_RESET    -1 // Reset pin # (or -1 if sharing Arduino reset pin)
#define SCREEN_ADDRESS 0x3C ///< See datasheet for Address; 0x3D for 128x64, 0x3C for 128x32
Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, OLED_RESET);

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
Macropad macropad(allLayers, &encoderButton, &keypad, &display);

void setup() {
  Serial.begin(9600);
  // Wait a moment for serial to connect if needed
  while (!Serial); 
  
  // SSD1306_SWITCHCAPVCC = generate display voltage from 3.3V internally
  if(!display.begin(SSD1306_SWITCHCAPVCC, SCREEN_ADDRESS)) {
    Serial.println(F("SSD1306 allocation failed"));
    for(;;); // Don't proceed, loop forever
  }
  
  // Show initial display buffer contents on the screen --
  // the library initializes this with an Adafruit splash screen.
  display.display();
  delay(500);

  // Initialize and load configuration from EEPROM
  configManager.begin();
  configManager.loadConfig(allLayers);
  configManager.dumpCurrentConfig(allLayers); // Debug - comment out

  // Initialize the macropad logic
  macropad.begin();
}

void loop() {
}

