using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using HeathenEngineering.UIX.Serialization;
using System.Reflection;
using HeathenEngineering.Events;

namespace HeathenEngineering.UIX
{
    public class LigatureHelper : MonoBehaviour
    {
        /// <summary>
        /// List of ligatures to test for
        /// </summary>
        public List<LigatureReference> map;
        /// <summary>
        /// used by the editor script
        /// </summary>
        private GameObject previousLinkedGameObject;
        /// <summary>
        /// The current linked game object for output
        /// </summary>
        public GameObject linkedGameObject;
        /// <summary>
        /// The last read of the available components on the linked game object
        /// </summary>
        public List<Component> linkedBehaviours;
        /// <summary>
        /// The selected component to be updated
        /// </summary>
        public Component linkedBehaviour;
        /// <summary>
        /// The last read of the available string field names on the linked behaviour
        /// </summary>
        public List<string> fields;
        /// <summary>
        /// The selected string field name to be updated
        /// </summary>
        public string field;
        /// <summary>
        /// Occurs when one or more ligatures are applied through the ParseStringEnd or ParseStringAll methods
        /// </summary>
        public event RoutedEvent<LigatureAplicationData> OnLigaturesApplied;

        public string ParseStringEnd(string value)
        {
            string working = value;
            LigatureAplicationData results = new LigatureAplicationData();
            results.targetGameObject = linkedGameObject;
            results.targetBehaviour = linkedBehaviour;
            results.targetField = field;
            results.originalString = value;
            results.appliedLigatures = new List<LigatureReference>();
            if(map != null)
            {
                foreach(LigatureReference lig in map)
                {
                    if (lig.EndsWith(value))
                        results.appliedLigatures.Add(lig);

                    working = lig.ReplaceEnd(working);
                }
            }

            if (value != working && linkedBehaviour != null && field != null)
            {
                SetLinkedFieldValue(working);

                if (OnLigaturesApplied != null)
                    OnLigaturesApplied(this, results);
            }

            return working;
        }

        public string ParseStringAll(string value)
        {
            string working = value;
            LigatureAplicationData results = new LigatureAplicationData();
            results.targetGameObject = linkedGameObject;
            results.targetBehaviour = linkedBehaviour;
            results.targetField = field;
            results.originalString = value;
            results.appliedLigatures = new List<LigatureReference>();
            if (map != null)
            {
                foreach (LigatureReference lig in map)
                {
                    if (lig.Contains(value))
                        results.appliedLigatures.Add(lig);

                    working = lig.ReplaceAll(working);
                }
            }

            if (value != working && linkedBehaviour != null && field != null)
            {
                SetLinkedFieldValue(working);

                if (OnLigaturesApplied != null)
                    OnLigaturesApplied(this, results);
            }

            return working;
        }

        /// <summary>
        /// Same as ParseStringEnd but does not return a value thus suitable for Unity OnValueChanged events
        /// </summary>
        /// <param name="value"></param>
        public void UnityActionParseStringEnd(string value)
        {
            ParseStringEnd(value);
        }

        /// <summary>
        /// Same as ParseStringAll but does not return a value thus suitable for Unity OnValueChanged events
        /// </summary>
        /// <param name="value"></param>
        public void UnityActionParseStringAll(string value)
        {
            ParseStringAll(value);
        }

        public List<Component> GetLinkedBehaviour()
        {
            if (linkedGameObject != null)
            {
                List<Component> results = new List<Component>();
                foreach (Component com in linkedGameObject.GetComponents<Component>())
                {
                    bool hasString = false;
                    foreach (PropertyInfo info in com.GetType().GetProperties())
                    {
                        if (info.PropertyType == typeof(string))
                        {
                            hasString = true;
                            break;
                        }
                    }

                    if (hasString)
                        results.Add(com);
                }
                return results;
            }
            else
                return null;
        }

        public void ValidateLinkedData()
        {
            if (linkedGameObject == null)
                return;

            if ((linkedBehaviours == null || previousLinkedGameObject != linkedGameObject || linkedBehaviours.Count <= 0))
            {
                previousLinkedGameObject = linkedGameObject;
                linkedBehaviours = GetLinkedBehaviour();
            }

            if ((linkedBehaviour == null || !linkedBehaviours.Contains(linkedBehaviour)) && linkedBehaviours.Count > 0)
                linkedBehaviour = linkedBehaviours[0];

            if (fields == null || fields.Count < 3)
                fields = GetStringFieldsInBehaviour();

            if (field == null || !fields.Contains(field))
                field = fields[0];
        }

        public List<string> GetStringFieldsInBehaviour()
        {
            if (linkedBehaviour != null)
            {
                List<string> results = new List<string>();
                foreach (PropertyInfo info in linkedBehaviour.GetType().GetProperties())
                {
                    if (info.PropertyType == typeof(string))
                        results.Add(info.Name);
                }
                return results;
            }
            else
                return null;
        }

        public string GetLinkedFieldValue()
        {
            if (field != null && linkedBehaviour != null)
            {
                return linkedBehaviour.GetType().GetProperty(field).GetValue(linkedBehaviour, null).ToString();
            }
            return null;
        }

        public void SetLinkedFieldValue(string value)
        {
            if (field != null && linkedBehaviour != null)
            {
                linkedBehaviour.GetType().GetProperty(field).SetValue(linkedBehaviour, value, null);
            }
        }
    }

    public class LigatureAplicationData
    {
        public List<LigatureReference> appliedLigatures;
        public string originalString;
        public string resultingString;
        public GameObject targetGameObject;
        public Component targetBehaviour;
        public string targetField;
    }
}
namespace HeathenEngineering.UIX.Serialization
{
    [Serializable]
    public class LigatureReference
    {
        [SerializeField]
        public string Characters;
        [SerializeField]
        public string Ligature;

        public LigatureReference()
        {
            Characters = "";
            Ligature = "";
        }

        public LigatureReference(string Chars, string Lig)
        {
            Characters = Chars;
            Ligature = Lig;
        }

        public bool EndsWith(string value)
        {
            return value.EndsWith(Characters);
        }

        public bool Contains(string value)
        {
            return value.Contains(Characters);
        }

        public string ReplaceAll(string value)
        {
            return value.Replace(Characters, Ligature);
        }

        public string ReplaceEnd(string value)
        {
            if (EndsWith(value))
            {
                return value.Substring(0, value.Length - Characters.Length) + Ligature;
            }
            else
                return value;
        }
    }
}