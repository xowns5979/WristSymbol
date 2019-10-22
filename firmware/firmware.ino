/* Firmware of delimiter-MMTD project 
 * First updated : 19.7.2 
 */

// LRA motor driver pin assignment
const int erm1 = 3;
const int erm2 = 5;
const int erm3 = 6;
const int erm4 = 9;

bool stringComplete = false;
char inData[200];
int dataIdx = 0;

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
    char c1 = line[0], c2 = line[1], c3 = line[2], c4 = line[3];
    int motorNum = 0;
    
    switch(c1)
    {
      case 'e': // ERM "A"
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
      case 'z':
        turnOffAll();
        break;
      default:
        break;
    }
    stringComplete = false;
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
