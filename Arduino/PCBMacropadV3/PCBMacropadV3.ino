#include <EncoderButton.h>
#include <LowPower.h>

#include "Keymap.h"
#include "ConfigManager.h"
#include "Macropad.h"
#include "config.h"

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

bool hasSlept = false;

void setup() {
  #ifdef ENABLE_SERIAL
    Serial.begin(9600);
    // Wait a moment for serial to connect if needed
    while (!Serial); 
  #endif
  
  // Start the U8g2 library. This also initializes the I2C communication.
  display.begin();
  displayStart();
  
  // Setup for encoder button
  encoderButton.setEncoderHandler(onEncoderEvent);
  encoderButton.setClickHandler(onEncoderClick);
  encoderButton.setLongPressHandler(onEncoderLongClick);

  configManager.begin();
  configManager.loadConfig(allLayers);
  
  macropad.begin();
  macropad.updateDisplay();

  DEBUG_PRINTLN(F("Macropad Initialized"));
  DEBUG_PRINTLN(F("Send 'd' to dump config"));
  DEBUG_PRINTLN(F("Send 'r' for factory reset"));
  DEBUG_PRINTLN(F("Send 'f' to check ram"));
}

void loop() {
  if (USBDevice.isSuspended())
  {
    display.clear();
    delay(500);

    LowPower.idle(SLEEP_8S, ADC_OFF, TIMER4_OFF, TIMER3_OFF, TIMER1_OFF, TIMER0_OFF, SPI_OFF, USART1_OFF, TWI_OFF, USB_OFF);
    hasSlept = true;
  }

  if (hasSlept && USBDevice.isSuspended() == false)
  {
    hasSlept = false;
    macropad.updateDisplay();
  }

  macropad.update();
  
  // Call 'update' for every EncoderButton
  encoderButton.update();

  #ifdef ENABLE_SERIAL
    handleSerialCommands();
  #endif
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
  delay(1000);
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

#ifdef ENABLE_SERIAL
  void handleSerialCommands() {
    if (Serial.available() > 0) {
      char command = Serial.read();

      #ifdef ENABLE_FREERAM_CHECK
        Serial.print(F("Free SRAM: "));
        Serial.println(freeMemory());
      #endif
      
      if (command == 'd') { // 'd' for Dump
        const size_t configSize = sizeof(Layer) * NUM_LAYERS;
        uint8_t serialBuffer[configSize];
        size_t bytesWritten = configManager.serializeConfig(allLayers, serialBuffer, configSize);

        if (bytesWritten > 0) {
          DEBUG_PRINTLN(F("--- BEGIN CONFIG DUMP ---"));
          DEBUG_PRINT(F("Size of config: ")); DEBUG_PRINTLN(configSize);
          for (size_t i = 0; i < bytesWritten; i++) {
            if (serialBuffer[i] < 0x10) DEBUG_PRINT("0");
            DEBUG_PRINT(serialBuffer[i], HEX);
            DEBUG_PRINT(" ");
          }
          DEBUG_PRINTLN();
          DEBUG_PRINTLN(F("--- END CONFIG DUMP ---"));
        } else {
          DEBUG_PRINTLN(F("Error: Serialization failed."));
        }
      }
      
      if (command == 'r') { // 'r' for Reset
          DEBUG_PRINTLN(F("Performing factory reset..."));
          configManager.factoryReset(allLayers);
          configManager.saveConfig(allLayers);
          delay(500);
          macropad.updateDisplay();
          DEBUG_PRINTLN(F("Reset complete"));
      }
    }
  }
#endif

#ifdef ENABLE_FREERAM_CHECK
  int freeMemory() {
    extern int __heap_start, *__brkval;
    int v;
    return (int)&v - (__brkval == 0 ? (int)&__heap_start : (int)__brkval);
  }
#endif