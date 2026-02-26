using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;
using TMPro;


public class visualBeat : MonoBehaviour
{
    //***OBSOLETO***
    
    //riceve da SuperCollider lo stato del TempoClock e cambia i colori sul sequencer

    [SerializeField] Material coloreBase;
    [SerializeField] Material coloreBeat;
    public OSCReceiver receiver;
    
    void Start()
    {   
        listen();
    }
    public void listen(){
        receiver.Bind("/beat", (OSCMessage message)=> {
            int beat = (int)message.Values[0].FloatValue;
            
            //cambia colore led
            foreach(Transform btn in gameObject.transform){
                if (int.Parse(btn.name) == beat){
                    btn.GetChild(2).GetComponent<MeshRenderer>().material = coloreBeat;
                }else btn.GetChild(2).GetComponent<MeshRenderer>().material = coloreBase;
            }

        });
    }


}
