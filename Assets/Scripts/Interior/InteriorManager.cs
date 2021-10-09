using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Diluvion.Ships;

namespace Diluvion
{
    /// <summary>
    /// Component attached to ALL interiors. Keeps track of the 'partner' 3D object, as well as inhabitants (characters) and sprites.
    /// </summary>
    public class InteriorManager : MonoBehaviour
    {
        #region declare

        [AssetsOnly]
        public LocTerm myName;
        [AssetsOnly]
        public LocTerm description;

        public delegate void InteriorDel();
        public InteriorDel onShow;
        public InteriorDel onHide;

        //[TabGroup("general")]
        //[Tooltip("If vulnerable, mistakes in salvaging will cause it to collapse")]
        //public bool vulnerable;

        //[TabGroup("general"), Range(1, 10), ShowIf("vulnerable")]
        //[Tooltip("The number of mistakes that can be made before this interior collapses")]
        //public int strikes = 3;

        [TabGroup("general"), Tooltip("Optional; focus on the object when viewing this interior")]
        public GameObject defaultFocus;

        [TabGroup("general"), Tooltip("The default offset for when this interior is transitioned to")]
        public Vector2 defaultOffset;

        [TabGroup("runtime"), ReadOnly, ShowInInspector]
        List<SpriteRenderer> allSprites = new List<SpriteRenderer>();

        [TabGroup("runtime"), ReadOnly]
        public List<Character> allInhabitants = new List<Character>();

        [TabGroup("runtime"), ReadOnly]
        public Bounds spriteBounds;

        [TabGroup("story"), OnValueChanged("RefreshStoryState")]
        public StoryState storyState;

        [TabGroup("general"), ReadOnly]
        [Tooltip("Mask is defunct, but the reference is left in so any that are remaining can be removed by code at runtime.")]
        public GameObject mask;

        [TabGroup("general")]
        public AKStateCall musicTrack;

        AkState akState;
        SideViewerStats sideViewStats;
        
        [ReadOnly, TabGroup("story")]
        public List<StoryStateGroup> stateGroups = new List<StoryStateGroup>();

        /// <summary>
        /// Get all children with story state group components, if any
        /// </summary>
        [Button, TabGroup("story")]
        void GetStateGroups()
        {
            stateGroups.Clear();
            stateGroups.AddRange(GetComponentsInChildren<StoryStateGroup>(true));
        }


        public void RefreshStoryState()
        {
            foreach (var g in stateGroups)
            {
                g.gameObject.SetActive(g.myStoryState == storyState);
            }
        }
        
        #endregion

        /// <summary>
        /// Returns the interior manager related to the given 3D world object
        /// </summary>
        public static InteriorManager Get(GameObject mainObject)
        {
            InteriorManager interior = mainObject.GetComponentInChildren<InteriorManager>();
            if (interior) return interior;

            SideViewerStats s = mainObject.GetComponentInChildren<SideViewerStats>();

            if (s) interior = s.intMan;
            return interior;
        }

        /// <summary>
        /// Gets ALL interior managers in the given 3D world object
        /// </summary>
        public static List<InteriorManager> GetAll(GameObject mainObject)
        {
            List<InteriorManager> returnList = new List<InteriorManager>();

            returnList.AddRange(mainObject.GetComponentsInChildren<InteriorManager>());

            foreach (SideViewerStats s in mainObject.GetComponentsInChildren<SideViewerStats>())
            {
                if (!s.intMan) continue;
                if (returnList.Contains(s.intMan)) continue;
                returnList.Add(s.intMan);
            }

            return returnList;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            
            //Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(spriteBounds.center, spriteBounds.size);
            
            // Draw the default position 
            Gizmos.color = Color.blue;
            Vector3 gizmoPos =  transform.position - transform.forward * 2 + transform.rotation * (Vector3) defaultOffset;
            Gizmos.DrawWireSphere(gizmoPos, .1f);
        }

