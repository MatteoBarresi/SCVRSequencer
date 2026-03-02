using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System;
using extOSC;
public class ButtonFunc : MonoBehaviour
{
    //applicato a ogni press (sequencer)
 

    //[SerializeField] UnityEvent TestoImmagineEvent; //sequenza scritta settata ogni volta che premo sequencer
    [SerializeField] GameObject testoImmagine;
    [SerializeField] GameObject tastiera;
    bool isPressed;
    bool enter;
    bool editMode;
    int index; 
    Dictionary<string, List<List<int>> > chord = new Dictionary<string, List<List<int>> >(); //tasti midi programmati per questo press (gameobject) e strumento (string), per questa battuta
    [SerializeField] UnityEvent hoverOnBtn; 
    [SerializeField] UnityEvent tastoProgrammato; //ObserverProgrammato.checkProgrammato


    void Start()
    {
        isPressed = false;
        enter = false;
        editMode = false;
        index = Int32.Parse(gameObject.GetComponentInChildren<TextMeshProUGUI>().text)-1;
        foreach(string nome in Synth.synthList.Keys){
            chord.Add(nome, new List<List<int> >(){new List<int>(){}});
        }
        
    }
    
    //new input system
    void Update(){ //gestisce editMode - implicitamente funziona solo per synth con sequencer
        if(enter){
            OVRInput.Update();
            if(OVRInput.GetDown(OVRInput.Button.One)){ //premo A mentre sono hover sul btn = entra/esci da editMode
                if(gameObject.GetComponent<Renderer>().material.color == Color.red || gameObject.GetComponent<Renderer>().material.color == Color.blue){ //tasto programmato (anche vuoto)
                        editMode = !editMode; 
                        Debug.Log("editMode " + editMode);
                        unico(); //un solo btn del sequencer in editMode
                        if(editMode //&& chord[UIGeneral.getSelection()].Count > 0
                        ){
                            gameObject.GetComponent<Renderer>().material.color = Color.blue;
                            
                            //fa vedere i tasti di quello spot
                            tastiera.transform.GetChild(tastiera.transform.childCount-1).GetComponentInChildren<MIDIkey>().reset(); //chiama reset da gameObject reset - NON CAMBIARE POSIZIONE ALTRIMENTI NON FUNZIONA
                            foreach(int value in chord[UIGeneral.getSelection()][UIGeneral.getBattuta()]){ //note premute per questo btn sequencer
                                foreach(Transform tasto in tastiera.transform){
                                    if(int.TryParse(tasto.name, out var foo)){ //evita errori per il tasto RESET
                                        if(value == int.Parse(tasto.name)){ //tasti della tastiera corrispondenti alle note premute
                                            //Debug.Log(tasto.name+" "+tasto.GetComponent<MIDIkey>().getPremuto()); //***da false sia che siano premuti o meno - però funziona -- immagino perché faccio reset a riga 54
                                            if(!tasto.GetComponent<MIDIkey>().getPremuto()) //***inutile perché sono tutti false 
                                                tasto.GetComponent<MIDIkey>().onclick();
                                        }
                                    }
                                }
                            }
                        }else{ //if(!editMode)
                            editModeEsci();

                            //debug
                           /* foreach(int value in chord[UIGeneral.getSelection()])
                                Debug.Log(value);
                            Debug.Log("fine stampa");*/
                        }
                }
            }
        }
    }


/////////////////////////////////////////////////////////////////////////
    public void editModeEsci(){
        chord[UIGeneral.getSelection()][UIGeneral.getBattuta()].Clear(); //tolgo le note per questo tasto per aggiornare con quelle attualmente premute
        gameObject.GetComponent<Renderer>().material.color = Color.red;
        SCsendNotes(UIGeneral.getSelection());
    }

 
    public void hoverInteraction(){ //se ho una tastiera e il raggio entra nel bottone del sequencer, posso entrare in editMode se premo A (funzione Update)
        if(Synth.synthList[UIGeneral.getSelection()].hasKeys)
            enter = !enter;
        //Debug.Log(enter);
        if(enter){
            if(Synth.synthList[UIGeneral.getSelection()].sequence){
                if(!editMode) 
                    GameManager.instance.spiegaz.GetComponentInChildren<TextMeshProUGUI>().text = "Premi tasto select per programmare\nPremi A per vedere accordo e riprogrammare";
                else 
                    GameManager.instance.spiegaz.GetComponentInChildren<TextMeshProUGUI>().text = "Premi A per confermare";
            }
        }else GameManager.instance.spiegaz.GetComponentInChildren<TextMeshProUGUI>().text = "";
    }


