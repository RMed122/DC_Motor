#define COMMAND_LENGHT 100
#include <DueTimer.h>
char command[COMMAND_LENGHT];
int refType;
int slt;
char c;
int numel;

double degs, degsAnt=0;
double sp;
double T=0.05;
volatile long int encoderPos;
int interruptPinA = 24;
int interruptPinB = 22;
double PWMvalue;
int analog_pin = 2;
int dir_pin = 50; 
int enable_pin = 52;
volatile bool pulsesA;
volatile bool pulsesB;

//Waveforms Variables
double t;
double f = 0.5;
double tIniSig;
double vMin = -50.00;
double vMax = 50.00;
double offset;
double Amp = 220.00;
double ref;

//controller
int control;
double error;
double u;
bool enable_DC;
double errorAnt = 0;
double uAnt = 0;
double error2Ant = 0;
double Kp;
double Ki;
double Kd;




  
void setup() {
  Serial.begin(115200);
  while(!Serial) {}

  analogWriteResolution(12);
  pinMode(dir_pin, OUTPUT);
  pinMode(enable_pin, OUTPUT);
  pinMode(analog_pin, OUTPUT);
  digitalWrite(enable_pin, HIGH);
  pinMode(interruptPinA,INPUT_PULLUP);
  pinMode(interruptPinB,INPUT_PULLUP);
  randomSeed(analogRead(7));

  attachInterrupt(digitalPinToInterrupt(interruptPinA), interruptA, CHANGE);
  attachInterrupt(digitalPinToInterrupt(interruptPinB), interruptB, CHANGE);

  Timer1.setPeriod(T*1e6); 
  Timer1.attachInterrupt(Core);
  Timer1.start();
  
  
}

void loop(){ //mandatory to be in the code

}

void serialEvent()
  {
    while(Serial.available())
      {
      c=Serial.read();
      if(c==13) //carriage return
      {
        command[slt]=0;
        numel=slt-1;
        slt=0;
        analizeString();
     }
      else
     {
    command[slt]=c;
    slt++;
    if(slt==COMMAND_LENGHT) slt=0; 
  }
 }
}

void interruptA(){
  pulsesA=digitalRead(interruptPinA) == HIGH;
  if(pulsesA!=pulsesB) encoderPos++;
  else encoderPos--;
}


void interruptB(){
  pulsesB=digitalRead(interruptPinB) == HIGH;
  if(pulsesB==pulsesA) encoderPos++;
  else encoderPos--; 
}

void Core(){
  
  degs = double(encoderPos)*1.4285;
  sp = (degs - degsAnt)/T;
  estimateRef();
  degsAnt = degs;
  printFunction();
  controller();  
  DCmotor();

}

