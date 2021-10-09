using UnityEngine;
using System.Collections;
using SpiderWeb;
using Diluvion;
using Diluvion.Ships;
using CharacterInfo = Diluvion.CharacterInfo;
using DUI;
using Sirenix.OdinInspector;
using System.Collections.Generic;

/// <summary>
/// Save file for characters
/// </summary>
[System.Serializable]
public class CharSave
{
    /// <summary>
    /// Name of the character info scriptable object.
    /// </summary>
    public string charObjName = "";

    /// <summary>
    /// Loc key of the station I was at when last saved.
    /// </summary>
    public string savedStation;

    /// <summary>
    /// The level of this character. Only currently used for officers.
    /// </summary>
    public int savedLevel = 1;

    /// <summary>
    /// Creates and returns an instance of the crew from this save. Since characters are never
    /// randomly generated, (sailors are) this is a simple load.
    /// </summary>
    public virtual Character CreateCharacter ()
    {
        CharacterInfo charInfo = CharactersGlobal.GetCharacter(charObjName);
        if (charInfo == null) return null;
        Character newChar = charInfo.InstantiateCharacter();
        newChar.station = savedStation;
        savedLevel = Mathf.Clamp(savedLevel, 1, GameManager.Mode().maxOfficerLevel);
        newChar.level = savedLevel;
        return newChar;
    }
}

namespace Diluvion
{

