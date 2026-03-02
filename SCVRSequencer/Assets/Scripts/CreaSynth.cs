using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreaSynth : MonoBehaviour
{
    
    void Awake()
    {
        string panDesc = "posizione della sorgente sonora in stereo";
        string ampDesc = "volume";
        string rateDesc = "velocità di riproduzione";
        string bitcrushDesc = "Usare con cautela!!!";
        string sustainDesc ="sustain time dell'inviluppo";
        string levelDesc = "volume durante il sustain";
        string relDesc = "release time";

        new Synth("/kick", new Dictionary<string, string>() {
            {"amp", ampDesc},
            {"rate", rateDesc},
            {"bitcrush", bitcrushDesc},
            //{"pan", panDesc},
            }, true, true, false, true
        );
        
        new Synth("/hat", new Dictionary<string, string>() {
            {"amp", ampDesc},
            {"rate", rateDesc},
            {"bitcrush", bitcrushDesc},
           // {"pan", panDesc},
            }, true, true, false, true
        );
        
        new Synth("/snare", new Dictionary<string, string>() {
            {"amp", ampDesc},
            {"rate", rateDesc},
            {"bitcrush", bitcrushDesc},
            //{"pan", panDesc},
            }, true, true, false, true
        );
        
        new Synth("/clap", new Dictionary<string, string>() {
            {"amp", ampDesc},
            {"rate", rateDesc},
            {"bitcrush", bitcrushDesc},
            //{"pan", panDesc},
            }, true, true, false, true
        );

        new Synth("/voice", new Dictionary<string, string>(){
            {"trigFreq", "ogni quanto viene generato un grano"},
            {"overlap", "durata singolo grano"},
            {"rate", rateDesc+ " singolo grano"},
            {"rFreq", "frequenza modulatore di rate"},
            {"rAmp", "ampiezza modulatore di rate"},
            {"pos", "posizione puntatore nel buffer"},
            {"diffusion", "fattore di scala del panning"},
           // {"panFreq", "velocità oscillatore panning"},
            {"amp", ampDesc + " del singolo grano"},
            }, false, true, false, true
        );

        new Synth("/pads", new Dictionary<string,string>(){
            {"detune", "distanza tra le frequenze degli oscillatori"},
            {"cutoff", "frequenza di taglio del filtro passa basso"},
            {"sustain", sustainDesc},
            {"level", levelDesc},
            {"rel", relDesc},
            {"amp", ampDesc},
            {"pan", panDesc},
            }, true, true, true, false
        );
        new Synth("/bass",  new Dictionary<string,string>(){ 
            {"width", "duty ratio dell'onda quadra"},
            {"sweepDur", "durata dello sweep del filtro passa basso"},
            {"sustain", sustainDesc},
            {"level", levelDesc},
            {"rel", relDesc},
            {"amp", ampDesc},
            //{"pan", panDesc},
            }, true, true, true, false
        );

        new Synth("/theremin", new Dictionary<string, string>(){
            {"carFreq", "frequenza di base"},
            {"modAmp", "ampiezza oscillatore che fa FM"},
            {"amp", ampDesc},
            }, false, true, false, false
        );
        
        new Synth("/piano", new Dictionary<string,string>(), false, false, true, false);
    }
}