        void Awake()
        {
            sideViewStats = GetComponentInParent<SideViewerStats>();
            if (sideViewStats) sideViewStats.intMan = this;

            InteriorView.AddInterior(this);
        }

        void Start()
        {
            // Get the music state
            if (akState == null) akState = GetComponent<AkState>();
            if (akState) musicTrack.StateStats(akState);

            // Remove any local masks - we don't use them anymore
            if (mask) Destroy(mask);
            
            allSprites = new List<SpriteRenderer>();
            allSprites.AddRange(GetComponentsInChildren<SpriteRenderer>());
            
            MakeBoundingBox();
        }


        /// <summary>
        /// Returns the 'partner' 3D object that this is the interior of.
        /// </summary>
        public Transform GetWorldParent()
        {
            if (!sideViewStats) return transform.parent;
            return sideViewStats.transform;
        }

        /// <summary>
        /// Returns the bridge linked to the 'partner' 3D object that this is the interior of.
        /// </summary>
        public Bridge Bridge()
        {
            return GetWorldParent().GetComponent<Bridge>();
        }

        /// <summary>
        /// Is the camera viewing (2D or 3D) the thing that I'm the interior of?
        /// </summary>
        public bool SubjectOfCamera()
        {
            if (OrbitCam.CurrentTarget() == null) return false;
            Transform target = OrbitCam.CurrentTarget();

            SideViewerStats stats = target.GetComponent<SideViewerStats>();
            if (stats == null) return false;
            if (stats.intMan == null) return false;

            return (stats.intMan == this);
        }

        #region inhabitants
        /// <summary>
        /// This is called anytime a crew joins this interior.
        /// </summary>
        public void AddInhabitant(Character newCrew)
        {
            if (allInhabitants.Contains(newCrew)) return;
            allInhabitants.Add(newCrew);
        }

        /// <summary>
        /// This is called anytime a crew leaves this interior.
        /// </summary>
        public void RemoveInhabitant(Character newCrew)
        {
            allInhabitants.Remove(newCrew);
        }

        /// <summary>
        /// Returns the crew component of a character from a string name.
        /// </summary>
        public Character GetInhabitant(CharacterInfo requestedCrew)
        {
            foreach (Character crew in allInhabitants)
            {
                if (!crew.gameObject.activeInHierarchy) continue;
                if (crew.characterInfo == requestedCrew) return crew;
            }
            return null;
        }

        /// <summary>
        /// Disables all instances of the given character info but the selected one.
        /// </summary>
        public void IsolateInstance(Character character)
        {
            if (character == null)
            {
                Debug.LogError("The given character is null, can't isolate it!");
                return;
            }

            if (character.characterInfo == null)
            {
                Debug.Log("The given character has no info set, can't isolate it!");
                return;
            }
            
            foreach (var c in allInhabitants)
            {
                if (c == character) continue;

                if (c.characterInfo == character.characterInfo)
                    disabledInstances.Add(c);
            }
            
            foreach (var i in disabledInstances) i.gameObject.SetActive(false);
        }

        /// <summary>
        /// Returns all instances of characters that have been disabled by the neighbor system to their default state.
        /// </summary>
        public void EnableInstances()
        {
            foreach (var c in disabledInstances) c.gameObject.SetActive(true);
            disabledInstances.Clear();

            List<Character> charsWithNeighbors = new List<Character>();
            foreach (var c in allInhabitants)
            {
                if (charsWithNeighbors.Contains(c)) continue;
                if (c.hasNeighbors) charsWithNeighbors.Add(c); //c.DefaultNeighbors());
            }
            
            foreach (var c in charsWithNeighbors) c.DefaultNeighbors();
            charsWithNeighbors.Clear();
        }

        List<Character> disabledInstances = new List<Character>();
        
        #endregion

        #region animation
        
