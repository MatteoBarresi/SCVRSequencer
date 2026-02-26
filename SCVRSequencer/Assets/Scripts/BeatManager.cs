using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;

using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Threading.Tasks;
public class BeatManager : MonoBehaviour
{
    
    public static int Battute {get; private set;} = 1;
    [SerializeField] 
    private UnityEvent aggiornaNumeroBattutaVisual; //TestoBattutaAttuale.aggiornaNumeroBattuta
    
    void Start()
    {
        Debug.Log($"beatmanager battute vale {Battute}");
    }

    public void aggiungiBattuta(){ //legato a addBtn
        foreach(Synth synth in Synth.synthList.Values){
            //Debug.Log(synth.symbol);
            if(synth.sequence){
                synth.aggiungiBattuta(UIGeneral.getBattuta());
                Debug.Log(synth.symbol + ": numero di battute = " + synth.sequencer.Count);
                if(synth.hasKeys){//pads e bass - allarga chords
                    foreach(Transform btn in GameManager.instance.sequencer.transform){
                        btn.GetComponentInChildren<ButtonFunc>().AllargaChords(synth.symbol);
                    }
                }
            }
        }
        Battute ++;
        Debug.Log($"totale battute = {Battute}");
        aggiornaNumeroBattutaVisual.Invoke(); 
        gestioneBottoni();
    }

    public void eliminaBattuta(){ //legato delBtn
        foreach(Synth synth in Synth.synthList.Values){
            
            if(synth.sequence){
                synth.eliminaBattuta(UIGeneral.getBattuta());
                Debug.Log(synth.symbol + ": numero di battute = " + synth.sequencer.Count);
                if(synth.hasKeys){//pads e bass - elimina chords
                    foreach(Transform btn in GameManager.instance.sequencer.transform){
                        btn.GetComponentInChildren<ButtonFunc>().EliminaChords(synth.symbol);
                    }
                }
            }
        }
        Battute --;
        Debug.Log($"totale battute = {Battute}");
        gestioneBottoni();
        //solo per mandare nuovamente sequencerz
        cambiaBattuta(UIGeneral.getBattuta() > 0 ? -1: 0);

        aggiornaNumeroBattutaVisual.Invoke();
    }
    

    public IEnumerator Aspetta(){
        yield return new WaitWhile(()=> GameManager.instance.GetCurrentBeat() < 15);
        
        //cambiaBattuta(1);
        
        //-----------------------------------
            UIGeneral.setBattuta(UIGeneral.getBattuta() + 1 );
            if(Synth.synthList[UIGeneral.getSelection()].sequence){
                gestioneBottoni();
                GameManager.instance.SequencerButtons(); //non si vede il sequencer in questo caso, però dovrebbe far funzionare altro (testoIniziale.setString)

            }
            aggiornaNumeroBattutaVisual.Invoke(); 
            Debug.Log("siamo alla battuta " + UIGeneral.getBattuta());

            
            foreach(Synth synth in Synth.synthList.Values)
                GameManager.instance.SCsendBoth(synth); //lo fa già il bottone playAll (che invia e manda anche play)
        
        //-----------------------------------
        
        GameManager.instance.playAllBtn.GetComponent<Button>().onClick.Invoke();

    }

    public void cambiaBattutaAutomatico(){
        StartCoroutine(Aspetta()); 
    }


