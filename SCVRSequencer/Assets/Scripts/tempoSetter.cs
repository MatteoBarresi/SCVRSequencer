using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using extOSC;
public class tempoSetter : MonoBehaviour
{
    //prende il valore dello slider e lo passa a SuperCollider
    
    public void setTempo(){
        //scsend tipo (address, value)
        //GameManager.instance.SCsend("/tempo", gameObject.GetComponent<Slider>().value);
        var message = new OSCMessage("/tempo"); 
        message.AddValue(OSCValue.Float(gameObject.GetComponent<Slider>().value));
        GameManager.instance.transmitter.Send(message);
    }

}
