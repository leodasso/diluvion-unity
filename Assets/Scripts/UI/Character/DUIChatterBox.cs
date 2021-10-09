using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Diluvion;

namespace DUI
{
    /// <summary>
    ///  The chatterbox GUI element, also referred to s the 'Radio'. Shows chatter of characters & other 
    ///  subs talking to the player. The most recent chatter is highlighted
    ///  at the top of the element with an icon to give some info about the chatter.
    ///  
    /// The chatterbox expands whenever a new chatter happens, but after a few seconds of inactivity, it will return to a
    /// more discrete mode in the screen. 
    /// </summary>
    public class DUIChatterBox : DUIPanel
    {

        public static DUIChatterBox instance;

        public float collapsedPadding = 74;
        public float chatterItemPadding = 6;
        public float timeExpanded = 5;
        
        public float readTime = 3;

        public RectTransform chatterPanel;
        public RectTransform chatterParent;
        
        public DUIChatterItem chatterPrefab;

        [Space]

        public List<ChatterInfo> chatter = new List<ChatterInfo>();
        public List<DUIChatterItem> chatterInstances = new List<DUIChatterItem>();

        float _timer;
        float _expandTimer;
        float _yPos;

        #region static functions

        /// <summary>
        /// Returns existing instance. If no instance exists, creates and returns it.
        /// </summary>
        static DUIChatterBox Get ()
        {
            if (instance) return instance;

            instance = UIManager.Create(UIManager.Get().chatterPanel as DUIChatterBox);
            return instance;
        }

        /// <summary>
        /// Add the given conversation to the chatter box.
        /// </summary>
        public static void AddChatter (Convo c, Dialogue dialogInstance)
        {
            // check if this has been shown recently
            if (!c.IntervalOkay()) return;

            // Check if this convo is already visible in the chatter stack
            if (Get().IsShowingAlready(c)) return;
            
            // Check if the chatter has been saved as shown.
            if (!c.OkayToShowChatter()) return;

            Get().AddChatterItem(c, dialogInstance, ChatterType.crew);
        }


        public static void AddRadio (Convo c)
        {
            Debug.Log("Radio: " + c);
            Get().AddChatterItem(c, null, ChatterType.radio);
        }

        #endregion

        /// <summary>
        /// Adds the given conversation to the radio pending list
        /// </summary>
        void AddChatterItem (Convo convo, Dialogue dialogInstance, ChatterType cType)
        {
            ChatterInfo newInfo = new ChatterInfo(convo, dialogInstance.MyCharacter(), dialogInstance, cType);
            convo.SaveChatterShown();
            chatter.Add(newInfo);
        }


        /// <summary>
        /// Creates a new instance of the Chatter UI prefab using the given chatterinfo.
        /// </summary>
        void CreateChatterInstance (ChatterInfo info)
        {
            // Instantiate the new chatter item
            DUIChatterItem newChatter = Instantiate(chatterPrefab, chatterParent, false);

            // plug info into the new chatter item
            newChatter.Display(info);

            // add new chatter element to the list of instances
            chatterInstances.Insert(0, newChatter);

            _timer = 0;
            _expandTimer = 0;
        }
        
        
        /// <summary>
        /// Returns true if this convo is already being displayed somewhere in the chatter stack.
        /// </summary>
        bool IsShowingAlready(Convo convo)
        {
            // Check the instances of chatter for duplicates
            foreach (var chatter in chatterInstances)
            {
                if (chatter.chatterInfo == null) continue;
                if (chatter.chatterInfo.convo == null) continue;
                if (chatter.chatterInfo.convo == convo) return true;
            }

            // Check list of pending chatter for duplicates
            foreach (var c in chatter)
            {
                if (c == null) continue;
                if (!c.convo) continue;
                if (c.convo == convo) return true;
            }
            
            return false;
        }


        int _i;
        // Update is called once per frame
        protected override void Update ()
        {
            base.Update();

            if (OrbitCam.CamMode() != CameraMode.Normal) return;

            _timer += Time.unscaledDeltaTime;
            _expandTimer += Time.unscaledDeltaTime;

            // if there's pending chatters to show...
            if (chatter.Count > 0)
            {
                if (ChatterReady())
                {
                    _expandTimer = 0;
                    // Once the element is expanded, display the new chatter & remove from pending list
                    if (_yPos >= -0.002f)
                    {
                        CreateChatterInstance(chatter [0]);
                        chatter.RemoveAt(0);
                    }
                }
            }

            // If there's been [timeExpanded] seconds of inactivity, collapse the element.

            // start by getting the height of the most recently displayed chatter
            float chatterItemHeight = 0;
            if (chatterInstances.Count > 0)
            {
                DUIChatterItem displayedItem = chatterInstances[0];
                chatterItemHeight = displayedItem.MyHeight();
            }

            // Position this element so only the most recently displayed chatter is showing.
            if (_expandTimer > timeExpanded)
                _yPos = -chatterPanel.rect.height + chatterItemHeight + collapsedPadding;
            else
                _yPos = 0;

            // update the position of the parent
            chatterPanel.anchoredPosition = Vector2.Lerp(chatterPanel.anchoredPosition,
                new Vector2(chatterPanel.anchoredPosition.x, _yPos), Time.unscaledDeltaTime * 14);

            if (chatterInstances.Count < 1) return;

            // update the position of the chatter items (one per frame)
            
            // Get the height of all previous chatter elements
            float totalHeight = 0;
            for (int i = 0; i < _i; i++)
            {
                if (i >= chatterInstances.Count) continue;
                totalHeight -= chatterInstances[i].MyHeight() - chatterItemPadding;
            }
            
            // apply the position
            chatterInstances [_i].SetPosition(totalHeight, ShouldShowCharacter(_i, chatterInstances[_i]));

            // iterate the index
            _i++;
            if (_i >= chatterInstances.Count) _i = 0;
        }

        /// <summary>
        /// Returns true if chatterbox is ready to show a new chatter
        /// </summary>
        bool ChatterReady ()
        {
            if (chatterInstances.Count < 1) return true;
            if (chatterInstances [0] == null) return true;
            return _timer > readTime;
        }

        /// <summary>
        /// Should the given chatter item at the given index show the character name / portrait?
        /// This is used to reduce clutter & prevent showing the same character twice in a row.
        /// </summary>
        bool ShouldShowCharacter (int index, DUIChatterItem chatter)
        {
            if (index <= 0) return true;

            DUIChatterItem prevChatter = chatterInstances[index - 1];
            if (prevChatter == null) return true;

            if (prevChatter.chatterInfo.cType != ChatterType.crew) return true;

            if (prevChatter.chatterInfo.subject == chatter.chatterInfo.subject) return false;
            return true;
        }
    }



    public enum ChatterType
    {
        crew = 0,
        radio = 1,
        tutorial = 2
    }

    [System.Serializable]
    public class ChatterInfo
    {
        public ChatterType cType = ChatterType.crew;
        public Convo convo;
        public Character subject;
        public Dialogue dialogInstance;

        public ChatterInfo (Convo c, Character ch, Dialogue d, ChatterType type = ChatterType.crew)
        {
            convo = c;
            subject = ch;
            dialogInstance = d;
        }
    }

}