    /// <summary>
    /// Any interactible person. Can load dialogue, name, appearance, gender, 
    /// and voice from character info object.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D)), RequireComponent(typeof(SpriteRenderer)),
        RequireComponent(typeof(ColliderMatchSprite)), RequireComponent(typeof(Animator))]
    public class Character : MonoBehaviour, IDraggable
    {

        #region properties
        public static GameObject crewMoveParticle;

        [AssetsOnly, AssetList(Path = "Prefabs/Characters")]
        public CharacterInfo characterInfo;
        public string niceName {
            get
            {
                if (characterInfo)
                    return characterInfo.niceName;

                return gameObject.name;
            } 
        }

        [MinValue(1)] public int level = 1;

        [Space, ToggleLeft]
        [Tooltip("allows you to select other character instances that should only be shown when talking to this character")]
        public bool hasNeighbors;
        [ShowIf("hasNeighbors"), Tooltip("Hides neighbor instances until this instance is spoken to"), ToggleLeft]
        public bool hideNeighborsOnStart = true;
        [ShowIf("hasNeighbors")]
        public List<Character> neighbors = new List<Character>();

        

        [AssetsOnly]
        public List<Convo> convosToAdd = new List<Convo>();

        [Tooltip("Omits the Animator component. use this if there's another component controlling animation."), ToggleLeft]
        public bool omitAnimator;

        [ReadOnly, MultiLineProperty]
        public string swapInfo;

        [ReadOnly]
        public string station;

        [ReadOnly]
        public Station parentStation;

        [ReadOnly]
        public Station hoveredStation;

        // Delegates
        public delegate void OnHired ();
        public OnHired onHired;
        public delegate void OnTalkTo ();
        public OnTalkTo onTalkTo;
        public delegate void OnDialogCreated (Dialogue d);
        public OnDialogCreated onDialogCreated;

        public bool isVisible;

        protected Dialogue dialogueInstance;
        protected ColliderMatchSprite colMatcher;
        protected BoxCollider2D boxCol;
        protected Animator animator;
        protected SpriteRenderer spriteRenderer;
        protected string defaultAnimTrigger = "none";
        protected InteriorManager interiorManager;
        protected CrewManager crewManager;
        protected Transform parent;
        protected bool dragging;
        protected CharacterPlacer preDragSpot;

        DUICharacterBubbles _characterBubble;
        CrewOverview _crewOverview;

        #endregion
        
        #region unity events

        protected virtual void Start ()
        {
            if (!omitAnimator)
            {
                animator = GO.MakeComponent<Animator>(gameObject);
                animator.applyRootMotion = false;
            }

            boxCol = GO.MakeComponent<BoxCollider2D>(gameObject);
            spriteRenderer = GO.MakeComponent<SpriteRenderer>(gameObject);
            colMatcher = GO.MakeComponent<ColliderMatchSprite>(gameObject);

            // disable neighbors so there arent duplicates.
            if (hasNeighbors && hideNeighborsOnStart)
            {
                foreach (var n in neighbors) n.gameObject.SetActive(false);
            }

            SendMessageUpwards("AddInhabitant", this, SendMessageOptions.DontRequireReceiver);
        }

        protected virtual void Update ()
        {
            // Try to make the dialogue instance
            if (dialogueInstance == null) MakeDialog();

            isVisible = IsVisible();

            // Check for change in parent
            if (transform.parent != parent)
            {
                OnParentChange(parent, transform.parent);
                parent = transform.parent;
            }
        }

        void OnDestroy()
        {
            if (_characterBubble) _characterBubble.End(0);
        }
        
        
        void OnDisable ()
        {
            SendMessageUpwards("RemoveInhabitant", this, SendMessageOptions.DontRequireReceiver);
        }

        void OnEnable ()
        {
            SendMessageUpwards("AddInhabitant", this, SendMessageOptions.DontRequireReceiver);
        }
        
        #endregion

        /// <summary>
        /// Creates the save data to recreate this exact instance, and returns it.
        /// </summary>
        public virtual CharSave CreateSaveData ()
        {
            CharSave charSave = new CharSave();
            charSave.charObjName = characterInfo.name;
            charSave.savedStation = StationName();
            charSave.savedLevel = level;
            return charSave;
        }

        #region movement, boarding

        /// <summary>
        /// Called whenever the parent transform changes. Adds/removes inhabitant from interior managers,
        /// and checks new parent for a crew manager. Returns true if the new parent is a new interior manager; false otherwise.
        /// </summary>
        protected virtual bool OnParentChange (Transform oldParent, Transform newParent)
        {
            // Check for station in the old parent
            if (oldParent)
            {
                Station oldSt = oldParent.GetComponentInParent<Station>();
                if (oldSt) LeaveStation(oldSt);
            }

            // Check for station in the new parent
            Station st = GetComponentInParent<Station>();
            if (st) JoinStation(st);

            // Check for an interior manager in the new parent
            InteriorManager newInterior = GetComponentInParent<InteriorManager>();
            if (newInterior == interiorManager) return false;


            // If there was a previous interior, remove myself as an inhabitant.
            if (interiorManager) interiorManager.RemoveInhabitant(this);

            // Set the memorized interior
            interiorManager = newInterior;
            if (interiorManager) interiorManager.AddInhabitant(this);


            CrewManager newCrewManager = GetComponentInParent<CrewManager>();

            if (newCrewManager != crewManager && newCrewManager != null)
            {
                crewManager = newCrewManager;
                BoardShip(newCrewManager);
            }

            //if camera target is this, tell camera to remove it
            OrbitCam.ClearFocus(gameObject);

            return true;
        }

        /// <summary>
        /// Boards the ship of the given crew manager. Searches and attempts to join saved station, if any. If
        /// no station is saved, joins the heart station.
        /// </summary>
        /// <param name="targetShip">The CrewManager of the ship to board.</param>
        protected virtual void BoardShip (CrewManager targetShip)
        {
            //Debug.Log(name + "Boarding ship " + targetShip.name + " to search for station: " + station, gameObject);
            if (onHired != null) onHired();

            // Set color to regular color
            GO.MakeComponent<SpriteRenderer>(gameObject).color = Color.white;

            // Check if the saved station is on this ship
            Station s = targetShip.FindStationOfType(station);
            if (s) JoinStation(s);

            // If no saved station, join the heart station.
            else JoinHeartStation();

            QuestManager.Tick();
        }
        #endregion

        #region stations

        /// <summary>
        /// If I have a station saved, I'll join it. otherwise, join the heart station.
        /// </summary>
        public void JoinDefaultStation()
        {
            if (!crewManager)
            {
                Debug.LogError(name + " couldnt find crew manager while trying to join default station!");
                return;
            }
            
            // Check if the saved station is on this ship
            Station s = crewManager.FindStationOfType(station);
            if (s) JoinStation(s);

            // If no saved station, join the heart station.
            else JoinHeartStation();
        }

        public bool CanJoinHoveredStation()
        {
            if (!hoveredStation) return false;
            return CanJoinStation(hoveredStation);
        }
        
         protected virtual bool CanJoinStation (Station st)
        {
            return st.operational;
        }

        /// <summary>
        /// Returns a name appropriate for saving. Returns blank if not currently in a station.
        /// </summary>
        protected string StationName ()
        {
            Station st = GetComponentInParent<Station>();
            if (!st) return "";
            return st.stationLocKey;
        }

        /// <summary>
        /// Moves to the heart station.
        /// </summary>
        public void JoinHeartStation ()
        {
            if (crewManager == null)
            {
                Debug.LogError("No crewmanager found when trying to join heart station.", gameObject);
                return;
            }
            
            if (crewManager.heartStation == null)
            {
                Debug.LogError("No heart station found.", gameObject);
                return;
            }
            
            JoinStation(crewManager.heartStation);
        }

        /// <summary>
        /// Join the given station. For characters, just joins as a guest, doesn't change anything about the station.
        /// </summary>
        public virtual void JoinStation (Station st)
        {
            if (st == null)
            {
                Debug.LogError(niceName + " can't join station because it's null.");
                return;
            }
            
            //Debug.Log(niceName + " attempting to join station " + st.name);
            
            if (!st.operational)
            {
                Debug.Log(niceName + " can't join station " + st.name + " because it's not operational.");
                return;
            }

            // See if there's any spots available
            //Debug.Log("Checking for station spot...");
            CharacterPlacer mySpot = st.NextAvailableSpot();

            // If there's a spot available, make me visible & selectable
            if (mySpot != null)
            {
                st.NextAvailableSpot().FinalizePlacement(this);
                AddCrewMoveParticle();
            }
            //else Debug.Log(niceName + " couldn't find a spot to chill at in station " + st.name);

            parentStation = st;
            
            // play sound for all but heart stations
            if (!(st is HeartStation))
                SpiderSound.MakeSound("Play_Add_CrewMate_ToRoom", gameObject);

            st.UpdateStation();
        }

        /// <summary>
        /// Leaves whatever station I'm in
        /// </summary>
        public virtual void LeaveStation (Station st)
        {
            if (st == null) return;

            parentStation = null;
            
            // play sound for all but heart station
            if (!(st is HeartStation))
                SpiderSound.MakeSound("Play_Remove_CrewMate_FromRoom", gameObject);

            Debug.Log("Leaving station " + st.name);
            st.UpdateStation();
        }

        public void LeaveStation ()
        {
            Station s = GetComponentInParent<Station>();
            if (s) LeaveStation(s);

            parentStation = null;
        }


        /// <summary>
        /// Returns the first station that comes up in an overlap circle check of the given radius.
        /// </summary>
        /// <param name="radius">Radius around the character to check for stations.</param>
        protected Station FindStation (float radius = .1f)
        {
            foreach (Collider2D col in Physics2D.OverlapCircleAll(transform.position, .1f))
            {
                Station st = col.GetComponent<Station>();
                if (!st) continue;
                return st;
            }

            return null;
        }

        /// <summary>
        /// Returns the string to display in the info window when joining a station
        /// </summary>
        protected virtual string JoinStationInfo (Station st)
        {
            if (!st.operational) return CantJoinInfo(st);

            string s = "";

            s = string.Format(s, GetLocalizedName(), st.LocalizedName());
            return s;
        }

        /// <summary>
        /// Returns the string to display in the info window when leaving a station.
        /// </summary>
        protected virtual string LeaveStationInfo (Station st)
        {
            return "";
        }

        protected virtual string JoinHeartStationString ()
        {
            string s = "{0} will rest in the Crew Quarters and repair any damage.";
            return string.Format(s, GetLocalizedName());
        }

        protected virtual string LeaveHeartStationString ()
        {
            string s = "{0} will leave the Crew Quarters.";
            return string.Format(s, GetLocalizedName());
        }

        #endregion


        #region player interactions
        public void OnPointerEnter ()
        {
            if (!dialogueInstance) return;
            if (!dialogueInstance.HasDialogue()) return;
            
            CreateBubbleHud();
            if (!dragging && dialogueInstance)
            {
                if (_characterBubble) _characterBubble.ShowBubbles(true);
                
                if (!_crewOverview) _crewOverview = CrewOverview.CreateInstance(this);
            }
        }

        public void OnPointerExit ()
        {
            if (_characterBubble) _characterBubble.ShowBubbles(false);
            
            // end the crew overview UI
            if (_crewOverview && !dragging) _crewOverview.End();
        }

        public void OnClick () { }

        public void OnRelease ()
        {
            dragging = false;
            SpeakToMe();
        }
        

        public void OnDragStart ()
        {
            if (!CanDrag()) return;
            dragging = true;

            //Debug.Log("Dragging begin! " + name);

            // Show overview UI
            if (_crewOverview != null) _crewOverview.End();
            
            _crewOverview = CrewOverview.CreateInstance(this);

            if (_characterBubble) _characterBubble.ShowBubbles(false);
            
            CharacterPlacer placer = GetComponentInParent<CharacterPlacer>();
            if (placer) preDragSpot = placer;
        }

        public void OnDragEnd ()
        {
            dragging = false;
            Debug.Log("Dragging ended. " + name);

            hoveredStation = null;

            // end the crew overview UI
            if (_crewOverview) _crewOverview.End();

            Station st = FindStation();
            if (st)
            {
                if (CanJoinStation(st))
                {
                    Debug.Log("Setting parent to " + st.name);
                    transform.SetParent(st.transform);
                    return;
                }
            }
            else Debug.Log("No station was found in the vicinity of the drag end.");

            // If nothing happened, just return to my previous character spot (pre drag spot)
            if (preDragSpot) preDragSpot.PlaceHere(this);
            preDragSpot = null;
        }

        public void OnDrag (Vector2 dragPos)
        {
            if (!dragging) return;
            transform.position = dragPos;
            SetSwapInfo();
        }

        /// <summary>
        /// Sets the swap info string based on what station is nearby.
        /// </summary>
        void SetSwapInfo ()
        {
            swapInfo = "";

            // Check if hovering a new station
            Station s = FindStation();
            if (s && s != parentStation)
            {
                hoveredStation = s;

                if (s as HeartStation != null)
                    swapInfo = JoinHeartStationString();

                else swapInfo = JoinStationInfo(s);
            }
            else hoveredStation = null;
        }

        protected string CantJoinInfo (Station st)
        {
            string s = "{0} can't join {1} because it's not active!";
            return string.Format(s, GetLocalizedName(), st.LocalizedName());
        }


        /// <summary>
        /// Is this a member of the player's crew who can be dragged?
        /// </summary>
        protected virtual bool CanDrag ()
        {
            return PlayerManager.PlayerCrew().AllCharactersOnBoard().Contains(this);
        }

        public void OnFocus()
        {
            OrbitCam.SetFocusOffset(new Vector2(0, -.15f));
        }

        public void OnDefocus()
        {
            OrbitCam.ClearFocusOffset();
        }
        #endregion

        #region dialogue and conversations
        public virtual void SpeakToMe ()
        {
            if (!dialogueInstance)
            {
                Debug.LogWarning(name + " doesn't have a dialog instance!", gameObject);
                return;
            }
            
            OrbitCam.FocusOn(gameObject);

            // Show the dialog box if there's any conversations available
            if (dialogueInstance.HasDialogue() || dialogueInstance.OnPlayerCrew())
            {
                dialogueInstance.CreateUI();

                // show neighbors
                if (hasNeighbors)
                {
                    foreach (var n in neighbors)
                    {
                        n.gameObject.SetActive(true);
                        interiorManager.IsolateInstance(n);
                    }
                }
            }

            onTalkTo?.Invoke();
        }


        /// <summary>
        /// Echoes the command related to the given hashtag. "firing!", "pinging!", etc
        /// </summary>
        public void PushEcho (string hashtag)
        {
            if (dialogueInstance != null)
                dialogueInstance.PassHashtag(hashtag);
        }
        
        protected virtual void MakeDialog ()
        {
            if (characterInfo)
                if (characterInfo.dialogue)
                    PrepDialogue(characterInfo.dialogue);
        }

        /// <summary>
        /// Instantiates the dialog instance, calls 'onDialogueCreated' delegate
        /// </summary>
        protected void PrepDialogue (Dialogue d)
        {
            //have dialogue create an instance
            dialogueInstance = d.NewDialogInstance(this);

            // add convos on this instance
            dialogueInstance.manualAdded.AddRange(convosToAdd);

            onDialogCreated?.Invoke(dialogueInstance);
            
            // create character bubbles
            CreateBubbleHud();
        }
        #endregion


        public DUICharacterBubbles CreateBubbleHud()
        {
            if (_characterBubble) return _characterBubble;
            _characterBubble = DUICharacterBubbles.CreateInstance(this, dialogueInstance);
            return _characterBubble;
        }

        #region audio
        /// <summary>
        /// Makes the 'end conversation' grunt sfx
        /// </summary>
        public virtual void LeaveMe ()
        {
            if (characterInfo)
                ConvoEndSound(characterInfo.gender);

            if (hasNeighbors)
            {
                interiorManager.EnableInstances();
                DefaultNeighbors();
            }
        }

        /// <summary>
        /// Returns any neighbors I've activated for dialogue back to their default state.
        /// </summary>
        public void DefaultNeighbors()
        {
            foreach (var n in neighbors) 
                n.gameObject.SetActive(false);
        }

        protected void ConvoEndSound (Gender g)
        {
            SpiderSound.SetSwitch("Voice_Gender", g.ToString(), gameObject);
            SpiderSound.SetSwitch("Voice_Profile", g.ToString(), gameObject);
            SpiderSound.MakeSound("Play_Character_StopTalk", gameObject);
        }


        public virtual void ClickSound ()
        {
            if (characterInfo)
                PlayTalkSound(characterInfo.gender);
        }

        protected void PlayTalkSound (Gender g)
        {
            SpiderSound.SetSwitch("Voice_Gender", g.ToString(), gameObject);
            SpiderSound.SetSwitch("Voice_Profile", g.ToString(), gameObject);
            SpiderSound.MakeSound("Play_Character_Talk", gameObject);
        }


        protected virtual void DeathSound (bool violent)
        {
            if (characterInfo) PlayDeathSound(violent, characterInfo.gender);
        }

        protected void PlayDeathSound (bool violent, Gender g)
        {
            if (g == Gender.Male)
            {
                if (violent)
                    SpiderSound.MakeSound("Play_Male_01_Scream", gameObject);
                else
                    SpiderSound.MakeSound("Play_Male_01_Sigh", gameObject);
            }
            else
            {
                if (violent)
                    SpiderSound.MakeSound("Play_Female_01_Scream", gameObject);
                else
                    SpiderSound.MakeSound("Play_Female_01_Sigh", gameObject);
            }
        }

        #endregion

        #region naming and loc
        /// <summary>
        /// Returns the non localized name of this crew. Used for dialog saving purposes.
        /// </summary>
        public virtual string NonLocalizedName ()
        {
            if (characterInfo) return characterInfo.niceName;
            return niceName;
        }


        /// <summary>
        /// Whether pre-set or random generated, returns the language localized name of this instance.
        /// </summary>
        public virtual string GetLocalizedName ()
        {
            return Localization.GetFromLocLibrary(CharactersGlobal.namePrefix + characterInfo.niceName, niceName);
        }
        #endregion


        protected Animator Animator ()
        {
            if (omitAnimator) return null;
            animator = GetComponent<Animator>();
            return animator;
        }

        #region animation
        /// <summary>
        /// Tries to play the given animation for [playTime] seconds. If playTime is 0 or left
        /// default, it will play the new animation indefinitely.
        /// <param name="setDefault"> When a dialog is done, the character will return to their default animation.
        /// if setDefault = true, this will become the default animation. Use this for character placers, stations, etc.</param>
        /// </summary>
        public void SetAnimation (string trigger, float playTime = 0, bool setDefault = false)
        {
            if (omitAnimator) return;
            Animator().SetTrigger(trigger);

            // If this animation should only play for a set time, call a timer to return to the old anim
            if (playTime > 0) StartCoroutine(DelaySetAnimation(playTime, defaultAnimTrigger));
        }

        public void SetAnimation (CrewAnimationTool animTool)
        {
            SetAnimation(animTool.animTag, animTool.animTime, animTool.setAsDefault);
        }

        IEnumerator DelaySetAnimation (float delayTime, string animTrigger)
        {
            yield return new WaitForSeconds(delayTime);
            SetAnimation(animTrigger);
        }

        public void SetDefaultAnimation ()
        {
            SetAnimation(defaultAnimTrigger, 0, true);
        }
        #endregion


        public virtual Appearance GetAppearance ()
        {
            if (characterInfo == null) return null;
            return characterInfo.appearance;
        }

        Room _parentRoom;

        Room ParentRoom()
        {
            if (_parentRoom) return _parentRoom;
            _parentRoom = GetComponentInParent<Room>();
            return _parentRoom;
        }
        
        bool IsVisible()
        {
            
            if (!interiorManager) return false;
            if (transform.lossyScale.magnitude < .1f) return false;
            if (ParentRoom())
            {
                if (!ParentRoom().visible) return false;
            }
            
            
            return InteriorView.ViewedInterior() == interiorManager;
        }


        protected void AddCrewMoveParticle ()
        {
            if (crewMoveParticle == null) crewMoveParticle = Resources.Load("effects/crew move particle") as GameObject;

            GameObject particleInstance = Instantiate(crewMoveParticle, transform.position, Quaternion.identity) as GameObject;
            particleInstance.transform.SetParent(transform.parent);
        }

        public virtual void Die (bool leaveGravestone = true, bool showPopup = false, bool violent = false)
        {
            LeaveStation();

            PlayerManager.BoardingParty().Remove(this);

            if (leaveGravestone)
            {
                DeathSound(violent);

                if (showPopup)
                {
                    /* TODO
                    // Get title & body for 'crew died' popup
                    string titleText = Localization.GetFromLocLibrary("popup_crewDied_title", "titl");
                    string bodyText = Localization.GetFromLocLibrary("popup_crewDied_body", "_body");

                    // Format the strings so they appear with this crewmember's name
                    titleText = string.Format(titleText, GetLocalizedName());
                    bodyText = string.Format(bodyText, GetLocalizedName());

                    DUIController.Get().AddPopup(titleText, bodyText);
                    */
                }

                // Spawn the gravestone object
                GameObject graveStone = Resources.Load("gravestone") as GameObject;

                GameObject graveStoneInstance = Instantiate(graveStone.gameObject) as GameObject;
                graveStoneInstance.layer = gameObject.layer;
                graveStoneInstance.transform.parent = transform.parent;
                graveStoneInstance.transform.localPosition = transform.localPosition;
                graveStoneInstance.transform.localRotation = transform.localRotation;

                SpriteRenderer graveStoneSprite = graveStoneInstance.GetComponentInChildren<SpriteRenderer>();

                graveStoneSprite.sortingLayerID = spriteRenderer.sortingLayerID;
                graveStoneSprite.sortingOrder = spriteRenderer.sortingOrder;

                GraveStone graveStoneScript = graveStoneInstance.GetComponent<GraveStone>();
                graveStoneScript.Init(this, spriteRenderer.enabled);
            }

            Destroy(gameObject, .5f);
        }


        public Vector3 TopCenter ()
        {
            if (spriteRenderer == null) return Vector3.zero;

            // Get the bounds of the sprite renderer
            Bounds spriteBounds = spriteRenderer.bounds;

            // Get vector3 pos for top center of sprite renderer
            float yPos = spriteBounds.center.y + spriteBounds.extents.y;
            Vector3 topCenter = new Vector3(spriteBounds.center.x, yPos, spriteBounds.center.z);
            return topCenter;
        }

    }
}