#include "Arduino.h"
#include <HID-Project.h>

#include "ShortcutButton.h"

ShortcutButton::ShortcutButton(KeyboardKeycode _key, KeyboardMods _modifier, String _text)
{
  key = _key;
  modifier = _modifier;
  text = _text;
}

void ShortcutButton::Execute()
{
  // Testing for modifiers
  // modifier = MOD_LEFT_CTRL | MOD_LEFT_SHIFT;

  // if (modifier & MOD_LEFT_SHIFT)
  //   Serial.println("Shift pressed");

  // if (modifier & MOD_LEFT_CTRL)
  //   Serial.println("Ctrl pressed");

  // if (modifier & MOD_LEFT_ALT)
  //   Serial.println("Alt pressed");

  Keyboard.press(KEY_LEFT_CTRL);  // press and hold crtl
  Keyboard.press('c');            // press and hold c
  Keyboard.releaseAll();          // release both

  Keyboard.write(KEY_C | KEY_LEFT_CTRL);
}