using System.Collections.Generic;
using System.Linq;

public class Synth
{
    public string symbol {get; private set;}
    public List<List<int>> sequencer {get; private set;}
    public Dictionary<string, string> controlli {get; private set;}//= new Dictionary<string, string>(); //dict<nomeKr, descKr>
    public bool sequence {get; private set;}
    public bool isPlayable {get; private set;}
    public bool isPlaying {get; set;}
    public bool isPaused {get; set;}
    public bool hasKeys {get; private set;}
    public bool isSampled {get; private set;}
    public static Dictionary <string, Synth> synthList {get; private set;}//= new Dictionary<string, Synth>(); //dict<nomeSynth, oggSynth> 

    //costruttore per solo sequencer (controlli = Null)
    public Synth(string symbol){
        this.symbol = symbol;
        
        this.sequencer = new List<List<int>>() {Enumerable.Repeat(0, 16).ToList()};
        this.sequence = true;
        this.isPlayable = false;
        this.hasKeys = false;
        synthList.Add(this.symbol, this);
    }

    //costruttore kr + seq (anche solo kr)
    public Synth(string symbol, Dictionary<string, string> controlli, bool sequence, bool isPlayable, bool hasKeys, bool isSampled){
        this.symbol = symbol;
        this.controlli = controlli;
        if (sequence == true) //altrimenti è Null
        {
            this.sequencer = new List<List<int>>() {Enumerable.Repeat(0, 16).ToList()};
            this.sequence = true;
        }
        this.isPlaying = false; //== isPlayable anche se partono tutti muti
        this.isPlayable = isPlayable;
        this.isPaused = false;
        this.hasKeys = hasKeys;
        this.isSampled = isSampled;
        synthList.Add(this.symbol, this);
    }

    public void aggiungiBattuta(int currenBeat){
        sequencer.Insert(currenBeat + 1, Enumerable.Repeat(0, 16).ToList());
    }

    public void eliminaBattuta(int currentBeat){
        sequencer.RemoveAt(currentBeat);
    }


}
