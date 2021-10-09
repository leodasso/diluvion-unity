using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PathologicalGames;
using Diluvion.Sonar;
using Diluvion.Ships;

namespace Diluvion {


    public class Cutscene
    {
        public static GameObject cutscenePrefab;
        public static GameObject cutsceneInstance;

        /// <summary>
        /// If false, any player input will be ignored, including pause menu. (excluding skip buttons)
        /// </summary>
        public static bool playerControl = true;

        /// <summary>
        /// Instantiates the prefab cutscene object, calls for a fade in / out, 
        /// disables player control, turns off the UI, and sets the cutscene object's camera
        /// to be on and the other cameras to be off.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static GameObject ShowCutscene(GameObject prefab, float transitionTime, Color transitionColor)
        {
            Debug.Log("Bringing in cutscene: " + prefab.name);

            cutscenePrefab = prefab;

            // Call for a fade out. Have the actual functionality of the switch happen once things are fully faded out. (SpawnCutscenObject)
            FadeOverlay.FadeInThenOut(transitionTime, transitionColor, new FadeOverlay.OnTransition(SpawnCutsceneObject));

            return cutsceneInstance;
        }

        /// <summary>
        /// Begins a transition and ends any current cutscene.
        /// </summary>
        /// <param name="transitionTime"></param>
        /// <param name="transitionColor"></param>
        public static void EndCutscene(float transitionTime, Color transitionColor)
        {
            cutscenePrefab = null;
            FadeOverlay.FadeInThenOut(transitionTime, transitionColor, new FadeOverlay.OnTransition(ReturnToGameplay));
        }

        /// <summary>
        /// Destroys any current cutscene object, returns gameplay cams, UI, and player control.
        /// </summary>
        public static void ReturnToGameplay()
        {
            Debug.Log("Removing cutscene");

            if ( cutsceneInstance == null ) return;
            GameObject.Destroy(cutsceneInstance);

            SetUIVisible(true);
            SetGameplayCams(true);
            playerControl = true;
        }

        public static void SpawnCutsceneObject()
        {
            // destroy the old cutscene object
            if ( cutsceneInstance != null ) GameObject.Destroy(cutsceneInstance);

            // instantiate cutscene object
            cutsceneInstance = GameObject.Instantiate(cutscenePrefab) as GameObject;

            // turn off UI
            SetUIVisible(false);

            // Set gameplay cams
            SetGameplayCams(false);

            // turn off player control
            playerControl = false;
        }

        public static void SetUIVisible(bool active)
        {
            // TODO
        }

        public static void SetGameplayCams(bool active)
        {
            Debug.Log("Setting gameplay cameras enabled: " + active);

            OrbitCam oCam = OrbitCam.Get();
            if (oCam == null)
            {
                Debug.Log("Orbit cam can't be found!");
                return;
            }

            oCam.theCam.enabled = active;
            InteriorView.ActivateCam(active);
        }
    }

	public class ShipTools {


        //Gets the Angle compared to North +180/-180
        public static float GetWorldDegreeDirection(Vector3 start, Vector3 target)
        {
            Vector3 targetDirection = target-start;
            float returnFloat = Vector3.Angle(Vector3.forward, targetDirection);
            if(targetDirection.x<0)
                returnFloat*=-1;
            return returnFloat;
        }


		public static List<SonarStats> OrderFarthest(List<SonarStats> targetList, Transform caller) {
			
			//create a new game object list
			List<SonarStats> orderedList = new List<SonarStats>();
			
			//order by distance
			orderedList = targetList.OrderBy( go => Vector3.SqrMagnitude(caller.transform.position - go.transform.position) ).ToList();
			
			//reverse it so the largest distance is first
			orderedList.Reverse();
			return orderedList;
		}


		public static List<RaycastHit> OrderNearest(RaycastHit[] targetList, Transform caller) {
			
			//create a new game object list
			List<RaycastHit> orderedList = new List<RaycastHit>();
			
			//order by distance
			orderedList = targetList.OrderBy( go => Vector3.SqrMagnitude(caller.transform.position - go.point) ).ToList();
			
			return orderedList;
		}

