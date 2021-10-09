using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.EventSystems;
using HeathenEngineering.UIX.Serialization;
using HeathenEngineering.Events;

namespace HeathenEngineering.UIX
{
    public class Keyboard : MonoBehaviour
    {
        public bool autoLinkHID = false;
        public bool autoTargetInputfields = true;
        public KeyboardState state = KeyboardState.Normal;
        public KeyboardKey keyPrototype;
        public RectTransform keyContainer;
        public Serialization.KeyboardTemplate workingTemplate;
        public Serialization.KeyboardTemplate selectedTemplate;
        public List<KeyboardKey> keys = new List<KeyboardKey>();
        public RectTransform headerRowTransform;
        public List<RectTransform> rowTransforms;
        private GameObject previousLinkedGameObject;
        public GameObject linkedGameObject;
        public List<Component> linkedBehaviours;
        public Component linkedBehaviour;
        public List<string> fields;
        public string field;
        public event RoutedEvent<KeyboardKey> KeyboardKeyPressed;
        /// <summary>
        /// The point at which the character will be inserted; set to -1 to ignore thus always entering at the end
        /// This value will automaticly advance unless set to -1
        /// </summary>
        public int insertPoint = -1;
        public KeyboardKey ActiveKey;
        private KeyboardState statePrevious = KeyboardState.ShiftedAltGr;
        private UnityEngine.UI.InputField lastInputField;

        // Use this for initialization
        void Start()
        {
            UpdateKeyLinks();
        }

        void Update()
        {
            if (AltGrKeysHeld.Count > 0 || (useAltGrToggle && altGrToggle))
            {
                if ((ShiftKeysHeld.Count > 0 || (useShiftToggle && shiftToggle)) || capsLockToggle)
                    state = KeyboardState.ShiftedAltGr;
                else
                    state = KeyboardState.AltGr;
            }
            else if ((ShiftKeysHeld.Count > 0 || (useShiftToggle && shiftToggle)) || capsLockToggle)
                state = KeyboardState.Shifted;
            else
                state = KeyboardState.Normal;

            if (autoTargetInputfields)
            {
                if (EventSystem.current.currentSelectedGameObject != null)
                {
                    UnityEngine.UI.InputField inputField = EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>();

                    if (inputField != null)
                        lastInputField = inputField;

                    if (inputField != null && EventSystem.current.currentSelectedGameObject != linkedGameObject)
                    {
                        lastInputField = inputField;
                        linkedGameObject = EventSystem.current.currentSelectedGameObject;
                        linkedBehaviour = lastInputField;
                        field = "text";

                        if (insertPoint > -1)
                            insertPoint = lastInputField.caretPosition;
                    }

                    //We know the input field, we know it is active and we are supposed to track point
                    if (lastInputField != null && EventSystem.current.currentSelectedGameObject == linkedGameObject && insertPoint > -1)
                        insertPoint = lastInputField.caretPosition;
                }
            }

            if (autoLinkHID && EventSystem.current.currentSelectedGameObject != linkedGameObject)
            {
                foreach (KeyboardKey key in keys)
                {
                    if (Input.GetKeyDown(key.keyGlyph.code))
                    {
                        key.Press();
                    }
                }
            }

            if (statePrevious != state)
                UpdateState();
        }

        public void RegisterKey(KeyboardKey key)
        {
            if (!keys.Contains(key))
            {
                keys.Add(key);
                key.keyboard = this;
                key.pressed += keyPressed;
                key.isDown += keyIsDown;
                key.isUp += keyIsUp;
            }
        }

        public void UpdateKeyLinks()
        {
            foreach (KeyboardKey key in keys)
            {
                key.pressed -= keyPressed;
            }

            keys.Clear();

            foreach (KeyboardKey key in GetComponentsInChildren<KeyboardKey>())
            {
                RegisterKey(key);
            }
        }

