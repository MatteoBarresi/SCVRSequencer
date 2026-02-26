using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Synth
{
    public string symbol;
    //public List<int> sequencer; 
    public List<List<int>> sequencer;
  //  public int[][] midiNotes;
    //public string[] controlli;
    public Dictionary<string, string> controlli = new Dictionary<string, string>(); //dizionario con coppie controllo - descrizione
    public bool sequence;
    public bool isPlayable;
    public bool isPlaying;
    public bool isPaused;
    public bool hasKeys;
    public bool isSampled;
    public static Dictionary <string, Synth> synthList = new Dictionary<string, Synth>(); //dizionario con tutti i nomi dei synth e l'oggetto corrispondente

    //costruttore per solo sequencer (controlli = Null)
    public Synth(string symbol){
        this.symbol = symbol;
        //this.sequencer = new List<int>() {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
        this.sequencer = new List<List<int>>() {Enumerable.Repeat(0, 16).ToList()};
        this.sequence = true;
        this.isPlayable = false;
        this.hasKeys = false;
        synthList.Add(this.symbol, this);
    }

    //costruttore per synth kr + seq (anche solo kr)
    public Synth(string symbol, Dictionary<string, string> controlli, bool sequence, bool isPlayable, bool hasKeys, bool isSampled){ //prima era string[] controlli
        this.symbol = symbol;
        this.controlli = controlli;
        if (sequence == true) //altrimenti è Null
        {
            //this.sequencer = new List<int>() {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
            this.sequencer = new List<List<int>>() {Enumerable.Repeat(0, 16).ToList()};
            this.sequence = true;
        }
        this.isPlaying = false; //== isPlayable anche se partono tutti muti
        this.isPlayable = isPlayable;
        this.isPaused = false;
        this.hasKeys = hasKeys;
        this.isSampled = isSampled;
      /*  this.midiNotes = new int[8][];
        for(int i = 0; i<8; i++)
            this.midiNotes[i] = new int[] {0,0,0,0,0,0,0,0,0,0,0,0};*/
        synthList.Add(this.symbol, this);
    }

    public void aggiungiBattuta(int currenBeat){
        sequencer.Insert(currenBeat + 1, Enumerable.Repeat(0, 16).ToList());
    }

    public void eliminaBattuta(int currentBeat){
        sequencer.RemoveAt(currentBeat);
    }


}
