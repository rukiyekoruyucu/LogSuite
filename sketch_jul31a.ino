const int motorPin = 7;

enum MotorState {
  IDLE,
  RUNNING1,
  PAUSE,
  RUNNING2,
};

MotorState state = IDLE;
unsigned long stateStartTime = 0;

void setup() {
  pinMode(motorPin, OUTPUT);
  digitalWrite(motorPin, LOW);
  Serial.begin(9600);
}

void loop() {
  if (Serial.available()) {
    char received = Serial.read();

    if (received == '0') {
      state = RUNNING1;                 // Her 0 geldiğinde döngü baştan başlar
      stateStartTime = millis();
      digitalWrite(motorPin, HIGH);
    }

    if (received == '1') {
      digitalWrite(motorPin, LOW);      // Acil durdurma
      state = IDLE;
    }
  }

  unsigned long currentTime = millis();

  switch (state) {
    case RUNNING1:
      if (currentTime - stateStartTime >= 1500) {
        digitalWrite(motorPin, LOW);
        state = PAUSE;
        stateStartTime = currentTime;
      }
      break;

    case PAUSE:
      if (currentTime - stateStartTime >= 4000) {
        digitalWrite(motorPin, HIGH);
        state = RUNNING2;
        stateStartTime = currentTime;
      }
      break;

    case RUNNING2:
      if (currentTime - stateStartTime >= 1500) {
        digitalWrite(motorPin, LOW);
        state = IDLE; // Döngü tamamlandı, tekrar 0 bekle
      }
      break;

    case IDLE:
    default:
      // Beklemede
      break;
  }
}