using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using extOSC;

public class Director : MonoBehaviour
{
    //unity: xyz
    //sc   : yzx
    //sc asse z -> 0 = davanti; pi/2 = sx; -pi/2 = dx; +-pi = dietro
    //sc asse y -> 0 = davanti; pi/2 = sopra; -pi/2 = sotto; +-pi = dietro

    [SerializeField]
    private GameObject prefabStrumento; //prefab
    public UnityEvent gestioneDropdown; //UIGeneral.dropdownLog
    public UnityEvent gestioneKr; //UIGeneral.nascondiControlli
   //[SerializeField] private GameObject sequencer; //per uscire da editMode //GameManager.Esci tipo
    //public GameObject prova;
    private GameObject strumento;
    private Dictionary<string, GameObject> strumenti = new Dictionary<string, GameObject>();

    private Vector3 distanza;
    private Vector3 distanzaProva;
    private bool directorMode;
    //private Vector3 distanzaR;
    
    private float rotate;
    private float tumble;
    private float distance;

    //debug
    GameObject miacamera;

    void Start()
    {
        directorMode = false;
        rotate = 0;
        tumble = 90;
        miacamera = GameObject.FindWithTag("miacamera");
        
        //per ogni synth, crea un'immagine/ modello e mettili in fila
        /*strumento = Instantiate(prefabStrumento, 
                new Vector3(0,0,1.5f),//gameObject.transform  
                Quaternion.identity);*/
        
        //i prefab devono essere rivolti sempre verso la camera (lookAt o Quaternion.LookRotation(vettore differenza tra camera e prefab))
        //caso lookAt: transform.LookAt(transform verso cui rivolgersi)

        //nascondi
        //strumento.SetActive(false);
        var i = 0.0f;
        foreach(string synthName in Synth.synthList.Keys){
            strumenti.Add(synthName, Instantiate(prefabStrumento, 
                new Vector3(i,0,1.5f),//gameObject.transform  
                Quaternion.identity));
                strumenti[synthName].SetActive(false);
                strumenti[synthName].GetComponentInChildren<TextMeshProUGUI>().text = synthName;
                i = i + 0.3f;
        }
        StartCoroutine(SCsend());

    }

    
    void Update()
    {



        //distanzaProva = miacamera.transform.InverseTransformPoint(strumento.transform.position);
        //------distanza = Camera.main.transform.InverseTransformPoint(strumento.transform.position);
        //distanza = miacamera.transform.InverseTransformPoint(strumento.transform.position);
        
        //Camera.main.transform.eulerAngles.y //0 : 360
        //distanza = strumento.transform.position - miacamera.transform.position;
        //per disegnare (debug) devo avere punto di inizio e di fine, quindi a distanzaR devo sommare il vec della camera
        
        //distanzaR = (Quaternion.Euler(Camera.main.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, Camera.main.transform.eulerAngles.z) * distanza); //distanza + rotazione
        //distanzaR = (Quaternion.Euler(miacamera.transform.eulerAngles.x, miacamera.transform.eulerAngles.y, miacamera.transform.eulerAngles.z) * distanza); //distanza + rotazione
        
        //------rotate = Mathf.Atan2(distanza.x, distanza.z) * Mathf.Rad2Deg;
        //Debug.Log("rotate: " + (Mathf.Atan2(distanza.x, distanza.z) * Mathf.Rad2Deg));
        //Debug.Log("z prova: " + (Mathf.Atan2(distanzaProva.x, distanzaProva.z) * Mathf.Rad2Deg));
        
        //------tumble = Mathf.Atan2(distanza.y, Mathf.Sqrt(distanza.x * distanza.x + (distanza.z * distanza.z))) * Mathf.Rad2Deg;
        //Debug.Log("elevazione: " + (Mathf.Atan2(distanza.y, Mathf.Sqrt(distanza.x * distanza.x + (distanza.z * distanza.z))) * Mathf.Rad2Deg));

        //tumble = Mathf.Atan2(distanza.z, distanza.y) * Mathf.Rad2Deg;
        //Debug.Log("tumble: " + ((Mathf.Atan2(distanza.z, distanza.y) * Mathf.Rad2Deg)));
        //Debug.Log("y prova: " + ((Mathf.Atan2(distanzaProva.z, distanzaProva.y) * Mathf.Rad2Deg)));
        
        //Debug.Log("distanzaR: "+distanzaR);
        
 
        //GameManager.instance.SCsendValue("/director", rotate, tumble);
        //Debug.Log(Camera.main.transform.rotation);
        //Debug.Log("x prefab " + strumento.transform.forward.x); //-1.0 : 1.0; rot y per qualche motivo
        //strumento.transform.LookAt(Camera.main.transform);
        //Debug.Log("angolo " + Quaternion.Angle(Camera.main.transform.rotation, strumento.transform.rotation));
        
        /*foreach(KeyValuePair<string, GameObject> strumento in strumenti){
            if(Synth.synthList[strumento.Key].isPlaying){
                Debug.Log(strumento.Key + " distanza in update: "+ Vector3.Distance(strumento.Value.transform.position, Camera.main.transform.position));
            }
        }*/
    }


