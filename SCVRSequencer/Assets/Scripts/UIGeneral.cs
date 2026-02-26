using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;
using extOSC;
public class UIGeneral : MonoBehaviour
{
    //associato a dropdownList - fa un sacco di cose

    public GameObject leftInteractor; //drp e button?
    public GameObject rightInteractor; //drp e button?
    [SerializeField] private TMP_Dropdown dropdown; //dropdown--> va bene gameObject
    [SerializeField] private GameObject panel; //drp
    [SerializeField] private Button buttonPrefab; //drp
    
    //commentabili---
    [SerializeField] private GameObject keyboard; //drp (è il sequencer)
    [SerializeField] private GameObject midiKeyboard; //drp
    [SerializeField] private GameObject toggleButton; //playstop - usato da drp
    [SerializeField] private GameObject descImg; //drp
    //fine commentabili---

    //public OSCTransmitter transmitter;

    public static string selectedSynth="/kick"; //drp (poi viene passato tramite getSelection)
    public static int battuta = 0; //Synth.synthList[selectedSynth].sequencer[battuta][i]
    public Dictionary<string, Button[]> uibuttons = new Dictionary<string, Button[]>(); //array di UI btn (controlli) per ogni synthName

    [SerializeField] private UnityEvent gestioneBeatButtons;

    void Start() //gestisce la prima selezione
    {
        foreach(KeyValuePair<string, Synth> item in Synth.synthList) //itera per aggiungerli alla lista, prima di fare dropDownLog
            dropdown.options.Add(new TMP_Dropdown.OptionData(item.Key));
        dropdown.RefreshShownValue();
        
      //  if(gameObject.name=="Dropdown") //IF inutile perché lo usa solo dropdown
        creaControlli();
        dropDownLog(); //solo questo resterà
    }

    //DROPDOWN
    //si occupa dei controlli + play/stop
    public void dropDownLog(){ //quando scelgo un symbol, su console il nome + indice
        Debug.Log(gameObject.name);
        //destroyButtons();
        //editModeEsci();
        //ButtonFunc.editModeEsciSwitch(keyboard);
        GameManager.instance.editModeEsciSwitch();
        int index = dropdown.value;
        string voceTesto = dropdown.options[index].text; //options è visto come un array
        selectedSynth = voceTesto;
        Debug.Log(index + " " + voceTesto);


        //I CONTROLLI SU isREPRODUCING CONVIENE FARLI QUI SE POSSIBILE E NON SEPARATI IN OGNI FUNZIONE


        toggleControlli(selectedSynth);
        //fai apparire le cose del synth (se il sequencer è null non lo fa apparire)
        toggleSequencer(selectedSynth);
        togglePlayable(selectedSynth);
        toggleCancellaSeq(selectedSynth);
        toggleMidi(selectedSynth);
        toggleDesc(selectedSynth);


        //premo con mano dx su dropdown--> risulta true anche se sparisce (finché non entro e ri-esco su un UI)--> selezione left non possibile
        //quindi serve questo
        leftInteractor.GetComponent<ProvaLaser>().selecting = false;
        rightInteractor.GetComponent<ProvaLaser>().selecting = false;

        //se lo strumento è in play, mostra soloBtn
        GameManager.instance.GestioneSolo();


    }

   /* ---------funzione unica in ButtonFunc
   private void editModeEsci(){
        if(Synth.synthList[selectedSynth].hasKeys)
            foreach(Transform item in keyboard.transform){
                if(item.GetComponentInChildren<ButtonFunc>().getEditMode()){
                    item.GetComponentInChildren<ButtonFunc>().setEditMode(false);
                    item.GetComponentInChildren<ButtonFunc>().editModeEsci();
                }
            }
    }*/

    
    public void destroyButtons(){ //elimina bottoni figli di panel 
        while(panel.transform.childCount > 0){
            DestroyImmediate(panel.transform.GetChild(0).gameObject);
        }
    }