    public void cambiaBattuta(int amount){        
       

        //playAll, solo, play/stop


        UIGeneral.setBattuta(UIGeneral.getBattuta() + amount );
        if(Synth.synthList[UIGeneral.getSelection()].sequence){
            gestioneBottoni();
            gestioneTastiUI(false);
            GameManager.instance.SequencerButtons();

        }

        aggiornaNumeroBattutaVisual.Invoke();
        Debug.Log("siamo alla battuta " + UIGeneral.getBattuta());



        foreach(Synth synth in Synth.synthList.Values)
            GameManager.instance.SCsendBoth(synth);
        

        //aspetta

  
        //manda a SC messaggio con nuovi array (di tutti gli strumenti con sequencer) 
        /*foreach(Synth synth in Synth.synthList.Values){
            if(synth.hasKeys && synth.sequence){ //bass e pads 
                //per ogni bottone programmato, sendNotes (accordo) per bass e pads
                //altrimenti scsendMsg(selectedSynth, "rest")


                foreach(Transform btn in GameManager.instance.sequencer.transform){
                    
                    //se il chord di questo bottone non è vuoto, manda note
                    //per ogni nota in questo bottone (chord)
                    if(btn.GetComponentInChildren<ButtonFunc>().programmedChord(synth.symbol)){//non è programmato a vuoto
                        
                        var message = new OSCMessage(synth.symbol);
                        message.AddValue(OSCValue.Int(btn.GetComponentInChildren<ButtonFunc>().getIndex()));
                        //se ha accordi, mandali
                        foreach(int nota in btn.GetComponentInChildren<ButtonFunc>().getChord(synth.symbol))
                            message.AddValue(OSCValue.Int(nota));
                        StartCoroutine(MandaInizio(message)); 
                    }else {
                        btn.GetComponentInChildren<ButtonFunc>().SCsendMsg(synth.symbol, "rest");
                        
                    }
                }
                

            }else if(synth.sequence){ //solo sequencer
                GameManager.instance.SCsendSequence(synth.symbol);
            }
                
        }*/
        
        //GameManager.instance.SCsendString("/tempo", "real");   
        /*
        //mandava la nuova sequenza a tempoosc ma inutile
        var m = new OSCMessage("/tempo");
        //m.AddValue(OSCValue.String("real"));
        foreach(int item in Synth.synthList["/kick"].sequencer[UIGeneral.getBattuta()])
            m.AddValue(OSCValue.Int(item));
        GameManager.instance.transmitter.Send(m);*/
    }

    public void gestioneBottoni(){ //cambio scale perché mi servono attivi per una coroutine
        if(!Synth.synthList[UIGeneral.getSelection()].sequence 
            || (Synth.synthList[UIGeneral.getSelection()].sequence && AscoltaComposizione.isReproducing)
        ){
            GameManager.instance.delBtn.transform.parent.gameObject.SetActive(false);
            GameManager.instance.addBtn.transform.parent.gameObject.SetActive(false);
            GameManager.instance.backBtn.transform.parent.gameObject.transform.localScale = new Vector3(0,0,0);
            GameManager.instance.forwardBtn.transform.parent.gameObject.transform.localScale = new Vector3(0,0,0);

        }else{
            //check su un synth a caso - se ha più di una battuta -        //Battute non andava perché veniva aggiornato dopo forse (all'aggiunta/rimozione)
            //int totaleBattute = Synth.synthList[UIGeneral.getSelection()].sequencer.Count; 
            
            GameManager.instance.addBtn.transform.parent.gameObject.SetActive(true);
            if(/*totaleBattute*/ Battute > 1){
                //elimina battuta
                GameManager.instance.delBtn.transform.parent.gameObject.SetActive(true);

                //avanti e indietro
                if(UIGeneral.getBattuta() == 0) //sono alla prima
                    GameManager.instance.backBtn.transform.parent.gameObject.transform.localScale = new Vector3(0,0,0);
                else 
                    GameManager.instance.backBtn.transform.parent.gameObject.transform.localScale = new Vector3(1,1,1);
                
                if(UIGeneral.getBattuta() == (/*totaleBattute - 1*/ Battute - 1)) //sono all'ultima
                    GameManager.instance.forwardBtn.transform.parent.gameObject.transform.localScale = new Vector3(0,0,0);
                else 
                    GameManager.instance.forwardBtn.transform.parent.gameObject.transform.localScale = new Vector3(1,1,1);
            }else{//una sola battuta
                GameManager.instance.delBtn.transform.parent.gameObject.SetActive(false);
                GameManager.instance.backBtn.transform.parent.gameObject.transform.localScale = new Vector3(0,0,0);
                GameManager.instance.forwardBtn.transform.parent.gameObject.transform.localScale = new Vector3(0,0,0);
            }
        }
        
    }

    public void gestioneTastiUI(bool stato){
        if(!Synth.synthList[UIGeneral.getSelection()].isPlaying){
            GameManager.instance.play_stopBtn.transform.localScale = stato ? GameManager.instance.grandezze[GameManager.instance.play_stopBtn]: Vector3.zero;
        }
        GameManager.instance.soloBtn.transform.localScale = stato ? GameManager.instance.grandezze[GameManager.instance.soloBtn] : Vector3.zero;
        GameManager.instance.playAllBtn.transform.localScale = stato ? GameManager.instance.grandezze[GameManager.instance.playAllBtn] : Vector3.zero;

        if(!stato)
            StartCoroutine(GestioneHelper());

        IEnumerator GestioneHelper(){
            yield return new WaitWhile(()=> GameManager.instance.GetCurrentBeat() < 15);
            
            gestioneTastiUI(true);
        }


    }


    


}
