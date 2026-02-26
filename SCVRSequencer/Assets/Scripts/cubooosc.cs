using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;
public class cubooosc : MonoBehaviour
{

    //script per testare messaggi OSC

    public OSCTransmitter transmitter;
    public OSCReceiver receiver;
    [SerializeField] GameObject tastiera;
    //public GameObject tastiera;
    // Start is called before the first frame update
    void Start()
    {
        //listen();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void listen(){
        receiver.Bind("/comunica", (OSCMessage message)=> Debug.Log(message));
    }

    public void interazione(){
        //var message = new OSCMessage("/ext");
        //message.AddValue(OSCValue.Array(OSCValue.Int(1), OSCValue.String("asd"), OSCValue.Int(1),OSCValue.Int(0),OSCValue.Int(0),OSCValue.Int(0),OSCValue.Int(0)));

        //message.AddValue(OSCValue.String("kick"));
        
        /*
        message.AddValue(OSCValue.String("/s_new"));
        message.AddValue(OSCValue.String("kick"));
        message.AddValue(OSCValue.Int(-1));*/

        //transmitter.Send(message);
        /*
        foreach(Transform child in tastiera.transform)
        if(child.GetComponentInChildren<MIDIkey>().getPremuto()) //o getPremuto, funzione che restituisce bool
            child.GetComponentInChildren<MIDIkey>().onclick();*/
        //tastiera.transform.GetChild(0).gameObject.GetComponent<MIDIkey>().reset();
        //gameObject.GetComponent<MeshRenderer>().material = coloreBase;
        //tastiPremuti.Remove(int.Parse(gameObject.name));

        //in alternativa, vedi i tasti premuti e applica onclick
    }

}
