using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SceneNavigator : MonoBehaviour
{

    KeywordRecognizer keywordRecognizer;

    // Use this for initialization
    void Start()
    {
        //used for phrases and commands 
        if (keywordRecognizer == null)
        {
            keywordRecognizer = new KeywordRecognizer(new string[] { "next", "previous" });
            keywordRecognizer.OnPhraseRecognized += (args) =>
            {

                if (args.text == "next")
                    GotoNext();
                else if (args.text == "previous")
                    GotoPrevious();
            };
            keywordRecognizer.Start();
        }
    }

    private void OnDestroy()
    {
        if (keywordRecognizer != null)
        {
            keywordRecognizer.Stop();
        }
    }


    void GotoNext()
    {
        var current = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        int next = current.buildIndex + 1;
        this.DoLoadScene(next);

    }

    void DoLoadScene(int index)
    {
        try
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(index, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        catch (System.Exception ex)
        {

        }
    }

    void GotoPrevious()
    {
        var current = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        int next = current.buildIndex - 1;
        if (next >= 0)
        {
            DoLoadScene(next);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

