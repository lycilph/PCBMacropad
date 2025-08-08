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
    "Workflow", // Layer Name
    true,    // isEnabled
    { // Key Actions
      {KEY_F13, 0, "Open"}, 
      {KEY_F14, 0, "Place"}, 
      {KEY_F15, 0, "UI"},
      {KEY_F16, 0, "Tidal"}, 
      {KEY_F17, 0, "Notep"}, 
      {KEY_F18, 0, "Calc"},
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
      {KEY_C, MOD_LEFT_CTRL, "Copy"}, {KEY_V, MOD_LEFT_CTRL, "Paste"}, {KEY_X, MOD_LEFT_CTRL, "Cut"},
      {KEY_Z, MOD_LEFT_CTRL, "Undo"}, {KEY_Y, MOD_LEFT_CTRL, "Redo"}, {KEY_S, MOD_LEFT_SHIFT, "Save"},
      {KEY_DELETE, 0, "Del."},  {KEY_ENTER, 0, "Enter"},   {KEY_ESC, 0, "Esc"}
    }
  };
  layers[1] = layer1_data;

  // --- LAYER 2: Media ---
  const Layer layer2_data = {
    "Misc", // Layer Name
    true,    // isEnabled
    { // Key Actions
      {KEY_Q, 0, "Q"}, {KEY_W, 0, "W"}, {KEY_E, 0, "E"},
      {KEY_A, 0, "A"}, {KEY_S, 0, "S"}, {KEY_D, 0, "D"},
      {KEY_Z, 0, "Z"}, {KEY_X, 0, "X"}, {KEY_C, 0, "C"}
    }
  };
  layers[2] = layer2_data;
}