        private void UpdateState()
        {
            statePrevious = state;
            switch (state)
            {
                case KeyboardState.Normal:
                    foreach (KeyboardKey key in keys)
                    {
                        if (key.keyGlyph.normal != null)
                            key.keyGlyph.normal.gameObject.SetActive(true);
                        if (key.keyGlyph.shifted != null)
                            key.keyGlyph.shifted.gameObject.SetActive(false);
                        if (key.keyGlyph.altGr != null)
                            key.keyGlyph.altGr.gameObject.SetActive(false);
                        if (key.keyGlyph.shiftedAltGr != null)
                            key.keyGlyph.shiftedAltGr.gameObject.SetActive(false);
                    }
                    break;
                case KeyboardState.Shifted:
                    foreach (KeyboardKey key in keys)
                    {
                        if (key.keyGlyph.normal != null)
                            key.keyGlyph.normal.gameObject.SetActive(false);
                        if (key.keyGlyph.shifted != null)
                            key.keyGlyph.shifted.gameObject.SetActive(true);
                        if (key.keyGlyph.altGr != null)
                            key.keyGlyph.altGr.gameObject.SetActive(false);
                        if (key.keyGlyph.shiftedAltGr != null)
                            key.keyGlyph.shiftedAltGr.gameObject.SetActive(false);
                    }
                    break;
                case KeyboardState.AltGr:
                    foreach (KeyboardKey key in keys)
                    {
                        if (key.keyGlyph.normal != null)
                            key.keyGlyph.normal.gameObject.SetActive(false);
                        if (key.keyGlyph.shifted != null)
                            key.keyGlyph.shifted.gameObject.SetActive(false);
                        if (key.keyGlyph.altGr != null)
                            key.keyGlyph.altGr.gameObject.SetActive(true);
                        if (key.keyGlyph.shiftedAltGr != null)
                            key.keyGlyph.shiftedAltGr.gameObject.SetActive(false);
                    }
                    break;
                case KeyboardState.ShiftedAltGr:
                    foreach (KeyboardKey key in keys)
                    {
                        if (key.keyGlyph.normal != null)
                            key.keyGlyph.normal.gameObject.SetActive(false);
                        if (key.keyGlyph.shifted != null)
                            key.keyGlyph.shifted.gameObject.SetActive(false);
                        if (key.keyGlyph.altGr != null)
                            key.keyGlyph.altGr.gameObject.SetActive(false);
                        if (key.keyGlyph.shiftedAltGr != null)
                            key.keyGlyph.shiftedAltGr.gameObject.SetActive(true);
                    }
                    break;
            }
        }

