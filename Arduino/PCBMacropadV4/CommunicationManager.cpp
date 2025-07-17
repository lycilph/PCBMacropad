#include "CommunicationManager.h"
#include "config.h"

CommunicationManager::CommunicationManager(Layer* layerData)
  : layers(layerData) {}

void CommunicationManager::begin() {
  RawHID.begin(rawhidData, sizeof(rawhidData));
}

void CommunicationManager::update() {
  if (RawHID.available() > 0) {
    #ifdef ENABLE_FREERAM_CHECK
      Serial.print(F("Free SRAM: "));
      Serial.println(freeMemory());
    #endif

    // Read the incoming report into our packet buffer
    memset(packetBuffer, 0, sizeof(packetBuffer));
    RawHID.readBytes(packetBuffer, RAWHID_RX_SIZE);
    int command = packetBuffer[0];

    if (command == CMD_PC_GET_CONFIG) {
      handleGetConfig();
    } else if (command == CMD_PC_SET_CONFIG) {
      handleSetConfig();
    }  else if (command == CMD_PC_CONFIG_DATA) {
      handleConfigData();
    }
    // Add a command to reset the macropad
    // Should call the factoryReset in the config manager class (and save the reset config to eeprom)

    #ifdef ENABLE_FREERAM_CHECK
      Serial.print(F("Free SRAM: "));
      Serial.println(freeMemory());
    #endif
  }
}

void CommunicationManager::handleGetConfig() {
  const uint8_t* dataPtr = (const uint8_t*)layers;
  uint16_t bytesSent = 0;
  size_t bytesRemaining = CONFIG_DATA_SIZE; // Should be 282

  // --- Send START_RESPONSE Packet ---
  memset(packetBuffer, 0, sizeof(packetBuffer)); // Initialize to all zeros
  packetBuffer[0] = CMD_ARDUINO_SEND_CONFIG; // Command byte
  packetBuffer[1] = (uint8_t)(CONFIG_DATA_SIZE & 0xFF); // Total size (low byte)
  packetBuffer[2] = (uint8_t)((CONFIG_DATA_SIZE >> 8) & 0xFF); // Total size (high byte)
  
  int firstChunkSize = RAW_HID_PAYLOAD_SIZE - PROTOCOL_HEADER_SIZE;
  memcpy(&packetBuffer[3], dataPtr, firstChunkSize);
  RawHID.write(packetBuffer, RAW_HID_PAYLOAD_SIZE);
  bytesSent += firstChunkSize;

  delay(5); // Crucial delay for the PC to process the packet

  // --- Send RESPONSE_DATA_PACKETs ---
  while (bytesSent < bytesRemaining) {
    memset(packetBuffer, 0, sizeof(packetBuffer)); // Initialize next data packet to all zeros
    packetBuffer[0] = CMD_ARDUINO_CONFIG_DATA; // Command byte set to config data

    int chunkSize = min(RAW_HID_PAYLOAD_SIZE - 1, CONFIG_DATA_SIZE - bytesSent);
    memcpy(&packetBuffer[1], &dataPtr[bytesSent], chunkSize);
    
    RawHID.write(packetBuffer, RAW_HID_PAYLOAD_SIZE);
    bytesSent += chunkSize;

    delay(5); // Crucial delay for the PC to process the packet
  }
}

void CommunicationManager::handleSetConfig() {
  // Allocate buffer for the incoming configuration here (see https://cplusplus.com/reference/cstdlib/malloc/)
  // Check that there is enough free ram before and after....
}

void CommunicationManager::handleConfigData() {
  // When done free the allocated buffer (see https://cplusplus.com/reference/cstdlib/free/)
  // Check that there is enough free ram before and after....
}