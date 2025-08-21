#pragma once

#include <Arduino.h>
#include "HID-Project.h"

#include "Keymap.h"
#include "ConfigManager.h"

const uint8_t CMD_PC_GET_CONFIG = 'G';
const uint8_t CMD_PC_SET_CONFIG = 'S';
const uint8_t CMD_PC_CONFIG_DATA = 'D';
const uint8_t CMD_PC_RESET_CONFIG = 'R';

const uint8_t CMD_ARDUINO_SEND_CONFIG = 'C';
const uint8_t CMD_ARDUINO_CONFIG_DATA = 'D';

const int RAW_HID_PAYLOAD_SIZE = 64;
const int PROTOCOL_HEADER_SIZE = 3; // 1 byte for command, 2 for total size

const int CONFIG_DATA_SIZE = sizeof(Layer) * NUM_LAYERS;
const int MAX_BUFFER_SIZE = 512;

class CommunicationManager {
public:
  CommunicationManager(Layer* layerData, ConfigManager *cfgMgr);

  void begin();
  void update();
private:
  void handleGetConfig();
  void handleSetConfig();
  void handleConfigData();
  void handleResetConfig();

  void resetArduino();

  Layer* layers;
  ConfigManager* configManager;

  uint16_t totalDataSize = 0;
  uint16_t bytesReceived = 0;

  uint8_t rawhidData[150];
  uint8_t packetBuffer[RAWHID_RX_SIZE];
  uint8_t configDataBuffer[CONFIG_DATA_SIZE];
};