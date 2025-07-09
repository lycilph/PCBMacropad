#include <HID-Project.h>
#include <Button.h>

Button button1(0); // Connect your button between pin 0 and GND
Button button2(1);
Button button3(2);

uint8_t rawhidData[255];

// Define the maximum size of our reassembly buffer.
// It should be larger than the largest message you expect.
const int MAX_BUFFER_SIZE = 512;

// Define the size of the payload we receive in each RawHID report.
// For a Full-Speed device like the Pro Micro, this is 64 bytes.
const int RAW_HID_PAYLOAD_SIZE = 64;

// --- Protocol Definitions ---
// These must match the C# application definitions.
const uint8_t CMD_START_TRANSFER = 'S';       // PC -> Arduino: Start of a large data send
const uint8_t CMD_DATA_PACKET = 'D';          // PC -> Arduino: Subsequent data chunk
const uint8_t CMD_REQUEST_DATA = 'R';         // PC -> Arduino: Request for Arduino to send data back
const uint8_t CMD_START_RESPONSE = 'A';       // Arduino -> PC: 'A' for Answer/Acknowledge start
const uint8_t CMD_RESPONSE_DATA_PACKET = 'P'; // Arduino -> PC: 'P' for Payload packet

const int PROTOCOL_HEADER_SIZE = 3; // 1 byte for command, 2 for total size
const int PROTOCOL_PAYLOAD_SIZE = RAW_HID_PAYLOAD_SIZE - PROTOCOL_HEADER_SIZE; // 64 - 3 = 61

// Enum to manage our receiving state
enum ReceiveState {
  IDLE,
  RECEIVING
};

ReceiveState currentState = IDLE;
uint8_t largeBuffer[MAX_BUFFER_SIZE];
uint16_t totalDataSize = 0;
uint16_t bytesReceived = 0;

void setup() {
	Serial.begin(9600);
  while (!Serial);

  button1.begin();
  button2.begin();
  button3.begin();

  // Sends a clean report to the host. This is important on any Arduino type.
  Keyboard.begin();

  // Set the RawHID OUT report array.
  // Feature reports are also (parallel) possible, see the other example for this.
  RawHID.begin(rawhidData, sizeof(rawhidData));

  Serial.println("Arduino RawHID Two-Way Transceiver Ready.");
  Serial.println("Waiting for commands from the host...");
}

void loop() {
  if (button1.pressed())
  {
    Serial.println("Button 1 pressed");
    Keyboard.write('1');
  }
 
  if (button2.pressed())
  {
    Serial.println("Button 2 pressed");
    Keyboard.write('2');
  }
 
  if (button3.pressed())
  {
    Serial.println("Button 3 pressed");
    Keyboard.write('3');
  }

  // Check if the host has sent any data
  if (RawHID.available() > 0) {
    uint8_t packetBuffer[RAW_HID_PAYLOAD_SIZE];
    
    // Read the incoming report into our packet buffer
    RawHID.readBytes(packetBuffer, RAW_HID_PAYLOAD_SIZE);
    
    // Check for a data request command, which can happen in any state
    if (packetBuffer[0] == CMD_REQUEST_DATA) {
      Serial.println("Received a data request from PC. Preparing to send 400 bytes...");
      sendLargeDataToPC();
      return; // Handled the command, so we can exit the loop iteration
    }

    // Process other packets based on our current state
    switch (currentState) {
      case IDLE:
        if (packetBuffer[0] == CMD_START_TRANSFER) {
          // A new transfer is starting!
          totalDataSize = packetBuffer[1] | (packetBuffer[2] << 8);

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
          memcpy(&largeBuffer[bytesReceived], &packetBuffer[3], dataLength);
          bytesReceived += dataLength;
          
          currentState = RECEIVING;
        }
        break;

      case RECEIVING:
        if (packetBuffer[0] == CMD_DATA_PACKET) {
          int dataLength = RAW_HID_PAYLOAD_SIZE - 1; // Cmd
          memcpy(&largeBuffer[bytesReceived], &packetBuffer[1], dataLength);
          bytesReceived += dataLength;
        }
        
        if (bytesReceived >= totalDataSize) {
          Serial.println("\n--- PC->Arduino Transfer Complete! ---");
          processReceivedData();
          
          currentState = IDLE;
          bytesReceived = 0;
          totalDataSize = 0;
          Serial.println("\nState reset to IDLE. Waiting for next command.");
        }
        break;
    }
  }
}

// *** NEW FUNCTION: Sends 400 bytes from Arduino to PC ***
void sendLargeDataToPC() {
  uint8_t dataToSend[400];
  // Fill the buffer with sample data to send back.
  // For this example, it's a descending sequence: 255, 254, ...
  for (int i = 0; i < 400; i++) {
    dataToSend[i] = 255 - (i % 256);
  }

  uint16_t bytesSent = 0;

  // --- Send START_RESPONSE Packet ---
  uint8_t startPacket[RAW_HID_PAYLOAD_SIZE] = {0}; // Initialize to all zeros
  startPacket[0] = CMD_START_RESPONSE;
  startPacket[1] = (uint8_t)(400 & 0xFF); // Total size (low byte)
  startPacket[2] = (uint8_t)((400 >> 8) & 0xFF); // Total size (high byte)
  
  int firstChunkSize = RAW_HID_PAYLOAD_SIZE - PROTOCOL_HEADER_SIZE;
  memcpy(&startPacket[3], &dataToSend[0], firstChunkSize);
  RawHID.write(startPacket, RAW_HID_PAYLOAD_SIZE);
  bytesSent += firstChunkSize;

  delay(5); // Crucial delay for the PC to process the packet

  // --- Send RESPONSE_DATA_PACKETs ---
  while (bytesSent < 400) {
    uint8_t dataPacket[RAW_HID_PAYLOAD_SIZE] = {0};
    dataPacket[0] = CMD_RESPONSE_DATA_PACKET;

    int chunkSize = min(RAW_HID_PAYLOAD_SIZE - 1, 400 - bytesSent);
    memcpy(&dataPacket[1], &dataToSend[bytesSent], chunkSize);
    
    RawHID.write(dataPacket, RAW_HID_PAYLOAD_SIZE);
    bytesSent += chunkSize;
    
    delay(5); // Delay between each packet
  }
  
  Serial.println("Finished sending 400 bytes to PC.");
}

void processReceivedData() {
  Serial.print("Successfully received ");
  Serial.print(bytesReceived);
  Serial.println(" bytes from PC.");
  Serial.println("Printing first 20 bytes of the received message:");
  
  for (int i = 0; i < 20; i++) {
    Serial.print("0x");
    if (largeBuffer[i] < 0x10) Serial.print("0");
    Serial.print(largeBuffer[i], HEX);
    Serial.print(" ");
  }
  Serial.println();
}