        public void StopAnimation()
        {
            Animator animator = GetComponent<Animator>();
            if (animator != null) animator.speed = 0;
        }

        public void StartAnimation()
        {
            Animator animator = GetComponent<Animator>();
            if (animator != null) animator.speed = 1;
        }
        #endregion

        [Button]
        void MakeBoundingBoxEditor()
        {
            Start();
            GetSpriteBounds();
        }
        
        void MakeBoundingBox()
        {
            spriteBounds = new Bounds(CenterOfSprites(), Vector3.one * .1f);

            //encapsulate all the sprites bounds to make the total interior bounds
            foreach (SpriteRenderer sr in allSprites)
            {
                //ignore missing sprites
                if (sr == null) continue;
                if (!sr.sprite) continue;
                // ignore zero scaled sprites
                if (sr.transform.lossyScale.magnitude < .1f) continue;

                spriteBounds.Encapsulate(sr.bounds);
            }
        }

        Vector3 CenterOfSprites()
        {
            //get the center of all sprites - not necessarily the same as interior objects position
            Vector3 spriteCenter = Vector3.zero;

            //get average of position of all the sprites
            foreach (SpriteRenderer sr in allSprites)
            {
                if (sr == null) continue;
                spriteCenter += sr.transform.position;
            }
            spriteCenter = spriteCenter / allSprites.Count;

            return spriteCenter;
        }

        /// <summary>
        /// Returns the bounds of this interior's sprites.
        /// </summary>
        public Bounds GetSpriteBounds()
        {
            MakeBoundingBox();           
            return spriteBounds;
        }

        public void TranslateOutOfView()
        {
            transform.localPosition = new Vector3(1000, 1000, 1000);
            transform.localEulerAngles = Vector3.zero;
        }

        /// <summary>
        /// Returns the difference between the interior bounds center and the interior object. This can be used to center
        /// the camera over the interior's sprites when it's doing transitions.
        /// </summary>
        public Vector2 GetInteriorOffset()
        {
            return defaultOffset;
            //return sideViewStats.defaultOffset;
            //return spriteBounds.center - transform.position;
        }

        /// <summary>
        /// Sends the given trigger to the animator of each inhabitant. If a subject is specified, 
        /// will only send the trigger to that subject.
        /// </summary>
        public void TriggerCrewAnimations(CrewAnimationTool animTool, CharacterInfo subject = null)
        {
            // Call trigger for each inhabitant
            if (subject == null)
                foreach (Character c in allInhabitants) c.SetAnimation(animTool);

            // Call trigger for a specific inhabitant
            else
            {
                Character actualSubject = GetInhabitant(subject);
                if (actualSubject == null) return;

                actualSubject.SetAnimation(animTool);
            }
        }

        public void ChangeStoryState(StoryState newState)
        {
            storyState = newState;
            RefreshStoryState();
        }

        /// <summary>
        /// Starts or stops the audio for this interior.
        /// </summary>
        public void InteriorAudio(bool play)
        {
            // Play or stop music
            if (akState)
            {
                if (play) AKMusic.Get().AddState(musicTrack, this);
                else AKMusic.Get().RemoveState(musicTrack, this);
            }

            // Play or stop sfx
            if (play && GetComponent<AKTriggerPositive>())
                GetComponent<AKTriggerPositive>().TriggerPos();

            else if (GetComponent<AKTriggerNegative>())
                GetComponent<AKTriggerNegative>().TriggerNeg();
        }

        /*
        /// <summary>
        /// Remove one of the strikes from this interior. If it loses all, it implode!
        /// Doesn't do anything unless the interior is vulnerable
        /// <see cref="vulnerable"/><see cref="strikes"/>
        /// </summary>
        public bool Strike()
        {
            if (!vulnerable) return false;

            strikes--;

            if (strikes <= 0) Implode();

            return true;
        }

        [Button]
        public void Implode()
        {
            Debug.Log("I iz imploded.");
        }
        */
    }
}