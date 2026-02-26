using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;
public class creaTasti : MonoBehaviour
{
    public GameObject bianchi;
    public GameObject neri;
    public OSCTransmitter transmitter;
    [SerializeField] GameObject tastiera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void creaBianchi(){
        for(int i = 1; i<13; i++ ){
            Instantiate(bianchi, 
                new Vector3(i*0.055f,1,0),   
                Quaternion.identity, 
                tastiera.transform);
        }
    }
}
