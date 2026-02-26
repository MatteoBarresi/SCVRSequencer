using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using TMPro;
using extOSC;
using System.Linq;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    //public Transform tastiera;
    public OSCTransmitter transmitter;
    public OSCReceiver receiver;
    public GameObject tastiera;
    public GameObject sequencer; 
    public GameObject dropdown;
    public GameObject spiegaz; 
    public GameObject soloBtn; 
    public GameObject play_stopBtn; 
    public GameObject playAllBtn; 
    public GameObject stopAllBtn; 
    public GameObject cancellaSeqBtn; 
    public GameObject directorBtn; 

    public Dictionary<string,string> paths; 

    public GameObject addBtn; 
    public GameObject delBtn; 
    public GameObject backBtn; 
    public GameObject forwardBtn; 

    //colori led sequencer
    public Material coloreBase;
    public Material coloreBeat;

    int currentBeat;
    [HideInInspector] public Dictionary<GameObject, Vector3> grandezze;


//    List<Coroutine> CR = new List<Coroutine>();

    void Awake()
    {
        //no DontDestroyOnLoad perché dovrei fare un FindObjectWithTag per ogni oggetto (visto che sono in una scena diversa)
        
        if(GameManager.instance != null){
            Destroy(gameObject);
            return;
        }
        instance = this;
        //DontDestroyOnLoad(gameObject);
        
        instance = this;
        paths = new Dictionary<string, string>();

        if(sequencer != null 
        && tastiera != null 
        && playAllBtn !=null 
        && stopAllBtn != null
        && play_stopBtn != null
        && soloBtn != null
        ) 
            grandezze = new Dictionary<GameObject, Vector3>(){
                {GameManager.instance.sequencer,    GameManager.instance.sequencer.transform.localScale},
                {GameManager.instance.tastiera,     GameManager.instance.tastiera.transform.localScale},
                {GameManager.instance.playAllBtn,   GameManager.instance.playAllBtn.transform.localScale},
                {GameManager.instance.stopAllBtn,   GameManager.instance.stopAllBtn.transform.localScale},
                {GameManager.instance.soloBtn,      GameManager.instance.soloBtn.transform.localScale},
                {GameManager.instance.play_stopBtn, GameManager.instance.play_stopBtn.transform.localScale},
                /*{GameManager.instance.forwardBtn,   GameManager.instance.forwardBtn.transform.localScale},
                {GameManager.instance.backBtn,      GameManager.instance.backBtn.transform.localScale},
                {GameManager.instance.delBtn,       GameManager.instance.delBtn.transform.localScale},
                {GameManager.instance.addBtn,       GameManager.instance.addBtn.transform.localScale},
                */{GameManager.instance.dropdown,       GameManager.instance.dropdown.transform.localScale},
                {GameManager.instance.spiegaz,       GameManager.instance.spiegaz.transform.localScale},

            };

        if (forwardBtn != null) forwardBtn.transform.parent.gameObject.transform.localScale = new Vector3(0,0,0);
        if (backBtn != null) backBtn.transform.parent.gameObject.transform.localScale = new Vector3(0,0,0);
        //delBtn.transform.parent.gameObject.transform.localScale = new Vector3(0,0,0);
        if (delBtn != null) delBtn.transform.parent.gameObject.SetActive(false);
/*
        backBtn.transform.parent.gameObject.SetActive(false);
        forwardBtn.transform.parent.gameObject.SetActive(false);*/
    }

    void Start(){
        if(soloBtn != null) soloBtn.SetActive(false);
    }
    
    public void editModeEsciSwitch(){
        if(Synth.synthList[UIGeneral.getSelection()].hasKeys)
            foreach(Transform item in sequencer.transform){
                if(item.GetComponentInChildren<ButtonFunc>().getEditMode()){ //non tutti hanno buttonFunc
                    item.GetComponentInChildren<ButtonFunc>().setEditMode(false);
                    item.GetComponentInChildren<ButtonFunc>().editModeEsci();
                }
            }

    }
    
    public void hide(params GameObject[] lista){
        foreach(GameObject element in lista){
            element.SetActive(false);
        }
    }

    /*public void GestioneBottoniBattuta(bool stato){
        if(!stato){
            GameManager.instance.delBtn.transform.parent.gameObject.SetActive(false);
            GameManager.instance.addBtn.transform.parent.gameObject.SetActive(false);
            GameManager.instance.forwardBtn.transform.parent.gameObject.transform.localScale = Vector3.zero; 
            GameManager.instance.backBtn.transform.parent.gameObject.transform.localScale = Vector3.zero;
        }else {
            GameManager.instance.delBtn.transform.parent.gameObject.SetActive(true);
            GameManager.instance.addBtn.transform.parent.gameObject.SetActive(true);
            GameManager.instance.forwardBtn.transform.parent.gameObject.transform.localScale = Vector3.one; 
            GameManager.instance.backBtn.transform.parent.gameObject.transform.localScale = Vector3.one;

        }
    }*/

    public void SCsendValue(string synthName, string kr, float value){
        var message = new OSCMessage(synthName);
        message.AddValue(OSCValue.String(kr));
        message.AddValue(OSCValue.Float(value));
        transmitter.Send(message);
    }

    public void SCsendString(string path, params string[] lista){
        var message = new OSCMessage(path);
        foreach(string str in lista){
            message.AddValue(OSCValue.String(str));
        }
        transmitter.Send(message);
    }

    public void SCsendSequence(string synthName){//manda sequenza per questa battuta per questo synth
        var message = new OSCMessage(synthName);
        foreach(int item in Synth.synthList[synthName].sequencer[UIGeneral.getBattuta()])
            message.AddValue(OSCValue.Int(item));
        transmitter.Send(message);

    }
    public void SCsendNextSequence(string synthName){//manda sequenza per questa battuta per questo synth
        var message = new OSCMessage(synthName);
        if(UIGeneral.getBattuta() + 1 < BeatManager.Battute){
            foreach(int item in Synth.synthList[synthName].sequencer[UIGeneral.getBattuta() + 1])
                message.AddValue(OSCValue.Int(item));
            message.AddValue(OSCValue.String("pino"));
            transmitter.Send(message);
        }
    }

    /*public void SCsendNotes(string synthName){
        var message = new OSCMessage(synthName);
        List<int> valori = new List<int>(); 
        foreach (int item in MIDIkey.tastiPremuti){ //copio i valori uno a uno - se faccio valori = tastiPremuti non funziona (modifica tastiPremuti quando modifico valori)
                valori.Add(item);
                //chord[UIGeneral.getSelection()].Add(item);
                chord[synthName][UIGeneral.getBattuta()].Add(item);
            }
        //aggiungo indice e mando
        valori.Insert(0, index); 
        //OSCHandler.Instance.SendMessageToClient("SuperCollider",selectedSynth, valori);
        foreach(int item in valori)
            message.AddValue(OSCValue.Int(item));
        transmitter.Send(message);
    }*/

    public void listenPaths(){
        receiver.Bind("/path", (OSCMessage message)=> {
            //message.Values[0] è stringa strumento
            //message.Values[1] è path strumento
            /*GameManager.instance.*/paths.Add(message.Values[0].StringValue, message.Values[1].StringValue);
        });
    }

    public void listen(){
        receiver.Bind("/beat", (OSCMessage message)=> {
            int beat = (int)message.Values[0].FloatValue;
            currentBeat = beat-1;

            gestioneLedSequencer(beat); //non faccio -1 perché i bottoni partono da 1

        });
    }
    public int GetCurrentBeat(){
        return currentBeat;
    }

    public void gestioneLedSequencer(int beat){
        foreach(Transform btn in GameManager.instance.sequencer.transform){
            if (int.Parse(btn.name) == beat){
                btn.GetChild(2).GetComponent<MeshRenderer>().material = GameManager.instance.coloreBeat;
            }else 
                btn.GetChild(2).GetComponent<MeshRenderer>().material = GameManager.instance.coloreBase;
        }
    }

    public void SequencerButtons(){ //slot sequencer programmato o no (colora parte visuale)
        var synth = UIGeneral.getSelection();
        if (Synth.synthList[synth].sequence){
            for(int i = 0; i< Synth.synthList[synth].sequencer[UIGeneral.getBattuta()].Count; i++)
                {
                    GameObject g;
                    g = sequencer.transform.GetChild(i).gameObject.transform.GetChild(1).gameObject; //press
                    if (Synth.synthList[synth].sequencer[UIGeneral.getBattuta()][i] == 1)
                        g.GetComponent<ButtonFunc>().gestioneBottoni(true);
                    else g.GetComponent<ButtonFunc>().gestioneBottoni(false);
                }
        }
    }

    public void GestioneSolo(){
        StartCoroutine(Aspetta());
        
/*
        if(Synth.synthList[UIGeneral.getSelection()].isPlaying){
            //soloBtn.SetActive(true);

            //if(!AscoltaComposizione.isReproducing)
                StartCoroutine(Aspetta());
            //else 
            //    soloBtn.SetActive(true);
        }else 
            soloBtn.SetActive(false);

        IEnumerator Aspetta(){
            Debug.Log("entro in aspetta");
            yield return new WaitWhile(()=> GetCurrentBeat() < 15);
            soloBtn.SetActive(true);
            Debug.Log("esco da aspetta");
        }*/

        IEnumerator Aspetta(){
            Debug.Log("entro in aspetta");
            if(AscoltaComposizione.isReproducing || Synth.synthList[UIGeneral.getSelection()].isPlaying)
                yield return new WaitWhile(()=> GetCurrentBeat() < 15);
            if(Synth.synthList[UIGeneral.getSelection()].isPlaying)
                soloBtn.SetActive(true);
            else
                soloBtn.SetActive(false);
            Debug.Log("esco da aspetta");
        }
    }


    public IEnumerator MandaInizio(OSCMessage message){ //manda all'inizio della battuta - il gameobject deve rimanere Active o lancia errore
        yield return new WaitWhile(()=> GetCurrentBeat() < 15);
        transmitter.Send(message);
    }