    public static string getSelection(){
        return selectedSynth;
    }
    public static int getBattuta(){
        return battuta;
    }
    public static void setBattuta(int nuovaBattuta){
        battuta = nuovaBattuta;
    }



    private void creaControlli(){ //crea i GameObject degli UI btn per ogni controllo dell'oggetto Synth
        foreach(KeyValuePair<string, Synth> item in Synth.synthList){
            string synth = item.Key; //stringa
            //string[] krArr = Synth.synthList[synth].controlli; //versione senza dizionario e descrizioni
            List<string> krArr = new List<string>(); //contiene i nomi dei controlli
            foreach(string controllo in Synth.synthList[synth].controlli.Keys)
                krArr.Add(controllo); //.Keys
            Button[] btnkr = new Button[krArr.Count]; //contiene i bottoni
            if(krArr != null){
                for(int i = 0; i < krArr.Count; i++){ //for per spaziarli in modo equo
                //crea button
                Button btn = Instantiate(buttonPrefab, 
                new Vector3(0,0,0),  //panel.GetComponent<RectTransform>().localPosition, 
                Quaternion.identity, panel.transform);
                //in alternativa usare component layout
                btn.GetComponent<RectTransform>().anchoredPosition = new Vector2(80 * (i%4 - 1), (int)((i)/4) * -40); //pixel
                btn.GetComponent<RectTransform>().localScale = new Vector3(0.4f, 0.4f, 0.4f);
                //cambia testo sul button
                btn.GetComponentInChildren<TextMeshProUGUI>().text = krArr[i]; 
                
                //bool kr = false;
                
                //per quando vengono spawnati di nuovo --> farne una funzione
                //if (kr) btn.GetComponent<Image>().color = Color.green;
                //else btn.GetComponent<Image>().color = Color.white;

                //btn.GetComponent<krScript>().testo = descImg; //assegna il gameobj su cui visualizzare le descrizioni

                //funzione onclick dei  controlli
                btn.onClick.AddListener(()=>bottoneCliccato(btn, synth)
                /*    ()=> {
                    if(btn.GetComponent<Image>().color == Color.white)
                    Debug.Log("ora stai controllando " + btn.GetComponentInChildren<TextMeshProUGUI>().text); //mandare /n_set [nodeId, btn.GetComponentInChildren<TextMeshProUGUI>().text, valore]
                    else Debug.Log("NON CONTROLLI più");
                   
                   //entra solo se 1 solo interactor ha selecting == true (xor)
                   if(leftInteractor.GetComponent<ProvaLaser>().selecting ^ 
                        rightInteractor.GetComponent<ProvaLaser>().selecting) 
                   {
                    Debug.Log("1 solo hover");
                    kr = !kr; //se è true, puoi trasmettere
                    Debug.Log("component del button " + kr);

                    //btn.GetComponentInChildren<TextMeshProUGUI>().text è il controllo
                    if(kr){
                        btn.GetComponent<Image>().color = Color.green;
                        if(leftInteractor.GetComponent<ProvaLaser>().selecting)
                        {
                            Debug.Log("segui la mano sinistra!!!");
                            //keyboard.SetActive(false);
                            StartCoroutine(SCsend(leftInteractor, btn, selectedSynth));
                        }
                        else if(rightInteractor.GetComponent<ProvaLaser>().selecting)
                        {
                            Debug.Log("segui la mano destra!!!");
                            //keyboard.SetActive(false);
                            StartCoroutine(SCsend(rightInteractor, btn, selectedSynth));
                        }
                        //avvia bus su supercollider
                        var message = new OSCMessage(selectedSynth);
                        message.AddValue(OSCValue.String(btn.GetComponentInChildren<TextMeshProUGUI>().text));
                        message.AddValue(OSCValue.String("start"));  
                        transmitter.Send(message);

                    }else{
                        //funzione
                        btn.GetComponent<Image>().color = Color.white;
                        Debug.Log("non seguire nessuno");
                        var message = new OSCMessage(selectedSynth);
                        message.AddValue(OSCValue.String(btn.GetComponentInChildren<TextMeshProUGUI>().text));
                        message.AddValue(OSCValue.String("stop"));  
                        transmitter.Send(message);
                    }  

                   }else {
                    Debug.Log("non possibile");
                    if(kr){
                        btn.GetComponent<Image>().color = Color.green; //se seguiva qualcosa, rimane verde

                    }else {
                        btn.GetComponent<Image>().color = Color.red;
                        StartCoroutine(cambioColore(btn));      
                        }
                   }
                    if(checkTrasmissione())
                       keyboard.SetActive(false);
                    else if(Synth.synthList[synth].sequence) keyboard.SetActive(true);           
                }*/
                );

                
                ////
                btnkr[i] = btn;
                }
                uibuttons.Add(synth, btnkr);
            }
        }
    }


