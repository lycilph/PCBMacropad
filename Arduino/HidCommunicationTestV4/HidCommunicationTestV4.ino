#include <HID-Project.h>

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

// --- Protocol Commands ---
// These must match the C# application definitions.
const uint8_t CMD_PC_GET_CONFIG = 'G';
const uint8_t CMD_PC_SET_CONFIG = 'S';
const uint8_t CMD_PC_CONFIG_DATA = 'D';

const uint8_t CMD_ARDUINO_SEND_CONFIG = 'C';
const uint8_t CMD_ARDUINO_CONFIG_DATA = 'D';

const int RAW_HID_PAYLOAD_SIZE = 64;
const int PROTOCOL_HEADER_SIZE = 3; // 1 byte for command, 2 for total size

// --- RawHID Buffer ---
const int MAX_BUFFER_SIZE = 512;
uint8_t rawhidData[255];
uint8_t configDataBuffer[CONFIG_DATA_SIZE];
uint16_t totalDataSize = 0;
uint16_t bytesReceived = 0;

void setup() {
	Serial.begin(9600);
  while (!Serial);

  // Set the RawHID OUT report array.
  RawHID.begin(rawhidData, sizeof(rawhidData));

  loadDefaultKeymap(layers);
  printConfig();

  Serial.println("Arduino RawHID Two-Way Transceiver Ready.");
  Serial.print("RX size: "); Serial.print(RAWHID_RX_SIZE, DEC); Serial.print(" TX size: "); Serial.println(RAWHID_TX_SIZE, DEC);
  Serial.println("Waiting for commands from the host...");
}

void loop() {
  if (RawHID.available() > 0) {

    // Read the incoming report into our packet buffer
    uint8_t packetBuffer[RAWHID_RX_SIZE];
    RawHID.readBytes(packetBuffer, RAWHID_RX_SIZE);
    int command = packetBuffer[0];

    if (command == CMD_PC_GET_CONFIG) {
      handleGetConfig();
    } else if (command == CMD_PC_SET_CONFIG) {
      handleSetConfig(packetBuffer);
    }  else if (command == CMD_PC_CONFIG_DATA) {
      handleConfigData(packetBuffer);
    }
  }
}

void handleGetConfig() {
  const uint8_t* dataPtr = (const uint8_t*)layers;
  uint16_t bytesSent = 0;
  size_t bytesRemaining = CONFIG_DATA_SIZE; // Should be 282

  // --- Handle the first packet specially ---
  // It must contain the 'C' command byte, the size of the config plus the first 61 bytes of data.
  // --- Send START_RESPONSE Packet ---
  uint8_t packet[RAWHID_TX_SIZE] = {0}; // Initialize to all zeros
  packet[0] = CMD_ARDUINO_SEND_CONFIG;
  packet[1] = (uint8_t)(CONFIG_DATA_SIZE & 0xFF); // Total size (low byte)
  packet[2] = (uint8_t)((CONFIG_DATA_SIZE >> 8) & 0xFF); // Total size (high byte)
  
  int firstChunkSize = RAW_HID_PAYLOAD_SIZE - PROTOCOL_HEADER_SIZE;
  memcpy(&packet[3], dataPtr, firstChunkSize);
  RawHID.write(packet, RAW_HID_PAYLOAD_SIZE);
  bytesSent += firstChunkSize;

  Serial.print("Sending first packet: ");
  printPacket(packet);

  delay(5); // Crucial delay for the PC to process the packet

  // --- Send RESPONSE_DATA_PACKETs ---
  while (bytesSent < bytesRemaining) {
    memset(packet, 0, sizeof(packet));
    packet[0] = CMD_ARDUINO_CONFIG_DATA;

    int chunkSize = min(RAW_HID_PAYLOAD_SIZE - 1, CONFIG_DATA_SIZE - bytesSent);
    memcpy(&packet[1], &dataPtr[bytesSent], chunkSize);
    
    RawHID.write(packet, RAW_HID_PAYLOAD_SIZE);
    bytesSent += chunkSize;
    
    Serial.print("Sending data packet: ");
    printPacket(packet);

    delay(5); // Delay between each packet
  }
  
  Serial.println("Finished sending config to PC");
}

// A new transfer is starting!
void handleSetConfig(uint8_t *packet) {
  totalDataSize = packet[1] | (packet[2] << 8);

  if (totalDataSize > MAX_BUFFER_SIZE) {
    Serial.println("Error: Requested transfer size is too large.");
    totalDataSize = 0;
    return;
  }

  Serial.print("Received START command. Expecting ");
  Serial.print(totalDataSize);
  Serial.println(" bytes.");

  bytesReceived = 0;
  int dataLength = RAW_HID_PAYLOAD_SIZE - PROTOCOL_HEADER_SIZE;
  memcpy(&configDataBuffer[bytesReceived], &packet[3], dataLength);
  bytesReceived += dataLength;

  Serial.print("Received first packet: ");
  printPacket(packet);
}

void handleConfigData(uint8_t *packet) {
  int dataLength = RAW_HID_PAYLOAD_SIZE - 1; // Cmd
  memcpy(&configDataBuffer[bytesReceived], &packet[1], dataLength);
  bytesReceived += dataLength;

  Serial.print("Received data packet: ");
  printPacket(packet);

  if (bytesReceived >= totalDataSize) {
    Serial.println("\n--- PC->Arduino Transfer Complete! ---");

    memcpy(layers, configDataBuffer, CONFIG_DATA_SIZE);
    printConfig();
    
    bytesReceived = 0;
    totalDataSize = 0;
    Serial.println("\nWaiting for next command.");
  }
}

void printPacket(uint8_t* packet) {
  for (int i = 0; i < RAWHID_TX_SIZE; i++) {
    Serial.print(packet[i], HEX); Serial.print(" ");
  }
  Serial.println();
}

// Helper function to print the current config to the Serial Monitor for debugging
void printConfig() {
  Serial.println("--- CURRENT CONFIGURATION ---");
  for (int l = 0; l < NUM_LAYERS; l++) {
    Serial.print("Layer "); Serial.print(l);
    Serial.print(": '"); Serial.print(layers[l].name);
    Serial.print("', Enabled: "); Serial.println(layers[l].isEnabled);
    for (int b = 0; b < NUM_BUTTONS; b++) {
      Serial.print("  Btn "); Serial.print(b);
      Serial.print(": '"); Serial.print(layers[l].actions[b].text);
      Serial.print("', Key: 0x"); Serial.print(layers[l].actions[b].key, HEX);
      Serial.print(", Mod: 0x"); Serial.println(layers[l].actions[b].modifier, HEX);
    }
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