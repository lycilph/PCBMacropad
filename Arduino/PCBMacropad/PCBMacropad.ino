#include <HID-Project.h>
#include <Keypad.h>
#include <EncoderButton.h>
#include <LowPower.h>
#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

#define SCREEN_WIDTH 128 // OLED display width, in pixels
#define SCREEN_HEIGHT 64 // OLED display height, in pixels

// Declaration for an SSD1306 display connected to I2C (SDA, SCL pins)
// The pins for I2C are defined by the Wire-library. 
// On an arduino UNO:       A4(SDA), A5(SCL)
// On an arduino MEGA 2560: 20(SDA), 21(SCL)
// On an arduino LEONARDO:   2(SDA),  3(SCL), ...
#define OLED_RESET     -1 // Reset pin # (or -1 if sharing Arduino reset pin)
#define SCREEN_ADDRESS 0x3C ///< See datasheet for Address; 0x3D for 128x64, 0x3C for 128x32
Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, OLED_RESET);

// Layer led handling
const int numberOfLayers = 3;
char layerStrBuffer[20];
int currentLayer = 0;

// Rotary Encoder setup
const int buttonPin = 7;
const int encoderPin1 = 6;
const int encoderPin2 = 5;
EncoderButton eb1(encoderPin1, encoderPin2, buttonPin);

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
unsigned long keyHeldTime[LIST_MAX]; // Keep a list of how long each key is held

String mappedKeys[numberOfLayers][ROWS][COLS] = {
  {{"F13","F14","F15"},
  {"F16","F17","F18"},
  {"F19","F20","F21"}},
  {{"1","2","3"},
  {"4","5","6"},
  {"7","8","9"}},
  {{"Q","W","E"},
  {"A","S","D"},
  {"Z","X","C"}}
};

bool hasSlept = false;

bool invertOled = false;
unsigned long invertOledTimestamp;
unsigned long invertOledDuration = 300L * 1000;

void setup() {
  Serial.begin(9600); // Debug stuff

  // SSD1306_SWITCHCAPVCC = generate display voltage from 3.3V internally
  if(!display.begin(SSD1306_SWITCHCAPVCC, SCREEN_ADDRESS)) {
    Serial.println(F("SSD1306 allocation failed"));
    for(;;); // Don't proceed, loop forever
  }
  
  // Show initial display buffer contents on the screen --
  // the library initializes this with an Adafruit splash screen.
  display.display();
  delay(1000); // Pause for 1 seconds'

  StartupAnimation();
  
  // Show the keymap on the oled display
  ShowCurrentLayer();

  // Setup for encoder button
  eb1.setEncoderHandler(onEb1Encoder);
  eb1.setClickHandler(onEb1Click);
  eb1.setLongPressHandler(onEb1LongClick);

  invertOledTimestamp = millis();

  // Sends a clean report to the host. This is important on any Arduino type.
  Keyboard.begin();
}

void onEb1Click(EncoderButton& eb) {
  Consumer.write(MEDIA_PLAY_PAUSE);
}

void onEb1LongClick(EncoderButton& eb) {
  currentLayer += 1;
  if (currentLayer >= numberOfLayers)
    currentLayer = 0;

  ShowCurrentLayer();
}

