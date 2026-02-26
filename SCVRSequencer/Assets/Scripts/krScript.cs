using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class krScript : MonoBehaviour
{
    //serve a cambiare testo su imgDesc quando hover su un controllo - usato in UIGeneral

    public GameObject testo;
    private string desc = "";

    public void enter(){
        string kr = gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
        //Debug.Log(gameObject.GetComponentInChildren<TextMeshProUGUI>().text); //itera Synth.synthList[synth].controlli e trova stringa value di questa KEY
        
        desc += Synth.synthList[UIGeneral.getSelection()].controlli[kr];
        //testo.GetComponentInChildren<TextMeshProUGUI>().text = desc;
        GameManager.instance.spiegaz.GetComponentInChildren<TextMeshProUGUI>().text = desc;
    }
    
    public void exit(){
        desc = "";
        //testo.GetComponentInChildren<TextMeshProUGUI>().text = desc;
        GameManager.instance.spiegaz.GetComponentInChildren<TextMeshProUGUI>().text = desc;
    }
}
