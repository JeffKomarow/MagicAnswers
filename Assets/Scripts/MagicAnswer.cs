using UnityEngine;

public class MagicAnswer : MonoBehaviour
{
    // Update is called once per frame
    public static string AskAQuestion()
    {
        int randomNumber;

        Debug.Log("Welcome, what is your question?");

        randomNumber = Random.Range(0, 49);

        return randomNumber switch
        {
            0 => "Yes.",                       //Positive
            1 => "It is known.",
            2 => "For sure.",
            3 => "Positively.",
            4 => "It's going to happen.",
            5 => "Yeah.",
            6 => "Totally.",
            7 => "Probably.",
            8 => "Heck yes.",
            9 => "This is true.",
            10 => "Yep.",
            11 => "Don't count on it.",
            12 => "Naturally.",
            13 => "By all means.",
            14 => "Okay.",
            15 => "Approved.",
            16 => "Agreed.",
            17 => "I concur.",
            18 => "Confirmed.",
            19 => "A million times yes.",
            20 => "No.",                       //Negative
            21 => "Never.",
            22 => "Negatory.",
            23 => "Negative.",
            24 => "Void.",
            25 => "Vetoed.",
            26 => "Cancel that.",
            27 => "Not happening.",
            28 => "Rejected.",
            29 => "Declined.",
            30 => "Disapproved.",
            31 => "Prohibited.",
            32 => "Banned.",
            33 => "Dosavowed.",
            34 => "Oh no.",
            35 => "Gonna have to go with no..",
            36 => "I disagree.",
            37 => "Get out.",
            38 => "I say nay nay.",
            39 => "A million times no.",
            40 => "No opinion.",                //Neutral
            41 => "Ask again.",
            42 => "Can you repeat that?",
            43 => "Fog intterupts your vision, go again.",
            44 => "Ask another question.",
            46 => "I can't answer that for you.",
            47 => "Try again.",
            48 => "Ask something else.",
            49 => "Why?.",
        };
    }
}
