using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Diluvion;
using HeavyDutyInspector;
using I2.Loc;
using Rewired;



namespace SpiderWeb
{
    /// <summary>
    /// Extra Game Object tools
    /// </summary>
    public class GO
    {
        /// <summary>
        /// Returns the component if already attached to this object, otherwise adds the component and returns.
        /// </summary>
        public static T MakeComponent<T> (GameObject GO) where T : Component
        {
            if (GO.GetComponent<T>() == null)
                GO.gameObject.AddComponent<T>();

            return GO.GetComponent<T>();
        }

        /// <summary>
        /// Returns a component or interface on the given game object's self or parent.
        /// </summary>
        /// <typeparam name="T">Type of component to find.</typeparam>
        /// <param name="go">Game object to check</param>
        public static T ComponentInParentOrSelf<T> (GameObject go)
        {
            if (!go) return default(T);

            T component = go.GetComponent<T>();
            if (component == null) component = go.GetComponentInParent<T>();
            return component;
        }

        /// <summary>
        /// Destroys all the children of the given parent
        /// </summary>
        public static void DestroyChildren(Transform parent)
        {
            if (parent.childCount < 1) return;

            List<GameObject> children = new List<GameObject>();
            foreach (Transform t in parent)
                children.Add(t.gameObject);
            
            #if UNITY_EDITOR
children.ForEach(child => Object.DestroyImmediate(child));
#else
            children.ForEach(child => Object.Destroy(child));
            #endif
        }
    }

    public class String
    {
        public static string Format (string s, List<string> strings)
        {
            for (int i = 0; i < strings.Count; i++)
            {
                string search = "{" + i + "}";
                s = s.Replace(search, strings [i]);
            }

            return s;
        }
    }

    public class Localization
    {

        public static string key_dialog = "dialog/";
        public static string key_Question = "_question(player)";
        public static string key_Chatter = "_chatter";
        public static string key_controls = "GUI/control_";


        /// <summary>
        /// Adds the given key and text to the I2 localization library.
        /// </summary>
        /// <returns>The termData added to the library.</returns>
        /// <param name="newLocKey">The string localization key used to identify this term. i.e. lm_lighthouse</param>
        /// <param name="keyText">The string nice text that the key is for. i.e. Lighthouse</param>
        public static TermData AddToKeyLib (string newLocKey, string keyText)
        {

            if (string.IsNullOrEmpty(newLocKey))
            {
                Debug.LogError("Cant add to library with an empty key!!");
                return null;
            }

            // Grab the language source from resource
            LanguageSource source = Resources.Load<LanguageSource>("I2Languages");

            // Set the correct language index 
            int langIndex = source.GetLanguageIndex("English (United States)");

            // Create a new term data labeled by the given loc key
            TermData termData = source.AddTerm(newLocKey, eTermType.Text);

            // Apply the text to the english index of the term
            termData.Languages [langIndex] = keyText;

            return termData;
        }

        /// <summary>
        /// Returns the key without any invalid characters 
        /// </summary>
        public static string CleanKey (string oldKey)
        {
            if (string.IsNullOrEmpty(oldKey)) return "";
            string returnKey = oldKey;
            returnKey = returnKey.Replace("<", "-");
            returnKey = returnKey.Replace(">", "-");
            returnKey = returnKey.Replace("/", "-");
            return returnKey;
        }

        public static void RemoveTermFromLib (string termKey)
        {
            if (string.IsNullOrEmpty(termKey))
            {
                Debug.LogError("Cant remove from library with an empty key!!");
                return;
            }

            // Grab the language source from resource
            LanguageSource source = Resources.Load<LanguageSource>("I2Languages");

            source.RemoveTerm(termKey);
        }

        public static string GetFromLocLibrary (string locKey, string originalName, bool showWarning = true)
        {
            string newNiceName = ScriptLocalization.Get(locKey);
            
            if (!string.IsNullOrEmpty(newNiceName)) return newNiceName;

#if !UNITY_EDITOR
			Debug.Log("Localized text for " + locKey + " was not found in the library for the current language.");
#else
            if (showWarning)
                Debug.LogWarning("Localized text for " + locKey + " returned invalid result: " + newNiceName + " for current language(" + I2.Loc.LocalizationManager.CurrentLanguage + ")");
#endif

            return originalName;
        }
        
        
        
        

        public static string LocalizedElementName (ActionElementMap aem)
        {
            return GetFromLocLibrary(key_controls + aem.elementIdentifierName, aem.elementIdentifierName);
        }

        public static string LocalizedCEI (ControllerElementIdentifier cei)
        {
            return GetFromLocLibrary(key_controls + "cei_" + cei.name, cei.name);
        }
    }

    public class VectorPid
    {
        public float pFactor, iFactor, dFactor;

        private Vector3 integral;
        private Vector3 lastError;

        public VectorPid (float pFactor, float iFactor, float dFactor)
        {
            this.pFactor = pFactor;
            this.iFactor = iFactor;
            this.dFactor = dFactor;
        }

        public VectorPid (Vector3 pid)
        {
            this.pFactor = pid.x;
            this.iFactor = pid.y;
            this.dFactor = pid.z;
        }

        public Vector3 Update (Vector3 currentError, float timeFrame)
        {

            integral += currentError * timeFrame;
            Vector3 deriv = (currentError - lastError) / timeFrame;
            lastError = currentError;
            return currentError * pFactor
                + integral * iFactor
                    + deriv * dFactor;
        }

        public Vector3 Update (Vector3 currentError, float timeFrame, float pFactor1, float iFactor1, float dFactor1)
        {

            integral += currentError * timeFrame;
            Vector3 deriv = (currentError - lastError) / timeFrame;
            lastError = currentError;
            return currentError * pFactor1
                + integral * iFactor1
                    + deriv * dFactor1;


        }
    }

    public class Chance
    {
        public static bool FlipCoin ()
        {
            float dice = UnityEngine.Random.Range(0f, 1f);
            if (dice <= 0.5f) return true;
            else return false;
        }

        public static bool Roll (float odds)
        {
            float oddsPercentage = odds * 100;
            //Debug.Log("Rolling with a " + oddsPercentage + "% chance of success.");
            float dice = UnityEngine.Random.Range(0f, 1f);

            if (dice <= odds)
            {
                //Debug.Log("Roll successful!");
                return true;
            }

            //Debug.Log("Roll failed. :(");
            return false;
        }
        /// <summary>
        /// Constructs a rectangle along X and Z axis'
        /// </summary>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static Vector3 RandomPointOnRectangle (float X, float Y, float Z)
        {
            return new Vector3(Random.Range(-X, X), Random.Range(-Y, Y), Random.Range(-Z, Z));
        }