        /// <summary>
        /// Press the current active key
        /// </summary>
        public void PressKey()
        {
            if (ActiveKey != null)
                ActiveKey.Press();
        }
        /// <summary>
        /// Activate the selected key and press it
        /// </summary>
        /// <param name="key"></param>
        public void PressKey(KeyboardKey key)
        {
            if (key != null)
            {
                EventSystem.current.SetSelectedGameObject(key.gameObject);
                ActiveKey = key;
                key.Press();
            }
        }
        /// <summary>
        /// Find the key with the matching code and press it
        /// </summary>
        /// <param name="code"></param>
        public void PressKey(KeyCode code)
        {
            foreach (KeyboardKey key in keys)
            {
                if (key.keyGlyph.code == code)
                {
                    PressKey(key);
                    return;
                }
            }
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

        public void RefreshTemplate()
        {
            if (workingTemplate == null)
                workingTemplate = new Serialization.KeyboardTemplate() { TemplateName = "New Template" };

            if (headerRowTransform == null)
                workingTemplate.HeaderRow = null;
            else
            {
                if (workingTemplate.HeaderRow == null)
                    workingTemplate.HeaderRow = new Serialization.KeyboardTemplateRow();

                workingTemplate.HeaderRow.RowOffset = headerRowTransform.anchoredPosition3D;
                workingTemplate.HeaderRow.RowRotation = headerRowTransform.localEulerAngles;

                List<KeyboardKeyTemplate> keyTemplates = new List<KeyboardKeyTemplate>();

                foreach (Transform trans in headerRowTransform)
                {
                    KeyboardKey key = trans.gameObject.GetComponent<KeyboardKey>();
                    if (key != null)
                    {
                        key.UpdateTemplate(ref key.template);
                        keyTemplates.Add(key.template);
                    }
                }

                workingTemplate.HeaderRow.Keys = keyTemplates.ToArray();
            }

            List<KeyboardTemplateRow> rowTemplates = new List<KeyboardTemplateRow>();

            if (rowTransforms == null)
                rowTransforms = new List<RectTransform>();

            foreach (RectTransform row in rowTransforms)
            {
                if (row == null)
                    continue;

                KeyboardTemplateRow nRow = new KeyboardTemplateRow();
                nRow.RowOffset = row.anchoredPosition3D;
                nRow.RowRotation = row.localEulerAngles;

                List<KeyboardKeyTemplate> keyTemplates = new List<KeyboardKeyTemplate>();

                foreach (Transform trans in row)
                {
                    KeyboardKey key = trans.gameObject.GetComponent<KeyboardKey>();
                    if (key != null)
                    {
                        key.UpdateTemplate(ref key.template);
                        keyTemplates.Add(key.template);
                    }
                }

                nRow.Keys = keyTemplates.ToArray();
                rowTemplates.Add(nRow);
            }

            workingTemplate.PrimaryRows = rowTemplates.ToArray();
            workingTemplate.TemplateName = name;
        }

        public void RefreshKeyboard()
        {
            if (keyPrototype == null)
            {
                Debug.LogError("An attempt to refresh keyboard '" + name + "' failed with error: No Key Prototype present unable to generate keyboard.");
                return;
            }

            if (keyContainer == null)
            {
                Debug.LogWarning("An attempt to refresh keyboard '" + name + "' logged a warning: No Key Container present generating new transform.");
                GameObject nContainer = new GameObject("Key Container", typeof(RectTransform));
                keyContainer = nContainer.GetComponent<RectTransform>();
                keyContainer.SetParent(transform);
                keyContainer.localScale = Vector3.one;
                keyContainer.localEulerAngles = Vector3.zero;
                keyContainer.pivot = new Vector2(0.5f, 0.5f);
                keyContainer.anchoredPosition3D = Vector3.zero;
            }

            List<GameObject> killList = new List<GameObject>();
            for (int c = 0; c < keyContainer.childCount; c++)
            {
                killList.Add(keyContainer.GetChild(c).gameObject);
            }
            foreach (GameObject go in killList)
            {
                if (Application.isEditor)
                    DestroyImmediate(go);
                else
                    Destroy(go);
            }

            if (workingTemplate.HeaderRow != null)
            {
                GameObject headerRow = new GameObject("Header Row", typeof(RectTransform));
                RectTransform headerRowTrans = headerRow.GetComponent<RectTransform>();
                headerRowTransform = headerRowTrans;
                headerRowTrans.SetParent(keyContainer);
                headerRowTrans.localScale = Vector3.one;
                headerRowTrans.pivot = new Vector2(0.5f, 0.5f);
                headerRowTrans.anchoredPosition3D = workingTemplate.HeaderRow.RowOffset;
                headerRowTrans.localEulerAngles = workingTemplate.HeaderRow.RowRotation;

                foreach (Serialization.KeyboardKeyTemplate key in workingTemplate.HeaderRow.Keys)
                {
                    GameObject newKey = Instantiate<GameObject>(keyPrototype.gameObject);
                    newKey.name = (key.Code != KeyCode.None ? key.Code.ToString() : key.DisplayNormal) + " key";
                    KeyboardKey newKeyboardKey = newKey.GetComponent<KeyboardKey>();

                    newKeyboardKey.pressed += keyPressed;
                    newKeyboardKey.keyGlyph.ApplyTemplate(key);
                    if (newKeyboardKey.selfRectTransform == null)
                        newKeyboardKey.selfRectTransform = newKey.GetComponent<RectTransform>();

                    newKeyboardKey.selfRectTransform.SetParent(headerRowTrans);

                    newKeyboardKey.selfRectTransform.anchoredPosition3D = key.KeyOffset;
                    newKeyboardKey.selfRectTransform.localEulerAngles = key.KeyRotation;
                    newKeyboardKey.selfRectTransform.sizeDelta = key.KeySize;
                    newKeyboardKey.selfRectTransform.localScale = Vector3.one;

                    newKeyboardKey.keyGlyph.altGrString = key.AltGr;
                    newKeyboardKey.keyGlyph.shiftedString = key.Shifted;
                    newKeyboardKey.keyGlyph.normalString = key.Normal;
                    newKeyboardKey.keyGlyph.shiftedAltGrString = key.ShiftedAltGr;
                    newKeyboardKey.keyGlyph.code = key.Code;
                    newKeyboardKey.keyType = key.KeyType;

                    if (newKeyboardKey.keyGlyph.altGrDisplay != null)
                        newKeyboardKey.keyGlyph.altGrDisplay.text = key.DisplayAltGr;

                    if (newKeyboardKey.keyGlyph.shiftedDisplay != null)
                        newKeyboardKey.keyGlyph.shiftedDisplay.text = key.DisplayShifted;

                    if (newKeyboardKey.keyGlyph.normalDisplay != null)
                        newKeyboardKey.keyGlyph.normalDisplay.text = key.DisplayNormal;

                    if (newKeyboardKey.keyGlyph.shiftedAltGrDisplay != null)
                        newKeyboardKey.keyGlyph.shiftedAltGrDisplay.text = key.DisplayShiftedAltGr;

                }
            }

            int i = 1;
            rowTransforms.Clear();
            foreach (Serialization.KeyboardTemplateRow row in workingTemplate.PrimaryRows)
            {
                GameObject nRow = new GameObject("Row " + i.ToString(), typeof(RectTransform));
                RectTransform nRowTrans = nRow.GetComponent<RectTransform>();
                rowTransforms.Add(nRowTrans);
                nRowTrans.SetParent(keyContainer);
                nRowTrans.pivot = new Vector2(0.5f, 0.5f);
                nRowTrans.anchoredPosition3D = row.RowOffset;
                nRowTrans.localEulerAngles = row.RowRotation;
                nRowTrans.localScale = Vector3.one;

                foreach (Serialization.KeyboardKeyTemplate key in row.Keys)
                {
                    GameObject newKey = Instantiate<GameObject>(keyPrototype.gameObject);
                    newKey.name = (key.Code != KeyCode.None ? key.Code.ToString() : key.DisplayNormal) + " key";
                    KeyboardKey newKeyboardKey = newKey.GetComponent<KeyboardKey>();

                    newKeyboardKey.pressed += keyPressed;
                    newKeyboardKey.keyGlyph.ApplyTemplate(key);
                    if (newKeyboardKey.selfRectTransform == null)
                        newKeyboardKey.selfRectTransform = newKey.GetComponent<RectTransform>();

                    newKeyboardKey.selfRectTransform.SetParent(nRowTrans);

                    newKeyboardKey.selfRectTransform.anchoredPosition3D = key.KeyOffset;
                    newKeyboardKey.selfRectTransform.localEulerAngles = key.KeyRotation;
                    newKeyboardKey.selfRectTransform.sizeDelta = key.KeySize;
                    newKeyboardKey.selfRectTransform.localScale = Vector3.one;
                    newKeyboardKey.keyType = key.KeyType;

                    newKeyboardKey.keyGlyph.altGrString = key.AltGr;
                    newKeyboardKey.keyGlyph.shiftedString = key.Shifted;
                    newKeyboardKey.keyGlyph.normalString = key.Normal;
                    newKeyboardKey.keyGlyph.shiftedAltGrString = key.ShiftedAltGr;
                    newKeyboardKey.keyGlyph.code = key.Code;

                    if (newKeyboardKey.keyGlyph.altGrDisplay != null)
                        newKeyboardKey.keyGlyph.altGrDisplay.text = key.DisplayAltGr;

                    if (newKeyboardKey.keyGlyph.shiftedDisplay != null)
                        newKeyboardKey.keyGlyph.shiftedDisplay.text = key.DisplayShifted;

                    if (newKeyboardKey.keyGlyph.normalDisplay != null)
                        newKeyboardKey.keyGlyph.normalDisplay.text = key.DisplayNormal;

                    if (newKeyboardKey.keyGlyph.shiftedAltGrDisplay != null)
                        newKeyboardKey.keyGlyph.shiftedAltGrDisplay.text = key.DisplayShiftedAltGr;
                }
                i++;
            }
        }

        private void keyPressed(object sender, KeyboardKey key)
        {
            if (key.keyGlyph.code == KeyCode.CapsLock)
                capsLockToggle = !capsLockToggle;

            if (useShiftToggle &&
                (key.keyGlyph.code == KeyCode.LeftShift || key.keyGlyph.code == KeyCode.RightShift))
                shiftToggle = !shiftToggle;

            if (useAltGrToggle &&
                (key.keyGlyph.code == KeyCode.AltGr))
                altGrToggle = !altGrToggle;

            if (linkedBehaviour != null && field != null)
            {
                string initalValue = GetLinkedFieldValue();
                string resultingValue = key.ToString(initalValue, insertPoint);
                SetLinkedFieldValue(resultingValue);

                if (insertPoint > -1)
                {
                    insertPoint += (resultingValue.Length - initalValue.Length);
                }
            }

            if (KeyboardKeyPressed != null)
                KeyboardKeyPressed(sender, key);
        }


        #region Multi-touch code
        private bool capsLockToggle = false;
        public bool useShiftToggle = false;
        private bool shiftToggle = false;
        public bool useAltGrToggle = false;
        private bool altGrToggle = false;
        private List<KeyboardKey> AltGrKeysHeld = new List<KeyboardKey>();
        private List<KeyboardKey> ShiftKeysHeld = new List<KeyboardKey>();

        private void keyIsDown(object sender, KeyboardKey key)
        {
            //For modifiers consider a down movement a press if the board is not in this state
            if (key.keyType == KeyboardKeyType.Modifier)
            {
                //If we arent in AltGr mode for the AltGr key then press AltGr
                if (key.keyGlyph.code == KeyCode.AltGr && !AltGrKeysHeld.Contains(key))
                {
                    AltGrKeysHeld.Add(key);
                }
                else if ((key.keyGlyph.code == KeyCode.LeftShift || key.keyGlyph.code == KeyCode.RightShift) && !ShiftKeysHeld.Contains(key))
                {
                    ShiftKeysHeld.Add(key);
                }
            }
        }

        private void keyIsUp(object sender, KeyboardKey key)
        {
            //For modifiers consider a down movement a press if the board is not in this state
            if (key.keyType == KeyboardKeyType.Modifier)
            {
                //If we arent in AltGr mode for the AltGr key then press AltGr
                if (key.keyGlyph.code == KeyCode.AltGr && AltGrKeysHeld.Contains(key))
                {
                    AltGrKeysHeld.Remove(key);
                }
                else if ((key.keyGlyph.code == KeyCode.LeftShift || key.keyGlyph.code == KeyCode.RightShift) && ShiftKeysHeld.Contains(key))
                {
                    ShiftKeysHeld.Remove(key);
                }
            }
        }
        #endregion
    }

    public enum KeyboardState
    {
        Normal,
        Shifted,
        AltGr,
        ShiftedAltGr
    }
}
