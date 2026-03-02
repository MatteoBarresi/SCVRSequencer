using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using extOSC;
public class PlayStop : MonoBehaviour
{
    //associato all'UI button Play/Stop - manda messaggio per istanziare/liberare un synth o un pattern e interrompe i relativi controlli

    public UnityEvent stopTrasmissioni; //UIGeneral.stop
    public UnityEvent aggiornaIsPlaying; //aggiorna testo (testoIniziale.setString)
   
    int battutaPausa;

    //LEGATO A UIBUTTON0
    public void Play(){ //bisogna assegnare toggle button anche nel gameobj UIButton0 
        var selectedSynth = UIGeneral.selectedSynth;
        Synth.synthList[selectedSynth].isPlaying = !Synth.synthList[selectedSynth].isPlaying;
        aggiornaIsPlaying.Invoke();
        
        if(Synth.synthList[selectedSynth].isPlaying) //play
        {
            this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Stop"; 

            //manda sequenza anche qui
            
            //se ha midikeyboard, manda [rest, [60,63], rest, [65], rest, rest, ecc]
            //altrimenti manda sequenza normale [1, 0, 1, 1, 1, 0, 0] 
            GameManager.instance.SCsendBoth(Synth.synthList[selectedSynth]);//se lo metto dopo play non cambia niente

            var message = new OSCMessage(selectedSynth);
            message.AddValue(OSCValue.String("play"));
            StartCoroutine(GameManager.instance.MandaInizio(message));          
            
        }
        else {//stop
            this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Play";

            //elimina synth e resetta colori ecc - continua a trasmettere kr (NO stopTrasmissoni)
            GameManager.instance.LiberaSynth(selectedSynth, false);
            GameManager.instance.play_stopBtn.transform.localScale = Vector3.zero;
            StartCoroutine(EvitaSpam());
        }

        GameManager.instance.GestioneSolo();
    }    

    IEnumerator EvitaSpam(){
        yield return new WaitWhile(()=> GameManager.instance.GetCurrentBeat() > 0);
            GameManager.instance.play_stopBtn.transform.localScale = new Vector3(0.4f,0.4f,0.4f);
    }
    
    public void StopAll(){  //stoppa tutti indistintamente
        foreach(Synth synth in Synth.synthList.Values){
            synth.isPaused = false;
            if(synth.isPlaying){
                synth.isPlaying = false;

                GameManager.instance.LiberaSynth(synth.symbol, false);
                stopTrasmissioni.Invoke(); //setta kr a false
            }
        }
        aggiornaIsPlaying.Invoke();
        //cambia testo dell'UI play/stop
        GameManager.instance.play_stopBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Play"; //oppure evento
        GameManager.instance.GestioneSolo(); 
    }
    public void PlayAll(){ //fa partire tutti i sequenziabili programmati
        foreach(Synth synth in Synth.synthList.Values){
            if(synth.isPlaying || synth.isPaused)
              continue;
            var message = new OSCMessage(synth.symbol);
            message.AddValue(OSCValue.String("play"));
            if(synth.sequence){
                foreach(int value in synth.sequencer[UIGeneral.getBattuta()]){
                    if(value != 0)
                        Synth.synthList[synth.symbol].isPlaying = true;
                }
            }
            if(synth.isPlaying){ //manda messaggio a SC: avvia tutti i synth (attivati) 
                StartCoroutine(GameManager.instance.MandaInizio(message));  
            }
            GameManager.instance.SCsendBoth(Synth.synthList[synth.symbol]);
        }
        aggiornaIsPlaying.Invoke();
        //cambia testo dell'UI play/stop
        if(Synth.synthList[UIGeneral.getSelection()].isPlaying)
             GameManager.instance.play_stopBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Stop";      
        if(!AscoltaComposizione.isReproducing || (AscoltaComposizione.isReproducing && UIGeneral.getBattuta() > 0))
            GameManager.instance.GestioneSolo();     
    }

