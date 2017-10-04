# CocktailMixerCommunicator

A small API which can be used to send commands through a serial port.</br>
</br>
Im trying to build a Cocktailmixer using a Arduino UNO.</br> 
My aim is to control this from a computer. Not sure yet whether im going to set up a small website or use a Desktop-Application to order my drinks.</br>
</br>
Check out my Repository <a href="https://github.com/kahmannf/ArduinoCocktailMixerReceiver">ArduinoCocktailMixerReciever</a> for the code that runs on the Arduino.</br>
</br>
<h3>Basic idea:</h3>

I have a number of valves that can be opend by applying electricity to them.</br>
I have a row of bottles that are upside down and have two rubber tubes inside the cap:</br>
-one for airflow (end of the tube has to be above the highest poit of the bottle)</br>
-one connected the the valve</br>
Idea is that i can somehow order a drink with some kind of application. This application will communicate with the Arduino.</br>
The Arduino will then open the valves for a certain period of time.</br>

<h3>Basic functionality:</h3>

The Arduino recieves serial data. Each information package contains two bytes:</br>
-First byte specifies a output</br>
-Second byte is a status (0 or not 0)</br>
</br>
The Arduino will then switch the output off (on not 0) or on (on 0).</br>
</br>
The reversed logic here is intended, because of the electronics that control the valves:</br>
On a low signal, a relay will trigger a valve (for each putput a different of course).</br>
</br>
The program has to:</br>
-be able to take orders</br>
-be able to know which Cocktail/Longdrink/Recipe contains which beverages</br>
-know where (on which output) each beverage is connected</br>

I keep the data inside a file, because each configuration is specific to each CocktailMixer and because it is less administrative work (would have to set up a Database or somthing similar).