        public static List<SonarStats> OrderNearest(List<SonarStats> targetList, Transform caller)
        {

            //create a new game object list
            List<SonarStats> safeList = new List<SonarStats>();
            List<SonarStats> orderedList = new List<SonarStats>();
            foreach (SonarStats ss in targetList)
                if (ss != null&& ss.gameObject.activeInHierarchy)               
                    safeList.Add(ss);

            //order by distance
            orderedList = safeList.OrderBy(go => Vector3.SqrMagnitude( go.transform.position- caller.transform.position )).ToList();

            return orderedList;
        }



		public static List<GameObject> OrderFarthest(List<GameObject> targetList, GameObject caller) {

			//create a new game object list
			List<GameObject> orderedList = new List<GameObject>();

			//order by distance
			orderedList = targetList.OrderBy( go => Vector3.SqrMagnitude(caller.transform.position - go.transform.position) ).ToList();

			//reverse it so the largest distance is first
			orderedList.Reverse();
			return orderedList;
		}

		/// Creates a list ordered by nearest object first, farthest last.
		public static List<GameObject> OrderNearest(List<GameObject> targetList, GameObject caller) {
			
			//create a new game object list
			List<GameObject> orderedList = new List<GameObject>();
			
			//order by distance
			orderedList = targetList.OrderBy( go => Vector3.SqrMagnitude(caller.transform.position - go.transform.position) ).ToList();

			return orderedList;
		}

		public static List<GameObject> HighestHealth(List<GameObject> targetList) {
			
			//create a new game object list
			List<GameObject> orderedList = new List<GameObject>();
			
			//order by health percent
			orderedList = targetList.OrderBy( go => HealthPercent(go) ).ToList();

			orderedList.Reverse();

			return orderedList;
		}

		///Returns the interior of a ship / city. 
		public static InteriorManager GetInterior(GameObject obj) {

			SideViewerStats svs = obj.GetComponent<SideViewerStats>();

			if (obj == null) return null;
			if (svs == null) return null;

			if (svs.intMan == null) return null;

			InteriorManager objIntMan = svs.intMan.GetComponent<InteriorManager>();

			return objIntMan;
		}


		//Returns the set of interior paths that this waypoint belongs to
		public static PatrolPath GetPatrolPathWay(WaypointNode waypoint) {

			int limit = 0;
			GameObject objectToCheck = waypoint.gameObject;

			//check up to 20 objects up in the hierarchy
			while (limit < 20) {

                //check for InteriorPath component on parent
                PatrolPath paths = objectToCheck.transform.parent.GetComponent<PatrolPath>();
				if (paths != null) {
					return paths;
				}

				limit++;
				if (objectToCheck.transform.parent != null) {
					objectToCheck = objectToCheck.transform.parent.gameObject;
				}else {
					return null;
				}
			}
			return null;
		}

		public static List<GameObject> HighestValue(List<GameObject> objects) {

			if (objects.Count < 1) {return null;}

			List<GameObject> orderedList = new List<GameObject>();

			orderedList = objects.OrderBy( go => GetShipValue(go) ).ToList();
			orderedList.Reverse();

			return orderedList;
		}

		public static List<GameObject> LowestValue(List<GameObject> objects) {
			
			if (objects.Count < 1) {return null;}
			
			List<GameObject> orderedList = new List<GameObject>();
			
			orderedList = objects.OrderBy( go => GetShipValue(go) ).ToList();
			
			return orderedList;
		}

		public static List<GameObject> HighestDanger(List<GameObject> objects) {
			
			if (objects.Count < 1) {return null;}
			
			List<GameObject> orderedList = new List<GameObject>();
			
			orderedList = objects.OrderBy( go => DangerAmount(go) ).ToList();
			orderedList.Reverse();
			
			return orderedList;
		}
		
		public static List<GameObject> LowestDanger(List<GameObject> objects) {
			
			if (objects.Count < 1) {return null;}
			
			List<GameObject> orderedList = new List<GameObject>();
			
			orderedList = objects.OrderBy( go =>  DangerAmount(go)  ).ToList();
			
			return orderedList;
		}		


		public static float HealthPercent(GameObject go) {
			
			float hp = 0;
			float maxHP = 0;
			
			Hull hull = go.GetComponent<Hull>();
			
			if (hull != null) {
				hp = hull.CurrentHP();
				maxHP = hull.maxHealth;
			}
			
			//convert to a percentage, then return
			return hp/maxHP * 100;
		}

