# (WIP) SCVRSequencer
SCVRSequencer is a university VR project made with Unity. 
It is a virtual ambient in which the player can sequence the available synthesizers. 
The value of the parameters of said synthesizers change in real time with the position of the player's hands. 
Supercollider is used as the audio engine, and the [extOsc](https://github.com/Iam1337/extOSC/tree/master) tool is used to exchange messages with Unity. 
The position of the sound source is different for each synth and can be moved by the player while reproducing the song: Unity sends the coordinates of a source (compared to the player) to Supercollider, which manages the transformation to ambisonics format. The final decoding is stereo, so it's alright to use headphones. 
The project works via Meta Quest Link. 

## Features
+ 16 step sequencer (no limit of measures)
+ 2 octaves keyboard
+ 9 synths
  + 3 polyphonic instruments (bass, pads, piano)
  + 1 "only controllers" instrument
  + 5 audio files (drums + granular synth instrument), so the user can change the source for those
+ 3d audio mode ('A' button)
+ tempo change

## Dependencies
[Supercollider](https://github.com/supercollider/supercollider): you have to manually start the server by evaluating the file unity_synthdefs.scd (ctrl+enter anywhere in the script). The output device could be the VR headset (e.g. Meta Quest 2) or any other stereo output (you may have to set it manually in your OS settings, before starting the server). 
[ATK for Supercollider](https://github.com/ambisonictoolkit/atk-sc3) and [sc-3 plugins](https://github.com/supercollider/sc3-plugins). 

## TODO
+ saving the session
+ saving/loading midi files
+ recording: now you would have to start it manually from Supercollider or from a DAW
