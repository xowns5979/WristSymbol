/* Firmware of delimiter-MMTD project 
 * First updated : 19.7.2 
 */
#include <Servo.h>
Servo servo1; // height control
Servo servo2; // ERM motor rotating control

// LRA motor driver pin assignment
const int erm1 = 3;
const int erm2 = 5;
const int erm3 = 6;
const int erm4 = 9;
const int servo1Pin = 10;
const int servo2Pin = 11;

bool stringComplete = false;
char inData[200];
int dataIdx = 0;
int servo1Angle = 0;
int servo2Angle = 0;

bool ermOn[4] = {false, false, false, false};
bool ermburst = false;

//for motor test
int millistowait;
unsigned long recordedtime=99999999999999999999999999999;
unsigned long currenttime=0;
int currentMotorNum;

void setup() {
  pinMode (erm1, OUTPUT);
  pinMode (erm2, OUTPUT);
  pinMode (erm3, OUTPUT);
  pinMode (erm4, OUTPUT);

  digitalWrite(erm1, LOW);
  digitalWrite(erm2, LOW);
  digitalWrite(erm3, LOW);
  digitalWrite(erm4, LOW);

  servo1.write(85);
  servo2.write(5);


  // put your setup code here, to run once:
  servo1.attach(servo1Pin);
  servo2.attach(servo2Pin);
  Serial.begin(115200);
  while (! Serial);
  
  Serial.println("delimiter-MMTD project");
}
    
void loop() {
  loopSerial();
  loopMotorOnOff();
  currenttime = millis();
  if(currenttime - recordedtime >= 500)
  {
    if(ermburst)
    {
      ermOn[currentMotorNum] = false;
      ermburst = false;  
    }
  }
}

void serialEvent()
{
  while(Serial.available() && stringComplete == false)
  {
    char inChar = Serial.read();
    inData[dataIdx++] = inChar;

    if(inChar == '\n')
    {
      dataIdx = 0;
      stringComplete = true;
    }
  }
}

void loopSerial()
{
  if(stringComplete)
  {
    char line[1000];
    int lineIdx = 0;
    // Count command chars & init inData (error prone)
    while(inData[lineIdx] != '\n' && lineIdx < 100)
    {
      line[lineIdx] = inData[lineIdx];
      inData[lineIdx] = NULL;
      lineIdx++;
    }

    
    char c1 = line[0], c2 = line[1], c3 = line[2], c4 = line[3], c5 = line[4];
    int motorNum = 0;
    
    switch(c1)
    {
      case 'e': // ERM motor
        if(c2 == 'v')  // ERM motor vibrating
        {
          motorNum = (int)c3 - 49;
          if(0 <= motorNum && motorNum < 4)
            ermOn[motorNum] = true;
        }
        else if(c2 == 's')  // ERM motor stop
        {
          motorNum = (int)c3 - 49;
          if(0 <= motorNum && motorNum < 4)
            ermOn[motorNum] = false;
        }
        else if(c2 == 't')
        {
          motorNum = (int)c3 - 49;
          if(0 <= motorNum && motorNum < 4)
          {
            recordedtime = millis();
            currentMotorNum = motorNum;
            ermOn[motorNum] = true;
            ermburst = true;
          }
        }
        break;
      case 's': // height control motor
        servo1Angle = ((int)c2 - 48)*100 + ((int)c3 - 48)*10 + (int)c4 - 48;
        Serial.print("servo1Angle: ");
        Serial.println(servo1Angle);
        servoSlowMove(servo1,servo1Angle);
        break;
      case 'm': // motor rotating servo
        servo2Angle = ((int)c2 - 48)*100 + ((int)c3 - 48)*10 + (int)c4 - 48;
        Serial.print("servo2Angle: ");
        Serial.println(servo2Angle);
        servo2.write(servo2Angle);
        
        break;
      case 'z':
        turnOffAll();
        break;
      default:
        break;
      
    }
    stringComplete = false;
  }
}

void servoSlowMove (Servo servo, int servoAngle)
{
  int i;
  for(i=99;i<servoAngle;i++)
  {
    servo.write(i);
    delay(20);
  }
}

// Function: loopMotorOnOff
// Turn on LRA if true (166Hz full-powered)
void loopMotorOnOff ()
{
  ermOnOff();
}

void turnOffAll()
{
  int i;
  for(i=0;i<4;i++)
    ermOn[i] = false;
}

void ermOnOff()
{
  if(ermOn[0])
    digitalWrite(erm1, HIGH);
  else
    digitalWrite(erm1, LOW);

  if(ermOn[1])
    digitalWrite(erm2, HIGH);
  else
    digitalWrite(erm2, LOW);
    
  if(ermOn[2])
    digitalWrite(erm3, HIGH); 
  else
    digitalWrite(erm3, LOW);
    
  if(ermOn[3])
    digitalWrite(erm4, HIGH);
  else
    digitalWrite(erm4, LOW);
}
