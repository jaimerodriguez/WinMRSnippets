// LaserSword for Unity
// (c) 2016 Digital Ruby, LLC
// http://www.digitalruby.com
// 
// the UX for the light sabers comes from https://github.com/jjxtra/UnityLightsaber/tree/master/Assets/LaserSword

using UnityEngine;
using System.Collections;
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;

namespace DigitalRuby.LaserSword
{
    public class LaserSwordScript : MonoBehaviour
    {
        [Tooltip("Hilt game object.")]
        public GameObject Hilt;

        [Tooltip("Blade game object.")]
        public GameObject Blade;

        [Tooltip("Blade glow game object.")]
        public LineRenderer BladeGlow;

        [Tooltip("Light game object.")]
        public Light Light;

        [Tooltip("Audio source.")]
        public AudioSource AudioSource;

        [Tooltip("Audio source for looping.")]
        public AudioSource AudioSourceLoop;

        [Tooltip("How long it takes to turn the laser sword on and off")]
        public float ActivationTime = 0.5f;

        [Tooltip("Sound to play when the laser sword turns on")]
        public AudioClip StartSound;

        [Tooltip("Sound to play when the laser sword turns off")]
        public AudioClip StopSound;

        [Tooltip("Sound to play when the laser sword stays on")]
        public AudioClip ConstantSound;

        [Tooltip("Blade start")]
        public GameObject BladeStart;

        [Tooltip("Blade end")]
        public GameObject BladeEnd;

        private LaserSwordBladeCreatorScript creationScript;

        private int state; // 0 = off, 1 = on, 2 = turning off, 3 = turning on
        private GameObject temporaryBladeStart;
        private float bladeDir; // 1 = up, -1 = down
        private float bladeTime;
        private float bladeIntensity;

        private bool pressedTrigger = false;


        WinMRSnippets.TrackedController trackedController; 


        private void CheckState()
        {
            if (state == 2 || state == 3)
            {
                bladeTime += Time.deltaTime;
                float percent = Mathf.Lerp(0.01f, 1.0f, bladeTime / ActivationTime);
                Vector3 end = temporaryBladeStart.transform.position + (transform.up * bladeDir * percent * creationScript.BladeHeight);
                BladeEnd.transform.position = end;
                bladeIntensity = (state == 3 ? percent : (1.0f - percent));

                if (bladeTime >= ActivationTime)
                {
                    GameObject.Destroy(temporaryBladeStart);
                    bladeTime = 0.0f;
                    if (state == 2)
                    {
                        state = 0;
                    }
                    else
                    {
                        state = 1;
                    }
                }
            }
        }

        private void UpdateBlade()
        {
            float distance = Vector3.Distance(BladeEnd.transform.position, BladeStart.transform.position);
            float percent = distance / creationScript.BladeHeight;
            Blade.transform.localScale = new Vector3(1.0f, percent, 1.0f);
            if (percent < 0.01f)
            {
                Blade.SetActive(false);
                BladeGlow.gameObject.SetActive(false);
            }
            else
            {
                Blade.SetActive(true);
                //BladeGlow.gameObject.SetActive(true);
            }
            BladeGlow.SetColors(new Color(1.0f, 1.0f, 1.0f, bladeIntensity), new Color(1.0f, 1.0f, 1.0f, bladeIntensity));
            BladeGlow.SetPosition(0, BladeStart.transform.position - (transform.up * creationScript.BladeHeight * 0.075f));
            BladeGlow.SetPosition(1, BladeEnd.transform.position);
            Light.intensity = percent;
        }

        private void Start()
        {
            trackedController = GetComponent<WinMRSnippets.TrackedController>();
            trackedController.SelectPressed += TrackedController_SelectPressed;
            trackedController.SelectReleased += TrackedController_SelectReleased;
            creationScript = GetComponent<LaserSwordBladeCreatorScript>();
            BladeEnd.transform.position = BladeStart.transform.position;
        }

        private void TrackedController_SelectReleased(object sender, WinMRSnippets.MotionControllerStateChangedEventArgs args)
        {
            if (pressedTrigger)
            {
                TurnOn(false);
                pressedTrigger = false;
            }
        }

        private void TrackedController_SelectPressed(object sender, WinMRSnippets.MotionControllerStateChangedEventArgs args)
        {
            if (args.NewState.SelectValue > 0.1f && !pressedTrigger)
            {
                Debug.Log("Select Pressed");
                TurnOn(true);
                pressedTrigger = true;
            }
        }

        private void Update()
        {
            CheckState();
            UpdateBlade();
        }

        /// <summary>
        /// Pass true to turn on the laser sword, false to turn it off
        /// </summary>
        /// <param name="value">Whether the laser sword is on or off</param>
        /// <returns>True if success, false if invalid operation (i.e. laser sword is already on or off)</returns>
        public bool TurnOn(bool value)
        {
            if (state == 2 || state == 3 || (state == 1 && value) || (state == 0 && !value))
            {
                return false;
            }
            temporaryBladeStart = new GameObject("LaserSwordTemporaryBladeStart");
            temporaryBladeStart.hideFlags = HideFlags.HideAndDontSave;
            temporaryBladeStart.transform.parent = transform;
            temporaryBladeStart.transform.position = BladeEnd.transform.position;

            if (value)
            {
                bladeDir = 1.0f;
                state = 3;
                AudioSource.PlayOneShot(StartSound);
                AudioSourceLoop.clip = ConstantSound;
                AudioSourceLoop.Play();
            }
            else
            {
                bladeDir = -1.0f;
                state = 2;
                AudioSource.PlayOneShot(StopSound);
                AudioSourceLoop.Stop();
            }

            return true;
        }


        /// <summary>
        /// Turn on the laser sword
        /// </summary>
        public void Activate()
        {
            TurnOn(true);
        }

        /// <summary>
        /// Turn off the laser sword
        /// </summary>
        public void Deactivate()
        {
            TurnOn(false);
        }

        /// <summary>
        /// Activate or deactivate the laser sword
        /// </summary>
        /// <param name="active">True to activate, false to deactivate</param>
        public void SetActive(bool active)
        {
            if (active)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
        }
 
    }
}