/*
 * Arduino_Firmware.ino
 * Copyright (C) 2023 - Present, Julien Lecomte - All Rights Reserved
 * Licensed under the MIT License. See the accompanying LICENSE file for terms.
 */

// This will fail to compile if you don't have an nRF52 board.
// In the future, we might have to open this up to other architectures.
#if !defined(ARDUINO_ARCH_NRF52) && !defined(ARDUINO_NRF52_ADAFRUIT)
#error "This firmware only supports boards with an NRF52 processor."
#endif

// The "standard" ArduinoBLE library seems to be incompatible with the Adafruit
// nRF52 Feather Express board. Thankfully, Adafruit maintains a library named
// Bluefruit, which we include here. The documentation is available at:
// https://learn.adafruit.com/bluefruit-nrf52-feather-learning-guide/bluefruit-nrf52-api
#include <bluefruit.h>

// Uncomment this to debug the firmware when the device is connected via USB.
// Do not forget to comment it out before flashing the final version, or the
// device will not work when it is not connected to a computer via USB...
// #define DEBUG

const BLEUuid SERVICE_UUID("0d389e0f-25dc-4070-9135-400b81e543ce");
const BLEUuid CHARACTERISTIC_UUID("2a0f87c9-7270-4c3e-aaa3-647961dfffa3");

// Bluetooth® Low Energy Calibrator Service
BLEService calibratorService(SERVICE_UUID);

// Bluetooth® Low Energy Flat Panel Switch Characteristic
BLECharacteristic calibratorCharacteristic(CHARACTERISTIC_UUID);

#define VBATPIN A6

// Voltage indicator LED pins
#define VBATLED1 9
#define VBATLED2 10
#define VBATLED3 11
#define VBATLED4 12

// Pin controlling the intensity of the flat pane
#define LED_CONTROL_PIN 13

const uint16_t MIN_BRIGHTNESS = 0;
const uint16_t MAX_BRIGHTNESS = 1023;

const uint32_t LED_TOKEN = 0x004c4544; // LED in ASCII :)

HardwarePWM *pwm = NULL;

void setup() {
#ifdef DEBUG
  Serial.begin(9600);
  while (!Serial) ;
#endif

  pinMode(VBATLED1, OUTPUT);
  pinMode(VBATLED2, OUTPUT);
  pinMode(VBATLED3, OUTPUT);
  pinMode(VBATLED4, OUTPUT);

  bool succeeded = configurePinPWM();

  if (!succeeded) {
    // Light up built-in LED to show that there was a problem...
    digitalWrite(LED_BUILTIN, HIGH);
    return;
  }

  // Initialize brightness (i.e., PWM duty cycle) to 0.
  setBrightness(MIN_BRIGHTNESS);

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
  calibratorCharacteristic.setFixedLen(2);
  calibratorCharacteristic.setWriteCallback(write_callback);
  calibratorCharacteristic.begin();
  // Set the initial value for the characteristic:
  calibratorCharacteristic.write16(MIN_BRIGHTNESS);

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

bool configurePinPWM() {
  bool succeeded = false;

  // Add pin to one of the available Hw PWM

  // First, use existing HWPWM modules (already owned by LED)
  for (int i = 0; i < HWPWM_MODULE_NUM; i++) {
    if (HwPWMx[i]->isOwner(LED_TOKEN) && HwPWMx[i]->addPin(LED_CONTROL_PIN)) {
      pwm = HwPWMx[i];
      succeeded = true;
      break;
    }
  }

  // If we could not add to existing owned PWM modules, try to add to a new PWM module.
  if (!succeeded) {
    for (int i = 0; i < HWPWM_MODULE_NUM; i++) {
      if (HwPWMx[i]->takeOwnership(LED_TOKEN) && HwPWMx[i]->addPin(LED_CONTROL_PIN)) {
        pwm = HwPWMx[i];
        succeeded = true;
        break;
      }
    }
  }

  if (succeeded) {
    pinMode(LED_CONTROL_PIN, OUTPUT);

    // Set PWM frequency:
    // - Base clock frequency = 8MHz (PWM_PRESCALER_PRESCALER_DIV_2)
    // - PWM counter max value = 1024
    // Frequency = 8MHz / 1024 = ~3.92kHz
    // We want the frequency to be high enough to not cause any flickering
    // when taking flats with a short exposure (particularly important with
    // a luminance filter where a 0.1 second exposure may be used due to the
    // sensitivity of modern day CMOS cameras) But we also don't want the
    // frequency to be too high either because the MOSFET capacitance and the
    // gate resistor will cause issues at low duty cycle values because the
    // gate voltage will not have enough time to rise to turn the MOSFET on,
    // so the LED strip will not turn on at all...

    // 1=16MHz, -> 2=8MHz <-, 4=4MHz, 8=2MHz, 16=1MHz, 32=500kHz, 64=250kHz, 128=125kHz
    pwm->setClockDiv(PWM_PRESCALER_PRESCALER_DIV_2);

    // Set the resolution to 10 bits since the max value is 1023 (2^10-1)
    // This calls pwm->setMaxValue(1023), thereby setting the PWM counter maximum value to 1023.
    pwm->setResolution(10);
  }

  return succeeded;
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
  // Reset the value for the characteristic. It does not invoke the write
  // callback, so we "manually" turn off the LED strip as well...
  calibratorCharacteristic.write16(MIN_BRIGHTNESS);
  setBrightness(MIN_BRIGHTNESS);

  if (Serial) {
    Serial.print("Disconnected, reason = 0x");
    Serial.println(reason, HEX);
    Serial.println("Advertising!");
  }
}

void write_callback(uint16_t conn_hdl, BLECharacteristic* chr, uint8_t* data, uint16_t len)
{
  if (Serial) {
    Serial.print("data[0] = ");
    Serial.println(data[0]);
    Serial.print("data[1] = ");
    Serial.println(data[1]);
  }

  uint16_t value = (data[0] << 8) + data[1];

  if (Serial) {
    Serial.print("Received value: ");
    Serial.println(value);
  }

  // Change brightness of LED strip accordingly...
  setBrightness(value);
}

void setBrightness(uint16_t value)
{
  pwm->writePin(LED_CONTROL_PIN, value);
}

void format_ble_addr(uint8_t* addr, char *s)
{
  for (int i = 0; i < BLE_GAP_ADDR_LEN; i++) {
    sprintf(&s[i*5], "0x%02X%s", addr[i], i < BLE_GAP_ADDR_LEN - 1 ? " " : "");
  }
}

void loop()
{
  // Code snippet from Adafruit website to measure the battery voltage
  float measuredvbat = analogRead(VBATPIN);
  measuredvbat *= 2;    // we divided by 2, so multiply back
  measuredvbat *= 3.6;  // Multiply by 3.6V, our reference voltage
  measuredvbat /= 1024; // convert to voltage

  // Update our battery voltage indicator LEDs:
  digitalWrite(VBATLED1, measuredvbat > 3.2 ? HIGH : LOW);
  digitalWrite(VBATLED2, measuredvbat > 3.4 ? HIGH : LOW);
  digitalWrite(VBATLED3, measuredvbat > 3.6 ? HIGH : LOW);
  digitalWrite(VBATLED4, measuredvbat > 3.8 ? HIGH : LOW);

  delay(1000);
}
