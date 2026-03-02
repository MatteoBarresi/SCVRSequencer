using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using extOSC;

public class ReverbDryWet : MonoBehaviour
{
    //prende il valore dello slider e lo passa a SuperCollider
    public void DryWet(){
        //scsend tipo(address, kr, value)
        GameManager.instance.SCsendValue("/reverb", "drywet", gameObject.GetComponent<Slider>().value);
    }
}
