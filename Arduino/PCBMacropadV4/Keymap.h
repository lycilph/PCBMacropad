#pragma once

#include "HID-Project.h"

// --- Constants ---
const int NUM_BUTTONS = 9;
const int NUM_LAYERS = 3;
const int BUTTON_TEXT_LENGTH = 6;
const int LAYER_TEXT_LENGTH = 12;

// --- Data Structures ---
struct __attribute__((packed)) KeyAction
{
   KeyboardKeycode key;
   uint16_t modifier;
   char text[BUTTON_TEXT_LENGTH];
};
struct __attribute__((packed)) Layer
{
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