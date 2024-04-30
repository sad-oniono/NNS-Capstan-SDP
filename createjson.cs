using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drydock;
using System.IO;

public class createjson : MonoBehaviour
{
    Dock drydock;
    // Start is called before the first frame update
    void Start()
    {
        string path = Application.dataPath;
        drydock = new Dock(6, 0, 0);
        StreamWriter sw = new StreamWriter("settings.txt");
        sw.WriteLine(JsonUtility.ToJson(drydock));
        sw.Close();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
