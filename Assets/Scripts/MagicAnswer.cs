using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MagicAnswer : MonoBehaviour
{
    // Update is called once per frame
    public static string AskAQuestion()
    {
        int randomNumber = 0;

        Debug.Log("Welcome, what is your question?");

        randomNumber = Random.Range(0, 5);

        return randomNumber switch
        {
            0 => "It is certain",
            1 => "As I see it, yes.",
            2 => "Reply hazy, try again",
            3 => "No",
            4 => "No",
            5 => "Don't count on it.",
        };
    }
}
