using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

namespace WinMRSnippets.Samples.Utilities
{
    public class Command
    {
        public KeyCode key;
        public string name;
        public System.Action action;

        public Command(KeyCode k, string n, System.Action a)
        {
            key = k;
            name = n;
            action = a;
        }
    }


    public class CommandPanelView : MonoBehaviour {

        [SerializeField]
        private GameObject buttonPrefab;

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }


        public void PopulateCommands(List<Command> commands)
        {
            Debug.Assert(buttonPrefab != null);
            if (buttonPrefab == null)
                return; 


            foreach ( Command c in commands)
            {
                var newItem = GameObject.Instantiate(buttonPrefab, this.gameObject.transform, false);
                UnityEngine.UI.Button button = newItem.GetComponent<Button>();
                button.onClick.AddListener(() => { c.action(); }  ); 
               //  button.click += c.action; 
              //   button.onClick.AddListener( (UnityEngine.Events.UnityAction) c.action);
                var textElement = button.GetComponentInChildren<UnityEngine.UI.Text>();
                if (textElement != null)
                {
                    textElement.text = c.name;
                } 

            }
        }
    }

} 