    public void laserInteraction(){ // XRSimpleInteractable - Select Event (premo tasto sequencer)
        if(!editMode){ //non posso premere prima di uscire da editMode
            if(Synth.synthList[UIGeneral.getSelection()].hasKeys)
                chord[UIGeneral.getSelection()][UIGeneral.getBattuta()].Clear();
            string selectedSynth = UIGeneral.getSelection(); //per mandare OSC al selected synth
            
            //toggle
            //if(Synth.synthList[selectedSynth].sequence)
            Synth.synthList[selectedSynth].sequencer[UIGeneral.getBattuta()][index] = 
                (Synth.synthList[selectedSynth].sequencer[UIGeneral.getBattuta()][index] == 1) ? 0: 1;
            
            /*
            ///debug
            string a = "";
            foreach(int item in Synth.synthList[selectedSynth].sequencer[UIGeneral.getBattuta()]){
                a+= (item + " "); 
            }
            Debug.Log(a);
            */
            
            testoImmagine.GetComponent<testoIniziale>().setString();
            //TestoImmagineEvent?.Invoke();
            if(!isPressed){
                gestioneBottoni(true);
            }
            else{
                gestioneBottoni(false);
            }

            //spedisco pattern (se midinote, spedisco valori midi in array in un singolo slot)
            //se solo sequencer, spedisco pattern
               
            if(Synth.synthList[selectedSynth].hasKeys){ 
                //se sequencer ==1 -> trasmetti le note premute al posto dell'1 (null altrimenti)
                if(isPressed)
                    SCsendNotes(selectedSynth);
                else   
                    GameManager.instance.SCsendString(selectedSynth, "rest"); //scorre i tasti premuti e rimuove quello deselezionato 
                    //foreach (int item in MIDIkey.tastiPremuti) chord[UIGeneral.getSelection()].Remove(item); 
            }else //no midikeyboard
                GameManager.instance.SCsendSequence(selectedSynth);

            //GameManager.instance.SCsendBoth(Synth.synthList[selectedSynth]);
            tastoProgrammato.Invoke();
        }
    }

    public void SCsendNotes(string synthName){
        var message = new OSCMessage(synthName);
        List<int> valori = new List<int>(); //creo nuova lista
        foreach (int item in MIDIkey.tastiPremuti){ //copio i valori uno a uno - se faccio valori = tastiPremuti non funziona (modifica tastiPremuti quando modifico valori)
                valori.Add(item);
                //chord[UIGeneral.getSelection()].Add(item);
                chord[synthName][UIGeneral.getBattuta()].Add(item);
            }
        //aggiungo indice e mando
        valori.Insert(0, index); 
        
        foreach(int item in valori)
            message.AddValue(OSCValue.Int(item));
        GameManager.instance.transmitter.Send(message);
    }

   
    //non serve
    public void laserOut(){
        gameObject.transform.localPosition = new Vector3(0, 0.015f, 0);
      //  isPressed = false;
    }
    
    public bool getPremuto(){
        return isPressed;
    }

    public void gestioneBottoni(bool stato){ //aggiorna colore e posizione
        isPressed = stato;
        if(isPressed){
            gameObject.transform.localPosition = new Vector3(0, 0.003f, 0);
            gameObject.GetComponent<Renderer>().material.color = Color.red;

        }
        else{
            gameObject.transform.localPosition = new Vector3(0, 0.015f, 0);
            gameObject.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public bool getEditMode(){
        return editMode;
    }
    public void setEditMode(bool stato){
        this.editMode = stato;
    }

    public void AllargaChords(string synthName){
        chord[synthName].Insert(UIGeneral.getBattuta() + 1 ,new List<int>(){}); 
    }

    public void EliminaChords(string synthName){
        chord[synthName].RemoveAt(UIGeneral.getBattuta());
    }

    private void unico(){ //controlla se altri bottoni del sequencer sono in editMode (tranne questo che sto premendo)
        foreach(Transform item in gameObject.transform.parent.parent.transform){ //itera sui bottoni del sequencer
            if(item.name != gameObject.transform.parent.name){ //salta il controllo questo che ho premuto
                //Debug.Log(item.name +" "+ item.GetComponentInChildren<ButtonFunc>().getEditMode());
                
                //se sì cambia il colore a rosso (perché non può essere bianco) e metti editMode a false; 
                if(item.GetComponentInChildren<ButtonFunc>().getEditMode()){
                    item.GetComponentInChildren<ButtonFunc>().setEditMode(false);
                    //funzione per inviare a SC
                    //invia a SC nuova versione - se non lo faccio entra nell'else dell'UPDATE e lo fa in automatico?
                    item.GetComponentInChildren<ButtonFunc>().editModeEsci();
                }
            }

        }
        /*foreach(Transform item in gameObject.transform.parent.parent.transform)
            Debug.Log(item.name +" "+ item.GetComponentInChildren<ButtonFunc>().getEditMode());*/
    }

    public int getIndex(){
        return index;
    }

    public bool programmedChord(string synthName){
        return chord[synthName].Count > 0;
    }

    public List<int> getChord(string synthName){
        return chord[synthName][UIGeneral.getBattuta()];
    }
}
