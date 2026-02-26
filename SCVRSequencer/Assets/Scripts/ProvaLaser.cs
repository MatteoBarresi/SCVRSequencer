using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProvaLaser : MonoBehaviour
{
    public bool selecting = false; //monitora la selezione valida

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UIenter(){
        selecting = true;
        //Debug.Log(selecting + " mano interagente hover: " + this.gameObject.name);
    }
    public void UIexit(){
        selecting = false;
        //Debug.Log(selecting + " mano uscente hover: " + this.gameObject.name);
    }
/*
    public static bool isHover(){
        return selecting;
    }*/
    
}