		public static int GetShipValue(GameObject ship) {

			int inventoryValue = 0;
			int crewValue = 0;

			Inventory inventory = ship.GetComponent<Inventory>();
			Bridge shipBridge = ship.GetComponent<Bridge>();
			CrewManager crewM = null;

			if (inventory != null) {
				//inventoryValue = inventory.inventoryData.Value();
			}

			//get the crewManager of that ship
			if (shipBridge != null) {
				crewM = shipBridge.crewManager;
			}

			if (crewM != null)
            {
                crewValue = 5 * crewM.AllCharactersOnBoard().Count;
			}

			return inventoryValue + crewValue;
		}

		public static float DangerAmount (GameObject go) {

			float danger = 0;

			if (go.GetComponent<Rigidbody>() != null) {
				danger += go.GetComponent<Rigidbody>().mass/10;
			}

			//proxim mines are dangerous!  :(
			if (go.GetComponent<ProximityMine>() != null) {
				danger += 60;
			}

			return danger;
		}


		public static GameObject HighestValueObject (List<GameObject> goList) {

			//create a list ordered by the objects values
			List<GameObject> tempList = HighestValue(goList);

			if (tempList != null) {
				return tempList[0];
			}
			return null;
		}
			

		public static Bridge GetBridge(GameObject thisObject) {

			if (thisObject.GetComponent<Bridge>() ) {
				return thisObject.GetComponent<Bridge>();
			}


			//object to check for bridge is this parent
			Transform objToCheck = thisObject.transform;
			
			for (int i = 0; i < 30; i++) {
				if (objToCheck.GetComponent<Bridge>()) {
					return objToCheck.GetComponent<Bridge>();
				}else {
					if (objToCheck.parent) {
						objToCheck = objToCheck.parent;
					}
				}
			}

			return null;
		}
	}

    /// <summary>
    /// Population Tools and other world related garbage
    /// </summary>
	public class WorldTools
    {

        ///Do not reference this directly.
        public static SpawnPool worldPool;

        ///Use to get reference to world pool
        public static SpawnPool GetWorldPool()
        {

            if (worldPool != null) return worldPool;

            SpawnPool newWorldPool = GameObject.FindObjectOfType<SpawnPool>();
            if (newWorldPool != null)
            {

                worldPool = newWorldPool;
                return worldPool;
            }
            return null;
        }

        public static Vector3 ExplorableBox(SpawnableSize size)
        {
            //get the integer value of the size
            int sizeIndex = (int)size;

            // Get the correct size values for that size selection
            Vector3 newSize = new Vector3(5, 5, 10) * sizeIndex;
            return newSize;
        }





        public class CamTools
        {

            public static Camera interiorCam;


            public static Camera GetInteriorCam()
            {

                if (interiorCam != null)
                {
                    return interiorCam;
                }

                GameObject interiorViewer = GameObject.Find("interior viewer");

                if (interiorViewer != null)
                {
                    Camera returnCam = interiorViewer.GetComponentInChildren<Camera>();
                    interiorCam = returnCam;
                    return returnCam;
                }
                return null;
            }

            public static bool IsCamViewingBridge(Bridge theBridge)
            {

                CameraStats camStats = theBridge.GetComponent<CameraStats>();

                if (camStats == null)
                {
                    return false;
                }


                if (OrbitCam.CurrentTarget() == camStats.transform)
                {
                    return true;
                }
                return false;
            }


            /// <summary>
            /// returns a position to be used on UI with the interior cam that will follow
            /// the given world position
            /// </summary>
            public static Vector3 UIPosition(Vector3 worldPos, bool screenLock)
            {

                Camera interiorCam = GUITools.UICam();

                Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPos);

                if (screenLock)
                {
                    viewportPos = new Vector3(Mathf.Clamp01(viewportPos.x), Mathf.Clamp01(viewportPos.y), viewportPos.z);
                }

                //if the thing is behind camera
                if (viewportPos.z < 0)
                {
                    viewportPos = new Vector3(10, 10, 5);
                }

