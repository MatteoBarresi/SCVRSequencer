using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using extOSC;
public class PlayStop : MonoBehaviour
{
    //associato all'UI button Play/Stop - manda messaggio per istanziare/liberare un synth o un pattern e interrompe i relativi controlli

    //public OSCTransmitter transmitter; //per migliorare, fare uno script in cui mando solo messaggi - usare static per richiamare in altri script (mi serve passare parametri)
    public UnityEvent stopTrasmissioni; //UIGeneral.stop
    public UnityEvent aggiornaIsPlaying; //aggiorna testo (testoIniziale.setString)
    //[SerializeField] GameObject tastiera;
    //[SerializeField] GameObject sequencer;


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
            //GameManager.instance.SCsendSequence(Synth.synthList[selectedSynth].symbol);
            GameManager.instance.SCsendBoth(Synth.synthList[selectedSynth]);//se lo metto dopo play non cambia niente


            var message = new OSCMessage(selectedSynth);
            message.AddValue(OSCValue.String("play"));
            StartCoroutine(GameManager.instance.MandaInizio(message));
            //GameManager.instance.SCsendString(selectedSynth, "play");
            
            
        }
        else {//stop
            this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Play";
            
            //GameManager.instance.editModeEsciSwitch();

            //messaggio a SC per eliminare il synth e resetta colori ecc
            //liberaSynth(selectedSynth);
            //stopTrasmissioni.Invoke(); //resetta i controlli: SMETTI DI TRASMETTERE - settare kr a false
            
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


/*    public void LiberaSynth(string synthName, bool delete){
        //var message = new OSCMessage(synthName);
        if(Synth.synthList[synthName].sequence){ //ha un sequencer
            GameManager.instance.SCsendString(synthName, "free");

            if(delete){
                resetKeys();
                resetSequencer(); //inutile chiamarla per tutti i synth perché cambia solo il sequencer del synth attuale (se esiste)
            }    
        }else //caso solo continui (theremin) - mando sia stringa che valore
            GameManager.instance.SCsendValue(synthName, "gate", 0);
    }
*/
    /*public void SCsendMsg(string synthName, string comando){ 
        var message = new OSCMessage(synthName);
        message.AddValue(OSCValue.String(comando));
        transmitter.Send(message);
    }*/

    //può avere senso farle qui e passare i gameObject perché non ho script su quei gameObject (almeno riguardo queste funzionalità)
    /*public void resetKeys(){ //solo su quelli premuti
        foreach(Transform child in GameManager.instance.tastiera.transform)
            if(child.GetComponentInChildren<MIDIkey>().getPremuto())
                child.GetComponentInChildren<MIDIkey>().onclick(); //rimuove da Lista tastiPremuti e cambia aspetto
    }
    public void resetSequencer(){
        
        
        //aspetto grafico
        if(Synth.synthList[UIGeneral.getSelection()].sequence){
            foreach(Transform child in GameManager.instance.sequencer.transform){
                if(child.GetComponentInChildren<ButtonFunc>().getPremuto()){
                    child.GetComponentInChildren<ButtonFunc>().laserInteraction(); //cambia aspetto e stato nell'oggetto Synth, poi invia un mx OSC per ogni tasto premuto
                }
            }
        }
    }*/

    //fai una funzione ti prego
    public void StopAll(){  //stoppa tutti indistintamente
        foreach(Synth synth in Synth.synthList.Values){
            synth.isPaused = false;
            if(synth.isPlaying){
                synth.isPlaying = false;
                //GameManager.instance.editModeEsciSwitch();
                
                //manda messaggio a SC per eliminare il synth e resetta colori ecc
                //liberaSynth(synth.symbol);
                GameManager.instance.LiberaSynth(synth.symbol, false);

                /*if(synth.sequence){
                    //se non faccio questa cosa, rimangono settati i btn e i valori nei sequencer dei synth che non sono quello attualmente selezionato nel dropdown(ma SC non fa suono)
                    for(int i = 0; i < synth.sequencer.Count; i++){
                        for(int j = 0; j < synth.sequencer[i].Count; j++){
                            synth.sequencer[i][j] = 0;
                        }
                    }
                }*/

                stopTrasmissioni.Invoke(); //resetta i controlli: SMETTI DI TRASMETTERE - settare kr a false
            }
        }
        aggiornaIsPlaying.Invoke();
        //cambia testo dell'UI play/stop
        gameObject.transform.parent.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = "Play";   //non cambiare posizione; andrebbe fatto in uno script a parte ma poi dovrei fare metodi statici 
        GameManager.instance.GestioneSolo();     
    }
    public void PlayAll(){ //fa partire tutti i sequenziabili programmati
        foreach(Synth synth in Synth.synthList.Values){
            if(synth.isPlaying || synth.isPaused)
              continue;
            //GameManager.instance.SCsendBoth(Synth.synthList[synth.symbol]);
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
                //GameManager.instance.SCsendString(synth.symbol, "play");    
            }
            GameManager.instance.SCsendBoth(Synth.synthList[synth.symbol]);
        }
        aggiornaIsPlaying.Invoke();
        //cambia testo dell'UI play/stop
        if(Synth.synthList[UIGeneral.getSelection()].isPlaying)
            gameObject.transform.parent.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = "Stop";   //non cambiare posizione; andrebbe fatto in uno script a parte ma poi dovrei fare metodi statici      
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
                        //GameManager.instance.SCsendNextSequence(synth.symbol); //funziona per quelli senza tastiera
                        GameManager.instance.SCSendBothNext(synth); //funziona per entrambi


                    }


                    //old version
                    //SCsendMsg(synth.symbol, "resume");
                    /*var message = new OSCMessage(synth.symbol);
                    message.AddValue(OSCValue.String("resume"));
                    transmitter.Send(message);*/
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
                    //play a SC
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
                    //SCsendMsg(synth.symbol, "pause");
                    /* var message = new OSCMessage(synth.symbol);
                    message.AddValue(OSCValue.String("pause"));
                    transmitter.Send(message); */
                }else if (!synth.isPlaying) //per il cambio battuta
                    synth.isPaused = true; 
            }
            gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "ResumeAll";
        }
        aggiornaIsPlaying.Invoke();
    }

    public void CancellaSequenza(){
        Synth synth = Synth.synthList[UIGeneral.getSelection()];
        //GameManager.instance.editModeEsciSwitch();
        GameManager.instance.LiberaSynth(synth.symbol, true);
        stopTrasmissioni.Invoke();
        aggiornaIsPlaying.Invoke();
        GameManager.instance.GestioneSolo();
        synth.isPlaying = false;
        synth.isPaused = false;
        GameManager.instance.play_stopBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Play";  

        //se non faccio questa cosa, rimangono settati i btn e i valori nei sequencer dei synth che non sono quello attualmente selezionato nel dropdown(ma SC non fa suono)
        //cancella seq di questa battuta
        if(synth.sequence)
            for(int i = 0; i < synth.sequencer[UIGeneral.getBattuta()].Count; i++)
                synth.sequencer[UIGeneral.getBattuta()][i] = 0;
    }

    public void spiega(){
        //Object.ReferenceEquals(gameObject, GameManager.instance.soloBtn)
        //Object.ReferenceEquals(gameObject, GameManager.instance.play_stopBtn)
        //Object.ReferenceEquals(gameObject, GameManager.instance.playAllBtn)
        //Object.ReferenceEquals(gameObject, GameManager.instance.stopAllBtn)
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
