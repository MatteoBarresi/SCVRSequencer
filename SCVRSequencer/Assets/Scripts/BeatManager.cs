using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class BeatManager : MonoBehaviour
{
    
    public static int Battute {get; private set;} = 1;
    [SerializeField] 
    private UnityEvent<int, int> aggiornaNumeroBattutaVisual; //TestoBattutaAttuale.aggiornaNumeroBattuta

    public void aggiungiBattuta(){ //legato a addBtn
        foreach(Synth synth in Synth.synthList.Values){
            
            if(synth.sequence){
                synth.aggiungiBattuta(UIGeneral.getBattuta());
                //Debug.Log(synth.symbol + ": numero di battute = " + synth.sequencer.Count);
                if(synth.hasKeys){//pads e bass - allarga chords
                    foreach(Transform btn in GameManager.instance.sequencer.transform){
                        btn.GetComponentInChildren<ButtonFunc>().AllargaChords(synth.symbol);
                    }
                }
            }
        }
        Battute ++;
        //Debug.Log($"totale battute = {Battute}");
        aggiornaNumeroBattutaVisual.Invoke(UIGeneral.getBattuta() + 1, Battute);  
        gestioneBottoni();
    }

    public void eliminaBattuta(){ //legato delBtn
        foreach(Synth synth in Synth.synthList.Values){
            
            if(synth.sequence){
                synth.eliminaBattuta(UIGeneral.getBattuta());
               // Debug.Log(synth.symbol + ": numero di battute = " + synth.sequencer.Count);
                if(synth.hasKeys){//pads e bass - elimina chords
                    foreach(Transform btn in GameManager.instance.sequencer.transform){
                        btn.GetComponentInChildren<ButtonFunc>().EliminaChords(synth.symbol);
                    }
                }
            }
        }
        Battute --;
        //Debug.Log($"totale battute = {Battute}");
        gestioneBottoni();
        //solo per mandare nuovamente sequencerz
        cambiaBattuta(UIGeneral.getBattuta() > 0 ? -1: 0);

        aggiornaNumeroBattutaVisual.Invoke(UIGeneral.getBattuta() + 1, Battute);
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
            aggiornaNumeroBattutaVisual.Invoke(UIGeneral.getBattuta() + 1, Battute); 
            //Debug.Log("siamo alla battuta " + UIGeneral.getBattuta());

            
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

        aggiornaNumeroBattutaVisual.Invoke(UIGeneral.getBattuta() + 1, Battute);
        //Debug.Log("siamo alla battuta " + UIGeneral.getBattuta());

        foreach(Synth synth in Synth.synthList.Values)
            GameManager.instance.SCsendBoth(synth);
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
            //check su un synth a caso - se ha più di una battuta 
            //Battute non andava perché veniva aggiornato dopo forse (all'aggiunta/rimozione)
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
