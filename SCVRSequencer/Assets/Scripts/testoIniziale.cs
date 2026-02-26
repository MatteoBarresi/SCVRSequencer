using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using extOSC;
public class testoIniziale : MonoBehaviour
{
    //usato in testoSeq per settare i valori di ogni sequencer

    private int currentBeat; //punto in cui siamo in questa battuta
    
    void Start()
    {
        //listen();
        GameManager.instance.listen();
        setString();
    }
    void Update(){
        setString();
    }

    /*public void listen(){
        GameManager.instance.receiver.Bind("/beat", (OSCMessage message)=> {
            int beat = (int)message.Values[0].FloatValue;
            currentBeat = beat-1;

            gestioneLedSequencer(beat); //non faccio -1 perché i bottoni partono da 1

        });
    }

    void gestioneLedSequencer(int beat){
        //parte che gestisce i colori
        foreach(Transform btn in GameManager.instance.sequencer.transform){
            if (int.Parse(btn.name) == beat){
                btn.GetChild(2).GetComponent<MeshRenderer>().material = GameManager.instance.coloreBeat;
            }else 
                btn.GetChild(2).GetComponent<MeshRenderer>().material = GameManager.instance.coloreBase;
        }
    }*/


    //sequencer[UIGeneral.getBattuta()]
    public void setString(){
        gameObject.GetComponent<TextMeshProUGUI>().text = "";
        int counter = 0;
        foreach(KeyValuePair<string, Synth> item in Synth.synthList){
            if(item.Value.sequence){ //se il synth ha la sequenza, itera nei valori e modifica la stringa
                string valori = "";
                
                for(int i = 0; i < item.Value.sequencer[UIGeneral.getBattuta()].Count; i++){
                    if(i == GameManager.instance.GetCurrentBeat())
                        valori += "<color=\"green\">"+item.Value.sequencer[UIGeneral.getBattuta()][i]+"</color>" + " ";
                    else valori += "<color=\"black\">"+item.Value.sequencer[UIGeneral.getBattuta()][i]+"</color>" + " ";
                }

                gameObject.GetComponent<TextMeshProUGUI>().text += isPlaying(item.Key) + " : \t";
                if(item.Key.Length < 5)  
                    gameObject.GetComponent<TextMeshProUGUI>().text += "\t" + valori + "\n";
                else 
                    gameObject.GetComponent<TextMeshProUGUI>().text += valori + "\n";
                counter ++;
            }
        }
    }

    private string isPlaying(string symbol){
        if(Synth.synthList[symbol].isPaused) //prima cosa da controllare perché anche se in pausa non metto isPlaying a false
            return "<color=\"yellow\">" + symbol +"</color>";
        else if(Synth.synthList[symbol].isPlaying)
            return "<color=\"green\">" + symbol +"</color>";
        return symbol;
    }

}

