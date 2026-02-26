using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using System.IO;
using System.Linq;
using TMPro;
using extOSC;

public class GameMenu : MonoBehaviour
{

    //come attributi ha 
    // - prefab voce (cambia testo e percorso) - poi quando premo il tasto apre altro panel ("file system"), quindi aggiungere funzione anche da inspector
    // - padre del prefab (PaginaOpzioni)

    public static GameMenu instance;
    List<FileInfo> localPaths;
    string selectedInstrument;
    [SerializeField] GameObject vocePrefab;
    [SerializeField] Button pathBtnPrefab;
    [SerializeField] GameObject paginaPrincipale;
    [SerializeField] GameObject paginaOpzioni;
    [SerializeField] GameObject paginaNaviga;
    [SerializeField] GameObject navigaContent;
    [SerializeField] GameObject opzioniContent;


    void Awake(){
        //singleton? libro c# unity - prende il primo child sotto questo transform
    }
    
    
    void Start()
    {
        //fare path relativo in modo che funzioni sempre (Directory.GetCurrentDirectory o Environment.GetCurrentDirectory)
//        localPaths = Directory.GetFiles(@"C:\Users\vrace\Desktop", "*.*", SearchOption.AllDirectories)/*.AsEnumerable()*/.Where(s => s.EndsWith(".wav") || s.EndsWith(".mp3")).Take(40).ToList();
        localPaths = new List<FileInfo>();

        DirectoryInfo cartella = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        foreach(FileInfo file in cartella.GetFiles("*.*", SearchOption.AllDirectories).Where(s => s.Extension == ".wav" || s.Extension == "mp3")){
            localPaths.Add(file);
        }
        SetPaginaNaviga();
        //dopo aver avviato l'applicazione, possso mandare OSC da SC-> unity
        GameManager.instance.listenPaths();
    }


    public void Inizio(){
        Debug.Log("Hai premuto inizio");
        SceneManager.LoadScene("VRprova");
        //SceneManager.sceneLoaded += NomeFunzione;

    }

    public void Opzioni(){
        Debug.Log("Hai premuto opzioni");
        
        //aggiorna paths
        //riempi pagina opzioni settando i testi dei prefab "voce"
        foreach(KeyValuePair<string, string> cpl in GameManager.instance.paths){
            var voce = Instantiate(vocePrefab, opzioniContent.transform);
            voce.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = cpl.Key;
            voce.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = cpl.Value;
            voce.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(()=> {
                selectedInstrument = voce.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
                Debug.Log(selectedInstrument + " selezionato per cambiare path");
                paginaOpzioni.SetActive(false);
                paginaNaviga.SetActive(true);
            });
        }

    }

    public void SetPaginaNaviga(){ //una sola volta, scrivo i path sui bottoni della pagina "naviga"
        foreach(FileInfo path in localPaths){
            var btn = Instantiate(pathBtnPrefab, navigaContent.transform);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = path.Name;
            btn./*GetComponent<Button>().*/onClick.AddListener(()=> {
                ChangeText(path);
                paginaNaviga.SetActive(false);
                paginaOpzioni.SetActive(true);
            });
            btn.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(()=> {
                Debug.Log(path.FullName);
                GameManager.instance.SCsendString("/listenSample", path.FullName.Replace(Path.DirectorySeparatorChar, '/'));
            });
            //Debug.Log("percorso trovato: " + path);
        }
    }

    public void ChangeText(FileInfo path){
        string pathCompleto = path.FullName.Replace(Path.DirectorySeparatorChar, '/');
        Debug.Log(pathCompleto);
        foreach(Transform go in opzioniContent.transform){
            if(go.GetComponentInChildren<TextMeshProUGUI>().text == selectedInstrument){
                go.GetChild(1).GetComponent<TextMeshProUGUI>().text = path.Name;
                
                //serve cambiare GameManager.instance.path ?

                //manda messaggio a Supercollider
                GameManager.instance.SCsendString("/newpath", selectedInstrument, pathCompleto);
                
            }

        }
    }

    //funzione per selezionare nuovo testo - passa nel vecchio panel e sostituisce nella sezione path - quindi manda messaggio a SC per modificare path buffer 
}
