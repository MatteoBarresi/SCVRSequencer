using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class TestoBattutaAttuale : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        aggiornaNumeroBattuta();
    }
    
    public void aggiornaNumeroBattuta(){
        //Debug.Log($"TestoBattutaAttual.aggiornaNumeroBattuta dice battuta: {UIGeneral.getBattuta() +1}/{BeatManager.Battute}");
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = $"Battuta: {UIGeneral.getBattuta() + 1}/" + BeatManager.Battute;
    }
}
