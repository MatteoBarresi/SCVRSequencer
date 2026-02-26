using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using extOSC;

public class ReverbDryWet : MonoBehaviour
{
    //prende il valore dello slider e lo passa a SuperCollider

    //public OSCTransmitter transmitter;


    public void DryWet(){
        // Debug.Log(gameObject.GetComponent<Slider>().value);
        //scsend tipo(address, kr, value)
        GameManager.instance.SCsendValue("/reverb", "drywet", gameObject.GetComponent<Slider>().value);
        /*var message = new OSCMessage("/reverb"); 
        message.AddValue(OSCValue.String("drywet"));  
        message.AddValue(OSCValue.Float(gameObject.GetComponent<Slider>().value));
        transmitter.Send(message);*/
    }
}
