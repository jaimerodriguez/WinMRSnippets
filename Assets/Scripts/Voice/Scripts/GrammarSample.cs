using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class GrammarSample : MonoBehaviour {

    private GrammarRecognizer grammarRecognizer;
    // Use this for initialization
    void Start () {
		//Used for SRGS Grammar 
        if ( grammarRecognizer == null )
        { 
            grammarRecognizer = new GrammarRecognizer(Application.streamingAssetsPath + "/SRGS/myGrammar.xml");
            grammarRecognizer.OnPhraseRecognized += (args) =>
            {
               SemanticMeaning[] meanings = args.semanticMeanings;
                // do something; 
            }; 
            grammarRecognizer.Start();
        }



    }

// Update is called once per frame
void Update () {
		
	}
}
