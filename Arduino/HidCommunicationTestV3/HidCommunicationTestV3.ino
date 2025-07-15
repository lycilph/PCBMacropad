#include "HID-Project.h"

// --- Protocol Commands ---
const uint8_t CMD_PC_GET_CONFIG = 'G';
const uint8_t CMD_PC_SET_CONFIG = 'S';

const uint8_t CMD_ARDUINO_SEND_CONFIG = 'C';
const uint8_t CMD_ARDUINO_ACK = 'A';
const uint8_t CMD_ARDUINO_ERROR = 'E';

// --- Constants ---
const int NUM_BUTTONS = 9;
const int NUM_LAYERS = 3;
const int BUTTON_TEXT_LENGTH = 6;
const int LAYER_TEXT_LENGTH = 12;

// --- Data Structures with __attribute__((packed)) ---
struct __attribute__((packed)) KeyAction {
   uint8_t key;
   uint16_t modifier;
   char text[BUTTON_TEXT_LENGTH];
};

struct __attribute__((packed)) Layer {
  char name[LAYER_TEXT_LENGTH];
  bool isEnabled;
  KeyAction actions[NUM_BUTTONS];
};

Layer layers[NUM_LAYERS];
const size_t CONFIG_DATA_SIZE = sizeof(Layer) * NUM_LAYERS;

// --- RawHID Buffer ---
// Per your request, we provide a static buffer for RawHID to use.
uint8_t rawhidData[RAWHID_RX_SIZE];

void setup() {
  Serial.begin(9600);
  while(!Serial);

  Serial.println("\n--- Macropad Event-Driven Sketch ---");

  // Initialize RawHID with the provided static buffer
  RawHID.begin(rawhidData, sizeof(rawhidData));
  
  // Set a timeout for readBytes, e.g., 2 seconds.
  // This prevents the Arduino from locking up if the PC stops sending data mid-stream.
  RawHID.setTimeout(2000); 

  loadDefaultKeymap(layers);
  printConfig();

  Serial.println("Ready for commands from PC.");
}

void loop() {
  if (RawHID.available() > 0) {
    int command = RawHID.read(); // Read the first byte

    if (command == CMD_PC_GET_CONFIG) {
      handleGetConfig();
    } else if (command == CMD_PC_SET_CONFIG) {
      handleSetConfig();
    }
  }
}

// Handles the 'Get Config' command from the PC
void handleGetConfig() {
  Serial.println("Received 'Get' command. Sending configuration packet-by-packet...");

  // We will not use one giant buffer. Instead, we send the data chunk by chunk.
  const uint8_t* dataPtr = (const uint8_t*)layers;
  size_t bytesRemaining = CONFIG_DATA_SIZE; // Should be 282
  
  // --- Handle the first packet specially ---
  // It must contain the 'C' command byte plus the first 63 bytes of data.
  uint8_t firstPacket[RAWHID_TX_SIZE];
  firstPacket[0] = CMD_ARDUINO_SEND_CONFIG;
  memcpy(&firstPacket[1], dataPtr, RAWHID_TX_SIZE - 1); // Copy first 63 bytes

  Serial.println("First packet:");
  for (int i = 0; i < RAWHID_TX_SIZE; i++) {
    Serial.print(firstPacket[i], HEX); Serial.print(" ");
  }

  RawHID.write(firstPacket, sizeof(firstPacket));
  RawHID.flush(); // Crucial: Flush after every single packet write.

  // Update our pointers
  dataPtr += (RAWHID_TX_SIZE - 1);
  bytesRemaining -= (RAWHID_TX_SIZE - 1);
  
  // A small courtesy delay for the host to process the first packet
  delay(5);

  // --- Loop and send the rest of the data in chunks ---
  while (bytesRemaining > 0) {
    // Determine the size of the next chunk (will be 64, 64, 64, ..., 27)
    size_t bytesToSend = (bytesRemaining > RAWHID_TX_SIZE) ? RAWHID_TX_SIZE : bytesRemaining;
  
    Serial.println("Data packet:");
    for (int i = 0; i < bytesToSend; i++) {
      Serial.print(dataPtr[i], HEX); Serial.print(" ");
    }

    // Send the raw data chunk. No command byte needed in these packets.
    RawHID.write(dataPtr, bytesToSend);
    RawHID.flush(); // CRUCIAL: Flush after EVERY packet.

    // Update pointers for the next iteration
    dataPtr += bytesToSend;
    bytesRemaining -= bytesToSend;

    // Courtesy delay
    delay(5);
  }

  Serial.println("All packets sent.");
}

