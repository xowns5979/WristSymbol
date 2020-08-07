/* Firmware of delimiter-MMTD project 
 * First updated : 19.7.2 
 */

// LRA motor driver pin assignment

// 팔을 앞쪽으로
const int erm1 = 3; // 1
const int erm2 = 5; // 2
const int erm3 = 6; // 3
const int erm4 = 9; // 4

/*
// 팔을 몸쪽으로
const int erm2 = 3; // 1
const int erm4 = 5; // 2
const int erm1 = 6; // 3
const int erm3 = 9; // 4
*/
bool stringComplete = false;
char inData[200];
int dataIdx = 0;

bool ermOn[4] = {false, false, false, false};
int intensity[4] = {255, 255, 255, 255};
bool ermburst = false;


//for motor test
int millistowait;
unsigned long recordedtime=99999999999999;
unsigned long currenttime=0;
int currentMotorNum;

void setup() {
  pinMode (erm1, OUTPUT);
  pinMode (erm2, OUTPUT);
  pinMode (erm3, OUTPUT);
  pinMode (erm4, OUTPUT);

  
  analogWrite(erm1, 0);
  analogWrite(erm2, 0);
  analogWrite(erm3, 0);
  analogWrite(erm4, 0);

  //servo1.write(85);
  //servo2.write(5);

  // put your setup code here, to run once:
  //servo1.attach(servo1Pin);
  //servo2.attach(servo2Pin);
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

void loopSerial()
{

  if(Serial.available()>0)
  {
    String inString = Serial.readStringUntil('\n');
    char c1 = inString.charAt(0);
    char c2 = inString.charAt(1);
    char c3 = inString.charAt(2);
    char c4 = inString.charAt(3);
    char c5 = inString.charAt(4);

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
      case 'i':
        int tactor = (int)c2-49;
        intensity[tactor] = ((int)c3 - 48)*100 + ((int)c4 - 48)*10 + (int)c5 - 48;
        break;
      case 'z':
        turnOffAll();
        break;
      default:
        break;
      
    }
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
    analogWrite(erm1, intensity[0]);
  else
    analogWrite(erm1, 0);

  if(ermOn[1])
    analogWrite(erm2, intensity[1]);
  else
    analogWrite(erm2, 0);
    
  if(ermOn[2])
    analogWrite(erm3, intensity[2]); 
  else
    analogWrite(erm3, 0);
    
  if(ermOn[3])
    analogWrite(erm4, intensity[3]);
  else
    analogWrite(erm4, 0);
}
