using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{

   private void Start()
    { 
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P)) 
        {
            print("Captura tomada");
            ScreenCapture.CaptureScreenshot("Ejemplo.png", 2);
        }
    }
}