        /*public static Vector3 RandomPointOnRectangle(float minX, float maxX, float minZ,float maxZ)
        {
            return new Vector3(Random.Range(minX, maxX),0, Random.Range(minZ, maxZ));
        }*/

    }

    public enum TangentMode
    {
        Editable = 0,
        Smooth = 1,
        Linear = 2,
        Stepped = Linear | Smooth,
    }

    public enum TangentDirection
    {
        Left,
        Right
    }

    public class KeyframeUtil
    {

        public static Keyframe GetNew (float time, float value, TangentMode leftAndRight)
        {
            return GetNew(time, value, leftAndRight, leftAndRight);
        }

        public static Keyframe GetNew (float time, float value, TangentMode left, TangentMode right)
        {
            Keyframe boxed = new Keyframe(time, value); // cant use struct in reflection			

            SetKeyBroken(boxed, true);
            SetKeyTangentMode(boxed, 0, left);
            SetKeyTangentMode(boxed, 1, right);

            Keyframe keyframe = boxed;
            if (left == TangentMode.Stepped)
                keyframe.inTangent = float.PositiveInfinity;
            if (right == TangentMode.Stepped)
                keyframe.outTangent = float.PositiveInfinity;

            return keyframe;
        }


        // UnityEditor.CurveUtility.cs (c) Unity Technologies
        public static void SetKeyTangentMode (Keyframe keyframe, int leftRight, TangentMode mode)
        {


            int tangentMode = keyframe.tangentMode;

            if (leftRight == 0)
            {
                tangentMode &= -7;
                tangentMode |= (int)mode << 1;
            }
            else
            {
                tangentMode &= -25;
                tangentMode |= (int)mode << 3;
            }
            keyframe.inTangent = tangentMode;
            keyframe.outTangent = tangentMode;
            if (GetKeyTangentMode(tangentMode, leftRight) == mode)
                return;
            Debug.Log("bug"); //Great job Unity, good to know
        }

        // UnityEditor.CurveUtility.cs (c) Unity Technologies
        public static TangentMode GetKeyTangentMode (int tangentMode, int leftRight)
        {
            if (leftRight == 0)
                return (TangentMode)((tangentMode & 6) >> 1);
            else
                return (TangentMode)((tangentMode & 24) >> 3);
        }

        // UnityEditor.CurveUtility.cs (c) Unity Technologies
        public static TangentMode GetKeyTangentMode (Keyframe keyframe, int leftRight)
        {
            int tangentMode = keyframe.tangentMode;
            if (leftRight == 0)
                return (TangentMode)((tangentMode & 6) >> 1);
            else
                return (TangentMode)((tangentMode & 24) >> 3);
        }


        // UnityEditor.CurveUtility.cs (c) Unity Technologies
        public static void SetKeyBroken (Keyframe keyframe, bool broken)
        {
            int tangentMode = keyframe.tangentMode;

            if (broken)
                tangentMode |= 1;
            else
                tangentMode &= -2;
            keyframe.inTangent = tangentMode;
            keyframe.outTangent = tangentMode;
        }
    }

    public static class KeywordTools
    {
        /// <summary>
        /// Returns true if the list contains the given tag.
        /// </summary>
        public static bool HaveTag (Keyword tag, List<Keyword> keywordList)
        {
            foreach (Keyword k in keywordList)
            {
                // Debug.Log(k.fullPath + " == " + tag.fullPath);
                if (k.fullPath == tag.fullPath) return true;
            }

            return false;
        }


        /// <summary>
        /// Returns true if any of the tags in keywords1 matches any of the tags in keywords 2
        /// </summary>
		public static bool ShareTag (List<Keyword> keywords1, List<Keyword> keywords2)
        {
            foreach (Keyword k in keywords1)
            {
                if (HaveTag(k, keywords2)) return true;
            }
            return false;
        }
    }

    public static class CurveExtension
    {

        public static void UpdateAllLinearTangents (this AnimationCurve curve)
        {
            for (int i = 0; i < curve.keys.Length; i++)
            {
                UpdateTangentsFromMode(curve, i);
            }
        }

        // UnityEditor.CurveUtility.cs (c) Unity Technologies
        public static void UpdateTangentsFromMode (AnimationCurve curve, int index)
        {
            if (index < 0 || index >= curve.length)
                return;
            Keyframe key = curve[index];
            if (KeyframeUtil.GetKeyTangentMode(key, 0) == TangentMode.Linear && index >= 1)
            {
                key.inTangent = CalculateLinearTangent(curve, index, index - 1);
                curve.MoveKey(index, key);
            }
            if (KeyframeUtil.GetKeyTangentMode(key, 1) == TangentMode.Linear && index + 1 < curve.length)
            {
                key.outTangent = CalculateLinearTangent(curve, index, index + 1);
                curve.MoveKey(index, key);
            }
            if (KeyframeUtil.GetKeyTangentMode(key, 0) != TangentMode.Smooth && KeyframeUtil.GetKeyTangentMode(key, 1) != TangentMode.Smooth)
                return;
            curve.SmoothTangents(index, 0.0f);
        }

        // UnityEditor.CurveUtility.cs (c) Unity Technologies
        private static float CalculateLinearTangent (AnimationCurve curve, int index, int toIndex)
        {
            return (float)(((double)curve [index].value - (double)curve [toIndex].value) / ((double)curve [index].time - (double)curve [toIndex].time));
        }

    }

    public class DebugTools
    {

        public static void DrawLinePositions (List<Vector3> list, Color color, float duration)
        {

            Vector3 startPos;
            Vector3 nextPos;
            for (int i = 0; i < list.Count; i++)
            {
                startPos = list [i];
                if (i + 1 < list.Count)
                    nextPos = list [i + 1];
                else
                    nextPos = startPos;
                Debug.DrawLine(startPos, nextPos, color, duration);
            }
        }
    }

    public class Controls
    {

        static IEnumerable<ActionElementMap> actions;

        public static ControllerType LastUsedControllerType ()
        {
            // Get player
            Player player = ReInput.players.GetPlayer(0);
            if (player == null) return ControllerType.Mouse;

            // Get the controller
            Controller controller = player.controllers.GetLastActiveController();
            if (controller == null) return ControllerType.Mouse;

            return controller.type;
        }

