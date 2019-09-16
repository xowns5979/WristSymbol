/* Firmware of delimiter-MMTD project 
 * First updated : 19.7.2 
 */

// LRA motor driver pin assignment
const int ermA1 = 3;
const int ermA2 = 5;
const int ermA3 = 6;
const int ermA4 = 9;

const int ermB1 = 4;
const int ermB2 = 8;
const int ermB3 = 11;
const int ermB4 = 13;

/*
const int lra1_F = 2;
const int lra1_R = 4;
const int lra2_F = 7;
const int lra2_R = 8;
const int lra3_F = 10;
const int lra3_R = 11;
const int lra4_F = 12;
const int lra4_R = 13;
*/

bool stringComplete = false;
char inData[200];
int dataIdx = 0;

bool ermAOn[4] = {false, false, false, false};
bool ermBOn[4] = {false, false, false, false};
bool ermAburst = false;
bool ermBburst = false;

//bool lraOn[4] = {false, false, false, false};

/*
int lraFrequency = 166;  // Unit :  Hz
int lraTimePeriod = (int)(1000000.0 / lraFrequency); // Unit : us, microseconds
float lraAmplitude = 100.0; // Unit : %
*/

//int ermAmplitude = 255; // 0~255

//for motor test
int millistowait;
unsigned long recordedtime=99999999999999999999999999999;
unsigned long currenttime=0;
int currentMotorNum;

void setup() {
  pinMode (ermA1, OUTPUT);
  pinMode (ermA2, OUTPUT);
  pinMode (ermA3, OUTPUT);
  pinMode (ermA4, OUTPUT);

  pinMode (ermB1, OUTPUT);
  pinMode (ermB2, OUTPUT);
  pinMode (ermB3, OUTPUT);
  pinMode (ermB4, OUTPUT);

  digitalWrite(ermA1, LOW);
  digitalWrite(ermA2, LOW);
  digitalWrite(ermA3, LOW);
  digitalWrite(ermA4, LOW);

  digitalWrite(ermB1, LOW);
  digitalWrite(ermB2, LOW);
  digitalWrite(ermB3, LOW);
  digitalWrite(ermB4, LOW);
  /*
  pinMode (lra1_F, OUTPUT);
  pinMode (lra1_R, OUTPUT);
  pinMode (lra2_F, OUTPUT);
  pinMode (lra2_R, OUTPUT);
  pinMode (lra3_F, OUTPUT);
  pinMode (lra3_R, OUTPUT);
  pinMode (lra4_F, OUTPUT);
  pinMode (lra4_R, OUTPUT);
  */
  
  Serial.begin(115200);
  while (! Serial);
  
  //Serial.println("delimiter-MMTD project");

}

