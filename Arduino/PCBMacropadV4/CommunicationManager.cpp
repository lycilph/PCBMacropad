#include "CommunicationManager.h"
#include "config.h"

CommunicationManager::CommunicationManager(Layer* layerData, ConfigManager *cfgMgr)
  : layers(layerData), configManager(cfgMgr) {}

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

  DEBUG_PRINTLN("First packet sent");

  delay(5); // Crucial delay for the PC to process the packet

  // --- Send RESPONSE_DATA_PACKETs ---
  while (bytesSent < bytesRemaining) {
    memset(packetBuffer, 0, sizeof(packetBuffer)); // Initialize next data packet to all zeros
    packetBuffer[0] = CMD_ARDUINO_CONFIG_DATA; // Command byte set to config data

    int chunkSize = min(RAW_HID_PAYLOAD_SIZE - 1, CONFIG_DATA_SIZE - bytesSent);
    memcpy(&packetBuffer[1], &dataPtr[bytesSent], chunkSize);
    
    RawHID.write(packetBuffer, RAW_HID_PAYLOAD_SIZE);
    bytesSent += chunkSize;

    DEBUG_PRINTLN("Data packet sent");

    delay(5); // Crucial delay for the PC to process the packet
  }
}

void CommunicationManager::handleSetConfig() {
  bytesReceived = 0;
  totalDataSize = packetBuffer[1] | (packetBuffer[2] << 8);

  if (totalDataSize > MAX_BUFFER_SIZE) {
    DEBUG_PRINTLN("Error: Requested transfer size is too large.");
    totalDataSize = 0;
    return;
  }

  DEBUG_PRINT("Received START command. Expecting ");
  DEBUG_PRINT(totalDataSize);
  DEBUG_PRINTLN(" bytes.");

  int dataLength = RAW_HID_PAYLOAD_SIZE - PROTOCOL_HEADER_SIZE;
  memcpy(&configDataBuffer[bytesReceived], &packetBuffer[3], dataLength);
  bytesReceived += dataLength;
}

void CommunicationManager::handleConfigData() {
  int dataLength = RAW_HID_PAYLOAD_SIZE - 1; // Cmd
  memcpy(&configDataBuffer[bytesReceived], &packetBuffer[1], dataLength);
  bytesReceived += dataLength;

  DEBUG_PRINTLN("Received data packet");

  if (bytesReceived >= totalDataSize) {
    DEBUG_PRINTLN("\n--- PC->Arduino Transfer Complete! ---");

    memcpy(layers, configDataBuffer, CONFIG_DATA_SIZE);
    configManager->saveConfig(layers);

    bytesReceived = 0;
    totalDataSize = 0;
    DEBUG_PRINTLN("\nWaiting for next command.");
  }
}