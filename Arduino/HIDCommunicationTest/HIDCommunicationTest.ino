#include "HID-Project.h"

// Define the maximum size of our reassembly buffer.
// It should be larger than the largest message you expect.
const int MAX_BUFFER_SIZE = 512;

// Define the size of the payload we receive in each RawHID report.
// For a Full-Speed device like the Pro Micro, this is 64 bytes.
const int RAW_HID_PAYLOAD_SIZE = 64;

// --- Protocol Definitions ---
// These must match the C# application definitions.
const uint8_t CMD_START_TRANSFER = 'S'; // 'S' for Start
const uint8_t CMD_DATA_PACKET = 'D';    // 'D' for Data
const int PROTOCOL_HEADER_SIZE = 3; // 1 byte for command, 2 for total size
const int PROTOCOL_PAYLOAD_SIZE = RAW_HID_PAYLOAD_SIZE - PROTOCOL_HEADER_SIZE; // 64 - 3 = 61

// Enum to manage our receiving state
enum ReceiveState {
  IDLE,
  RECEIVING
};

// Global variables for the state machine
ReceiveState currentState = IDLE;
uint8_t largeBuffer[MAX_BUFFER_SIZE];
uint16_t totalDataSize = 0;
uint16_t bytesReceived = 0;


void setup() {
  Serial.begin(115200);
  while (!Serial); // Wait for serial port to open

  // Start RawHID. This enables receiving generic reports.
  RawHID.begin(largeBuffer, RAW_HID_PAYLOAD_SIZE);
  
  Serial.println("Arduino RawHID Receiver Ready.");
  Serial.println("Waiting for a START command from the host...");
}

void loop() {
  // Check if the host has sent any data
  if (RawHID.available() > 0) {
    uint8_t packetBuffer[RAW_HID_PAYLOAD_SIZE];
    
    // Read the incoming report into our packet buffer
    RawHID.read(packetBuffer);
    
    // Process the packet based on our current state
    switch (currentState) {
      case IDLE:
        // In IDLE state, we only care about START commands
        if (packetBuffer[0] == CMD_START_TRANSFER) {
          // A new transfer is starting!
          // The total size is sent as a 16-bit integer (little-endian)
          totalDataSize = packetBuffer[1] | (packetBuffer[2] << 8);

          if (totalDataSize > MAX_BUFFER_SIZE) {
            Serial.println("Error: Requested transfer size is too large.");
            // Do not proceed with this transfer. Stay in IDLE.
            totalDataSize = 0;
            return;
          }

          Serial.print("Received START command. Expecting ");
          Serial.print(totalDataSize);
          Serial.println(" bytes.");

          // Reset our counter
          bytesReceived = 0;
          
          // Copy the first chunk of data from this START packet
          int dataLength = RAW_HID_PAYLOAD_SIZE - PROTOCOL_HEADER_SIZE;
          memcpy(&largeBuffer[bytesReceived], &packetBuffer[PROTOCOL_HEADER_SIZE], dataLength);
          bytesReceived += dataLength;
          
          // Change state to start accepting DATA packets
          currentState = RECEIVING;
        }
        break;

      case RECEIVING:
        // In RECEIVING state, we only care about DATA commands
        if (packetBuffer[0] == CMD_DATA_PACKET) {
          // This is a subsequent data packet
          int dataLength = RAW_HID_PAYLOAD_SIZE - 1; // 1 byte for command
          memcpy(&largeBuffer[bytesReceived], &packetBuffer[1], dataLength);
          bytesReceived += dataLength;
          
          // Optional: Print progress
          // Serial.print("Received data chunk. Total bytes: ");
          // Serial.println(bytesReceived);
        }
        
        // Check if we have received all the expected data
        if (bytesReceived >= totalDataSize) {
          Serial.println("\n--- Transfer Complete! ---");
          processReceivedData();
          
          // Reset for the next transfer
          currentState = IDLE;
          bytesReceived = 0;
          totalDataSize = 0;
          Serial.println("\nState reset to IDLE. Waiting for next transfer.");
        }
        break;
    }
  }
}

// This function is called when the 400-byte message is fully assembled.
void processReceivedData() {
  Serial.print("Successfully received ");
  Serial.print(bytesReceived); // Should be 400 or slightly more due to chunking
  Serial.println(" bytes.");
  Serial.println("Printing first 20 bytes of the message:");
  
  for (int i = 0; i < 20; i++) {
    Serial.print("0x");
    if (largeBuffer[i] < 0x10) Serial.print("0");
    Serial.print(largeBuffer[i], HEX);
    Serial.print(" ");
  }
  Serial.println();
}