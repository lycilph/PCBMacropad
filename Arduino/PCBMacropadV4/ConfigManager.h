#pragma once

#include "Keymap.h"

class ConfigManager {
public:
  ConfigManager();
  void begin();
  
  void loadConfig(Layer* layers);
  void saveConfig(Layer* layers);
  
  void factoryReset(Layer* layers);

  size_t serializeConfig(const Layer* layers, uint8_t* buffer, size_t bufferSize);
  bool deserializeConfig(Layer* layers, const uint8_t* buffer, size_t bufferSize);

private:
  const int EEPROM_VERSION = 2; // Change this if you update the struct layout
  const int EEPROM_ADDRESS = 0;
};
