using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using extOSC;

public class MIDIkey : MonoBehaviour
{
    //usato da tutti i figli della tastiera - discutibile perché dovrei dividere le funzionalità

    [SerializeField] Material coloreBase;
    [SerializeField] Material colorePremuto;
    //[SerializeField] GameObject tastiera; //usato solo dal bottone reset
    public static List<int> tastiPremuti = new List<int>() {};  
    //[SerializeField] OSCTransmitter transmitter;
    private bool premuto = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void clickSenzaPiano(){ //onclick fa questo se il synth non è /piano
        if (!premuto){
            premuto = true;
            gameObject.GetComponent<MeshRenderer>().material = colorePremuto;
            //gameObject.GetComponent<Renderer>().material.color = Color.green;
            tastiPremuti.Add(int.Parse(gameObject.name));
        }else{
            premuto = false;
            gameObject.GetComponent<MeshRenderer>().material = coloreBase;
            //gameObject.GetComponent<Renderer>().material.color = Color.white;
            tastiPremuti.Remove(int.Parse(gameObject.name));
        }
    }

    public void onclick(){
        if(Synth.synthList[UIGeneral.selectedSynth].sequence){ //è sequenziabile ? 
            clickSenzaPiano();
        }
        else { //es piano - suono in tempo reale
            SCsendValue();
            /*var message = new OSCMessage(UIGeneral.selectedSynth);
            message.AddValue(OSCValue.Int(int.Parse(gameObject.name)));
            transmitter.Send(message);*/
            StartCoroutine(cambioColore(gameObject));
        }
    }

    private void SCsendValue(){
        var message = new OSCMessage(UIGeneral.selectedSynth);
        message.AddValue(OSCValue.Int(int.Parse(gameObject.name)));
        GameManager.instance.transmitter.Send(message);

    }

    public bool getPremuto(){
        return premuto;
    }

    IEnumerator cambioColore(GameObject tasto){
        gameObject.GetComponent<MeshRenderer>().material = colorePremuto;
        //gameObject.GetComponent<Renderer>().material.color = Color.green;
        yield return new WaitForSeconds(0.5f); 
        gameObject.GetComponent<MeshRenderer>().material = coloreBase;
        //gameObject.GetComponent<Renderer>().material.color = Color.white;
    }

    public void reset(){ //switch su quelli premuti
        foreach(Transform child in GameManager.instance.tastiera.transform)
        if(child.GetComponentInChildren<MIDIkey>().getPremuto()) //o getPremuto, funzione che restituisce bool
            child.GetComponentInChildren<MIDIkey>().onclick();
        
    }
    public static void resetPiano(GameObject item){ //switch su quelli premuti
        foreach(Transform child in item.transform)
        if(child.GetComponentInChildren<MIDIkey>().getPremuto()) //o getPremuto, funzione che restituisce bool
            child.GetComponentInChildren<MIDIkey>().clickSenzaPiano();
    }



}