// Handles the 'Set Config' command from the PC
void handleSetConfig() {
  Serial.println("Received 'Set' command. Awaiting configuration data...");

  uint8_t configBuffer[CONFIG_DATA_SIZE];

  // Use readBytes to reliably read the exact amount of data we expect.
  // This has a built-in timeout set in setup().
  size_t bytesRead = RawHID.readBytes(configBuffer, CONFIG_DATA_SIZE);

  uint8_t response[1];
  if (bytesRead == CONFIG_DATA_SIZE) {
    // Success! We received all the data.
    memcpy(layers, configBuffer, CONFIG_DATA_SIZE);
    
    response[0] = CMD_ARDUINO_ACK;
    RawHID.write(response, sizeof(response));
    RawHID.flush();
    
    Serial.println("New configuration received and applied successfully.");
    printConfig();
  } else {
    // Failure (timeout or not enough data)
    response[0] = CMD_ARDUINO_ERROR;
    RawHID.write(response, sizeof(response));
    RawHID.flush();
    
    Serial.print("Error receiving configuration. Expected ");
    Serial.print(CONFIG_DATA_SIZE);
    Serial.print(" bytes, but received ");
    Serial.println(bytesRead);
  }
}

// Helper function to print the current config
void printConfig() {
  Serial.println("\n--- CURRENT CONFIGURATION ---");
  for (int l = 0; l < NUM_LAYERS; l++) {
    Serial.print("Layer "); Serial.print(l);
    Serial.print(": '"); Serial.print(layers[l].name);
    Serial.print("', Enabled: "); Serial.println(layers[l].isEnabled);
  }
  Serial.println("---------------------------\n");
}

// --- Default Keymap Configuration ---
void loadDefaultKeymap(Layer* layers) {
  // --- LAYER 0: Functions ---
  const Layer layer0_data = {
    "Functions", // Layer Name
    true,    // isEnabled
    { // Key Actions
      {KEY_C, MOD_LEFT_CTRL, "Copy"}, {KEY_V, MOD_LEFT_CTRL, "Paste"}, {KEY_F15, 0, "F15"},
      {KEY_F16, 0, "F16"}, {KEY_F16, MOD_LEFT_SHIFT, "S+F16"}, {KEY_F18, 0, "F18"},
      {KEY_ESC, MOD_LEFT_CTRL | MOD_LEFT_SHIFT, "T.Mgr"}, 
      {KEY_X, MOD_LEFT_CTRL | MOD_LEFT_SHIFT | MOD_LEFT_ALT, "CSA+X"},
      {KEY_F21, 0, "F21"}
    }
  };
  // Now, assign the fully formed object. This is always valid.
  layers[0] = layer0_data;

  // --- LAYER 1: Shortcuts ---
  const Layer layer1_data = {
    "Shortcuts", // Layer Name
    true,    // isEnabled
    { // Key Actions
      {'c', MOD_LEFT_CTRL, "Copy"}, {'v', MOD_LEFT_CTRL, "Paste"}, {'x', MOD_LEFT_CTRL, "Cut"},
      {'z', MOD_LEFT_CTRL, "Undo"}, {'s', MOD_LEFT_CTRL, "Save"}, {'a', MOD_LEFT_CTRL, "Sel.A"},
      {KEY_DELETE, 0, "Del."},  {KEY_ENTER, 0, "Enter"},   {'p', MOD_LEFT_GUI, "Proj"}
    }
  };
  layers[1] = layer1_data;

  // --- LAYER 2: Media ---
  const Layer layer2_data = {
    "Media", // Layer Name
    true,    // isEnabled
    { // Key Actions
      {KEY_A, 0, "Mute"}, {KEY_B, 0, "Vol-"}, {KEY_C, 0, "Vol+"},
      {KEY_D, 0, "Prev"}, {KEY_E, 0, "Play"}, {KEY_F, 0, "Next"},
      {KEY_ESC, 0, "Esc"}, {KEY_TAB, 0, "Tab"}, {'l', MOD_LEFT_GUI, "Lock"}
    }
  };
  layers[2] = layer2_data;
}