void onEb1Encoder(EncoderButton& eb) {
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

void ShowCurrentLayer()
{
  sprintf(layerStrBuffer, "Layer: %d", currentLayer);

  // Clear the buffer
  display.clearDisplay();
  display.setTextSize(1); //height=9 pixels, width=5 pixels at textsize 1
  display.setTextColor(WHITE);
  display.setRotation(2); //rotates text on OLED 1=90 degrees, 2=180 degrees

  display.setCursor(0, 0);
  display.print(layerStrBuffer);
  
  display.setCursor(15, 16);
  display.print(mappedKeys[currentLayer][0][0]);
  display.setCursor(57, 16);
  display.print(mappedKeys[currentLayer][0][1]);
  display.setCursor(99, 16);
  display.print(mappedKeys[currentLayer][0][2]);

  display.setCursor(15, 32);
  display.print(mappedKeys[currentLayer][1][0]);
  display.setCursor(57, 32);
  display.print(mappedKeys[currentLayer][1][1]);
  display.setCursor(99, 32);
  display.print(mappedKeys[currentLayer][1][2]);

  display.setCursor(15, 48);
  display.print(mappedKeys[currentLayer][2][0]);
  display.setCursor(57, 48);
  display.print(mappedKeys[currentLayer][2][1]);
  display.setCursor(99, 48);
  display.print(mappedKeys[currentLayer][2][2]);

  // Invert display to prevent burn in
  display.invertDisplay(invertOled);

  display.display();
}

void StartupAnimation() {
  display.clearDisplay();

  for(int16_t i=0; i<max(display.width(),display.height())/2; i+=2) {
    display.drawCircle(display.width()/2, display.height()/2, i, SSD1306_WHITE);
    display.display();
    delay(1);
  }

  delay(500);
}

void loop() {
  if (USBDevice.isSuspended())
  {
    display.clearDisplay();
    display.display();
    delay(500);

    LowPower.idle(SLEEP_8S, ADC_OFF, TIMER4_OFF, TIMER3_OFF, TIMER1_OFF, TIMER0_OFF, SPI_OFF, USART1_OFF, TWI_OFF, USB_OFF);
    hasSlept = true;
  }

  if (hasSlept && USBDevice.isSuspended() == false)
  {
    hasSlept = false;
    ShowCurrentLayer();
  }

  // Call 'update' for every EncoderButton
  eb1.update();

  // Check if the display should be inverted (to prevent burn in)
  if (millis() - invertOledTimestamp >= invertOledDuration)
  {
    invertOled = !invertOled;
    invertOledTimestamp = millis();

    ShowCurrentLayer();
  }

  // 'Update' the keypad (and check for pressed keys)
  if (keypad.getKeys())
  {
    for (int i=0; i<LIST_MAX; i++)
    {
      if (keypad.key[i].stateChanged && keypad.key[i].kstate == PRESSED)
      {
        //Serial.println(keypad.key[i].kchar);
        HandleSingleKey(keypad.key[i].kchar);
      }
    }
  }
  // Check for held keys
  for (int i=0; i<LIST_MAX; i++)
  {
    if (keypad.key[i].kstate == HOLD && (millis() - keyHeldTime[i]) > 100)
    {
      //Serial.print(keypad.key[i].kchar);
      //Serial.println(" - held");
      HandleSingleKey(keypad.key[i].kchar);
      keyHeldTime[i] = millis();
    }
  }
}

void HandleSingleKey(char key)
{
  switch (currentLayer) {
    case 0:
      HandlerLayer0(key);
      break;
    case 1:
      HandlerLayer1(key);
      break;
    case 2:
      HandlerLayer2(key);
      break;
  }
}

void HandlerLayer0(char key)
{
  switch (key) {
    case '1':
      Keyboard.write(KEY_F13);
      break;
    case '2':
      Keyboard.write(KEY_F14);
      break;
    case '3':
      Keyboard.write(KEY_F15);
      break;
    case '4':
      Keyboard.write(KEY_F16);
      break;
    case '5':
      Keyboard.write(KEY_F17);
      break;
    case '6':
      Keyboard.write(KEY_F18);
      break;
    case '7':
      Keyboard.write(KEY_F19);
      break;
    case '8':
      Keyboard.write(KEY_F20);
      break;
    case '9':
      Keyboard.write(KEY_F21);
      break;
  }
}

void HandlerLayer1(char key)
{
  switch (key) {
    case '1':
      Keyboard.write('1');
      break;
    case '2':
      Keyboard.write('2');
      break;
    case '3':
      Keyboard.write('3');
      break;
    case '4':
      Keyboard.write('4');
      break;
    case '5':
      Keyboard.write('5');
      break;
    case '6':
      Keyboard.write('6');
      break;
    case '7':
      Keyboard.write('7');
      break;
    case '8':
      Keyboard.write('8');
      break;
    case '9':
      Keyboard.write('9');
      break;
  }
}

void HandlerLayer2(char key) {
  switch (key) {
    case '1':
      Keyboard.write('q');
      break;
    case '2':
      Keyboard.write('w');
      break;
    case '3':
      Keyboard.write('e');
      break;
    case '4':
      Keyboard.write('a');
      break;
    case '5':
      Keyboard.write('s');
      break;
    case '6':
      Keyboard.write('d');
      break;
    case '7':
      Keyboard.write('z');
      break;
    case '8':
      Keyboard.write('x');
      break;
    case '9':
      Keyboard.write('c');
      break;
  }
}