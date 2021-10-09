
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Diluvion;
using Diluvion.SaveLoad;
using SpiderWeb;

namespace DUI
{

    public enum verticalPos
    {
        upper, center, lower
    }

    public enum horizontalPos
    {
        left, center, right
    }

    /// <summary>
    /// Tutorial panel can show a tutorial for a target. Takes a tutorial object and displays it.
    /// <see cref="TutorialObject"/><see cref="TutorialCaller"/>
    /// </summary>
    public class TutorialPanel : DUIPanel
    {
        public TextMeshProUGUI text;
        public GameObject meshLine;
        public Transform pointer;
        public Transform mainPanel;

        TutorialObject _tutObject;
        Transform _target;
        Camera _cam;
        float _timer;
        bool _ended;
        string _colorString = "=#ffff00ff";

        static List<TutorialPanel> _queue = new List<TutorialPanel>();

        /// <summary>
        /// Create a new blank instance of the tutorial panel
        /// </summary>
        static TutorialPanel Create()
        {
            TutorialPanel instance = UIManager.Create(UIManager.Get().tutorialPanel as TutorialPanel);
            return instance;
        }

        /// <summary>
        /// Remove the given tutorial panel from the queue, and attempt to show the next in line.
        /// </summary>
        static void RemoveAndShowNext(TutorialPanel panel)
        {
            Debug.Log("Removing panel " + panel._tutObject.name, panel);
            _queue.Remove(panel);
            if (_queue.Count < 1) return;

            //Debug.Log("Showing next panel " + _queue[0]._tutObject.name, _queue[0]);
            _queue[0].gameObject.SetActive(true);
            _queue[0].Show();
        }

        /// <summary>
        /// Shows a tutorial UI with the given tutorial object. Will only show one at a time,
        /// so if multiple are called, queues them.
        /// </summary>
        /// <param name="tut">The tutorial object reference.</param>
        /// <param name="t">Transform to point at</param>
        /// <param name="c">Camera that the given transform is being rendered on. use null for a UI element.</param>
        /// <returns>Returns the new panel created.</returns>
        public static TutorialPanel ShowTutorial(TutorialObject tut, Transform t = null)
        {
            // Check if this tutorial has already been shown
            if (DSave.current != null)
                if (DSave.current.tutorialsCompleted.Contains(tut.name))
                {
                    //Debug.Log("Tutorial " + tut.name + " has already been shown.", tut);
                    return null;
                }

            if (Queued(tut)) return null;

            TutorialPanel newPanel = Create();

            // Initialize the panel
            newPanel.ApplyStats(tut, t);

            // If there's already panels queued up, turn this panel off.
            if (_queue.Count > 0) newPanel.gameObject.SetActive(false);
            else newPanel.Show();

            _queue.Add(newPanel);
            return newPanel;
        }

        /// <summary>
        /// If a tutorial panel for the given tutorial is showing, completes it.
        /// </summary>
        public static void CompleteTutorial(TutorialObject tut)
        {
            //Debug.Log("attempting to complete tutorial " + tut.name);
            if (_queue.Count < 1) return;
            if (_queue[0]._tutObject == tut) _queue[0].Complete();
        }

        protected override void Start()
        {
            base.Start();

            SetAnimationSpeed(0);
            alpha = 0;
            SnapAlpha(0);
        }

        protected override void Update()
        {
            base.Update();

            if (!_tutObject) return;

            // Check time limit
            if (_tutObject.useTimeLimit)
            {
                _timer += Time.unscaledDeltaTime;
                if (_timer > _tutObject.timeLimit) End();
            }

            // Check input
            if (_tutObject.InputWasUsed()) Complete();

            if (_target)
            {
                // Have the pointer point to the target
                Vector3 targetPos = _target.position;
                if (_cam != null)
                    targetPos = FollowTransform(_target.position, 30, _cam);
                pointer.transform.position = Vector3.Lerp(pointer.transform.position, targetPos, Time.unscaledDeltaTime * 20);
            }
        }

        /// <summary>
        /// Has the given tutorial object already been queued?
        /// </summary>
        static bool Queued(TutorialObject obj)
        {
            foreach (TutorialPanel p in _queue)
                if (p._tutObject == obj) return true;
            return false;
        }

        /// <summary>
        /// Has the instance prep to show the tutorial. 
        /// </summary>
        void ApplyStats(TutorialObject tut, Transform t)
        {
            _tutObject = tut;
            _target = t;
            
            // meshline pointer control
            meshLine.SetActive(tut.showPointer);

            // Position the panel
            Transform newParent = null;
            foreach (PanelPosition p in GetComponentsInChildren<PanelPosition>())
            {
                if (p.Match(tut.hPos, tut.vPos))
                {
                    newParent = p.transform;
                    break;
                }
            }
            if (newParent) mainPanel.SetParent(newParent, false);
            
            // Determine the correct camera to use
            if (t != null)
            {
                if (t.gameObject.layer == LayerMask.NameToLayer("Interior"))
                    _cam = InteriorView.Get().localCam;
                else if (t.gameObject.layer == LayerMask.NameToLayer("UI"))
                    _cam = null;
                else _cam = OrbitCam.Cam();
            }else
            {
                // turn off tail
                meshLine.SetActive(false);
            }

            text.text = tut.LocalizedText();
        }

        void Show()
        {
            StartCoroutine(DelayShow(_tutObject.delayTime));
        }

        IEnumerator DelayShow(float delayTime)
        {
            float t = 0;
            while (t < delayTime)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            FinalizeText();
            alpha = 1;
            SetAnimationSpeed(1);
        }

        void SetAnimationSpeed(float newSpeed)
        {
            foreach (var a in GetComponentsInChildren<Animator>())
                a.speed = newSpeed;
        }

        /// <summary>
        /// Finalizes the text by replacing any # with the button / axis name
        /// </summary>
        void FinalizeText()
        {
            string keyMapName = Controls.InputMappingName(_tutObject.inputName);

            // Optional second input
            if (_tutObject.twoInputs)
                keyMapName += ", " + Controls.InputMappingName(_tutObject.inputName2);

            text.text = text.text.Replace("#", keyMapName);

            //replace text highlight color with this color
            text.text = text.text.Replace("=red", _colorString);
        }

        /// <summary>
        /// The objective for this tutorial has been completed. Call this when the player does the right thing!
        /// </summary>
        public void Complete()
        {
            DelayedEnd(2);
        }

        public override void End()
        {
            // Only allow end once
            if (_ended) return;
            _ended = true;

            DSave.current.tutorialsCompleted.Add(_tutObject.name);
            RemoveAndShowNext(this);
            base.End();
        }
    }
}