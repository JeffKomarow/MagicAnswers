using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Magic8Ball : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public static string MagicEightBall()
    {
        int rannum = 0;

        Debug.Log("Welcome to the magic 8 ball application");
        Debug.Log("What is your query");

        //readline

        rannum = Random.Range(0, 3);

        switch (rannum)
        {
            case 0:
                return "It is certain";
            case 1:
                return "As I see it, yes.";
            case 2:
                return "Reply hazy, try again";
            default:
                return "Don't count on it.";
        }
        // console.Readline();
     
    }
}
