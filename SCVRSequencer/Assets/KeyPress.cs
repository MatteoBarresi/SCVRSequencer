using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPress : MonoBehaviour
{


    public string selection="/kick";
    private static List<int> kickValues = new List<int>() {0,0,0,0,0,0,0,0};
    private static List<int> hhValues = new List<int>() {0,0,0,0,0,0,0,0};
    public KeyCode[] tasti = {
        KeyCode.Alpha1,
		KeyCode.Alpha2,
		KeyCode.Alpha3,
		KeyCode.Alpha4,
		KeyCode.Alpha5,
		KeyCode.Alpha6,
		KeyCode.Alpha7,
		KeyCode.Alpha8,
    };
    Dictionary<string, List<int>> dict = new Dictionary<string, List<int>>(){
            {"/kick", kickValues},
            {"/hh", hhValues},
        };
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //seleziona
        if (Input.GetKeyDown("k"))
            {
                selection = "/kick";
                Debug.Log("selezionato kick");
            };    
        if (Input.GetKeyDown("h"))
            {
                //cambia entry del dizionario
                selection ="/hh";
                Debug.Log("selezionato high hat");
            };
            
        //preme numero
        for (int i = 0; i< tasti.Length; i++){
            if(Input.GetKeyDown(tasti[i]/*.ToString()*/))
            {
                //toggle
                if(dict[selection][i] == 1)
                    dict[selection][i] = 0;
                else dict[selection][i] = 1;
                //entry dizionario
                OSCHandler.Instance.SendMessageToClient("SuperCollider",selection,dict[selection]);
            }

       };
        


/*

        if(Input.GetKeyDown("up"))
         {
            Debug.Log("up premuto");
             //action
            OSCHandler.Instance.SendMessageToClient("SuperCollider", "/testing", 1.0f);
         };
*/

/*
         if (Input.GetKeyDown(KeyCode.UpArrow))
            Debug.Log("Up Arrow key was pressed.");*/
    }
}
