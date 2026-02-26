using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;
using extOSC;

public class AscoltaComposizione : MonoBehaviour
{
    Dictionary<GameObject, Vector3> grandezze;
    public static bool isReproducing {get; private set;}
    [SerializeField] UnityEvent gestioneBeatButtons; //BeatManager.gestioneBottoni
    [SerializeField] UnityEvent GUIFineAscoltoEvent; //UIGeneral.dropdownLog

    
    Coroutine aspettaCoroutine;
    //!!!IEnumerator aspettaCoroutine;
    
    //Associato a Riproduci composizione. se nessuno strumento è in play o pausa, appare 
    void Start()
    {
        //!!!aspettaCoroutine = GameManager.instance.forwardBtn.GetComponentInChildren<BeatManager>().Aspetta();
        isReproducing = false;
        GestioneTesto();

        GameManager.instance.receiver.Bind("/go", (OSCMessage msg)=> {
            Debug.Log("go ricevuto a "+ GameManager.instance.GetCurrentBeat());
            StartCoroutine(Avanti());
        });
        grandezze = new Dictionary<GameObject, Vector3>(){
            {GameManager.instance.sequencer,    GameManager.instance.sequencer.transform.localScale},
            {GameManager.instance.tastiera,     GameManager.instance.tastiera.transform.localScale},
            {GameManager.instance.playAllBtn,   GameManager.instance.playAllBtn.transform.localScale},
            {GameManager.instance.stopAllBtn,   GameManager.instance.stopAllBtn.transform.localScale},

        };
    }

    
    void Update()
    {
        
    }


    IEnumerator Avanti(){        
        yield return new WaitWhile(()=> GameManager.instance.GetCurrentBeat() < 15);
        if(UIGeneral.getBattuta() < BeatManager.Battute -1 /* Synth.synthList[UIGeneral.getSelection()].sequencer.Count - 1*/ ){ //avanti fino all'ultima
            
            Debug.Log("Entrato in Avanti----");
            
            //GameManager.instance.forwardBtn.GetComponentInChildren<BeatManager>().cambiaBattutaAutomatico();//cambiaBattuta(1);
            //!!!StartCoroutine(aspettaCoroutine);
            aspettaCoroutine = StartCoroutine(GameManager.instance.forwardBtn.GetComponentInChildren<BeatManager>().Aspetta());
            //GameManager.instance.playAllBtn.GetComponent<Button>().onClick.Invoke();
        } 
        else {
            Debug.Log($"sono all'ultima battuta ({UIGeneral.getBattuta()})");
            yield return new WaitWhile(()=> GameManager.instance.GetCurrentBeat() < 15);

            //Riproduci(); //in questo caso stoppa tutto
            
            GameManager.instance.SCsendString("/piece", "fine");
            GameManager.instance.stopAllBtn.GetComponent<Button>().onClick.Invoke();
            isReproducing = false;
            
            
            GestioneOggetti(true);
            GestioneTesto();
            
            if(BeatManager.Battute > 1)
                StopCoroutine(aspettaCoroutine);
            GameManager.instance.soloBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Solo"; //pericoloso perché se cambio stringa in PlayStop, devo farlo anche qui
            
        }
    }

    private void GestioneOggetti(bool stato){


        //per questi conviene fare "oggetto.transform.localScale = new Vector3(0,0,0);" perché voglio tenere gli script attivi        
        GameManager.instance.playAllBtn.transform.localScale    = stato ? /*GameManager.instance.*/grandezze[GameManager.instance.playAllBtn] : Vector3.zero; 
        GameManager.instance.stopAllBtn.transform.localScale    = stato ? /*GameManager.instance.*/grandezze[GameManager.instance.stopAllBtn] : Vector3.zero; 


        GameManager.instance.sequencer.transform.localScale     = stato ? /*GameManager.instance.*/grandezze[GameManager.instance.sequencer] : Vector3.zero ;
        if(UIGeneral.getSelection() == "piano") //alt Synth.synthList[UIGeneral.getSelection()].hasKeys && !Synth.synthList[UIGeneral.getSelection()].sequence 
            GameManager.instance.tastiera.transform.localScale      = stato ? /*GameManager.instance.*/grandezze[GameManager.instance.tastiera] : Vector3.zero;
        
        //if(Synth.synthList[UIGeneral.getSelection()].sequence){
        GameManager.instance.sequencer.SetActive(Synth.synthList[UIGeneral.getSelection()].sequence);
        GameManager.instance.tastiera.SetActive(Synth.synthList[UIGeneral.getSelection()].hasKeys);
        //}
        gestioneBeatButtons.Invoke();   

        if(!stato){
            GameManager.instance.play_stopBtn.SetActive         (false); 
            //GameManager.instance.soloBtn.SetActive              (stato);
            GameManager.instance.cancellaSeqBtn.SetActive       (false);
            
            
            GameManager.instance.soloBtn.SetActive       (false);
            
            //aspetta fino alla fine della measure
            //StartCoroutine(SoloHelper());
            GameManager.instance.GestioneSolo();
        }else 
            GUIFineAscoltoEvent.Invoke();

        /*IEnumerator SoloHelper(){
            yield return new WaitWhile(()=> GameManager.instance.GetCurrentBeat() < 15);
            GameManager.instance.GestioneSolo();
        }*/

   }


    public void Riproduci(){
        isReproducing = !isReproducing;
        GestioneTesto();

        if(isReproducing){
            GameManager.instance.editModeEsciSwitch();
            GestioneOggetti(false); //nasconde i tasti play, play all, stop all, solo + sequencer e tastiera midi
                

            //parte dalla battuta 0 (ANCORA NO) e arriva alla fine
            GameManager.instance.playAllBtn.GetComponent<Button>().onClick.Invoke(); //avvia tutti (usa mandaInizio quindi prima di fare "parti" aspetta la nuova battuta)
            GameManager.instance.SCsendString("/piece", "parti");


            //intanto si può stoppare (per la pausa, prima controlla che funziona normalmente)

        }else{
            Debug.Log("FERMATA RIPRODUZIONE");
            GameManager.instance.SCsendString("/piece", "fine");
            GameManager.instance.stopAllBtn.GetComponent<Button>().onClick.Invoke();
            GestioneOggetti(true);
            
            
            if(BeatManager.Battute > 1)
                StopCoroutine(aspettaCoroutine);


            //torna alla prima battuta
        }

    }

    private void GestioneTesto(){
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = (isReproducing ? "Ferma" : "Ascolta Composizione");
    }
}
