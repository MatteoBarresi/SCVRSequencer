using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class buttonScript : MonoBehaviour
{
    //imposta il numero giusto sui bottoni del sequencer

    TextMeshProUGUI buttonText;  
    // Start is called before the first frame update
    void Start()
    {
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if(buttonText.text.Length == 1)
            buttonText.text = gameObject.name; //assegnaNome
    }

}
