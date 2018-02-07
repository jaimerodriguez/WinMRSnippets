using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogViewComponent : MonoBehaviour
{
    public GameObject logLinePrefab;

    private LinkedList<GameObject> logLines = new LinkedList<GameObject>();
    private TextGenerator textGen;

    public void LogLine(string line)
    {

#if DEBUG
        if (System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debug.WriteLine(line );
        }
#endif 

        if (this.textGen == null)
        {
            this.textGen = new TextGenerator();
        }

        var newLine = GameObject.Instantiate(logLinePrefab, this.gameObject.transform, false);
        var textComponent = newLine.GetComponent<Text>();
        textComponent.text = line;
        float textHeight = this.textGen.GetPreferredHeight(
            line,
            textComponent.GetGenerationSettings(((RectTransform)this.gameObject.transform).rect.size));
        var layout = newLine.GetComponent<LayoutElement>();
        layout.minHeight = textHeight;
        logLines.AddLast(newLine);
     }

    void Start()
    {
        Debug.Assert(this.logLinePrefab.GetComponent<Text>() != null);
        Debug.unityLogger.logHandler = new DebugHandler(this); 

    }

    
    float lastClearTime = 0f;
    float debounceTime = 1f;
    void Update()
    {
        if (Input.GetKey("space")   /* && (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))*/ )
        {
            Debug.Log("Space");
            if (Time.time > (lastClearTime + debounceTime))
            {
                Clear();
                lastClearTime = Time.time;
            }

        }

        var viewTransform = (RectTransform)this.gameObject.transform;

        if (this.logLines.Last == null)
        {
            return;
        }

        var lastLineTransform = (RectTransform)this.logLines.Last.Value.transform;
        float overflow = -((lastLineTransform.localPosition.y - (lastLineTransform.rect.height/2)) - viewTransform.rect.yMin); // Postive overflow is a negative delta

        while (overflow > 0)
        {
            var firstLineNode = this.logLines.First;
            if (firstLineNode == null)
            {
                break;
            }
            var firstLineObj = firstLineNode.Value;
            var firstLineHeight = ((RectTransform)firstLineObj.transform).rect.height;
            GameObject.Destroy(firstLineObj);
            this.logLines.Remove(firstLineNode);
            overflow -= firstLineHeight;
        }
	}

    public void Clear ( )
    {
        foreach ( GameObject go in logLines )
        {
            Destroy(go); 
        }
        logLines.Clear();
    }



    private class DebugHandler : ILogHandler
    {
         
        LogViewComponent logView; 

        public DebugHandler(LogViewComponent view)
        {
            this.logView = view; ;
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            if (this.logView != null)
            {
                this.logView.LogLine(exception.ToString());
            }
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            if (this.logView != null)
            {
                this.logView.LogLine(string.Format(format, args));
            }
        }
    }
}
