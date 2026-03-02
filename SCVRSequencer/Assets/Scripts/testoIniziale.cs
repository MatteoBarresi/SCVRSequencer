using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class testoIniziale : MonoBehaviour
{
    //usato in testoSeq per settare i valori di ogni sequencer
    
    void Start()
    {
        GameManager.instance.listen();
        setString();
    }
    void Update(){
        //gestibile con eventi in play/stop (UI) e buttonfunc
        setString();
    }

   
    public void setString(){ //play(all)/stop(all); cambio battuta; premo sequencer; cambia beat (clock SC)
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

