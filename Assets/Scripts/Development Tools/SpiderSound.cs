using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Diluvion;

namespace SpiderWeb
{
    public class SpiderSound
    {
        #region Wwise Audio Calls        
        #region local scope sounds
        /// <summary>
        /// Makes a sound event of name attached to targetObject
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="targetObject"></param>
        public static void MakeSound(string eventName, GameObject targetObject)
        {
            SetAKObjects(targetObject);
            GameManager.LoadSoundBanks();
           
            AkSoundEngine.PostEvent(eventName, targetObject);         
        }

     
        //
        public static void MakeSound(uint eventID, GameObject targetObject)
        {
            SetAKObjects(targetObject);
            GameManager.LoadSoundBanks();

              AkSoundEngine.PostEvent(eventID, targetObject);
        }


        public static void SetAKObjects(GameObject targetObj)
        {
            if (targetObj == null) return;
          
            if (!targetObj.GetComponent<AkGameObj>())
            {
                targetObj.AddComponent<AkGameObj>();
                if (!targetObj.GetComponent<Rigidbody>())
                    if(targetObj.GetComponent<AkGameObj>())
                        targetObj.GetComponent<AkGameObj>().isEnvironmentAware = false;
            }
        }
        #endregion
        #region global scope sounds
        //Global scope sounds (UI etc) for script clarity
        public static void MakeGlobalSound(string eventName)
        {
            GameManager.LoadSoundBanks();
            AkSoundEngine.PostEvent(eventName, null);
        }


        public static void MakeGlobalSound(uint eventID)
        {
            GameManager.LoadSoundBanks();
            AkSoundEngine.PostEvent(eventID, null);
        }

        #endregion

        public static void SetState(AKStateCall eventCall)
        {
            if (eventCall.akGroupInt != 0 && eventCall.akEventInt != 0)
            {
                SetState(eventCall.akGroupInt, eventCall.akEventInt);
                return;
            }
            
            if (string.IsNullOrEmpty(eventCall.akGroup) || string.IsNullOrEmpty(eventCall.akEvent))
            {
                Debug.LogError("Can't set state; the eventcall group or event is null.");
                return;
            }
            
            SetState(eventCall.akGroup, eventCall.akEvent);
          
        }

        public static void SetState(uint stateGroupID, uint stateID)
        {
            //Debug.Log("setting state to: " + stateGroupID + "/" + stateID);
            GameManager.LoadSoundBanks();
            AkSoundEngine.SetState(stateGroupID, stateID);
        }

        public static void SetState(string stateGroupName, string state)
        {
            GameManager.LoadSoundBanks();
            AkSoundEngine.SetState(stateGroupName, state);
        }


        public static void SetTrigger(string eventName, GameObject target)
        {
            GameManager.LoadSoundBanks();
            AkSoundEngine.PostTrigger(eventName, target);
        }

        public static void SetSwitch(string switchName, string switchState, GameObject target)
        {
            GameManager.LoadSoundBanks();
            AkSoundEngine.SetSwitch(switchName, switchState, target);
        }


        #region Event Actions
        /// <summary>
        /// Stops the sound of name eventname over fadeout miliseconds on targetObject
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="fadeOut"></param>
      /*  public static void StopSound(string eventName, GameObject targetObject, float fadeOutSeconds)
        {            
            AkSoundEngine.ExecuteActionOnEvent(eventName, AkActionOnEventType.AkActionOnEventType_Stop, targetObject, FloatToMillisecondInt(fadeOutSeconds), AkCurveInterpolation.AkCurveInterpolation_Sine);
        }*/

        /// <summary>
        /// Pauses the sound of name eventname over fadeout miliseconds on targetObject
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="fadeOut"></param>
        public static void PauseSound(string eventName, GameObject targetObject, float fadeOutSeconds)
        {
            AkSoundEngine.ExecuteActionOnEvent(eventName, AkActionOnEventType.AkActionOnEventType_Pause, targetObject, FloatToMillisecondInt(fadeOutSeconds), AkCurveInterpolation.AkCurveInterpolation_Sine);
        }

        /// <summary>
        /// Pauses the sound of name eventname over fadeout miliseconds on targetObject
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="fadeOut"></param>
        public static void ResumeSound(string eventName, GameObject targetObject, float fadeOutSeconds)
        {
            AkSoundEngine.ExecuteActionOnEvent(eventName, AkActionOnEventType.AkActionOnEventType_Resume, targetObject, FloatToMillisecondInt(fadeOutSeconds), AkCurveInterpolation.AkCurveInterpolation_Sine);
        }
        
        
        #endregion
        #region RTCP Actions