//--------------------------- possibile funzione unica

    public void SCsendBoth(Synth synth){ //manda la sequenza di questo synth
        
        
        
        
        //foreach(Synth synth in Synth.synthList.Values){
            if(synth.hasKeys && synth.sequence){ //bass e pads 
                /*foreach(Coroutine c in CR){
                    StopCoroutine(c);
                }*/

                //per ogni bottone programmato, sendNotes (accordo) per bass e pads
                //altrimenti scsendMsg(selectedSynth, "rest")

                foreach(Transform btn in sequencer.transform){
                    
                    //se il chord di questo bottone non è vuoto, manda note
                    //per ogni nota in questo bottone (chord)
                    if(btn.GetComponentInChildren<ButtonFunc>().programmedChord(synth.symbol)){//non è programmato a vuoto
                        
                        var message = new OSCMessage(synth.symbol);
                        message.AddValue(OSCValue.Int(btn.GetComponentInChildren<ButtonFunc>().getIndex()));
                        //se ha accordi, mandali
                        foreach(int nota in btn.GetComponentInChildren<ButtonFunc>().getChord(synth.symbol))
                            message.AddValue(OSCValue.Int(nota));
                        //CR.Add(StartCoroutine(MandaInizio(message))); 
                        transmitter.Send(message);
                    }else btn.GetComponentInChildren<ButtonFunc>().SCsendMsg(synth.symbol, "rest");
                }
            }else if(synth.sequence) //solo sequencer
                SCsendSequence(synth.symbol);
                /*{var m = new OSCMessage(synth.symbol);
                foreach(int item in synth.sequencer[UIGeneral.getBattuta()])
                    m.AddValue(OSCValue.Int(item));
                StartCoroutine(MandaInizio(m));
                }*/
       // }
    }

    public void SCSendBothNext(Synth synth){
        var message = new OSCMessage(synth.symbol);
        
               
        if(UIGeneral.getBattuta() + 1 < BeatManager.Battute){
            if(synth.hasKeys && synth.sequence){ //in realtà questo è uguale a sendBoth (non mando \pino) - per come è organizzato lato SC (uso una variabile con gli accordi), quando riprende, lo fa dal punto giusto
                foreach(Transform btn in sequencer.transform){
                    
                    //se il chord di questo bottone non è vuoto, manda note
                    //per ogni nota in questo bottone (chord)
                    if(btn.GetComponentInChildren<ButtonFunc>().programmedChord(synth.symbol)){//non è programmato a vuoto
                        
                        
                        message.AddValue(OSCValue.Int(btn.GetComponentInChildren<ButtonFunc>().getIndex()));
                        //se ha accordi, mandali
                        foreach(int nota in btn.GetComponentInChildren<ButtonFunc>().getChord(synth.symbol))
                            message.AddValue(OSCValue.Int(nota));
                        StartCoroutine(MandaInizio(message)); 
                    }else btn.GetComponentInChildren<ButtonFunc>().SCsendMsg(synth.symbol, "rest");
                }

            }else if(synth.sequence){
                foreach(int item in Synth.synthList[synth.symbol].sequencer[UIGeneral.getBattuta() + 1])
                    message.AddValue(OSCValue.Int(item));
                message.AddValue(OSCValue.String("pino"));
                transmitter.Send(message);
            }

        }

    }