void loop() {
  loopSerial();
  loopMotorOnOff();
  currenttime = millis();
  /*
  Serial.print("currenttime : ");
  Serial.print(currenttime);
  Serial.print("recordedtime : ");
  Serial.println(recordedtime);
  */
  if(currenttime - recordedtime >= 500)
  {
    if(ermAburst)
    {
      ermAOn[currentMotorNum] = false;
      ermAburst = false;  
    }
    if(ermBburst)
    {
      ermBOn[currentMotorNum] = false;
      ermBburst = false;  
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

// Function: loopSerial
/*
 * Command(Serial input) convention
 * 1. LRA
 *  lf166 : set LRA frequency to 166Hz
 *  la050 : set LRA amplitude to 50%
 *  lv1 : vibrate LRA motor #1
 *  ls1 : stop LRA motor #1
 * 2. ERM
 *  ea200 : set ERM amplitude to 200  (argument of analogueWrite is 200)
 *  ev1 : vibrate ERM motor #1
 *  es1 : stop ERM motor #1
 */
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
      /*
      case 'l':
        if(c2 == 'f') // LRA frequency setting
        {
          lraFrequency = ((int)c3-48)*100+((int)c4-48)*10+((int)c5-48);
          lraTimePeriod = (int)(1000000.0 / lraFrequency); // Unit : us, microseconds
          
          Serial.print("lraFrequency : ");
          Serial.print(lraFrequency);
          Serial.print(", lraTimePeriod : ");
          Serial.println(lraTimePeriod);
          Serial.flush();
          
        }
        else if (c2 == 'a') // LRA amplitude setting
        {
          lraAmplitude = ((int)c3-48)*100+((int)c4-48)*10+((int)c5-48);
          
          Serial.print("lraAmplitude : ");
          Serial.println(lraAmplitude);
 
          Serial.print("First half, up, (int)((lraTimePeriod / 2)*(lraAmplitude/100)) : ");
          Serial.println((int)((lraTimePeriod / 2)*(lraAmplitude/100)));
          Serial.print("First half, down, (int)((lraTimePeriod / 2)*(1-(lraAmplitude/100))) : ");
          Serial.println((int)((lraTimePeriod / 2)*(1-(lraAmplitude/100))));
          Serial.print("Second half, down, (int)((lraTimePeriod / 2)*(lraAmplitude/100)) : ");
          Serial.println((int)((lraTimePeriod / 2)*(lraAmplitude/100)));
          Serial.print("Second half, up, (int)((lraTimePeriod / 2)*(1-(lraAmplitude/100))) : ");
          Serial.println((int)((lraTimePeriod / 2)*(1-(lraAmplitude/100))));
          Serial.flush();
          
          break;
        }
        else if (c2 =='v')  // LRA motor vibrating
        {
          //turnOffAll();
          motorNum = (int)c3 - 49;
          if(0 <= motorNum && motorNum < 4)
          {
            lraOn[motorNum] = true;
            
            
            Serial.print("LRA motor ");
            Serial.print(motorNum+1);
            Serial.println(" : ON");
            
            Serial.flush();
            
          }
        }
        else if (c2 =='s')  // LRA motor stop
        {
          motorNum = (int)c3 - 49;
          if(0 <= motorNum && motorNum < 4)
          {
            lraOn[motorNum] = false;
            
            Serial.print("LRA motor ");
            Serial.print(motorNum+1);
            Serial.println(" : OFF");
            Serial.flush();
            
          }
        }
        break;
      */
      case 'a': // ERM "A"
        /*
        if(c2 == 'a') //ERM amplitude setting
        {
          ermAmplitude = ((int)c3-48)*100+((int)c4-48)*10+((int)c5-48);
          
          Serial.print("ermAmplitude : ");
          Serial.println(ermAmplitude);
          Serial.flush();
          
        }
        */
        if(c2 == 'v')  // ERM motor vibrating
        {
          //turnOffAll();
          motorNum = (int)c3 - 49;
          if(0 <= motorNum && motorNum < 4)
          {
            ermAOn[motorNum] = true;
            /*
            Serial.print("ERM motor ");
            Serial.print(motorNum+1);
            Serial.println(" : ON");
            Serial.flush();
            */
          }
        }
        else if(c2 == 's')  // ERM motor stop
        {
          motorNum = (int)c3 - 49;
          if(0 <= motorNum && motorNum < 4)
          {
            ermAOn[motorNum] = false;
            /*
            Serial.print("ERM motor ");
            Serial.print(motorNum+1);
            Serial.println(" : OFF");
            Serial.flush();
            */
          }
        }
        else if(c2 == 't')
        {
          motorNum = (int)c3 - 49;
          if(0 <= motorNum && motorNum < 4)
          {
            recordedtime = millis();
            currentMotorNum = motorNum;
            ermAOn[motorNum] = true;
            ermAburst = true;
            /*
            Serial.print("ERM motor ");
            Serial.print(motorNum+1);
            Serial.println(" : ON");
            Serial.flush();
            */
          }
        }
        break;
      case 'b': // ERM "B"
        /*
        if(c2 == 'a') //ERM amplitude setting
        {
          ermAmplitude = ((int)c3-48)*100+((int)c4-48)*10+((int)c5-48);
          
          Serial.print("ermAmplitude : ");
          Serial.println(ermAmplitude);
          Serial.flush();
          
        }
        */
        if(c2 == 'v')  // ERM motor vibrating
        {
          //turnOffAll();
          motorNum = (int)c3 - 49;
          if(0 <= motorNum && motorNum < 4)
          {
            ermBOn[motorNum] = true;
            /*
            Serial.print("ERM motor ");
            Serial.print(motorNum+1);
            Serial.println(" : ON");
            Serial.flush();
            */
          }
        }
        else if(c2 == 's')  // ERM motor stop
        {
          motorNum = (int)c3 - 49;
          if(0 <= motorNum && motorNum < 4)
          {
            ermBOn[motorNum] = false;
            /*
            Serial.print("ERM motor ");
            Serial.print(motorNum+1);
            Serial.println(" : OFF");
            Serial.flush();
            */
          }
        }
        else if(c2 == 't')
        {
          motorNum = (int)c3 - 49;
          if(0 <= motorNum && motorNum < 4)
          {
            recordedtime = millis();
            currentMotorNum = motorNum;
            ermBOn[motorNum] = true;
            ermBburst = true;
            /*
            Serial.print("ERM motor ");
            Serial.print(motorNum+1);
            Serial.println(" : ON");
            Serial.flush();
            */
          }
        }
        break;
      case 'z':
        turnOffAll();
        /*
        Serial.println("Stop all");
        Serial.flush();
        */
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
  ermAOnOff();
  ermBOnOff();
}

void turnOffAll()
{
  int i;
  for(i=0;i<4;i++)
  {
    ermAOn[i] = false;
    ermBOn[i] = false;
  }
}

void ermAOnOff()
{
  digitalWrite(ermA1, LOW);
  digitalWrite(ermA2, LOW);
  digitalWrite(ermA3, LOW);
  digitalWrite(ermA4, LOW);
  if(ermAOn[0])
    digitalWrite(ermA1, HIGH);
  if(ermAOn[1])
    digitalWrite(ermA2, HIGH);
  if(ermAOn[2])
    digitalWrite(ermA3, HIGH); 
  if(ermAOn[3])
    digitalWrite(ermA4, HIGH);
}

void ermBOnOff()
{
  digitalWrite(ermB1, LOW);
  digitalWrite(ermB2, LOW);
  digitalWrite(ermB3, LOW);
  digitalWrite(ermB4, LOW);
  if(ermBOn[0])
    digitalWrite(ermB1, HIGH);
  if(ermBOn[1])
    digitalWrite(ermB2, HIGH);
  if(ermBOn[2])
    digitalWrite(ermB3, HIGH); 
  if(ermBOn[3])
    digitalWrite(ermB4, HIGH);
}