    //funzione che gestisce toggle controlli--> gli passo il synth: se ha controlli, li mostra
    private void toggleControlli(string synth){
        //itera synth e mostra i btn di quello attivo (synth)
        foreach(KeyValuePair<string, Button[]> item in uibuttons){
            if(synth == item.Key){//mostra i bottoni
                foreach(Button btn in item.Value)
                    btn.gameObject.SetActive(true);
            }else foreach(Button btn in item.Value)
                btn.gameObject.SetActive(false);
        }
    

      /*  string[] krArr =  Synth.synthList[synth].controlli;
         if(krArr != null){
            foreach(string item in krArr)
                Debug.Log(item);
            for(int i = 0; i < krArr.Length; i++){
                //crea button
                Button btn = Instantiate(buttonPrefab, 
                new Vector3(i,1,0),  //panel.GetComponent<RectTransform>().localPosition, 
                Quaternion.identity, panel.transform);
                
                //cambia testo sul button
                btn.GetComponentInChildren<TextMeshProUGUI>().text = krArr[i]; 
                bool kr = false;
                //per quando vengono spawnati di nuovo --> farne una funzione
                if (kr) btn.GetComponent<Image>().color = Color.green;
                else btn.GetComponent<Image>().color = Color.white;

                //funzione onclick dei  controlli
                btn.onClick.AddListener(()=> {
                   if(btn.GetComponent<Image>().color == Color.white)
                    Debug.Log("ora stai controllando " + btn.GetComponentInChildren<TextMeshProUGUI>().text); //mandare /n_set [nodeId, btn.GetComponentInChildren<TextMeshProUGUI>().text, valore]
                    else Debug.Log("NON CONTROLLI più");
                   
                   //entra solo se 1 solo interactor ha selecting == true (xor)
                   if(leftInteractor.GetComponent<ProvaLaser>().selecting ^ 
                        rightInteractor.GetComponent<ProvaLaser>().selecting) 
                   {
                    Debug.Log("1 solo hover");
                    kr = !kr; //se è true, puoi trasmettere
                    Debug.Log("component del button " + kr);

                    //btn.GetComponentInChildren<TextMeshProUGUI>().text è il controllo
                    if(kr){
                        btn.GetComponent<Image>().color = Color.green;
                        if(leftInteractor.GetComponent<ProvaLaser>().selecting)
                        {
                            Debug.Log("segui la mano sinistra!!!");
                            //keyboard.SetActive(false);
                            StartCoroutine(SCsend(leftInteractor, btn));
                        }
                        else if(rightInteractor.GetComponent<ProvaLaser>().selecting)
                        {
                            Debug.Log("segui la mano destra!!!");
                            //keyboard.SetActive(false);
                            StartCoroutine(SCsend(rightInteractor, btn));
                        }

                    }else{
                        btn.GetComponent<Image>().color = Color.white;
                        Debug.Log("non seguire nessuno");
                        
                    }  

                   }else {
                    Debug.Log("non possibileeeeeeeeeeeee");
                    if(kr){
                        btn.GetComponent<Image>().color = Color.green; //se seguiva qualcosa, rimane verde

                    }else {
                        btn.GetComponent<Image>().color = Color.red;
                        StartCoroutine(cambioColore(btn));      
                        }
                   }
                    if(checkTrasmissione())
                       keyboard.SetActive(false);
                    else keyboard.SetActive(true);           
                });
                
            }
        
        }
        else Debug.Log("no controlli");*/


    }

