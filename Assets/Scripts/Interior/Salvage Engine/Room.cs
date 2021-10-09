using System;
using System.Collections;
using System.Collections.Generic;
using Diluvion.SaveLoad;
using UnityEngine;
using Sirenix.OdinInspector;
using SpiderWeb;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Diluvion
{
    /// <summary>
    /// A single room inside an interior.
    /// </summary>
    public class Room : MonoBehaviour
    {
        [ReadOnly, Tooltip("The gold value given to this room by room placer.")]
        public float goldValue;
        [ReadOnly, Tooltip("The danger value given to this room by room placer.")]
        public float danger;

        [ReadOnly] public int rewardsFound;
        
        [BoxGroup("visibility", false), ToggleLeft]
        public bool startVisible;

        [BoxGroup("visibility", false), ToggleLeft]
        public bool visible;
        
        public delegate void roomDelegate();
        public roomDelegate onBecomeVisible;

        #region saving vars

        [BoxGroup("save", false)]
        public bool saveState;

        [ValidateInput("ValidSaveID",  "This ID isn't unique!")]
        [ShowIf("saveState")]
        [BoxGroup("save", false)]
        public string saveID = "enter unique ID";

        /// <summary>
        /// Check if this save ID is unique. (only checks other rooms in scene)
        /// </summary>
        bool ValidSaveID (string ID)
        {
            if (!saveState) return true;

            foreach (Room r in FindObjectsOfType<Room>())
            {
                if (!r.saveState) continue;
                if (r == this) continue;
                if (r.saveID == ID) return false;
            }

            return true;
        }

        [Button]
        void ShowMe()
        {
            visible = true;
            SetVisible();
        }
        
        #endregion

        bool MissingDoor()
        {
            return !door;
        }

        #region local center
        [Space, PropertyOrder(1)]
        [BoxGroup("center", false)]
        [ReadOnly, ValidateInput("ValidateCenter", "No center point seems to be set on this room.", InfoMessageType.Warning)]
        public Vector3 localCenter;

        
        [BoxGroup("center", false)]
        [Button, PropertyOrder(2)]
        public void FindCenter ()
        {
            Bounds b;

            // encapsulate every renderer
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            {
                if (!ValidForBounds(sr)) continue;
                b = new Bounds(sr.transform.position, Vector3.one * .01f);
                GrowBounds(b);
                break;
            }
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
       
        #endregion

        [Space]

        [BoxGroup("travel", false)]
        [PropertyOrder(102)]
        public Room nextRoom;

        [PropertyOrder(99)]
        [BoxGroup("travel", false)]
        [ReadOnly, InfoBox("No door found! Please add a door.", InfoMessageType.Error, "MissingDoor")]
        public Door door;

        [Space]

        [InlineEditor(InlineEditorModes.FullEditor, DrawHeader = false, Expanded = true)]
        [AssetsOnly]
        [ShowIf("HasTableEntry"), PropertyOrder(500)]
        public RoomEntry roomEntry;
        
        #if UNITY_EDITOR

        [Button("Create/find table entry")]
        [HideIf("HasTableEntry")]
        void GetRoomEntry ()
        {
            roomEntry = ScriptableObjectUtility.CreateRoomAsset(gameObject);
            EditorUtility.SetDirty(this);
        }
        #endif

        Vector3 _initScale;

        bool ValidateCenter(Vector3 center)
        {
            if (center == Vector3.zero) return false;
            return true;
        }

        bool HasTableEntry()
        {
            return (roomEntry != null);
        }
        

        void OnDrawGizmosSelected()
        {
            if (nextRoom)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(RoomCenter(), nextRoom.RoomCenter());
            }

            Gizmos.color = new Color(0, 0, 1, .2f);
            Gizmos.DrawWireSphere(localCenter + transform.position, .01f);
        }

        void Start()
        {
            _initScale = transform.localScale;
            if (startVisible) SetVisible();
            else SetHidden();
            
            // If there's an interior manager, have it update my visibility whenever it shows
            InteriorManager parentInterior = GetComponentInParent<InteriorManager>();
            if (!parentInterior) return;
            
            parentInterior.onShow += UpdateVisibility;
        }


        void UpdateVisibility()
        {
            if (saveState)
            {
                visible = DSave.RoomIsOpen(saveID);
                if (visible) SetVisible();
                else SetHidden();
            }
        }


        /// <summary>
        /// Sets reference to the next room, and disables / enables the door accordingly.
        /// </summary>
        public void SetNextRoom(Room r)
        {
            nextRoom = r;

            if (nextRoom) door.gameObject.SetActive(true);
            else door.gameObject.SetActive(false);
        }

        public void ShowNextRoom()
        {
            if (!nextRoom) return;
            
            if (nextRoom.HasHazard())
            {
                // allow changes to boarding party if none exist
                if (PlayerManager.BoardingParty().Count < 1)
                    PlayerManager.AskForBoardingParty();
            }
            
            StartCoroutine(ShowNextRoomSequence());
        }

        IEnumerator ShowNextRoomSequence()
        {
            Debug.Log("next room sequence: waiting for boarding party query...");
            
            yield return new WaitForSeconds(.3f);
            
            Debug.Log("show next room sequence");

            if (PlayerManager.BoardingParty().Count < 1 && nextRoom.HasHazard())
            {
                Debug.Log("Player still has no boarding party; cancelling sequence.");
                yield break;
            }

            PathAnimator.MakePath(door.transform.position, nextRoom.RoomCenter(), transform.parent);

            yield return new WaitForSeconds(.5f);
            
            // once the next room shows, turn off my door.
            nextRoom.onBecomeVisible += TurnMyDoorOff;
            
            nextRoom.TryEnterRoom();
        }

        bool HasHazard()
        {
            return GetComponent<HazardContainer>() != null;
        }

        /// <summary>
        /// The center of the room in world space
        /// </summary>
        public Vector3 RoomCenter()
        {
            return transform.position + localCenter;
        }

        /// <summary>
        /// Checks if there's any hazards on this room. If so, brings up the hazard window. If not, continues.
        /// </summary>
        void TryEnterRoom()
        {
            HazardContainer c = GetComponent<HazardContainer>();

            if (c) c.Appear();
            else EnterRoom();
        }

        /// <summary>
        /// Make the room visible, call onBecomeVisible delegates, and handle music
        /// </summary>
        public void EnterRoom()
        {
            //if (!nextRoom) GetComponentInParent<RoomPlacer>()?.AllClearMusic();
            
            // Play the 'all clear' music if this is part of a dungeon, and if this is the last room
            if (GetComponentInParent<RoomPlacer>())
            {
                if (!nextRoom) AKMusic.Get().SetAdventure(AdventureDifficulty.Cleared);
            }

            if (saveState) DSave.SaveRoom(saveID);
            
            onBecomeVisible?.Invoke();
            SetVisible();
            
            SpiderSound.MakeSound("Play_Room_Appearance", gameObject);
        }


        public void SetVisible()
        {
            visible = true;
            StartCoroutine(LerpScale(Vector3.one, .3f));
        }

        void SetHidden()
        {
            visible = false;
            StartCoroutine(LerpScale(Vector3.zero, .1f));
        }

        IEnumerator LerpScale(Vector3 end, float t)
        {
            Vector3 startScale = transform.localScale;

            float progress = 0;
            while (progress < 1)
            {
                transform.localScale = Vector3.Lerp(startScale, end, progress);                
                progress += Time.deltaTime / t;
                yield return null;
            }
            transform.localScale = end;
        }
        
        #region bounds

        Bounds GrowBounds(Bounds b)
        {
            // encapsulate every renderer
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            {
                if (!ValidForBounds(sr))
                {
                    continue;
                }
                b.Encapsulate(sr.bounds);
            }

            localCenter = b.center - transform.position;
            return b;
        }

        bool ValidForBounds (SpriteRenderer sr)
        {
            if (!sr.enabled)
            {
                Debug.Log(sr.name + " not enabled.");
                return false;
            }
            if (sr.sprite == null)
            {
                Debug.Log(sr.name + " no sprite.");
                return false;
            }
            if (sr.sortingLayerName == "Default")
            {
                Debug.Log(name + " wrong sorting layer. (default layer sprites arent counted)");
                return false;
            }
            return true;
        }

        #endregion


        #region door
        /// <summary>
        /// Turns off my door - it will no longer be clickable, since it's already been opened.
        /// </summary>
        void TurnMyDoorOff()
        {
            door?.TurnDoorOff();
        }
        
        [ButtonGroup("door"), PropertyOrder(100), ShowIf("MissingDoor")]
        void GetDoor()
        {
            door = GetComponentInChildren<Door>(true);
            if (!door) Debug.LogError("No door available in " + name);
        }

        [ButtonGroup("door"), PropertyOrder(100), ShowIf("MissingDoor")]
        void CreateDoor()
        {
            GameObject newDoor = new GameObject();
            Transform t = newDoor.transform;
            t.parent = transform;
            t.localScale = Vector3.one;
            t.localEulerAngles = t.localPosition = Vector3.zero;
            newDoor.layer = gameObject.layer;
            newDoor.name = "door";
            door = newDoor.AddComponent<Door>();

            SpriteRenderer doorSR = newDoor.GetComponent<SpriteRenderer>();
            doorSR.sprite = door.spriteSet.normal;
            doorSR.sortingLayerName = "BG1";
            doorSR.sortingOrder = 50;
        }
        
        #endregion
    }
}