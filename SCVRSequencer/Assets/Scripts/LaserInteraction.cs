using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaserInteraction : MonoBehaviour
{

    public bool selecting {get; set;} //monitora la selezione valida
    

    public void UIenter(){
        selecting = true;
    }
    public void UIexit(){
        selecting = false;
    }
}
