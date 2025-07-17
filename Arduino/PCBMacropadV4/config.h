#pragma once

#define ENABLE_SERIAL

#define ENABLE_FREERAM_CHECK

#ifdef ENABLE_FREERAM_CHECK
  inline int freeMemory() {
    extern int __heap_start, *__brkval;
    int v;
    return (int)&v - (__brkval == 0 ? (int)&__heap_start : (int)__brkval);
  }
#endif

// --- Debug Configuration ---
// Comment out this line for the final build to save ~1.5KB of flash memory
#define ENABLE_DEBUG

#ifdef ENABLE_DEBUG
  #define DEBUG_PRINT(...) Serial.print(__VA_ARGS__)
  #define DEBUG_PRINTLN(...) Serial.println(__VA_ARGS__)
#else
  // The empty definitions also need to be variadic
  #define DEBUG_PRINT(...)
  #define DEBUG_PRINTLN(...)
#endif