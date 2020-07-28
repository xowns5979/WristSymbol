// ERM
const int erm1 = 3; // 1
const int erm2 = 5; // 2
const int erm3 = 6; // 3
const int erm4 = 9; // 4

bool ermOn[4] = {false, false, false, false};
bool ermBurst[4] = {false, false, false, false};
unsigned long lraBurstStartTime[4] = {0, 0, 0, 0};

int ermBumpRunMS[4] = {100, 100, 100, 100}; // bump 진동 시간(ms)
int ermBumpStopMS[4] = {0, 0, 0, 0};    // bump 쉬는 시간(ms)
unsigned long ermRunStartTime[4] = {0, 0, 0, 0};  // erm 틀기 시작한 시간

int intensity[4] = {255, 255, 255, 255};


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

  
  Serial.begin(115200);
  while (! Serial);
  
  Serial.println("delimiter-MMTD project");
}
    
void loop() {
  loopSerial();
  loopMotorOnOff();

  /*
  currenttime = millis();
  if(currenttime - recordedtime >= 500)
  {
    if(ermburst)
    {
      ermOn[currentMotorNum] = false;
      ermburst = false;  
    }
  }
  */
  
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
    char c6 = inString.charAt(5);

    if(c2 == 'v')
    {
      int motorNum = (int)c1 - 49;
      if(0 <= motorNum && motorNum < 4)
      {
        ermOn[motorNum] = true;
        ermRunStartTime[motorNum] = millis();
            
      }  
    }
    else if (c2 == 's')
    {
      int motorNum = (int)c1 - 49;
      if(0 <= motorNum && motorNum < 4)
      {
        ermOn[motorNum] = false;
        ermTurnOff(motorNum);
      }  
    }
    else if (c2 == 't')
    {
      int motorNum = (int)c1 - 49;
      if(0 <= motorNum && motorNum < 4)
      {
        ermOn[motorNum] = true;
        ermBurst[motorNum] = true;
        ermRunStartTime[motorNum] = millis();  


      }
    }
    else if (c2 == 'b' && c3 == 'v')
    {
      int motorNum = (int)c1 - 49;
      int runMS = ((int)c4-48)*100+((int)c5-48)*10+((int)c6-48);
      if(0 <= motorNum && motorNum < 4)
      {
        ermBumpRunMS[motorNum] = runMS;
        
        Serial.print("ERM motor ");
        Serial.print(motorNum+1);
        Serial.print(" BumpRunMS: ");
        Serial.println(runMS);
        Serial.flush();
        
      }
    }
    else if (c2 == 'b' && c3 == 's')
    {
      int motorNum = (int)c1 - 49;
      int stopMS = ((int)c4-48)*100+((int)c5-48)*10+((int)c6-48);
      if(0 <= motorNum && motorNum < 4)
      {
        ermBumpStopMS[motorNum] = stopMS;
        
        Serial.print("ERM motor ");
        Serial.print(motorNum+1);
        Serial.print(" BumpStopMS: ");
        Serial.println(stopMS);
        Serial.flush();
        
      }
    }
    
  }
}

// Function: loopMotorOnOff
// Turn on LRA if true (166Hz full-powered)
void loopMotorOnOff ()
{
  int i;
  for(i=0;i<4;i++)
  {
    if(ermBurst[i])
    {
      unsigned long passedTime = millis() - ermRunStartTime[i];

      /*
      Serial.print("millis():  ");
      Serial.print(millis());
      Serial.print(", ermRunStartTime[i]: ");
      Serial.print(ermRunStartTime[i]);
      Serial.print(", passedTime: ");
      Serial.println(passedTime);
      */
      if(passedTime > 500)
      {       
        ermTurnOff(i);
        ermOn[i] = false;
        ermBurst[i] = false;
      }  
    }
    if(ermOn[i])
    {
      unsigned long overDrivingTime = 10;
      
      unsigned long passedTime = ((millis() - ermRunStartTime[i]) %(ermBumpRunMS[i]+ ermBumpStopMS[i]));
      Serial.print("passedTime: ");
      Serial.print(passedTime);


       if(0 <= passedTime && passedTime < overDrivingTime)
        {
          ermTurnOn(i, 255);
          Serial.println(", OverDrivingTime ");
        }
        else if(overDrivingTime <= passedTime && passedTime < ermBumpRunMS[i])
        {
          ermTurnOn(i, 100);  
          Serial.println(", On Time ");
        }
        else
        {
          ermTurnOff(i);
          Serial.println(", Off Time ");
        }

        /*
      if(i == 2)
      {
        if(0 <= passedTime && passedTime < overDrivingTime)
        {
          ermTurnOn(i, 255);
          Serial.println(", OverDrivingTime ");
        }
        else if(overDrivingTime <= passedTime && passedTime < ermBumpRunMS[i])
        {
          ermTurnOn(i, 200);  
          Serial.println(", On Time ");
        }
        else
        {
          ermTurnOff(i);
          Serial.println(", Off Time ");
        }
      }
      else if (i == 1)
      {
        if(0 <= passedTime && passedTime < ermBumpRunMS[i])
        {
          ermTurnOn(i, 200);  
          Serial.println(", On Time ");
        }
        else
        {
          ermTurnOff(i);
          Serial.println(", Off Time ");
        }
      }
      */
      
      /*
      else
      {
        lraStop(i);
        Serial.println(" - BumpStop");    
      }
      */
        
    }  
  }

  
}

void turnOffAll()
{
  int i;
  for(i=0;i<4;i++)
    ermOn[i] = false;
}

void ermTurnOn(int motorNum, int intensity)
{
  if(motorNum == 0)
    analogWrite(erm1, intensity);
  else if (motorNum == 1)
    analogWrite(erm2, intensity);
  else if (motorNum == 2)
    analogWrite(erm3, intensity);
  else if (motorNum == 3)
    analogWrite(erm4, intensity);
}

void ermTurnOff(int motorNum)
{
  if(motorNum == 0)
    analogWrite(erm1, 0);
  else if (motorNum == 1)
    analogWrite(erm2, 0);
  else if (motorNum == 2)
    analogWrite(erm3, 0);
  else if (motorNum == 3)
    analogWrite(erm4, 0);
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