                Vector3 newWorldPos = interiorCam.ViewportToWorldPoint(viewportPos);
                newWorldPos = new Vector3(newWorldPos.x, newWorldPos.y, 5);

                return newWorldPos;
            }


            public static Vector3 UIFrom2DPosition(Vector3 worldPos, bool screenLock)
            {

                Camera camUI = GUITools.UICam();
                Camera cam2D = CamTools.GetInteriorCam();

                //get the target's position in viewport coordinates
                Vector3 viewportPos = cam2D.WorldToViewportPoint(worldPos);

                //keep the target's center on screen
                if (screenLock)
                {
                    viewportPos = new Vector3(Mathf.Clamp01(viewportPos.x), Mathf.Clamp01(viewportPos.y), viewportPos.z);
                }

                //if the thing is behind camera
                if (viewportPos.z < 0)
                {
                    viewportPos = new Vector3(10, 10, 5);
                }

                Vector3 newWorldPos = camUI.ViewportToWorldPoint(viewportPos);
                newWorldPos = new Vector3(newWorldPos.x, newWorldPos.y, 5);

                return newWorldPos;
            }


            /// <summary>
            /// returns a position to be used on UI with the interior cam that will follow
            /// the given transform.
            /// </summary>
            public static Vector3 UIPosition(Transform transToFollow, bool screenLock)
            {
                return UIPosition(transToFollow.position, screenLock);
            }
        }

        public class UIColors
        {

            public static Color FocusColor = new Color(1, .2f, .4f, 1);
            public static Color ActiveColor = Color.white;
            public static Color InactiveColor = new Color(1, 1, 1, .4f);
            public static Color InvisColor = new Color(1, 1, 1, 0);
            public static Color DarkColor = new Color(.1f, .1f, .1f, 1);
            public static Color GoColor = new Color(.4f, 1, .2f, 1);
        }

        public class DialogueTools
        {

            public static string ToSentence(string inputString)
            {
                return new string(inputString.ToCharArray().SelectMany(c => char.IsUpper(c) ? new char[] { ' ', c } : new char[] { c }).ToArray());
            }
        }

        public class GUITools
        {

            public static Camera uiCam;

            public static void SetSpriteColor(GameObject target, Color newColor)
            {

                SpriteRenderer[] allSprites = target.GetComponentsInChildren<SpriteRenderer>();

                foreach (SpriteRenderer sr in allSprites)
                {
                    sr.color = newColor;
                }

                if (target.GetComponent<SpriteRenderer>())
                {
                    target.GetComponent<SpriteRenderer>().color = newColor;
                }
            }

            //OLD GUI
            //		public static void SetTextColor (GameObject target, Color newColor) {
            //			SpriteText[] allText = target.GetComponentsInChildren<SpriteText>();
            //
            //			foreach (SpriteText st in allText) {
            //				st.Color1 = newColor;
            //				st.Color2 = newColor;
            //			}
            //
            //			SpriteText targetSpriteText = target.GetComponent<SpriteText>();
            //			if (targetSpriteText) {
            //				targetSpriteText.Color1 = newColor;
            //				targetSpriteText.Color2 = newColor;
            //			}
            //		}
            //
            //		public static void SetTextAlpha (GameObject target, float alpha) {
            //			SpriteText[] allText = target.GetComponentsInChildren<SpriteText>();
            //
            //			float transparency = 1 - alpha;
            //			
            //			foreach (SpriteText st in allText) {
            //				st.Transparency = transparency;
            //			}
            //			
            //			SpriteText targetSpriteText = target.GetComponent<SpriteText>();
            //			if (targetSpriteText) {
            //				targetSpriteText.Transparency = transparency;
            //			}
            //		}

            public static Vector3 ScreenCenter()
            {

                Vector3 v = UICam().ViewportToWorldPoint(new Vector3(.5f, .5f, 1));
                return v;
            }

            public static Camera UICam()
            {

                //if (uiCam != null) {
                //return uiCam;
                //}

                //Camera theCam = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();

                //if (theCam == null) {
                //theCam = GameObject.Find("UI cam").GetComponent<Camera>();
                //}

                //if (theCam == null) {
                //Debug.LogWarning("No UI camera found.");
                return null;
                //}

                //uiCam = theCam;
                //return theCam;
            }
        }
    }
}