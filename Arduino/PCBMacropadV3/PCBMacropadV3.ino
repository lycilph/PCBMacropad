#include <EncoderButton.h>

#include "Keymap.h"
#include "ConfigManager.h"
#include "Macropad.h"

// --- Debug Configuration ---
// Comment out this line for the final build to save ~1.5KB of flash memory
#define ENABLE_DEBUG

#ifdef ENABLE_DEBUG
  #define DEBUG_PRINT(...) Serial.print(__VA_ARGS__)
  #define DEBUG_PRINTLN(...) Serial.println(__VA_ARGS__)
#else
  // The empty definitions also need to be variadic
  #define DEBUG_PRINT(...)
  #define DEBUG_PRINTLN(...)
#endif

U8G2_SSD1306_128X64_NONAME_1_HW_I2C display(U8G2_R2, /* reset=*/ U8X8_PIN_NONE);

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
  // Start the U8g2 library. This also initializes the I2C communication.
  display.begin();
  displayStart();

  configManager.begin();
  configManager.loadConfig(allLayers);
  
  macropad.begin();
  macropad.updateDisplay();

  DEBUG_PRINTLN("Macropad Initialized.");
  DEBUG_PRINTLN("Send 'd' to dump config.");
  DEBUG_PRINTLN("Send 'r' for factory reset.");
}

void displayStart()
{
  display.firstPage();
  do {
    // --- START OF DRAWING COMMANDS ---
    display.setDrawColor(1);
    display.setFont(u8g2_font_profont12_tr);
    display.drawStr(10, 40, "Starting...");
    // --- END OF DRAWING COMMANDS ---
  } while (display.nextPage()); // Sends the completed "page" to the display and loops

  // Add a delay so the screen doesn't refresh constantly in this example
  delay(2000);
}

void loop() {
  macropad.update();

  handleSerialCommands();
}

void handleSerialCommands() {
  if (Serial.available() > 0) {
    char command = Serial.read();

    if (command == 'd') { // 'd' for Dump
      const size_t configSize = sizeof(Layer) * NUM_LAYERS;
      uint8_t serialBuffer[configSize];
      size_t bytesWritten = configManager.serializeConfig(allLayers, serialBuffer, configSize);

      if (bytesWritten > 0) {
        DEBUG_PRINTLN("--- BEGIN CONFIG DUMP ---");
        for (size_t i = 0; i < bytesWritten; i++) {
          if (serialBuffer[i] < 0x10) DEBUG_PRINT("0");
          DEBUG_PRINT(serialBuffer[i], HEX);
          DEBUG_PRINT(" ");
        }
        DEBUG_PRINTLN();
        DEBUG_PRINTLN("--- END CONFIG DUMP ---");
      } else {
        DEBUG_PRINTLN("Error: Serialization failed.");
      }
    }
    
    if (command == 'r') { // 'r' for Reset
        DEBUG_PRINTLN("Performing factory reset...");
        configManager.factoryReset(allLayers);
        configManager.saveConfig(allLayers);
        macropad.updateDisplay();
        DEBUG_PRINTLN("Reset complete.");
    }
  }
}