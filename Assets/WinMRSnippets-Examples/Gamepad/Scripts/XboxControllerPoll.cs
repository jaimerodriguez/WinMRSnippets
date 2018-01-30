//TODO:  SANITIZE 

using UnityEngine;


namespace WinMRSnippets
{
    public struct GamepadState
    {
        public bool AButtonPressed;
        public bool BButtonPressed;
        public bool XButtonPressed;
        public bool YButtonPressed;

        public bool BackButtonPressed;
        public bool StartButtonPressed;
        public bool XboxButtonPresse;

        public bool LeftBumperPressed;
        public bool RightBumperPressed;

        public bool LeftStickClicked;
        public bool RightStickClicked;
        public float LeftStickX;
        public float LeftStickY;
        public float RightStickX;
        public float RightStickY;

        public float LeftTrigger;
        public float RightTrigger;

        public float DPadX;
        public float DPadY;

    }



    public class XboxControllerPoll : MonoBehaviour
    {
        void Start()
        {
#if TRACING_VERBOSE
        Debug.Log("XboxControllerPoll Started");
#endif

            GamepadWatcher.Current.Start();
        }

        void Update()
        {
            GamepadState state = new GamepadState();
            if (GamepadWatcher.Current.IsPresent)
            {
                state.AButtonPressed = Input.GetButtonDown(UnityInputAxis.XboxController_AButton);
                state.BButtonPressed = Input.GetButtonDown(UnityInputAxis.XboxController_BButton);
                state.XButtonPressed = Input.GetButtonDown(UnityInputAxis.XboxController_XButton);
                state.YButtonPressed = Input.GetButtonDown(UnityInputAxis.XboxController_YButton);

                state.LeftBumperPressed = Input.GetButtonDown(UnityInputAxis.XboxController_LeftBumper);
                state.RightBumperPressed = Input.GetButtonDown(UnityInputAxis.XboxController_RightBumper);


                state.BackButtonPressed = Input.GetButtonDown(UnityInputAxis.XboxController_BackButton);
                state.StartButtonPressed = Input.GetButtonDown(UnityInputAxis.XboxController_StartButton);

                state.LeftStickClicked = Input.GetButtonDown(UnityInputAxis.XboxController_LeftStickButton);
                state.RightStickClicked = Input.GetButtonDown(UnityInputAxis.XboxController_RightStickButton);


                state.RightStickX = Input.GetAxis(UnityInputAxis.XboxController_RightStickX);
                state.RightStickY = Input.GetAxis(UnityInputAxis.XboxController_RightStickY);

                state.LeftStickX = Input.GetAxis(UnityInputAxis.XboxController_LeftStickX);
                state.LeftStickY = Input.GetAxis(UnityInputAxis.XboxController_LeftStickY);

                state.DPadX = Input.GetAxis(UnityInputAxis.XboxController_DPadX);
                state.DPadY = Input.GetAxis(UnityInputAxis.XboxController_DPadY);

                float trigger = Input.GetAxis(UnityInputAxis.XboxController_Trigger);
                if (trigger != 0)
                {
                    if (trigger < 0)
                        state.LeftTrigger = trigger;
                    else
                        state.RightTrigger = trigger;
                }

                bool hasChanges;
                string gamepadState = state.GetTraceState(out hasChanges);
                if (hasChanges)
                {
                    Debug.Log(gamepadState);
                }
            }
        }



        void JustDemoPurposes_GamepadAdded(object sender, System.EventArgs unused)
        {
            Debug.Log("XboxController Gamepad added handler");
        }

        void JustDemoPurposes_GamepadRemoved(object sender, System.EventArgs unused)
        {
            Debug.Log("XboxController Gamepad removed handler");
        }

        private void OnDestroy()
        {
            GamepadWatcher.Current.GamepadAdded -= JustDemoPurposes_GamepadAdded;
            GamepadWatcher.Current.GamepadRemoved -= JustDemoPurposes_GamepadRemoved;
            GamepadWatcher.Current.Stop();
        }
    }
} 