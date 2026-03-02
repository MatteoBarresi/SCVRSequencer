using UnityEngine;
using TMPro;

public class TestoBattutaAttuale : MonoBehaviour
{
    void Start()
    {
       aggiornaNumeroBattuta(1,1);
    }
    
    public void aggiornaNumeroBattuta(int actual, int tot){
        //gameObject.GetComponentInChildren<TextMeshProUGUI>().text = $"Battuta: {UIGeneral.getBattuta() + 1}/" + BeatManager.Battute;
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = $"Battuta: {actual}/" + tot;
    }
}