    public void NascondiControlli(){
        foreach(KeyValuePair<string, Button[]> item in uibuttons)
            foreach(Button btn in item.Value)
                btn.gameObject.SetActive(false);
    }

    public void bottoneCliccato(Button btn, string synth){ //kr UI
        Color coloreIniziale = btn.GetComponent<Image>().color;
        if(coloreIniziale == Color.white){
            //Debug.Log("ora stai controllando " + btn.GetComponentInChildren<TextMeshProUGUI>().text); //mandare /n_set [nodeId, btn.GetComponentInChildren<TextMeshProUGUI>().text, valore]
            //btn.GetComponent<Image>().color = Color.green;
        }
        else {
            //Debug.Log("NON CONTROLLI più");
            //btn.GetComponent<Image>().color = Color.white;
        } 
        
        //entra solo se 1 solo interactor ha selecting == true (xor)
        if((leftInteractor.GetComponent<ProvaLaser>().selecting ^ 
            rightInteractor.GetComponent<ProvaLaser>().selecting) 
            ||
            (!rightInteractor.GetComponent<ProvaLaser>().selecting && !leftInteractor.GetComponent<ProvaLaser>().selecting) //caso in cui la funzione viene richiamata da codice quanto premo play/stop
            )
        {
        //Debug.Log("1 solo hover");
        //kr = !kr; //se è true, puoi trasmettere
        //Debug.Log("component del button " + kr);

        //btn.GetComponentInChildren<TextMeshProUGUI>().text è il controllo
        if(btn.GetComponent<Image>().color == Color.white){
            btn.GetComponent<Image>().color = Color.green;
            if(leftInteractor.GetComponent<ProvaLaser>().selecting)
            {
                //Debug.Log("segui la mano sinistra!!!");
                //keyboard.SetActive(false);
                StartCoroutine(SCsend(leftInteractor, btn, selectedSynth));
            }
            else if(rightInteractor.GetComponent<ProvaLaser>().selecting)
            {
                //Debug.Log("segui la mano destra!!!");
                //keyboard.SetActive(false);
                StartCoroutine(SCsend(rightInteractor, btn, selectedSynth));
            }
            //avvia bus su supercollider
            //scsend tipo (synthName, kr, start)
            GameManager.instance.SCsendString(selectedSynth, btn.GetComponentInChildren<TextMeshProUGUI>().text, "start");
            /*var message = new OSCMessage(selectedSynth);
            message.AddValue(OSCValue.String(btn.GetComponentInChildren<TextMeshProUGUI>().text));
            message.AddValue(OSCValue.String("start"));  
            transmitter.Send(message);
*/
        }else{
            //funzione
            btn.GetComponent<Image>().color = Color.white;
            //Debug.Log("non seguire nessuno");
            //scsend tipo (synthName, kr, stop)
            GameManager.instance.SCsendString(selectedSynth, btn.GetComponentInChildren<TextMeshProUGUI>().text, "stop");
            /*var message = new OSCMessage(selectedSynth);
            message.AddValue(OSCValue.String(btn.GetComponentInChildren<TextMeshProUGUI>().text));
            message.AddValue(OSCValue.String("stop"));  
            transmitter.Send(message);*/
        }  

        }else {
            //Debug.Log("non possibile");
            if(btn.GetComponent<Image>().color == Color.green){
                btn.GetComponent<Image>().color = Color.green; //se seguiva qualcosa, rimane verde
            }else {
                btn.GetComponent<Image>().color = Color.red;
                StartCoroutine(cambioColore(btn));
            }
        }
        if(checkTrasmissione())
            //keyboard.SetActive(false);
            GameManager.instance.sequencer.SetActive(false);
        else if(Synth.synthList[synth].sequence) 
            //keyboard.SetActive(true);           
            GameManager.instance.sequencer.SetActive(true);
    }