    //oltre rotate e tumble anche Vector3.Distance(strumento.transform.position, Camera.main.transform.position)
    
    IEnumerator SCsend(){ //va sempre, perché anche se non vedo gli strumenti, se giro la testa, deve cambiare la percezione
       while(true){ //directorMode
        foreach(KeyValuePair<string, GameObject> strumento in strumenti){
            if(Synth.synthList[strumento.Key].isPlaying && !Synth.synthList[strumento.Key].isPaused){
                distanza = Camera.main.transform.InverseTransformPoint(strumento.Value.transform.position);
                rotate = Mathf.Atan2(distanza.x, distanza.z) * Mathf.Rad2Deg;
                tumble = Mathf.Atan2(distanza.y, Mathf.Sqrt(distanza.x * distanza.x + (distanza.z * distanza.z))) * Mathf.Rad2Deg;
                GameManager.instance.SCsendValue(strumento.Key, "rotate", rotate);
                GameManager.instance.SCsendValue(strumento.Key, "tumble", tumble);
                GameManager.instance.SCsendValue(strumento.Key, "distanza", Vector3.Distance(strumento.Value.transform.position, Camera.main.transform.position));
            }
        }
        yield return new WaitForSeconds(0.08f);
       } 
    }

    public void Direction(){
        GameManager.instance.editModeEsciSwitch(); //ButtonFunc.editModeEsciSwitch(tastiera);
        directorMode = !directorMode;
        foreach(GameObject strumento in strumenti.Values)
            strumento.SetActive(directorMode);
        //StartCoroutine(SCsend());

        gestioneKr.Invoke();
        //da nascondere: keyboard, sequencer, tasto play/stop centrale, dropdown, tabellone spiegazza, tasto solo
        NascondiCose(directorMode);
        



        //GameManager.instance.SCsendKr()
        if(!directorMode){
            gestioneDropdown.Invoke();
            GameManager.instance.dropdown.SetActive(true);
            //GameManager.instance.soloBtn.SetActive(true);
            GameManager.instance.GestioneSolo();
        }
    }

    public void spiegaEnter(){
        GameManager.instance.spiegaz.GetComponentInChildren<TextMeshProUGUI>().text = "organizza il suono nello spazio.\nTrigger per fare grab, \nanalogico per avvicinare o allontanare";
    }
    public void spiegaEsci(){
        GameManager.instance.spiegaz.GetComponentInChildren<TextMeshProUGUI>().text = "";
    }

    void NascondiCose(bool stato){
        
        GameManager.instance.delBtn.transform.parent.gameObject.SetActive(false);
        GameManager.instance.addBtn.transform.parent.gameObject.SetActive(false);
        GameManager.instance.forwardBtn.transform.parent.gameObject.transform.localScale = Vector3.zero;
        GameManager.instance.backBtn.transform.parent.gameObject.transform.localScale = Vector3.zero;
       
        GameManager.instance.spiegaz.transform.localScale   =       stato ? Vector3.zero : GameManager.instance.grandezze[GameManager.instance.spiegaz]     ; 
        GameManager.instance.tastiera.transform.localScale  =       stato ? Vector3.zero : GameManager.instance.grandezze[GameManager.instance.tastiera]    ; 
        GameManager.instance.sequencer.transform.localScale =       stato ? Vector3.zero : GameManager.instance.grandezze[GameManager.instance.sequencer]   ; 
        GameManager.instance.soloBtn.transform.localScale   =       stato ? Vector3.zero : GameManager.instance.grandezze[GameManager.instance.soloBtn]     ; 
        GameManager.instance.play_stopBtn.transform.localScale =    stato ? Vector3.zero : GameManager.instance.grandezze[GameManager.instance.play_stopBtn]; 
        GameManager.instance.dropdown.transform.localScale      =   stato ? Vector3.zero : GameManager.instance.grandezze[GameManager.instance.dropdown]    ; 

    }




}