        public static bool PlayerUsingGamepad()
        {
            return LastUsedControllerType() == ControllerType.Joystick;
        }

        /// <summary>
        /// Returns all action element maps (basically all the inputs that can be used) for the current action. Pairs mouse and keyboard
        /// and treats them as a single controller
        /// </summary>
        public static IEnumerable<ActionElementMap> AllActionElementMaps(string actionName)
        {
            Player player = GameManager.Player();
            Controller controller = player.controllers.GetLastActiveController();
            
            if (controller == null)
            {
                // if no input was used, check the keyboard / mouse
                actions = player.controllers.maps.ElementMapsWithAction(actionName, false);
            }
            else 
            {
                // If last used controller was mouse or keyboard, return the actions as if they're a combo
                if (controller.type == ControllerType.Keyboard || controller.type == ControllerType.Mouse)
                {
                    // get all the action elements from keyboard
                    var keyboardActions = player.controllers.maps.ElementMapsWithAction(ControllerType.Keyboard, actionName, false);
                    // get all action elements from mouse
                    var mouseActions = player.controllers.maps.ElementMapsWithAction(ControllerType.Mouse, actionName, false);

                    // add them together brother
                    actions = keyboardActions.Concat(mouseActions);
                }
                
                // If they're not using the mouse/keyboard combo, just return whatever controller they're using
                else actions = player.controllers.maps.ElementMapsWithAction(controller, actionName, false);
            }

            return actions;
        }

        /// <summary>
        /// Returns the name of the key(s) mapped to the given action name. For example if you put in the action name 'fire' it will return 'left click' or 
        /// 'right trigger.' Automatically checks which input was last used.
        /// </summary>
        public static string InputMappingName (string actionName)
        {
            actions = AllActionElementMaps(actionName);
            
            string posInput = null;
            string negInput = null;

            // Cycle through each action element map related to the actionName on the given controller
            foreach (ActionElementMap aem in actions)
            {
                if (aem.elementType == ControllerElementType.Axis)
                {
                    if (string.IsNullOrEmpty(posInput)) posInput = Localization.LocalizedElementName(aem);
                }
                else
                {
                    if (aem.axisContribution == Pole.Positive)
                        if (string.IsNullOrEmpty(posInput)) posInput = Localization.LocalizedElementName(aem);

                    if (aem.axisContribution == Pole.Negative)
                        if (string.IsNullOrEmpty(negInput)) negInput = Localization.LocalizedElementName(aem);
                }
            }

            string finalString = null;

            if (string.IsNullOrEmpty(negInput))
            {
                finalString = posInput;
            }
            else
            {
                finalString = negInput + ", " + posInput;
            }

            return finalString;
        }
    }

    public static class Calc
    {
        public static Vector3 MultiLerp(Vector3[] waypoints, float ratio)
        {
            Vector3 position = Vector3.zero;
            float totalDistance = waypoints.MultiDistance();
            float distanceTravelled = totalDistance * ratio;
 
            int indexLow = GetVectorIndexFromDistanceTravelled(waypoints, distanceTravelled);
            int indexHigh = indexLow + 1;
 
            // we're done
            if (indexHigh > waypoints.Length - 1)
                return waypoints[waypoints.Length - 1];
 
             
            // calculate the distance along this waypoint to the next
            Vector3[] completedWaypoints = new Vector3[indexLow + 1];
 
            for (int i = 0; i < indexLow + 1; i++)
            {
                completedWaypoints[i] = waypoints[i];
            }
 
            float distanceCoveredByPreviousWaypoints = completedWaypoints.MultiDistance();
            float distanceTravelledThisSegment = 
                distanceTravelled - distanceCoveredByPreviousWaypoints;
            float distanceThisSegment = Vector3.Distance(waypoints[indexLow], waypoints[indexHigh]);
 
            float currentRatio = distanceTravelledThisSegment / distanceThisSegment;
            position = Vector3.Lerp(waypoints[indexLow], waypoints[indexHigh], currentRatio);
 
            return position;
        }
        
        public static float MultiDistance(this Vector3[] waypoints)
        {
            float distance = 0f;
 
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (i + 1 > waypoints.Length - 1)
                    break;
 
                distance += Vector3.Distance(waypoints[i], waypoints[i + 1]);
            }
 
            return distance;
        }
 
        public static int GetVectorIndexFromDistanceTravelled(
            Vector3[] waypoints, float distanceTravelled)
        {
            float distance = 0f;
 
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (i + 1 > waypoints.Length - 1)
                    return waypoints.Length - 1;
 
                float segmentDistance = Vector3.Distance(waypoints[i], waypoints[i + 1]);
 
                if (segmentDistance + distance > distanceTravelled)
                {
                    return i;
                }
 
                distance += segmentDistance;
            }
 