    public void PauseResumeAll(){
        string stato = gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
        if(stato == "ResumeAll"){
            //resumeAll
            GameManager.instance.playAllBtn.SetActive(false);
            foreach(Synth synth in Synth.synthList.Values){
                if(synth.isPlaying && synth.isPaused){
                    synth.isPaused = false;
                    
                    if(!AscoltaComposizione.isReproducing && UIGeneral.getBattuta() == battutaPausa)
                        GameManager.instance.SCsendString(synth.symbol, "resume"); //funziona 
                    else {
                        
                        //mando all'inizio della nuova battuta, altrimenti continua eventi della battuta in cui ho messo pausa - gestito con quant in SC
                        
                        //var message = new OSCMessage(synth.symbol);
                        //message.AddValue(OSCValue.String("resumeB"));
                        //StartCoroutine(GameManager.instance.MandaInizio(message));
                        GameManager.instance.SCsendString(synth.symbol, "resumeB"); //funziona 

                        //manda sequenza con quant 0
                        GameManager.instance.SCSendBothNext(synth); //funziona per entrambi (con o senza midikeyboard)
                    }
                }
                //riprende alla prossima battuta - un po' strano perché alla prossima potrebbe non essere programmato 
                else if(!synth.isPlaying && synth.isPaused){
                    synth.isPaused = false;
                    if(synth.sequence){
                        foreach(int value in synth.sequencer[UIGeneral.getBattuta()]){
                            if(value != 0)
                                Synth.synthList[synth.symbol].isPlaying = true;
                        }
                    }
                    //play-> SC
                    if(synth.isPlaying){
                        GameManager.instance.SCsendBoth(Synth.synthList[synth.symbol]);
                        var message = new OSCMessage(synth.symbol);
                        message.AddValue(OSCValue.String("play"));
                        StartCoroutine(GameManager.instance.MandaInizio(message));
                    }

                }                
            }
            gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Solo";
        }else {
            //solo
            GameManager.instance.playAllBtn.SetActive(true);
            battutaPausa = UIGeneral.getBattuta();
            foreach(Synth synth in Synth.synthList.Values){
                if(synth.isPlaying && synth.symbol != UIGeneral.getSelection()){
                    synth.isPaused = true;
                    GameManager.instance.SCsendString(synth.symbol, "pause");
                }else if (!synth.isPlaying) //per il cambio battuta
                    synth.isPaused = true; 
            }
            gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "ResumeAll";
        }
        aggiornaIsPlaying.Invoke();
    }

    public void CancellaSequenza(){
        Synth synth = Synth.synthList[UIGeneral.getSelection()];
        GameManager.instance.LiberaSynth(synth.symbol, true);
        stopTrasmissioni.Invoke();
        aggiornaIsPlaying.Invoke();
        GameManager.instance.GestioneSolo();
        synth.isPlaying = false;
        synth.isPaused = false;
        GameManager.instance.play_stopBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Play";  

        //se non faccio questa cosa, rimangono settati i btn e i valori nei sequencer dei synth che non sono quello attualmente selezionato nel dropdown (ma SC non fa suono)
        //cancella seq di questa battuta
        if(synth.sequence)
            for(int i = 0; i < synth.sequencer[UIGeneral.getBattuta()].Count; i++)
                synth.sequencer[UIGeneral.getBattuta()][i] = 0;
    }

    public void spiega(){
        if(gameObject == GameManager.instance.soloBtn)
            GameManager.instance.spiegaz.GetComponentInChildren<TextMeshProUGUI>().text = "ascolta solo lo strumento attualmente selezionato";
        else if(gameObject == GameManager.instance.play_stopBtn)
            GameManager.instance.spiegaz.GetComponentInChildren<TextMeshProUGUI>().text = "Avvia/Cancella sequenza programmata per questa battuta";
        else if(gameObject == GameManager.instance.playAllBtn)
            GameManager.instance.spiegaz.GetComponentInChildren<TextMeshProUGUI>().text = "Avvia tutte le sequenze programmate per questa battuta";
        else if(gameObject == GameManager.instance.stopAllBtn)
            GameManager.instance.spiegaz.GetComponentInChildren<TextMeshProUGUI>().text = "Stoppa tutte le sequenze programmate per questa battuta";
        else if(gameObject == GameManager.instance.cancellaSeqBtn)
            GameManager.instance.spiegaz.GetComponentInChildren<TextMeshProUGUI>().text = "Cancella tutte le sequenze programmate per questa battuta";
        else if(gameObject == GameManager.instance.directorBtn)
            GameManager.instance.spiegaz.GetComponentInChildren<TextMeshProUGUI>().text = "Organizza suoni nello spazio";
        
        Debug.Log(this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text);

    }
    public void esciSpiega(){
        GameManager.instance.spiegaz.GetComponentInChildren<TextMeshProUGUI>().text = "";
    }    

}
