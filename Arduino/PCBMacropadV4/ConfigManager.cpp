#include <EEPROM.h>
#include <Wire.h>

#include "ConfigManager.h"
#include "config.h"

ConfigManager::ConfigManager() {}

void ConfigManager::begin() {
  // The EEPROM library is initialized automatically
}

void ConfigManager::loadConfig(Layer* layers) {
  int storedVersion = 0;
  EEPROM.get(EEPROM_ADDRESS, storedVersion);

  if (storedVersion == EEPROM_VERSION) {
    DEBUG_PRINTLN(F("Valid EEPROM config found. Loading via deserialization..."));

    const size_t dataSize = sizeof(Layer) * NUM_LAYERS;
    uint8_t buffer[dataSize]; // Create a temporary buffer

    // Read the raw bytes from EEPROM into our buffer
    EEPROM.get(EEPROM_ADDRESS + sizeof(EEPROM_VERSION), *buffer);
    
    // Use our function to parse the buffer and populate the layers struct
    if (!deserializeConfig(layers, buffer, dataSize)) {
      // This should ideally never happen if version matches
      DEBUG_PRINTLN(F("ERROR: Deserialization failed! Performing factory reset."));
      factoryReset(layers);
      saveConfig(layers);
    }
  } else {
    DEBUG_PRINTLN(F("Invalid version. Performing factory reset"));
    factoryReset(layers);
    saveConfig(layers);
  }

}

void ConfigManager::saveConfig(Layer* layers) {
  DEBUG_PRINTLN(F("Saving config using serialization..."));

  EEPROM.put(EEPROM_ADDRESS, EEPROM_VERSION); // Simpler and should work for a basic type like int.
  
  // --- Serialize and Save Layer Data ---
  const size_t dataSize = sizeof(Layer) * NUM_LAYERS;
  uint8_t buffer[dataSize]; // Create a temporary buffer on the stack
  serializeConfig(layers, buffer, dataSize);
  EEPROM.put(EEPROM_ADDRESS+ sizeof(EEPROM_VERSION), buffer);

  DEBUG_PRINTLN(F("Saving config done."));
}

void ConfigManager::factoryReset(Layer* layers) {
  loadDefaultKeymap(layers);
}

size_t ConfigManager::serializeConfig(const Layer* layers, uint8_t* buffer, size_t bufferSize) {
  const size_t dataSize = sizeof(Layer) * NUM_LAYERS;

  // Safety check to prevent buffer overflow
  if (bufferSize < dataSize) {
    return 0; // Return 0 to indicate failure
  }

  // Copy the entire layers array into the buffer
  memcpy(buffer, layers, dataSize);

  return dataSize;
}

bool ConfigManager::deserializeConfig(Layer* layers, const uint8_t* buffer, size_t bufferSize) {
  const size_t expectedDataSize = sizeof(Layer) * NUM_LAYERS;

  // Safety check to ensure we have the correct amount of data
  if (bufferSize != expectedDataSize) {
    return false; // Indicate failure
  }

  // Copy the data from the buffer into the layers array
  memcpy(layers, buffer, expectedDataSize);

  return true;
}