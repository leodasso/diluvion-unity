using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using Diluvion;
using Diluvion.Sonar;
using Rewired;
using Rewired.UI.ControlMapper;

namespace DUI
{
    /// <summary>
    /// Sits in the scene and contains the canvases. UIManager instantiates this when it needs to place 
    /// a UI element.
    /// <see cref="UIManager"/>
    /// </summary>
    public class DUIController : MonoBehaviour
    {
        [Tooltip("These canvas groups will be hidden / visible when visibility is toggled.")]
        public List<CanvasGroup> canvases = new List<CanvasGroup>();
        public List<CanvasGroup> canvasesToHide = new List<CanvasGroup>();
        public CompassRose compassRosePrefab;
        public ControlMapper controlMapper;

        bool _isHidden;

        public Selectable FirstSelectable()
        {
            if (Selectable.allSelectables.Count < 1) return null;
            return Selectable.allSelectables.First();
        }

        void Start()
        {
            //controlMapper.rewiredInputManager = GameManager.RewiredInputManager();
        }

        void Update()
        {
            // debug hide UI
            if (Input.GetKeyDown(KeyCode.H))
            {
                SetHidden(_isHidden);
                _isHidden = !_isHidden;
            }
        }


        public void SetHidden(bool hidden)
        {
            foreach (CanvasGroup cg in canvasesToHide)
            {
                if (hidden) cg.alpha = 0;
                else cg.alpha = 1;
            }
        }

        //Plays contact sound if its not a bad guy
        void PlayContactSound(SonarStats sStats)
        {
            SpiderSound.MakeSound("Play_Contact_Neutral", sStats.gameObject);
        }

        #region Hostile Sound

        //Plays the hostile sound when not on cooldown
        void PlayHostileSound(SonarStats sStats)
        {
            if (playedHostileSound) return;
            StartCoroutine(HostileSoundCooldown());

            SpiderSound.MakeSound("Play_Contact_Hostile", sStats.gameObject);
        }


        //Delay between Contact Sounds
        bool playedHostileSound;
        IEnumerator HostileSoundCooldown()
        {
            playedHostileSound = true;
            yield return new WaitForSeconds(3);
            playedHostileSound = false;
        }
        #endregion

        //Checks for hostile flags in the sonarStats' behaviour, AI and type
        bool CheckIfDanger(SonarStats stats, int level)
        {
            return false;
            //return stats.IsHostileToPlayer(level);
        }
    }
}