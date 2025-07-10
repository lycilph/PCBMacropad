#pragma once
#include <Arduino.h>
#include "HID-Project.h"

// --- Constants ---
const int NUM_BUTTONS = 9;
const int NUM_LAYERS = 3;
const int TEXT_LENGTH = 8; // For key text and layer names (7 characters + null terminator)

// --- Data Structures ---

// Represents the action for a single key
struct KeyAction {
  uint8_t key;
  uint16_t modifier; // This holds a bitmask of the modifiers (see ImprovedKeylayouts.h in the hid-project libray)
  char text[TEXT_LENGTH];
};

// Represents a full layer of 9 key actions AND a name
struct Layer {
  char name[TEXT_LENGTH];
  KeyAction actions[NUM_BUTTONS];
};

// --- Default Keymap Configuration ---
// UPDATED: Now includes a name for each layer.
inline void loadDefaultKeymap(Layer* layers) {
  // --- LAYER 0: Functions ---
  const Layer layer0_data = {
    "Funcs", // Layer Name
    { // Key Actions
      {KEY_F13, 0, "F13"}, {KEY_F14, 0, "F14"}, {KEY_F15, 0, "F15"},
      {KEY_F16, 0, "F16"}, {KEY_F17, 0, "F17"}, {KEY_F18, 0, "F18"},
      {KEY_ESC, MOD_LEFT_CTRL | MOD_LEFT_SHIFT, "TaskMgr"}, 
      {'x', MOD_LEFT_CTRL | MOD_LEFT_SHIFT | MOD_LEFT_ALT, "C+S+A+X"},
      {KEY_F21, 0, "F21"}
    }
  };
  // Now, assign the fully formed object. This is always valid.
  layers[0] = layer0_data;

  // --- LAYER 1: Shortcuts ---
  const Layer layer1_data = {
    "Shrtcts", // Layer Name
    { // Key Actions
      {'c', MOD_LEFT_CTRL, "Copy"}, {'v', MOD_LEFT_CTRL, "Paste"}, {'x', MOD_LEFT_CTRL, "Cut"},
      {'z', MOD_LEFT_CTRL, "Undo"}, {'s', MOD_LEFT_CTRL, "Save"}, {'a', MOD_LEFT_CTRL, "SelectA"},
      {KEY_DELETE, 0, "Delete"},  {KEY_ENTER, 0, "Enter"},   {'p', MOD_LEFT_GUI, "Proj"}
    }
  };
  layers[1] = layer1_data;

  // --- LAYER 2: Media ---
  const Layer layer2_data = {
    "Media", // Layer Name
    { // Key Actions
      {MEDIA_VOLUME_MUTE, 0, "Mute"}, {MEDIA_VOLUME_DOWN, 0, "Vol-"}, {MEDIA_VOLUME_UP, 0, "Vol+"},
      {MEDIA_PREVIOUS, 0, "Prev"}, {MEDIA_PLAY_PAUSE, 0, "Play"}, {MEDIA_NEXT, 0, "Next"},
      {KEY_ESC, 0, "Esc"}, {KEY_TAB, 0, "Tab"}, {'l', MOD_LEFT_GUI, "Lock"}
    }
  };
  layers[2] = layer2_data;
}