void analizeString() {
 //Engine Switch 
  switch(command[0]){
    case 'a':
      analogWrite(analog_pin, PWMvalue);
      Serial.println("BStarting motor");
      break;
    case 'b':
      analogWrite(analog_pin, 0);
      Serial.println("BStopping the motor");
      break;
    case 'c':
      digitalWrite(dir_pin, LOW); 
      Serial.println("BClockwise");
      break;
    case 'd':
      digitalWrite(dir_pin, HIGH);
      Serial.println("BAnticlockwise");
      break;
    case 'e':
      digitalWrite(enable_pin, LOW);
      Serial.println("Bpin disabled");
      break;    
    case 'f':
      digitalWrite(enable_pin, HIGH);
      Serial.println("Bpin enabled");
      break;
    case 'n':
      PWMvalue = atoi(&command[1]);
      analogWrite(analog_pin, PWMvalue);
      break;
    case 's':
      refType = atoi(&command[1]);//decide signal form
      break;
    case 'M':
       vMax = atof(&command[1]);//set vMax
       break;
    case 'N':
       vMin = atof(&command[1]);//set vMin
       break;
    case 'F':
       f = atof(&command[1]);//set Frequency
       break;  
    case 'm': //value of the manual signal
       refType = 4;
       ref = atof(&command[1]);
       break;
    case 'A':
       control = 1;
       Kp = 0.00888;
       Ki = 0.124;
       break;
    case 'B':
       control = 2;
       Kp = 0.00527;
       Ki = 0.0986;
       Kd = 0.0000199;
       break;
    case 'C':
       control = 3;
       Kp = 0.0449;
       Ki = 0.00358;    
       break;
    case 'D':
       control = 4;
       Kp = 0.06467;
       Ki = 0.04329;
       Kd = 0.003145;
       break;
    case 'p':
      Kp = atof(&command[1]);
      break;
    case 'o':
      Kd = atof(&command[1]);
      break;
    case 'i':
      Ki = atof(&command[1]);
      break;
               
  }
     
 
}
//signals
void estimateRef(){
  int rnd_dir;     
  switch(refType){  
    case 1: //step
      t = double(millis())/1000 - tIniSig;
      if (t>=1/f) {tIniSig = double(millis())/1000; t = 0;}
      if (t<(1/f)/2)         ref = vMin;
      else                   ref = vMax;
      break; 
    case 2: //ramp  
      t = double(millis())/1000 - tIniSig;
      if (t>=(1/f)) {tIniSig = double(millis())/1000; t = 0;}
      ref = ((vMax - vMin)/(1/f))*t + vMin; 
      break; 
    case 3: //sinusoid
      t = double(millis())/1000; 
      Amp = vMax + vMin;
      offset = Amp/2 + vMin;
      ref = offset * sin(2.0*PI*f*t);
      break;
    case 4: //Manual 
      break;
    case 5: //random
       t = double(millis())/1000 - tIniSig;
       if (t>=(1/f)/2) 
        {
          tIniSig = double(millis())/1000; 
          rnd_dir = random(0,2);
          digitalWrite(dir_pin, rnd_dir); 
          PWMvalue = random(400,4095);
          analogWrite(analog_pin, PWMvalue);
          if(rnd_dir == 1) {PWMvalue = -PWMvalue;}
          
        }
       break;
             
  
  }
}  

void controller() {
  switch(control){
   case 1: //PI controller speed
     error = ref - sp;
     u = 0.5*(2*Kp*error - 2*Kp*errorAnt + T*Ki*error + T*Ki*errorAnt + 2*uAnt);
     uAnt = u;
     errorAnt = error;
     if(ref != 0){enable_DC = true;}
     break;
   
   case 2: //PID controller speed
     error = ref - sp;
     u = (Kp + Ki*T*0.5 + Kd/T)*error + (-Kp + Ki*T*0.5 - 2*Kd/T)*errorAnt + (Kd/T)*error2Ant + uAnt;
     uAnt = u;
     errorAnt = error;
     error2Ant = errorAnt;
     if(ref != 0){enable_DC = true;}
     break;   
     
   case 3: //PI controller position
     error = ref - degs;
     u = (Kp + 0.5*T*Ki)*error + (0.5*Ki*T - Kp)*errorAnt + uAnt;
     //u = 0.5*(2*Kp*error - 2*Kp*errorAnt + Ki*T*error + Ki*T*errorAnt + 2*uAnt);
     uAnt = u;
     errorAnt = error;
     if(ref != 0){enable_DC = true;}
     break;

   case 4: //PID controller position
     error = ref - degs;
     u = (Kp + Ki*T*0.5 + Kd/T)*error + (-Kp + Ki*T*0.5 - 2*Kd/T)*errorAnt + (Kd/T)*error2Ant + uAnt;
     uAnt = u;
     errorAnt = error;
     error2Ant = errorAnt;
     if(ref != 0){enable_DC = true;}
     break;
   
  }

   if(u > 6) {u = 6;}
   if(u < -6){u = -6;}    
   
   
       
}

void DCmotor() {
  if(enable_DC){
      analogWrite(analog_pin, abs(u*682.5));
          if(u > 0){
            digitalWrite(dir_pin, LOW);
          }
          else
          {
            digitalWrite(dir_pin, HIGH);
          }
  }
}

void printFunction() {

  Serial.print(degs,2);
  Serial.print(" ");
  Serial.print(sp);
  Serial.print(" ");
  Serial.print(ref);
  Serial.print(" ");
  Serial.print(millis());
  Serial.print(" ");
  Serial.print(PWMvalue*0.0014652); //PWM voltage
  Serial.print(" ");
  Serial.print(u);//signal voltage
  Serial.print(" ");
  Serial.println(error);
   
}










 