    IEnumerator cambioColore(Button btn){ //da x a rosso per qualche secondo
        //yield return new WaitForSeconds(1);
        //waitwhile sospende la coroutine finché la condizione non diventa false
        yield return new WaitWhile(()=> !(leftInteractor.GetComponent<ProvaLaser>().selecting ^ rightInteractor.GetComponent<ProvaLaser>().selecting)); //funzione perché deve essere valutata ogni secondo?
        if(btn.GetComponent<Image>().color == Color.red)
            btn.GetComponent<Image>().color = Color.white;
    }

    IEnumerator SCsend(GameObject interactor, Button btn, string synth){
        while(btn.GetComponent<Image>().color == Color.green)
        {
            //OSCHandler.Instance.SendMessageToClient("SuperCollider",selectedSynth, interactor.transform.position.x);
            //scsend tipo (synthName, kr, float)
            //var message = new OSCMessage(synth); //selectedSynth
            //message.AddValue(OSCValue.String(btn.GetComponentInChildren<TextMeshProUGUI>().text));
            if(interactor == leftInteractor)
                GameManager.instance.SCsendValue(synth, btn.GetComponentInChildren<TextMeshProUGUI>().text, 0.6f - interactor.transform.position.x);
                //message.AddValue(OSCValue.Float(0.6f - interactor.transform.position.x));
            else 
                GameManager.instance.SCsendValue(synth, btn.GetComponentInChildren<TextMeshProUGUI>().text, interactor.transform.position.x);
                //message.AddValue(OSCValue.Float(interactor.transform.position.x));
            //transmitter.Send(message);
            //Debug.Log("interactor: " + interactor + " " + interactor.transform.position.x);
            yield return new WaitForSeconds(0.5f);

        }
    }

    //funzione che gestisce toggle sequencer--> gli passo il synth: se ha un sequencer, lo mostra
    private void toggleSequencer(string synth){
        if(!AscoltaComposizione.isReproducing){
            if(!Synth.synthList[synth].sequence) {
                keyboard.SetActive(false);
                //GameManager.instance.GestioneBottoniBattuta(false);
            }
            else {
                /*for(int i = 0; i< Synth.synthList[synth].sequencer[getBattuta()].Count; i++)
                {
                    GameObject g;
                    g = keyboard.transform.GetChild(i).gameObject.transform.GetChild(1).gameObject; //press
                    if (Synth.synthList[synth].sequencer[getBattuta()][i] == 1)
                        g.GetComponent<ButtonFunc>().gestioneBottoni(true);
                    else g.GetComponent<ButtonFunc>().gestioneBottoni(false);
                }*/
                GameManager.instance.SequencerButtons();
                //se non ho kr attivi
                if(checkTrasmissione()){
                    keyboard.SetActive(false);
                    //GameManager.instance.GestioneBottoniBattuta(false); 
                }
                else{
                    keyboard.SetActive(true);
                    //keyboard.transform.localScale = GameManager.instance.grandezze[keyboard];
                    //GameManager.instance.GestioneBottoniBattuta(true); 

                } 
                    
            }
        }//controllo per /piano
        /*else if(!Synth.synthList[synth].sequence && !Synth.synthList[synth].isPlayable){
            keyboard.SetActive(true);
        }*/
        gestioneBeatButtons.Invoke();   
    }

    private bool checkTrasmissione(){//true = sto trasmettendo con il synth attuale
        bool check = false; 
        foreach(Button btn in uibuttons[selectedSynth]){
            if(btn.GetComponent<Image>().color == Color.green)
                check = true;
        }
        return check;
    }