//---------------------------

     public void resetKeys(){ //solo su quelli premuti
        foreach(Transform child in tastiera.transform)
            if(child.GetComponentInChildren<MIDIkey>().getPremuto())
                child.GetComponentInChildren<MIDIkey>().onclick(); //rimuove da Lista tastiPremuti e cambia aspetto
    }
    public void resetSequencer(){
        //aspetto grafico
        if(Synth.synthList[UIGeneral.getSelection()].sequence){
            foreach(Transform child in sequencer.transform){
                if(child.GetComponentInChildren<ButtonFunc>().getPremuto()){
                    child.GetComponentInChildren<ButtonFunc>().laserInteraction(); //cambia aspetto e stato nell'oggetto Synth, poi invia un mx OSC per ogni tasto premuto
                }
            }
        }
    }

    
    public void LiberaSynth(string synthName, bool delete){
        //var message = new OSCMessage(synthName);
        editModeEsciSwitch();
        if(Synth.synthList[synthName].sequence){ //ha un sequencer
            SCsendString(synthName, "free");

            if(delete){
                resetKeys();
                resetSequencer(); //inutile chiamarla per tutti i synth perché cambia solo il sequencer del synth attuale (se esiste)
            }    
        }else //caso solo continui (theremin) - mando sia stringa che valore
            SCsendValue(synthName, "gate", 0);
    }

}
