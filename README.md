# CocktailMixerCommunicator

A small API which can be used to send commands through a serial port.

Im trying to build a Cocktailmixer using a Arduino UNO. 
My aim is to control this from a computer. Not sure yet whether im going to set up a small website
or use a Desktop-Application to order my drinks.

Check out my Repository <a href="https://github.com/kahmannf/ArduinoCocktailMixerReceiver">ArduinoCocktailMixerReciever</a> for the code that runs on the Arduino.

<h3>Basic idea:</h3>

I have a number of valve that can be opend by applying electricity to them.
I have a row of bottles that are upside down and have two rubber tubes inside the cap:
-one for airflow (end of the tube has to be above the highest poit of the bottle)
-one connected the the valve
Idea is that i can somehow order a drink with some kind of application. This application will communicate with the Arduino.
The Arduino will then open the valves for a certain period of time.

<h3>Basic functionality:</h3>

The Arduino recieves serial data. Each information package contains two bytes:
-First byte specifies a output
-Second byte is a status (0 or not 0)

The Arduino will then switch the output off (on not 0) or on (on 0).

The reversed logic here is intended, because of the electronics that control the valves:
On a low signal, a relay will trigger a valve (for each putput a different of course).

The program has to:
-be able to take orders
-be able to know which Cocktail/Longdrink/Recipe contains which beverages
-know where (on which output) each beverage is connected

I keep the data inside a file, because each configuration is specific to each CocktailMixer and because it is less administrative work (would have to set up a Database or somthing similar).