            return waypoints.Length - 1;
        }

        public static Bounds GetColliderBounds(GameObject parent)
        {

            Bounds b = new Bounds(parent.transform.position, Vector3.one);
            foreach (Collider col in parent.GetComponentsInChildren<Collider>())
            {
                if (col.isTrigger) continue;
                b.Encapsulate(col.bounds);
            }

            return b;
        }

        public static List<T> ShuffleList<T> (List<T> inputList)
        {
            for (int i = 0; i < inputList.Count; i++)
            {
                T temp = inputList[i];
                int randomIndex = Random.Range(i, inputList.Count);
                inputList [i] = inputList [randomIndex];
                inputList [randomIndex] = temp;
            }

            return inputList;
        }

        /// <summary>
        /// Returns a value between 0 and maxOutput, where if value = halfpoint, it will return exactly half of maxOutput.
        /// </summary>
        public static float LogBase (float value, float halfPoint, float maxOutput)
        {
            return (1 - halfPoint / (halfPoint + value)) * maxOutput;
        }

        /// <summary>
        /// Snaps the given vector to increments of the snap angle.
        /// </summary>
        public static Vector3 SnapTo (Vector3 v3, float snapAngle)
        {
            float   angle = Vector3.Angle (v3, Vector3.up);

            if (angle < snapAngle / 2.0f)          // Cannot do cross product 
                return Vector3.up * v3.magnitude;  //   with angles 0 & 180

            if (angle > 180.0f - snapAngle / 2.0f)
                return Vector3.down * v3.magnitude;

            float t = Mathf.Round(angle / snapAngle);
            float deltaAngle = (t * snapAngle) - angle;

            Vector3 axis = Vector3.Cross(Vector3.up, v3);
            Quaternion q = Quaternion.AngleAxis (deltaAngle, axis);
            return q * v3;
        }

        /// <summary>
        /// Returns a layermask that includes all things munition should interact with.
        /// </summary>
        public static LayerMask GunsafeLayer ()
        {
            //List of layers we raycast against with our munition
            LayerMask returnLayer = IncludeLayer(new string[] { "Default", "BlockSonar", "IgnoreSonar", "Torpedo", "Terrain", "IKCreature" });
            return returnLayer;
        }


        public static LayerMask GunsafeLayerAndFriends (GameObject go)
        {

            string myLayer = LayerMask.LayerToName(go.layer);
            LayerMask returnLayer = LayerMask.GetMask("Munition", myLayer);
            // Debug.Log("MunitionLayers" + munitionLayer.value.ToString());
            return ~returnLayer;

        }

        public static LayerMask IncludeLayer (string layer)
        {
            LayerMask returnLayer = LayerMask.GetMask(layer);
            return returnLayer;
        }

        public static LayerMask IncludeLayer (string [] layers)
        {
            LayerMask returnLayer = LayerMask.GetMask(layers);
            return returnLayer;
        }

        public static LayerMask ExcludeLayer (string layer)
        {
            LayerMask returnLayer = LayerMask.GetMask(layer);
            return ~returnLayer;
        }

        public static LayerMask ExcludeLayer (string [] layers)
        {
            LayerMask returnLayer = LayerMask.GetMask(layers);

            return ~returnLayer;
        }

        /// <summary>
        /// Checks to see if the two input spheres intersect
        /// </summary>
        /// <param name="threshHold">How much they must intersect by</param>
        /// <param name="sp1pos">Sphere1 Position</param>
        /// <param name="sp1rad">Sphere1 Radius</param>
        /// <param name="sp2pos">Sphere2 Position</param>
        /// <param name="sp2rad">Sphere2 Radius</param>
        /// <returns>True if the twho spheres intersect at some point within the threshold</returns>
        public static bool SpheresIntersect (float threshHold = 0.1f, Vector3 sp1pos = new Vector3(), float sp1rad = 1, Vector3 sp2pos = new Vector3(), float sp2rad = 1)
        {

            float distance = Vector3.Distance(sp2pos, sp1pos);
            bool intersect = false;
            float diff = sp2rad + sp1rad - distance;
            // Debug.Log("SP1 (" + sp1pos + " r: " +sp1rad + " ) SP2 (" + sp2pos + " r: " + sp2rad + ") " + " diff= " + diff + "/"+ distance);

            if (diff < threshHold)
                intersect = false;
            else
                intersect = true;


            Color sphereColor = Color.green;
            if (intersect == false)
                sphereColor = Color.red;

            Debug.DrawLine(sp1pos, sp2pos, sphereColor, 25);
            //DrawSphereCross(sphereColor, sp1pos, sp1rad);
            //DrawSphereCross(sphereColor, sp2pos, sp2rad);

            return intersect;
        }

        static void DrawSphereCross (Color col, Vector3 position, float radius)
        {

            Debug.DrawRay(position, Vector3.up * radius, col, 15);
            Debug.DrawRay(position, -Vector3.up * radius, col, 15);
            Debug.DrawRay(position, Vector3.right * radius, col, 15);
            Debug.DrawRay(position, -Vector3.right * radius, col, 15);
            Debug.DrawRay(position, Vector3.forward * radius, col, 15);
            Debug.DrawRay(position, -Vector3.forward * radius, col, 15);
        }

        public static bool VectorNaN (Vector3 v)
        {
            if (float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z))
                return true;
            return false;
        }

        public static bool WithinDistance (float distance, Vector3 v1, Vector3 v2)
        {
            //if (v1 == null) return false;
            //if (v2 == null) return false;
            float sqr = (v1 - v2).sqrMagnitude;
            float dist = distance * distance;
            return !(dist < sqr);
        }

        public static bool WithinDistance (float distance, Transform t1, Transform t2)
        {
            if (t1 == null) return false;
            if (t2 == null) return false;
            float sqr = (t1.position - t2.position).sqrMagnitude;
            float dist = distance * distance;
            return !(dist < sqr);
        }

        public static bool WithinDistance (float distance, Transform t1, Vector3 t2)
        {
            if (t1 == null) return false;

            float sqr = (t1.position - t2).sqrMagnitude;
            float dist = distance * distance;
            return !(dist < sqr);
        }

        public static bool WithinDistanceSqr (float sqrDist, Transform v1, Transform v2)
        {
            if (v1 == null) return false;
            if (v2 == null) return false;
            float sqr = (v1.position - v2.position).sqrMagnitude;
            float dist = sqrDist;
            return !(dist < sqr);
        }


        public static bool OutsideDistance (float distance, Transform t1, Vector3 t2)
        {
            float sqr = (t1.position - t2).sqrMagnitude;
            float dist = distance * distance;
            return !(dist > sqr);
        }



        public static Vector3 ShakePos (float intensity, Vector3 originPos)
        {
            Vector3 outPos = originPos + Random.insideUnitSphere * intensity;

            return outPos;
        }

        public static int PosOrNeg (float input)
        {
            if (input > 0)return 1;
            return -1;
        }

        public static float NiceRounding (float number, int digits = 2)
        {
            float multiplier = Mathf.Pow(10, digits);
            float newNum = Mathf.Round(number * multiplier);
            return newNum / multiplier;
        }


        //IF WE DONT HIT ANYTHING OF MASK mask, return true
        public static bool IsLOS (Vector3 from, Vector3 to, LayerMask mask, float error = 0)
        {
            float range = (from - to).magnitude;

            RaycastHit hit;

            if (!Physics.Raycast(from, to - from, out hit, range * 1.02f, mask.value))
            {
                return true;
            }

            // If the raycast hit something before getting to target, check how far from target
            if (WithinDistance(error, hit.point, to)) return true;

            return false;
        }

        //IF WE DONT HIT ANYTHING OF MASK mask, return true
        public static bool IsLOS (Vector3 from, Vector3 to, LayerMask mask, Color col)
        {
            float range = (from - to).magnitude;

            //If we dont hit anything on our way to the point from point
            if (!Physics.Raycast(from, to - from, range * 1.02f, mask))
            {
                Debug.DrawRay(from, (to - from).normalized * range * 1.01f, Color.green, 1);
                return true;
            }
            Debug.DrawRay(from, (to - from).normalized * range * 1.01f, col, 1);
            return false;
        }

    
        public static bool IsLOS (Transform caller, Transform target, LayerMask mask)
        {
            float range = (target.position - caller.position).magnitude;
            RaycastHit hit;

            if (Physics.Raycast(caller.position, (target.position - caller.position), out hit, range * 1.02f, ~mask))
            {
                //Debug.DrawLine(caller.position, hit.point, Color.yellow, 1);
                ///   Debug.Log("HIT!" + hit.transform.name);
                if (hit.transform == target)
                {
                    // Debug.DrawRay(caller.position, (target.position - caller.position), Color.cyan, 5);
                    return true;
                }
                else
                {
                    //  Debug.DrawRay(caller.position, (target.position - caller.position), Color.magenta, 5);
                    return false;
                }
            }
            //Debug.DrawRay(caller.position, (target.position - caller.position), Color.yellow, 5);
            return false;
        }

        public static bool IsLOS (Transform caller, Transform target, LayerMask mask, Color col)
        {
            float range = (target.position - caller.position).magnitude;
            RaycastHit hit;

            if (Physics.Raycast(caller.position, (target.position - caller.position), out hit, range * 1.1f, mask))
            {
                Debug.DrawRay(caller.position, (target.position - caller.position).normalized * hit.distance, col, 1);
                Debug.Log("I hit " + hit.collider.gameObject.name, hit.collider.gameObject);
                if (hit.transform == target)
                {
                    Debug.DrawRay(caller.position, (target.position - caller.position), Color.green, 5);
                    return true;
                }
                else
                {
                    //Debug.DrawRay(caller.position, (target.position - caller.position), Color.magenta, 5);
                    return false;
                }
            }
            //Debug.DrawRay(caller.position, (target.position - caller.position), Color.yellow, 5);
            return false;
        }
        public static Quaternion ShakeRot (float intensity, Quaternion originRot)
        {

            Quaternion outRot = new Quaternion(
                originRot.x + UnityEngine.Random.Range(-intensity, intensity) * .2f,
                originRot.y + UnityEngine.Random.Range(-intensity, intensity) * .2f,
                originRot.z + UnityEngine.Random.Range(-intensity, intensity) * .2f,
                originRot.w + UnityEngine.Random.Range(-intensity, intensity) * .2f);
            return outRot;

        }



        //Calculates the median position of a bunch of dudes
        public static Vector3 Median (GameObject [] gos)
        {
            Vector3 tempVector = Vector3.zero;
            Vector3 minPos = Vector3.zero;
            Vector3 maxPos = Vector3.zero;

            float maxX = -99999;
            float maxY = -99999;
            float maxZ = -99999;
            float minX = 99999;
            float minY = 99999;
            float minZ = 99999;

            foreach (GameObject c in gos)
            {
                if (c == null) continue;
                tempVector = c.transform.position;
                maxX = Mathf.Max(tempVector.x, maxX);
                maxY = Mathf.Max(tempVector.y, maxY);
                maxZ = Mathf.Max(tempVector.z, maxZ);
                minX = Mathf.Min(tempVector.x, minX);
                minY = Mathf.Min(tempVector.y, minY);
                minZ = Mathf.Min(tempVector.z, minZ);
            }
            minPos = new Vector3(minX, minY, minZ);
            maxPos = new Vector3(maxX, maxY, maxZ);

            Debug.DrawLine(minPos, maxPos, Color.red, 5);
            Debug.DrawLine(minPos, maxPos + (minPos - maxPos) / 2, Color.cyan, 5);

            return maxPos + (minPos - maxPos) / 2;

        }


        public static Vector3 OldFashionedRotation (Vector3 currentEuler, Vector3 targetEuler, float maxSpeed)
        {

            if (currentEuler.x < targetEuler.x)
            {
                currentEuler.x += maxSpeed * Time.deltaTime;
            }
            if (currentEuler.x > targetEuler.x)
            {
                currentEuler.x -= maxSpeed * Time.deltaTime;
            }

            if (currentEuler.y < targetEuler.y)
            {
                currentEuler.y += maxSpeed * Time.deltaTime;
            }
            if (currentEuler.y > targetEuler.y)
            {
                currentEuler.y -= maxSpeed * Time.deltaTime;
            }

            if (currentEuler.z < targetEuler.z)
            {
                currentEuler.z += maxSpeed * Time.deltaTime;
            }
            if (currentEuler.z > targetEuler.z)
            {
                currentEuler.z -= maxSpeed * Time.deltaTime;
            }

            return currentEuler;
        }

        /// <summary>
        /// A simple tween that doesn't use lerping.  Useful for angles and such.		
        /// </summary>
        /// <returns>The tweened float</returns>
        /// <param name="currentNum">Current number.</param>
        /// <param name="targetNum">The number you want current number to be!</param>
        /// <param name="maxSpeed">Max speed.</param>
        /// <param name="slowDist">if the difference between currentNum and TargetNum is less than this, it will move slow and accurate</param>
        public static float OldFashionedTween (float currentNum, float targetNum, float maxSpeed, float slowDist)
        {

            float f = currentNum;
            float diff = Mathf.Abs(targetNum - currentNum);
            int direction = PosOrNeg(targetNum - currentNum);


            if (diff > slowDist)
            {
                f += Mathf.Clamp((diff * 5), -maxSpeed * 20, maxSpeed * 20) * Time.deltaTime * direction;
                return f;
            }

            else
            {
                f += Mathf.Clamp((diff), -maxSpeed, maxSpeed) * Time.deltaTime * direction;
                return f;
            }

        }

        /// <summary>
        ///  Am ordered, circle of points along the X/Y axis.
        /// </summary>
        /// <returns>List of ordered points arranged in a circle, following the points down the list will return you to the first one.</returns>
        /// <param name="center">Center of Circle.</param>
        /// <param name="radius">Radius of Circle.</param>
        /// <param name="points">how many points in the circle(more than 2).</param>
        public static List<Vector3> VectorCircle (Vector3 center, float radius, int points)
        {
            List<Vector3> returnList = new List<Vector3>();
            if (points > 1)
            {
                for (int i = 0; i < points; i++)
                {
                    float fraction = (i * 1.0f) / points;
                    float angle = fraction * Mathf.PI * 2;
                    float z = Mathf.Cos(angle) * radius;
                    float x = Mathf.Sin(angle) * radius;
                    Vector3 position = new Vector3(x, 0, z) + center;
                    returnList.Add(position);
                }
            }
            else
                returnList.Add(center);

            return returnList;
        }
        /// <summary>
        /// Constructs a bunch of points on a sphere around the point.
        /// </summary>
        /// <returns>List of Vectors that make up the search pattern .</returns>
        /// <param name="center">Center of sphere.</param>
        /// <param name="radius">Radius of sphere.</param>
        /// <param name="points">Amount of points on sphere.</param>
        public static List<Vector3> RandomVectorSphere (Vector3 center, float radius, int points)
        {
            List<Vector3> returnList = new List<Vector3>();
            if (points > 1)
            {
                for (int i = 0; i < points; i++)
                {
                    Vector3 position = (UnityEngine.Random.onUnitSphere * radius) + center;
                    returnList.Add(position);
                }
            }
            else
                returnList.Add(center);

            return returnList;
        }

        //first-order intercept using absolute target position
        public static Vector3 FirstOrderIntercept (Vector3 shooterPosition, Vector3 shooterVelocity, float shotSpeed, Vector3 targetPosition, Vector3 targetVelocity)
        {
            Vector3 targetRelativePosition = targetPosition - shooterPosition;
            Vector3 targetRelativeVelocity = targetVelocity - shooterVelocity;
            float t = FirstOrderInterceptTime
            (
                shotSpeed,
                targetRelativePosition,
                targetRelativeVelocity
            );

            Vector3 returnLead = targetPosition + t * (targetRelativeVelocity);

            if (VectorNaN(returnLead))
                return targetRelativePosition;

            return returnLead;
        }

        //first-order intercept using relative target position
        public static float FirstOrderInterceptTime (float shotSpeed, Vector3 targetRelativePosition, Vector3 targetRelativeVelocity)
        {
            float velocitySquared = targetRelativeVelocity.sqrMagnitude;
            if (velocitySquared < 0.001f)
            {
                //  Debug.Log("targetRelativeVelocity is 0");
                return 0f;
            }
            float a = velocitySquared - shotSpeed * shotSpeed;

            //handle similar velocities
            if (Mathf.Abs(a) < 0.001f)
            {
                float t = -targetRelativePosition.sqrMagnitude / (2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition));
                // Debug.Log("Projectilespeed is lower than shotspeed" );

                return Mathf.Max(t, 0f); //don't shoot back in time
            }

            float b = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
            float c = targetRelativePosition.sqrMagnitude;
            float determinant = b * b - 4f * a * c;

            if (determinant > 0f)
            { //determinant > 0; two intercept paths (most common)
              //   Debug.Log("TWO DETERMINANTS");

                float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a),
                        t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
                if (t1 > 0f)
                {
                    if (t2 > 0f)
                        return Mathf.Min(t1, t2); //both are positive
                    else
                        return t1; //only t1 is positive
                }
                else
                    return Mathf.Max(t2, 0f); //don't shoot back in time
            }
            else if (determinant < 0f) //determinant < 0; no intercept path
            {
                //Debug.Log("determinant is less than 0");
                return 0f;
            }
            else //determinant = 0; one intercept path, pretty much never happens
            {
                //Debug.Log("determinant is 0");
                return Mathf.Max(-b / (2f * a), 0f); //don't shoot back in time
            }
        }

        /// <summary>
        /// Returns a vector 3 that has the largest dimension as the given float 'max'. For example, given a v3 of (1, 4, 2) and a max of 1
        /// would return (.25, 1, .5)
        /// </summary>
        /// <param name="input">The vector3 to use as input</param>
        /// <param name="max">The value that the largest dimension should be.</param>
        public static Vector3 SetMaxDimension (Vector3 input, float max)
        {
            float largestDimension = Mathf.Max(Mathf.Abs(input.x), Mathf.Abs(input.y), Mathf.Abs(input.z));
            largestDimension = Mathf.Clamp(largestDimension, .001f, 10000000);

            float multiplier = max / largestDimension;
            return input * multiplier;
        }


        //Clamps an Angle around 360 degrees, making sure that we get proper rotations
        public static float ClampAngle (float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }


        public static float HalfClampAngle (float angle, float min, float max)
        {
            if (angle < -180f)
                angle += 180f;
            if (angle > 180f)
                angle -= 180f;
            return Mathf.Clamp(angle, min, max);
        }

        public static float CleanAngle180 (float angle)
        {

            while (angle >= 360)
            {
                angle -= 360;
            }

            while (angle <= -360)
            {
                angle += 360;
            }

            if (angle > 180)
            {
                float diff = angle - 180;
                angle = -1 * (180 - diff);
            }
            else
            if (angle < -180)
            {
                float diff2 = angle + 180;
                angle = 180 + diff2;
            }
            return angle;
        }

        public static float LongestDimension (Vector3 box)
        {

            float baseNum = 0;

            if (Mathf.Abs(box.x) > baseNum)
            {
                baseNum = Mathf.Abs(box.x);
            }

            if (Mathf.Abs(box.y) > baseNum)
            {
                baseNum = Mathf.Abs(box.y);
            }

            if (Mathf.Abs(box.z) > baseNum)
            {
                baseNum = Mathf.Abs(box.z);
            }

            return baseNum;
        }


        /*
        public static Hull FindNearestHull(List<Hull> targetList, Transform caller)
        {

            //create a new game object list
            List<Hull> orderedList = new List<Hull>();
            orderedList.AddRange(targetList);
            foreach (Hull h in orderedList)
            {             
                if (h == null)
                {
                    targetList.RemoveAt(targetList.IndexOf(h));
                    continue;
                }

                if (h.gameObject.activeInHierarchy)
                {
                    targetList.Remove(h);
                    continue;
                }
            }
            orderedList.Clear();
            //order by distance
            orderedList = targetList.OrderBy(go => Vector3.SqrMagnitude(caller.position - go.transform.position)).ToList();
            if (orderedList.Count > 1)
                return orderedList[0];
            else
                return null;
        }
        */



        public static FixedJoint FindNearestTransform (List<FixedJoint> targetList, Transform caller)
        {

            //create a new game object list
            List<FixedJoint> orderedList = new List<FixedJoint>();

            //order by distance
            orderedList = targetList.OrderBy(go => Vector3.SqrMagnitude(caller.position - go.transform.position)).ToList();

            return orderedList [0];
        }




        public static Transform FindNearestTransform (Transform [] targetList, Transform caller)
        {

            //create a new game object list
            List<Transform> orderedList = new List<Transform>();

            //order by distance
            orderedList = targetList.OrderBy(go => Vector3.SqrMagnitude(caller.position - go.position)).ToList();

            return orderedList [0];
        }


        public static Transform FindNearestTransform (List<Transform> targetList, Transform caller)
        {

            //create a new game object list
            List<Transform> orderedList = new List<Transform>();

            //order by distance
            orderedList = targetList.OrderBy(go => Vector3.SqrMagnitude(caller.position - go.position)).ToList();

            return orderedList [0];
        }

        public static PathMono FindNearestPathMono (List<PathMono> targetList, Vector3 position)
        {
            if (targetList == null)
            {
                Debug.LogError("Null Path list, check the Scene's landmark manager to re-set the landmarks");
                return null;
            }
            if (targetList.Count < 1)
            {
                Debug.LogError("Empty Path list, check the Scene's landmark manager to re-set the landmarks");
                return null;
            }
            //create a new game object list
            List<PathMono> orderedList = new List<PathMono>();

            foreach (PathMono path in targetList)
            {
                if (path == null)
                {
                    //Debug.Log("There's a null fucking point in here motherfucker");                 
                    continue;
                }

                orderedList.Add(path);
            }



            //order by distance
            orderedList = orderedList.OrderBy(go => Vector3.SqrMagnitude(position - go.transform.position)).ToList();
            Debug.DrawLine(position, orderedList [0].transform.position, Color.yellow, 0.1f);
            return orderedList [0];
        }


        public static WaypointNode FindNearestWayPoint (List<PathMono> targetList, Vector3 position)
        {
            if (targetList == null)
            {
                Debug.LogError("Null Path list, check the Scene's landmark manager to re-set the landmarks");
                return null;
            }
            if (targetList.Count < 1)
            {
                Debug.LogError("Empty Path list, check the Scene's landmark manager to re-set the landmarks");
                return null;
            }
            if (targetList.Contains(null))
            {
                // Debug.LogError("Target List Has bad waypoints, or has been destroyed, returning null");
                return null;
            }

            List<WaypointNode> returnWPs = new List<WaypointNode>();

            //create a new game object list
            List<PathMono> orderedList = new List<PathMono>();

            //order by distance
            orderedList = targetList.OrderBy(go => Vector3.SqrMagnitude(position - go.transform.position)).ToList();

            foreach (WaypointNode wp in orderedList)
                returnWPs.Add(wp);

            return returnWPs.First();
        }


        public static GameObject FindNearestGameObject (GameObject [] targetList, GameObject caller)
        {

            //create a new game object list
            List<GameObject> orderedList = new List<GameObject>();

            //order by distance
            orderedList = targetList.OrderBy(go => Vector3.SqrMagnitude(caller.transform.position - go.transform.position)).ToList();

            return orderedList [0];
        }



        public static Rigidbody FindNearestRigidbody (List<Rigidbody> rbList, GameObject caller)
        {

            //create a new game object list
            List<Rigidbody> orderedList = new List<Rigidbody>();

            //order by distance
            orderedList = rbList.OrderBy(go => Vector3.SqrMagnitude(caller.transform.position - go.transform.position)).ToList();

            return orderedList [0];
        }


        public static float ShortestDimension (Vector3 box)
        {

            float baseNum = 10000000;

            if (Mathf.Abs(box.x) < baseNum)
            {
                baseNum = Mathf.Abs(box.x);
            }

            if (Mathf.Abs(box.y) < baseNum)
            {
                baseNum = Mathf.Abs(box.y);
            }

            if (Mathf.Abs(box.z) < baseNum)
            {
                baseNum = Mathf.Abs(box.z);
            }

            return baseNum;
        }

        //Returns -1 when left and 1 when right, 0 when forwards or backwards
        public static float AngleDir (Vector3 fwd, Vector3 targetDir, Vector3 up)
        {
            Vector3 perp = Vector3.Cross(fwd, targetDir);
            float dir = Vector3.Dot(perp, up);

            if (dir > 0.0f)
            {
                return 1.0f;
            }
            else if (dir < 0.0f)
            {
                return -1.0f;
            }
            else
            {
                return 0.0f;
            }
        }

        public static Atmosphere LerpAtmosphere (Atmosphere start, Atmosphere end, float value)
        {

            Atmosphere returnAtmo = new Atmosphere();

            //lerp ambient light
            if (start == null || end == null) return null;
            returnAtmo.ambientLightColor = Color.Lerp(start.ambientLightColor, end.ambientLightColor, value);

            returnAtmo.fogColor = Color.Lerp(start.fogColor, end.fogColor, value);

            //returnAtmo.fogExposure = Mathf.Lerp(start.fogExposure, end.fogExposure, value);
            
            returnAtmo.fogDist = Vector2.Lerp(start.fogDist, end.fogDist, value);

            returnAtmo.bloomIntensity = Mathf.Lerp(start.bloomIntensity, end.bloomIntensity, value);

            returnAtmo.directionalLightIntensity = Mathf.Lerp(start.directionalLightIntensity, end.directionalLightIntensity, value);

            returnAtmo.directionalLightColor = Color.Lerp(start.directionalLightColor, end.directionalLightColor, value);

            //returnAtmo.aboveColor = Color.Lerp(start.aboveColor, end.aboveColor, value);;

           // returnAtmo.belowColor =Color.Lerp(start.belowColor, end.belowColor, value);;

            return returnAtmo;

        }

        public static float EaseBothLerp (float start, float end, float value)
        {
            float tempValue = value * Mathf.PI;
            float output = .5f + -Mathf.Cos(tempValue) / 2;

            return Mathf.Lerp(start, end, output);
        }

        public static Vector3 EaseBothLerp (Vector3 start, Vector3 end, float value)
        {
            float tempValue = value * Mathf.PI;
            float output = Mathf.Clamp01(.5f + -Mathf.Cos(tempValue) / 2);

            return Vector3.Lerp(start, end, output);
        }

        public static float EaseInLerp (float start, float end, float value)
        {
            float tempValue = value * Mathf.PI * .5f;
            float output = 1 + -Mathf.Cos(tempValue);

            return Mathf.Lerp(start, end, output);
        }

        public static Vector3 EaseInLerp (Vector3 start, Vector3 end, float value)
        {
            float tempValue = value * Mathf.PI * .5f;
            float output = Mathf.Clamp01(1 + -Mathf.Cos(tempValue));

            return Vector3.Lerp(start, end, output);
        }


        public static float EaseOutLerp (float start, float end, float value)
        {
            float tempValue = value * Mathf.PI * .5f;
            float output = Mathf.Sin(tempValue);

            return Mathf.Lerp(start, end, output);
        }

        public static Vector3 EaseOutLerp (Vector3 start, Vector3 end, float value)
        {
            float tempValue = value * Mathf.PI * .5f;
            float output = Mathf.Clamp01(Mathf.Sin(tempValue));

            return Vector3.Lerp(start, end, output);
        }

        public static float Hermite (float start, float end, float value)
        {
            return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
        }

        public static float Sinerp (float start, float end, float value)
        {
            return Mathf.Lerp(start, end, Mathf.Sin(value * Mathf.PI * 0.5f));
        }

        public static float Coserp (float start, float end, float value)
        {
            return Mathf.Lerp(start, end, 1.0f - Mathf.Cos(value * Mathf.PI * 0.5f));
        }

        public static float SignedAngleY (Vector3 f, Vector3 t)
        {
            float angle = Vector3.Angle(f, t); // calculate angle
                                               // assume the sign of the cross product's Y component:
            return angle * Mathf.Sign(Vector3.Cross(f, t).y);
        }

        public static float SignedAngleX (Vector3 f, Vector3 t)
        {
            float angle = Vector3.Angle(f, t); // calculate angle
                                               // assume the sign of the cross product's Y component:
            return angle * Mathf.Sign(Vector3.Cross(f, t).x);
        }

        /// <summary>
        /// Checks if a float is between two arbitrary values
        /// </summary>
        /// <param name="target">Target.</param>
        /// <param name="value1">Value1.</param>
        /// <param name="value2">Value2.</param>
        public static bool Between (float target, float value1, float value2)
        {
            if (target < value1 && target > value2)
                return true;

            if (target > value1 && target < value2)
                return true;

            return false;
        }






    }

    public class GetObj
    {
        public GameObject returnedObj;
        public GetObj (Camera currentCam, Vector2 screenPosition)
        {
            Ray ray = currentCam.ScreenPointToRay(new Vector3(screenPosition.x, screenPosition.y, 0));
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                returnedObj = hit.collider.gameObject;
                //				inspect = new Inspector(hit.collider.gameObject);
            }
        }

    }

    public class Cam
    {
        Vector3 originPos;
        Quaternion originRot;

        /// <summary> 
        ///Casts a ray from a cam through 3d space, looking for a boxcollider.
        /// </summary>		 
        public static BoxCollider2D CastToBox2D (Camera cam, int mask)
        {
            Ray r = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(r, 100f, 1 << mask);
            if (hit)
            {
                if (hit.collider.GetType() == typeof(BoxCollider2D))
                {
                    return hit.collider as BoxCollider2D;
                }
                else
                    return null;
            }
            else
                return null;
        }



        /// <summary>
        /// Checks if the point v can be seen by the camera
        /// </summary>
        /// <param name="Camera to check"></param>
        /// <param name="Point to Check"></param>
        /// <returns></returns>
        public static bool CanIsee (Camera cam, Vector3 v)
        {
            if (cam == null) return false;
            float farClipPlane = cam.farClipPlane;

            //Debug.DrawRay(cam.transform.position, v - cam.transform.position, Color.cyan, 0.1f);
            if (!Calc.WithinDistance(farClipPlane, cam.transform.position, v)) return false; //if the vector is not within the max possible distance, return false

            Vector3 pointOnCam = cam.WorldToViewportPoint(v);
            //  Debug.Log("ViewPortPoint = " + pointOnCam);

            if (pointOnCam.z < 0) return false;//If its behind the camera, return false

            if (pointOnCam.x < 0 || pointOnCam.x > 1 //if its outside any of the pointoncam normalized values, return false
                || pointOnCam.y < 0 || pointOnCam.y > 1) return false;

            return true;//otherwise it must be in front of you
        }

        /// <summary>
        /// Returns true if you can see All of the ones in the list
        /// </summary>
        /// <param name="cam"></param>
        /// <param name="vectors"></param>
        /// <returns></returns>
        public static bool CanIseeAll (Camera cam, List<Vector3> vectors)
        {
            foreach (Vector3 v in vectors)
            {
                if (!CanIsee(cam, v))
                    return false;
            }

            return true;//otherwise it must be in front of you
        }

        /// <summary>
        /// returns true if you can see ANY of the ones in the list
        /// </summary>
        public static bool CanISeeAny (Camera cam, List<Vector3> vectors)
        {
            foreach (Vector3 v in vectors)
            {
                if (CanIsee(cam, v))
                    return true;
            }

            return false;
        }


        public static Collider2D CursorHitUI ()
        {

            Camera cam = Diluvion.WorldTools.GUITools.UICam();

            Ray r = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(r.origin, r.direction * 100, Color.red);

            //check for all UI layer next - layer 15
            RaycastHit2D hitUI = Physics2D.GetRayIntersection(r, 1000, 1 << 5);
            if (hitUI)
            {
                return hitUI.collider;
            }
            return null;
        }


        public static Collider2D RayHit2D (Camera cam, Vector3 raycastPos)
        {

            Ray r = cam.ScreenPointToRay(raycastPos);

            LayerMask crewMask = LayerMask.GetMask("Crew");
            //check for characters first -- layer 17
            RaycastHit2D hit = Physics2D.GetRayIntersection(r, 100, crewMask);
            if (hit)
            {
                return hit.collider;
            }

            LayerMask interiorMask = LayerMask.GetMask("Interior");
            //check for all interior layer next - layer 18
            RaycastHit2D hit2 = Physics2D.GetRayIntersection(r, 100, interiorMask);
            if (hit2)
            {
                return hit2.collider;
            }

            return null;
        }


        /// <summary>
        /// Returns any 2D collider the cursor is currently over
        /// </summary>
        public static Collider2D CursorHit2D (Camera cam)
        {
            return RayHit2D(cam, Input.mousePosition);
        }
    }

    public class StringOps
    {
        public static string UpperCaseFirst (string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a [0] = char.ToUpper(a [0]);
            return new string(a);
        }

        public static List<string> RemoveEmptyStrings (List<string> input)
        {
            List<string> output = new List<string>();
            foreach (string str in input)
            {
                if (str.Length > 2)
                    output.Add(str);
            }
            return output;
        }
    }

}