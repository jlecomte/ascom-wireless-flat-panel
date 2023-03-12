/*
 * Arduino_Firmware.ino
 * Copyright (C) 2023 - Present, Julien Lecomte - All Rights Reserved
 * Licensed under the MIT License. See the accompanying LICENSE file for terms.
 */

// The "standard" ArduinoBLE library seems to be incompatible with the Adafruit
// nRF52 Feather Express board. Thankfully, Adafruit maintains a library named
// Bluefruit, which we include here. The documentation is available at:
// https://learn.adafruit.com/bluefruit-nrf52-feather-learning-guide/bluefruit-nrf52-api
#include <bluefruit.h>

// Uncomment this to debug the firmware when the device is connected via USB.
// Do not forget to comment it out before flashing the final version, or the
// device will not work when not connected to a computer via USB...
// #define DEBUG

const BLEUuid SERVICE_UUID("0d389e0f-25dc-4070-9135-400b81e543ce");
const BLEUuid CHARACTERISTIC_UUID("2a0f87c9-7270-4c3e-aaa3-647961dfffa3");

// Bluetooth® Low Energy Calibrator Service
BLEService calibratorService(SERVICE_UUID);

// Bluetooth® Low Energy Flat Panel Switch Characteristic
BLECharacteristic calibratorCharacteristic(CHARACTERISTIC_UUID);

const int controlPin = 8;

const uint8_t MIN_BRIGHTNESS = 0;
const uint8_t MAX_BRIGHTNESS = 255;

void setup() {
#ifdef DEBUG
  Serial.begin(9600);
  while (!Serial) ;
#endif

  pinMode(controlPin, OUTPUT);

  Bluefruit.begin();

  Bluefruit.setName("DarkSkyGeek Calibrator");

  // Set the connect/disconnect callback handlers
  Bluefruit.Periph.setConnectCallback(connect_callback);
  Bluefruit.Periph.setDisconnectCallback(disconnect_callback);

  // Configure and start the calibrator service
  calibratorService.begin();

  // Configure our characteristic
  // Note: A BLECharacteristic is automatically be added to the last BLEService
  // that had it's `.begin()` function called.
  calibratorCharacteristic.setProperties(CHR_PROPS_READ | CHR_PROPS_WRITE);
  calibratorCharacteristic.setPermission(SECMODE_OPEN, SECMODE_OPEN);
  calibratorCharacteristic.setFixedLen(1);
  calibratorCharacteristic.setWriteCallback(write_callback);
  calibratorCharacteristic.begin();
  // Set the initial value for the characeristic:
  calibratorCharacteristic.write8(MIN_BRIGHTNESS);

  // Advertise device name in secondary scan response packet (optional)
  // (there is no room for 'Name' in advertising packet)
  Bluefruit.ScanResponse.addName();

  // Advertise BLE Calibrator Service
  Bluefruit.Advertising.addService(calibratorService);
  Bluefruit.Advertising.restartOnDisconnect(true);
  Bluefruit.Advertising.setInterval(32, 244); // in unit of 0.625 ms
  Bluefruit.Advertising.setFastTimeout(30); // number of seconds in fast mode
  Bluefruit.Advertising.start(0); // 0 = Don't stop advertising after n seconds

  if (Serial) {
    uint8_t addr[BLE_GAP_ADDR_LEN];
    Bluefruit.getAddr(addr);
    char sz_addr[40];
    format_ble_addr(addr, sz_addr);
    Serial.print("BLE device initialized. Bluetooth address is: ");
    Serial.println(sz_addr);
  }

  // Blink built-in LED a few times to show that initialization succeeded
  for (int i = 0; i < 5; i++) {
    digitalWrite(LED_BUILTIN, HIGH);
    delay(300);
    digitalWrite(LED_BUILTIN, LOW);
    delay(300);
  }
}

void connect_callback(uint16_t conn_handle)
{
  if (Serial) {
    BLEConnection* connection = Bluefruit.Connection(conn_handle);

    // Print central device name
    char central_name[32] = { 0 };
    connection->getPeerName(central_name, sizeof(central_name));
    Serial.print("Connected to central device: ");
    Serial.print(central_name);

    // Print central device address
    Serial.print(". Bluetooth address is: ");
    uint8_t* central_addr = connection->getPeerAddr().addr;
    char sz_addr[40];
    format_ble_addr(central_addr, sz_addr);
    Serial.println(sz_addr);
  }
}

void disconnect_callback(uint16_t conn_handle, uint8_t reason)
{
  if (Serial) {
    Serial.print("Disconnected, reason = 0x");
    Serial.println(reason, HEX);
    Serial.println("Advertising!");
  }
}

void write_callback(uint16_t conn_hdl, BLECharacteristic* chr, uint8_t* data, uint16_t len)
{
  uint8_t value = data[0];

  if (Serial) {
    Serial.print("Received value: ");
    Serial.println(value);
  }

  // Change brightness of LED strip accordingly...
  analogWrite(controlPin, value);
}

void format_ble_addr(uint8_t* addr, char *s) {
  for (int i = 0; i < BLE_GAP_ADDR_LEN; i++) {
    sprintf(&s[i*5], "0x%02X%s", addr[i], i < BLE_GAP_ADDR_LEN - 1 ? " " : "");
  }
}

void loop() {}