    //funzione che gestisce toggle play/stop--> gli passo il synth ecc
    private void togglePlayable(string synth){
        if(
            (Synth.synthList[synth].isPlayable && !AscoltaComposizione.isReproducing) 
            || 
            (AscoltaComposizione.isReproducing && !(Synth.synthList[synth].sequence || Synth.synthList[synth].hasKeys)) 
        ){ //mostra il bottone
            //toggleButton.SetActive(true);
            GameManager.instance.play_stopBtn.SetActive(true);
            if(Synth.synthList[selectedSynth].isPlaying) //tasto premuto--> manda messaggio a SC
                //toggleButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Stop"; 
                GameManager.instance.play_stopBtn.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Stop"; 
            //else toggleButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Play";
            else GameManager.instance.play_stopBtn.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Play";
        }
        else 
            //toggleButton.SetActive(false);
            GameManager.instance.play_stopBtn.SetActive(false);
    }

    private void toggleCancellaSeq(string synth){
        GameManager.instance.cancellaSeqBtn.SetActive(Synth.synthList[synth].sequence && !AscoltaComposizione.isReproducing ? true : false);
    }

    private void toggleMidi(string synth){
        var tastiera = GameManager.instance.tastiera;
        if(Synth.synthList[synth].hasKeys){
            //midiKeyboard.SetActive(true);
            tastiera.SetActive(true);
            tastiera.transform.localScale = GameManager.instance.grandezze[tastiera];

            if(synth == "/piano"){ //nasconde il tasto reset
                //midiKeyboard.transform.GetChild(midiKeyboard.transform.childCount-1).gameObject.SetActive(false);
                tastiera.transform.GetChild(tastiera.transform.childCount-1).gameObject.SetActive(false);
                
                MIDIkey.resetPiano(tastiera/*midiKeyboard*/);
                if(AscoltaComposizione.isReproducing)
                    tastiera.transform.localScale = GameManager.instance.grandezze[tastiera];
            }
            else  
                /*midiKeyboard*/tastiera.transform.GetChild(/*midiKeyboard*/tastiera.transform.childCount-1).gameObject.SetActive(true);
        }
        else /*midiKeyboard*/
        //tastiera.SetActive(false);
            tastiera.transform.localScale = Vector3.zero;
        

    }

    private void toggleDesc(string synth){
        if(Synth.synthList[synth].controlli.Keys.Count > 1)
            /*descImg*/GameManager.instance.spiegaz.SetActive(true);
        else /*descImg*/GameManager.instance.spiegaz.SetActive(false);

    }

    public void stop(){ //funzione chiamata quando premo stop sul synth attuale
        foreach(Button btn in uibuttons[selectedSynth]){
            if(btn.GetComponent<Image>().color == Color.green){
                btn.GetComponent<Button>().onClick.Invoke(); 
            }
                
        }
    }
    public void stopAll(){
        foreach(Synth synth in Synth.synthList.Values){
            foreach(Button btn in uibuttons[synth.symbol]){
                if(btn.GetComponent<Image>().color == Color.green){
                    btn.GetComponent<Button>().onClick.Invoke(); 
                }
                    
            }

        }

    }

    public void liberaSynth(string synthName){
        //scsend tipo (synthName, string)
        //var message = new OSCMessage(synthName);
        if(Synth.synthList[synthName].sequence){ //ha un sequencer
            GameManager.instance.SCsendString(synthName, "free");
            //message.AddValue(OSCValue.String("free"));  
            //transmitter.Send(message);
        }else{ //caso solo continui (theremin)
            //scsend tipo (synthName, kr, value)
            GameManager.instance.SCsendValue(synthName, "gate", 0);

            /*var message = new OSCMessage(synthName);
            message.AddValue(OSCValue.String("gate"));
            message.AddValue(OSCValue.Int(0));
            transmitter.Send(message);*/
        }
    } 
    



  
    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        foreach(KeyValuePair<string, Button[]> item in uibuttons){
            foreach(Button btn in item.Value)
                if(btn.GetComponent<Image>().color == Color.green)
                    btn.GetComponent<Button>().onClick.Invoke();   
        };
        //gate e free (tipo premi stop se isPlaying)
        foreach(string synthName in Synth.synthList.Keys)
            {
                //if(Synth.synthList[synthName.isPlaying])
                liberaSynth(synthName);
            }
    }

}
