using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObserverProgrammato : MonoBehaviour
{
    //associato a PlayAll

    bool programmato = false;

    void Start(){
        gameObject.SetActive(false);
    }
    public void checkProgrammato(){
        //controlla che ci sia almeno un tasto premuto in quelli che hanno un sequencer
        programmato = false;
     
        foreach(Synth synth in Synth.synthList.Values){
            if(synth.sequence){
                //Debug.Log(synth.symbol);
                foreach(int value in synth.sequencer[UIGeneral.getBattuta()]){
                    if(value == 1){
                        programmato = true;
                        break;
                    }
                }
            }
        }
        if(programmato) 
            gameObject.SetActive(true);
        else 
            gameObject.SetActive(false);
        //Debug.Log("programmato " + programmato);
    }    
}