        public static void TweakRTPC(string rtcpName, float newValue, GameObject gobj)
        {          
            AkSoundEngine.SetRTPCValue(rtcpName, newValue, gobj);
        }

        public static float GetRtpc(string rtpcName, GameObject go)
        {
            uint rtpcPlayingID = 0;
            uint[] playingIds = new uint[1];
            float rtpcFloatValue = 0;
            int rtpcType = 1;
            AkSoundEngine.GetPlayingIDsFromGameObject(go, ref rtpcPlayingID, playingIds);       
            AkSoundEngine.GetRTPCValue(rtpcName, go, rtpcPlayingID, out rtpcFloatValue, ref rtpcType);
            //Debug.Log("RTPCName: " + rtpcName +" PlayingID: "+ rtpcPlayingID + " rtpcFloatValue: " + rtpcFloatValue + " rtpcIntValue: " + (RTPCValue_type)rtpcType + " playingIds[0]: " + playingIds[0]);
            return rtpcFloatValue;
        }
        #endregion
        #endregion
        #region AudioTools
        #region Doppler

        /// <summary>
        /// Calculates the doppler pitch value from raw Vectors, 1 is neutral, 0 is moving away each other, and 2 is moving towards
        /// </summary>
        /// <param name="dopplerFactor"> The doppler multiplier</param>
        /// <param name="listenerToTargetDir"> The direction vector from the listener to the target</param>
        /// <param name="listenerVelocity"> The listeners current velocity</param>
        /// <param name="targetVelocity"> The Target's current Velocity</param>
        /// <returns></returns>

        static float speedOfSound = 334.3f;
        public static float DopplerPitch(float dopplerFactor,  Vector3 listenerToTargetDir ,  Vector3 listenerVelocity, Vector3 targetVelocity)
        {
            float distance = listenerToTargetDir.magnitude;

            float listenerRelativeSpeed = Vector3.Dot(listenerToTargetDir, listenerVelocity) / distance;
         
            float emitterRelativeSpeed = Vector3.Dot(listenerToTargetDir, targetVelocity) / distance;
            listenerRelativeSpeed = Mathf.Min(listenerRelativeSpeed, (speedOfSound / dopplerFactor));
            emitterRelativeSpeed = Mathf.Min(emitterRelativeSpeed, (speedOfSound / dopplerFactor));
            float dopplerPitch = (speedOfSound + (listenerRelativeSpeed * dopplerFactor)) / (speedOfSound + (emitterRelativeSpeed * dopplerFactor));
            return dopplerPitch;
        }

        /// <summary>
        /// Overload for DopplerPitch with a dopplerfactor of 1
        /// </summary>
        /// <param name="listenerToTargetDir"></param>
        /// <param name="listenerVelocity"></param>
        /// <param name="targetVelocity"></param>
        /// <returns></returns>
        public float DopplerPitch(Vector3 listenerToTargetDir, Vector3 listenerVelocity, Vector3 targetVelocity)
        {
            return DopplerPitch(1, listenerToTargetDir, listenerVelocity, targetVelocity);
        }

        /// <summary>
        /// Get the doppler value with two input gameObjects;
        /// </summary>
        /// <param name="listenerToTargetDir"></param>
        /// <param name="listenerVelocity"></param>
        /// <param name="targetVelocity"></param>
        /// <returns></returns>
        public float DopplerPitch(float dopplerFactor, GameObject listener, GameObject target)
        {
            Vector3 listenerVelocity = GetVelocity(listener);
            Vector3 targetVelocity = GetVelocity(target);
            Vector3 listenerTotargetDir = listener.transform.position - target.transform.position;

            return DopplerPitch(dopplerFactor, listenerTotargetDir, listenerVelocity, targetVelocity);
        }

        /// <summary>
        /// Overload for DopplerPitch with a dopplerfactor of 1
        /// </summary>
        /// <param name="listenerToTargetDir"></param>
        /// <param name="listenerVelocity"></param>
        /// <param name="targetVelocity"></param>
        /// <returns></returns>
        public float DopplerPitch(GameObject listener, GameObject target)
        { 
            return DopplerPitch(1, listener, target);
        }
        #endregion

        public Vector3 GetVelocity(GameObject go)
        {
            if (go.GetComponent<PseudoVelocity>())
                return go.GetComponent<PseudoVelocity>().velocity;
            else if (go.GetComponent<Rigidbody>())
                return go.GetComponent<Rigidbody>().velocity;
            return Vector3.zero;
        }
        public static int FloatToMillisecondInt(float seconds)
        {
            return Mathf.FloorToInt(seconds * 1000);
        }
        #endregion
    }
    

}
