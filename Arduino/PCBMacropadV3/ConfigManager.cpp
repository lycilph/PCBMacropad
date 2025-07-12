#include <EEPROM.h>
#include "ConfigManager.h"

ConfigManager::ConfigManager() {}

void ConfigManager::begin() {
  // The EEPROM library is initialized automatically
}

void ConfigManager::loadConfig(Layer* layers) {
  // Check if a valid configuration is stored
  // We use a "magic number" or version at the start of the EEPROM
  if (EEPROM.read(EEPROM_ADDRESS) == EEPROM_VERSION) {
    Serial.println("Loading config from eeprom");
    // Valid config found, load it
    EEPROM.get(EEPROM_ADDRESS + 1, *layers);
  } else {
    Serial.println("Factory new config");
    // No valid config, load defaults and save them
    factoryReset(layers);
    saveConfig(layers);
  }
}

void ConfigManager::saveConfig(const Layer* layers) {
  Serial.println("Saving config");
  EEPROM.put(EEPROM_ADDRESS, EEPROM_VERSION);
  EEPROM.put(EEPROM_ADDRESS + 1, *layers);
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

void ConfigManager::dumpCurrentConfig(Layer* layers)
{
  // Create a buffer exactly the size of our configuration data
  const size_t configSize = sizeof(Layer) * NUM_LAYERS;
  uint8_t serialBuffer[configSize];

  // Serialize the current configuration into our buffer
  size_t bytesWritten = serializeConfig(layers, serialBuffer, configSize);

  if (bytesWritten > 0) {
    Serial.println("--- BEGIN CONFIG DUMP ---");
    // Print the byte array in HEX format so it's readable
    for (size_t i = 0; i < bytesWritten; i++) {
      if (serialBuffer[i] < 0x10) {
        Serial.print("0"); // Add leading zero for single-digit hex
      }
      Serial.print(serialBuffer[i], HEX);
      Serial.print(" ");
    }
    Serial.println();
    Serial.println("--- END CONFIG DUMP ---");
  } else {
    Serial.println("Error: Serialization failed.");
  }
}