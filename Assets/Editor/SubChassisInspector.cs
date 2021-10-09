using UnityEngine;
using UnityEditor;
using System.Collections;
using Diluvion;
using Diluvion.Ships;

/*
//[CustomEditor(typeof(SubChassis)), CanEditMultipleObjects]
public class SubChassisInspector : Editor {

    SerializedProperty shipPrefab;
    SerializedProperty shipIcon;
    SerializedProperty skin;
    SerializedProperty cost;
    SerializedProperty DLC;
    SerializedProperty armaments;
    SerializedProperty shipLevel;
    SerializedProperty stationSlots;
    SerializedProperty bonusSlots;
    SerializedProperty chassisBonus;
    SerializedProperty bonusMultiplier;
    SerializedProperty hasUpgrade;
    SerializedProperty upgrade;
    SerializedProperty defaultLoadout;
    SerializedProperty altNames;

    SubChassis subChassis;

    private void OnEnable()
    {
        shipPrefab = serializedObject.FindProperty("shipPrefab");
        shipIcon = serializedObject.FindProperty("shipIcon");
        skin = serializedObject.FindProperty("skin");
        cost = serializedObject.FindProperty("cost");
        DLC = serializedObject.FindProperty("DLC");
        armaments = serializedObject.FindProperty("armaments");
        shipLevel = serializedObject.FindProperty("shipLevel");
        stationSlots = serializedObject.FindProperty("stationSlots");
        bonusSlots = serializedObject.FindProperty("bonusSlots");
        chassisBonus = serializedObject.FindProperty("chassisBonus");
        bonusMultiplier = serializedObject.FindProperty("bonusMultiplier");
        hasUpgrade = serializedObject.FindProperty("hasUpgrade");
        upgrade = serializedObject.FindProperty("upgrade");
        defaultLoadout = serializedObject.FindProperty("defaultLoadout");
        altNames = serializedObject.FindProperty("altNames");

        subChassis = (SubChassis)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(shipPrefab);
        if (shipPrefab.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Enter a ship prefab here to continue.", MessageType.Info);
            return;
        }

        GameObject shipObject = shipPrefab.objectReferenceValue as GameObject;
        EditorGUILayout.PropertyField(skin);

        EditorGUILayout.Space();

        if (GUILayout.Button("Instantiate prefab"))
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(shipObject) as GameObject;
            instance.transform.position = SceneView.lastActiveSceneView.pivot;
            instance.transform.eulerAngles = new Vector3(0, -90, 0);
            instance.name = "ship_" + target.name;
            Skin s = SpiderWeb.GO.MakeComponent<Skin>(instance);
            s.ApplySkin(skin.objectReferenceValue as Material);
            Selection.activeGameObject = instance;
        }
        if (GUILayout.Button("Test NPC instantiation"))
        {
            GameObject instance = subChassis.InstantiateChassis(null, false, defaultLoadout.objectReferenceValue as SubLoadout);
            instance.transform.position = SceneView.lastActiveSceneView.pivot;
            Selection.activeGameObject = instance;
            SceneView.lastActiveSceneView.FrameSelected();
        }

        if (Application.isPlaying)
            if (GUILayout.Button("Test as-player instantiation"))
            {
                subChassis.TestAsPlayer();
                Selection.activeGameObject = PlayerManager.pBridge.gameObject;
                //SceneView.lastActiveSceneView.FrameSelected();
            }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(altNames, true);

        Rect levelPos = EditorGUILayout.GetControlRect();

        IntSlider(levelPos, shipLevel, 1, 5, new GUIContent("Ship Level"));
        EditorGUILayout.PropertyField(shipIcon);
        EditorGUILayout.PropertyField(DLC);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(defaultLoadout);
        EditorGUILayout.PropertyField(cost);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(stationSlots);
        if (shipObject)
        {
            int slots = shipObject.GetComponent<Bridge>().StationSlots().Count;
            if (slots != stationSlots.intValue)
                EditorGUILayout.HelpBox("Actual number of station slots(" + slots + ") in prefab doesn't match intended value!", MessageType.Warning);
        }
        EditorGUILayout.PropertyField(bonusSlots);
        if (shipObject)
        {
            int bonusNum = shipObject.GetComponentsInChildren<BonusSlot>().Length;
            if (bonusNum != bonusSlots.intValue)
                EditorGUILayout.HelpBox("Actual number of bonus slots(" + bonusNum + ") in prefab doesn't match intended value!", MessageType.Warning);

            EditorGUILayout.Space();
            ShipMover s = shipObject.GetComponentInChildren<ShipMover>(true);
            if (s)
            {
                if (!s.engine) EditorGUILayout.HelpBox("Ship prefab is missing an engine!", MessageType.Warning);
            }

            bool mountsReady = true;
            foreach (Mount m in shipObject.GetComponentsInChildren<Mount>(true))
            {
                if (m.weaponModule == null)
                {
                    mountsReady = false;
                    break;
                }
            }
            if (!mountsReady) EditorGUILayout.HelpBox("Ship prefab contains mounts without a linked weapon type!", MessageType.Warning);
        }
        EditorGUILayout.PropertyField(chassisBonus);

        if (chassisBonus.objectReferenceValue != null)
            EditorGUILayout.PropertyField(bonusMultiplier);

        //get armaments
        int oldArmaments = armaments.arraySize;
        subChassis.FindArmaments();
        if (armaments.arraySize != oldArmaments)
            EditorUtility.SetDirty(target);
        EditorGUILayout.PropertyField(armaments, true);

        EditorGUILayout.PropertyField(hasUpgrade);
        if (hasUpgrade.boolValue)
            EditorGUILayout.PropertyField(upgrade);

        serializedObject.ApplyModifiedProperties();

    }

    // A slider function that takes a SerializedProperty
    void IntSlider(Rect position, SerializedProperty property, int leftValue, int rightValue, GUIContent label )
    {
        label = EditorGUI.BeginProperty(position, label, property);

        EditorGUI.BeginChangeCheck();
        int newValue = EditorGUI.IntSlider(position, label, property.intValue, leftValue, rightValue);
        // Only assign the value back if it was actually changed by the user.
        // Otherwise a single value will be assigned to all objects when multi-object editing,
        // even when the user didn't touch the control.
        if (EditorGUI.EndChangeCheck())
            property.intValue = newValue;

        EditorGUI.EndProperty();
    }
}
*/