#ifndef ShortcutButton_h
#define ShortcutButton_h

#include "Arduino.h"
#include <HID-Project.h>

class ShortcutButton
{
public:
  ShortcutButton(KeyboardKeycode _key, KeyboardMods _modifier, String _text);
  void Execute();
private:
  KeyboardKeycode key;
  KeyboardMods modifier;
  String text;
};

#endif