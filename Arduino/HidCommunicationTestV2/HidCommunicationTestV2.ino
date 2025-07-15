#include "HID-Project.h"

// --- Protocol Commands ---
const uint8_t CMD_PC_GET_CONFIG = 'G';
const uint8_t CMD_PC_SET_CONFIG = 'S';
const uint8_t CMD_PC_NEXT_PACKET = 'N';

const uint8_t CMD_ARDUINO_READY_TO_SEND = 'R';
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

// --- Default Keymap Configuration ---
inline void loadDefaultKeymap(Layer* layers) {
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

Layer layers[NUM_LAYERS];
const size_t CONFIG_DATA_SIZE = sizeof(Layer) * NUM_LAYERS;

// ==========================================================
// ==              STATE MACHINE VARIABLES                 ==
// ==========================================================
enum TransferState {
  STATE_IDLE,          // Waiting for a command from the PC
  STATE_SENDING_CONFIG // In the middle of sending the config, packet by packet
};

TransferState currentState = STATE_IDLE;
size_t bytesSent = 0;
unsigned long lastPacketRequestTime = 0;
// ==========================================================

// --- RawHID Variables ---
uint8_t rawhidData[255];

void setup() {
  Serial.begin(9600);
  while(!Serial);
  Serial.println("\n--- Arduino Macropad Initializing (State Machine Version) ---");

  RawHID.begin(rawhidData, sizeof(rawhidData));
  
  loadDefaultKeymap(layers);
  printConfig();

  Serial.println("Ready.");
}

void loop() {
  // The main loop now simply checks for incoming data and calls the state machine handler.
  // This loop runs very fast, constantly servicing the USB stack.
  if (RawHID.available() > 0) {
    uint8_t receiveBuffer[RAWHID_RX_SIZE];
    RawHID.readBytes(receiveBuffer, sizeof(receiveBuffer));
    handleHidPacket(receiveBuffer);
  }

  // We also check for timeouts within the state machine logic itself.
  checkTransferTimeout();
}

void handleHidPacket(const uint8_t* buffer) {
  uint8_t command = buffer[0]; // The command from the PC

  switch (currentState) {
    case STATE_IDLE:
      // While idle, we only care about 'Get' or 'Set' commands.
      if (command == CMD_PC_GET_CONFIG) {
        startGetConfigTransfer();
      } else if (command == CMD_PC_SET_CONFIG) {
        // The 'Set' logic can still be blocking as it's simpler.
        handleSetConfig(); 
      }
      break;

    case STATE_SENDING_CONFIG:
      // While sending, we only care about the 'Next Packet' command.
      if (command == CMD_PC_NEXT_PACKET) {
        sendNextConfigPacket();
      }
      break;
  }
}

void startGetConfigTransfer() {
  Serial.println("Received 'Get' command. Transitioning to SENDING_CONFIG state.");
  
  // Update state machine variables
  currentState = STATE_SENDING_CONFIG;
  bytesSent = 0;
  lastPacketRequestTime = millis(); // Start the timeout timer

  // Send the 'Ready' signal to the PC
  uint8_t readyPacket[RAWHID_TX_SIZE] = {0};
  readyPacket[0] = CMD_ARDUINO_READY_TO_SEND;
  RawHID.write(readyPacket, sizeof(readyPacket));
  RawHID.flush();

  delay(50); // Crucial delay for the PC to process the packet
}

void sendNextConfigPacket() {
  // The PC has requested the next chunk of data.
  Serial.print("Received 'Next' command. ");
  
  if (bytesSent >= CONFIG_DATA_SIZE) {
    // This case should not happen if PC logic is correct, but it's a good safeguard.
    Serial.println("PC asked for a packet, but transfer is already complete. Resetting.");
    currentState = STATE_IDLE;
    return;
  }

  lastPacketRequestTime = millis(); // Reset the timeout timer

  const uint8_t* dataPtr = (const uint8_t*)layers;
  
  // Determine how many bytes to send in this packet
  size_t bytesRemaining = CONFIG_DATA_SIZE - bytesSent;
  size_t bytesToSend = (bytesRemaining > RAWHID_TX_SIZE) ? RAWHID_TX_SIZE : bytesRemaining;

  // Send the chunk of data
  RawHID.write(dataPtr + bytesSent, bytesToSend);
  RawHID.flush();
  delay(50); // Crucial delay for the PC to process the packet
  
  bytesSent += bytesToSend;
  Serial.print("Sent ");
  Serial.print(bytesToSend);
  Serial.print(" bytes. Total sent: ");
  Serial.println(bytesSent);

  // Check if the transfer is now complete
  if (bytesSent >= CONFIG_DATA_SIZE) {
    Serial.println("All data sent. Transfer complete. Returning to IDLE state.");
    currentState = STATE_IDLE;
  }
}

void checkTransferTimeout() {
  // If we are in the middle of a transfer and the PC hasn't asked for a packet
  // for a while, assume the PC has disconnected and reset to idle.
  if (currentState == STATE_SENDING_CONFIG) {
    if (millis() - lastPacketRequestTime > 5000) { // 5-second timeout
      Serial.println("Timeout waiting for PC to request next packet. Aborting transfer.");
      currentState = STATE_IDLE;
    }
  }
}

// This function can remain the same. It is simple enough that blocking is okay.
void handleSetConfig() {
  Serial.println("Received 'Set Config' command.");
  uint8_t configBuffer[CONFIG_DATA_SIZE];
  size_t receivedBytes = 0;
  unsigned long startTime = millis();

  while(receivedBytes < CONFIG_DATA_SIZE && (millis() - startTime < 2000)) {
    if (RawHID.available() > 0) {
      int bytesToRead = RawHID.available();
      if (receivedBytes + bytesToRead > CONFIG_DATA_SIZE) {
        bytesToRead = CONFIG_DATA_SIZE - receivedBytes;
      }
      receivedBytes += RawHID.readBytes(&configBuffer[receivedBytes], bytesToRead);
    }
  }

  if (receivedBytes == CONFIG_DATA_SIZE) {
    memcpy(layers, configBuffer, CONFIG_DATA_SIZE);
    uint8_t ackMsg[1] = { CMD_ARDUINO_ACK };
    RawHID.write(ackMsg, sizeof(ackMsg));
    RawHID.flush();
    Serial.println("New config received and applied.");
    printConfig();
  } else {
    uint8_t errorMsg[1] = { CMD_ARDUINO_ERROR };
    RawHID.write(errorMsg, sizeof(errorMsg));
    RawHID.flush();
    Serial.print("Error receiving config. Expected ");
    Serial.print(CONFIG_DATA_SIZE);
    Serial.print(", got ");
    Serial.println(receivedBytes);
  }
}

// Helper functions (no changes)
void loadDefaultConfig() { /* ... your existing code ... */ }
void printConfig() { /* ... your existing code ... */ }


// #include "HID-Project.h"

// // --- Protocol Commands ---
// const uint8_t CMD_PC_GET_CONFIG = 'G';
// const uint8_t CMD_PC_SET_CONFIG = 'S';
// const uint8_t CMD_PC_NEXT_PACKET = 'N';

// const uint8_t CMD_ARDUINO_READY_TO_SEND = 'R';
// const uint8_t CMD_ARDUINO_ACK = 'A';
// const uint8_t CMD_ARDUINO_ERROR = 'E';

// // --- Constants ---
// const int NUM_BUTTONS = 9;
// const int NUM_LAYERS = 3;
// const int BUTTON_TEXT_LENGTH = 6;
// const int LAYER_TEXT_LENGTH = 12;

// // --- Data Structures ---
// struct __attribute__((packed)) KeyAction
// {
//    KeyboardKeycode key;
//    uint16_t modifier;
//    char text[BUTTON_TEXT_LENGTH];
// };
// struct __attribute__((packed)) Layer
// {
//   char name[LAYER_TEXT_LENGTH];
//   bool isEnabled;
//   KeyAction actions[NUM_BUTTONS];
// };


// // --- RawHID Variables ---
// uint8_t rawhidData[255];

// // --- Global Variables ---
// Layer layers[NUM_LAYERS];
// const size_t CONFIG_DATA_SIZE = sizeof(Layer) * NUM_LAYERS;

// void setup() {
//   Serial.begin(9600); // For debug messages
//   while(!Serial);

//   Serial.println("Arduino Macropad Initializing...");

//   loadDefaultKeymap(layers);

//   Serial.println("\nPrinting initial default configuration:");
//   printConfig();

//   RawHID.begin(rawhidData, sizeof(rawhidData));

//   Serial.print("Configuration size is: ");
//   Serial.println(CONFIG_DATA_SIZE);
//   Serial.println("Ready to communicate.");
// }

// void loop() {
//   if (RawHID.available() > 0) {
//     uint8_t receiveBuffer[RAWHID_RX_SIZE];
//     RawHID.readBytes(receiveBuffer, sizeof(receiveBuffer));

//     // First byte is the command
//     uint8_t command = receiveBuffer[0];

//     switch(command) {
//       case CMD_PC_GET_CONFIG:
//         Serial.println("Received 'Get Config' command from PC.");
//         handleGetConfig();
//         break;
      
//       case CMD_PC_SET_CONFIG:
//         Serial.println("Received 'Set Config' command from PC. Preparing to receive data.");
//         handleSetConfig();
//         break;

//       default:
//         Serial.print("Unknown command received: ");
//         Serial.println(command, HEX);
//         break;
//     }
//   }
// }

// void handleGetConfig() {
//   Serial.println("Received Get command. Signaling 'Ready'.");

//   // Step 1: Signal to the PC that we are ready to send data.
//   uint8_t readyPacket[RAWHID_TX_SIZE] = {0};
//   readyPacket[0] = CMD_ARDUINO_READY_TO_SEND;
//   RawHID.write(readyPacket, sizeof(readyPacket));
//   RawHID.flush();

//   // Step 2: Wait for the PC to request packets one by one.
//   const uint8_t* dataPtr = (const uint8_t*)layers;
//   size_t bytesSent = 0;
  
//   unsigned long timeoutStart = millis();

//   while (bytesSent < CONFIG_DATA_SIZE) {
//     // Wait for a 'Next Packet' command from the PC
//     if (RawHID.available() > 0) {
//       uint8_t receiveBuffer[RAWHID_RX_SIZE];
//       RawHID.readBytes(receiveBuffer, sizeof(receiveBuffer));

//       if (receiveBuffer[0] == CMD_PC_NEXT_PACKET) {
//         // PC is asking for the next chunk.
//         timeoutStart = millis(); // Reset timeout since we got a valid command

//         // Determine how many bytes to send
//         size_t bytesRemaining = CONFIG_DATA_SIZE - bytesSent;
//         size_t bytesToSend = bytesRemaining;
//         if (bytesToSend > RAWHID_TX_SIZE) {
//           bytesToSend = RAWHID_TX_SIZE;
//         }

//         Serial.println("Sending chunk:");
//         for (int i = 0; i < bytesToSend; i++)
//         {
//           Serial.print(dataPtr[i], DEC);
//           Serial.print(" ");
//         }
//         Serial.println();

//         // Send the next chunk
//         Serial.print("PC requested next packet. Sending ");
//         Serial.print(bytesToSend);
//         Serial.println(" bytes.");

//         RawHID.write(dataPtr + bytesSent, bytesToSend);
//         RawHID.flush();

//         bytesSent += bytesToSend;
//       }
//     } // end if RawHID.available

//     // Failsafe timeout in case the PC stops asking
//     if (millis() - timeoutStart > 5000) {
//       Serial.println("Timed out waiting for PC to request next packet. Aborting.");
//       return;
//     }
//   }
  
//   Serial.println("Finished sending all configuration data.");
// }

// // void handleGetConfig() {
// //   Serial.println("Sending config packet by packet...");

// //   // 1. Send the command byte FIRST in its own packet.
// //   // This clearly signals the start of a transfer.
// //   uint8_t commandPacket[RAWHID_TX_SIZE] = {0}; // Zero out the buffer
// //   commandPacket[0] = CMD_ARDUINO_SEND_CONFIG;
// //   RawHID.write(commandPacket, sizeof(commandPacket));
// //   RawHID.flush();

// //   // Give the host a moment to process the command before we blast data.
// //   delay(10); 

// //   // 2. Now, send the configuration data in chunks.
// //   const uint8_t* dataPtr = (const uint8_t*)layers;
// //   size_t bytesRemaining = CONFIG_DATA_SIZE; // Should be 282
  
// //   while (bytesRemaining > 0) {
// //     // Determine how many bytes to send in this packet
// //     size_t bytesToSend = bytesRemaining;
// //     if (bytesToSend > RAWHID_TX_SIZE) {
// //       bytesToSend = RAWHID_TX_SIZE;
// //     }

// //     Serial.println("Sending chunk:");
// //     for (int i = 0; i < bytesToSend; i++)
// //     {
// //       Serial.print(dataPtr[i], DEC);
// //       Serial.print(" ");
// //     }
// //     Serial.println();

// //     // Send the chunk
// //     RawHID.write(dataPtr, bytesToSend);
// //     RawHID.flush();

// //     // Advance our pointers and counters
// //     dataPtr += bytesToSend;
// //     bytesRemaining -= bytesToSend;

// //     // A small delay is crucial for flow control!
// //     // It gives the PC time to receive and process the packet.
// //     delay(5); 
// //   }
  
// //   Serial.println("Finished sending all configuration data.");
// // }

// void handleSetConfig() {
//   uint8_t configBuffer[CONFIG_DATA_SIZE];
//   size_t receivedBytes = 0;

//   // We need to read the entire configuration, which will arrive in multiple packets.
//   // We'll read from RawHID until we have the expected number of bytes.
  
//   // Set a timeout for receiving data (e.g., 2 seconds)
//   unsigned long startTime = millis();
//   while(receivedBytes < CONFIG_DATA_SIZE && (millis() - startTime < 2000)) {
//     if (RawHID.available() > 0) {
//       // Read available data into our buffer
//       int bytesToRead = RawHID.available();
//       // Make sure we don't overflow our buffer
//       if (receivedBytes + bytesToRead > CONFIG_DATA_SIZE) {
//         bytesToRead = CONFIG_DATA_SIZE - receivedBytes;
//       }
      
//       // We need to pass a pointer to the correct location in our configBuffer
//       receivedBytes += RawHID.readBytes(&configBuffer[receivedBytes], bytesToRead);
//     }
//   }

//   if (receivedBytes == CONFIG_DATA_SIZE) {
//     // We got all the data! Deserialize it.
//     memcpy(layers, configBuffer, CONFIG_DATA_SIZE);
    
//     // Send Acknowledge
//     uint8_t ackMsg[1] = { CMD_ARDUINO_ACK };
//     RawHID.write(ackMsg, sizeof(ackMsg));
    
//     Serial.println("New configuration received and applied successfully.");

//      // Print the newly received configuration to verify it was parsed correctly.
//     Serial.println("\nPrinting new configuration received from PC:");
//     printConfig();
//   } else {
//     // Timeout or wrong data size
//     uint8_t errorMsg[1] = { CMD_ARDUINO_ERROR };
//     RawHID.write(errorMsg, sizeof(errorMsg));
    
//     Serial.print("Error receiving configuration. Expected ");
//     Serial.print(CONFIG_DATA_SIZE);
//     Serial.print(" bytes, but received ");
//     Serial.println(receivedBytes);
//   }
// }

// // Helper function to print the current config to the Serial Monitor for debugging
// void printConfig() {
//   Serial.println("--- CURRENT CONFIGURATION ---");
//   for (int l = 0; l < NUM_LAYERS; l++) {
//     Serial.print("Layer "); Serial.print(l);
//     Serial.print(": '"); Serial.print(layers[l].name);
//     Serial.print("', Enabled: "); Serial.println(layers[l].isEnabled);
//     for (int b = 0; b < NUM_BUTTONS; b++) {
//       Serial.print("  Btn "); Serial.print(b);
//       Serial.print(": '"); Serial.print(layers[l].actions[b].text);
//       Serial.print("', Key: 0x"); Serial.print(layers[l].actions[b].key, HEX);
//       Serial.print(", Mod: 0x"); Serial.println(layers[l].actions[b].modifier, HEX);
//     }
//   }
//   Serial.println("---------------------------\n");
// }
