using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using Diluvion.Roll;




[CustomEditor(typeof(Table))]
[CanEditMultipleObjects]
public class RollTableEditor : Editor
{
    List<TableEntry> rollTable;
    SerializedProperty newEntryTable;
    SerializedProperty lastRolled;
    SerializedProperty totalWeight;
    SerializedProperty entryArraySize;
    SerializedProperty rollArraySize;
    SerializedProperty addEntry;
    SerializedProperty minDanger;
    SerializedProperty minValue;
    Table targetTable;

  
    MethodInfo totalWeightMethod;
    MethodInfo dangerCalculation;
    MethodInfo valueCalculation;
    MethodInfo rollMethod;
    Object targetObj;
    bool showRollElements;
    bool sizeChanged;
    int oldSize;
    string containedTypeString = "Types: ";


    void InvokeDanger()
    {
        if (dangerCalculation != null) dangerCalculation.Invoke(targetObj, null);
        dangerCalculation = targetObj.GetType().GetMethod("RecalcDanger"); 
        dangerCalculation.Invoke(targetObj, null);
    }

    
    void InvokeValue()
    {
        if (valueCalculation != null) valueCalculation.Invoke(targetObj, null);
        valueCalculation = targetObj.GetType().GetMethod("RecalcValue"); 
        valueCalculation.Invoke(targetObj, null);
    }

    
    void OnEnable()
    {
        newEntryTable = serializedObject.FindProperty("tableEntries");
        targetTable = (Table)target;
        rollTable = targetTable.rollTable;
        lastRolled = serializedObject.FindProperty("lastRolledObject");
        addEntry = serializedObject.FindProperty("entryToAdd");
        totalWeight = serializedObject.FindProperty("_totalWeight");
        minDanger = serializedObject.FindProperty("minDanger");
        minValue= serializedObject.FindProperty("minValue");
        //Cache the object and totalWeight recalc method
        targetObj = serializedObject.targetObject;     
        totalWeightMethod = targetObj.GetType().GetMethod("TotWeight"); // Get the total weight
        TotalWeight();
        entryArraySize = newEntryTable.FindPropertyRelative("Array.size");
      
        targetTable.GetUniqueTypes();
        MakeTypeList();

        rollMethod = targetObj.GetType().GetMethod("TestRoll");
    }

    #region typeFilters
    /// <summary>
    /// Updates the ContainedTypesString to fit all the contained types
    /// </summary>
    void MakeTypeList()
    {
       // Debug.Log("remaking Type list");
        targetTable.typeList.Clear();
        //containedTypeString = "Contained Types: ";
        int typeCount = targetTable.containedTypes.Count;

        for(int i = 0; i < typeCount; i++)
        {
            SerializableSystemType t = targetTable.containedTypes[i];
         
            targetTable.typeList.Add(new TypeToggle(t));          
        }        
    }

    /// <summary>
    /// Safely resizes the list without losing the current toggles
    /// </summary>
    void ResizeTypeList()
    {
        targetTable.GetUniqueTypes();
        //Debug.Log("resizing Type list");
        List<TypeToggle> ttremoveList = new List<TypeToggle>();
        
        foreach (TypeToggle tt in targetTable.typeList)
            if (!targetTable.ContainsType(tt.type))
                ttremoveList.Add(tt);
        
        foreach (SerializableSystemType t in targetTable.containedTypes)
        {
            bool missingType = true;
            foreach (TypeToggle tt in targetTable.typeList)
                if (tt.type== t)
                    missingType = false;
            if (missingType)
                targetTable.typeList.Add(new TypeToggle(t));
        }

        foreach (TypeToggle tt in ttremoveList)
            targetTable.typeList.Remove(tt);
    }

    int filterCount = 0;
    int oldFilterCount =0;
    bool showTypeFilter = false;
    /// <summary>
    /// Draws the filter toggles, and rerolls the totalweight with the remaining entries
    /// </summary>
    void DrawTypeFilters()
    {
        showTypeFilter = EditorGUILayout.Foldout(showTypeFilter, "Table Types" );
        if (!showTypeFilter) return;
        ResizeTypeList();
        EditorGUILayout.BeginVertical();
        {         
            filterCount = 0;
            int totalCount = 0;

            EditorGUILayout.BeginHorizontal();
            {
                foreach (TypeToggle t in targetTable.typeList)
                {
                    totalCount++;
                    DrawTypeFilter(t);
                    if (totalCount % 4 == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (oldFilterCount != filterCount)//only recalculate the weight if we changed the filter
            {
                oldFilterCount = filterCount;
                TotalWeight();
            }
        }
        EditorGUILayout.EndVertical();

        GUI.Box(GUILayoutUtility.GetLastRect(), "");
    }

    /// <summary>
    /// Draws a single Type Toggle
    /// </summary>
    /// <param name="t"></param>
    void DrawTypeFilter(TypeToggle t)
    {
        if (t == null) return;
        t.toggled = GUILayout.Toggle(t.toggled, t.type.Name);
        if (t.toggled)
            filterCount++;
    }    

    /// <summary>
    /// Checks the filter list to see if any of them match the entry's type
    /// </summary>
    /// <param name="entries"></param>
    /// <returns></returns>
    bool Filtered(Entry e)
    {
        foreach (TypeToggle tt in targetTable.typeList)
        {
            if (tt.toggled)
            {
                Debug.Log("Comparing: " + tt.type.SystemType.ToString() + " / " + e.GetType().ToString());
                if (tt.type.SystemType == e.GetType())
                    return true;
            }
        }
        return false;
    }

    #endregion


    bool readyToPaint = false;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (newEntryTable == null) return;

        // EditorGUILayout.LabelField(lastRolled.name);
        //EditorGUILayout.LabelField(containedTypeString);
        DrawTypeFilters();

        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Roll"))
                rollMethod.Invoke(targetObj, null);
            if (GUILayout.Button("Roll 100 times"))
            {
                for (int i = 0; i < 100; i++)
                    rollMethod.Invoke(targetObj, null);
            }

            if (GUILayout.Button("ResetRoll#"))
                targetTable.ResetRollCounter();
        }
        EditorGUILayout.EndHorizontal();
       
        EditorGUI.BeginChangeCheck();
        {
            EditorGUILayout.LabelField("Drag and drop Entry objects onto the Roll Table to add them. Enforces unique objects.");
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(Screen.width / 3));
            {
                EditorGUILayout.LabelField("Add One Entry>>", GUILayout.MaxWidth(Screen.width / 6));
                EditorGUILayout.PropertyField(addEntry, GUIContent.none, GUILayout.MaxWidth(Screen.width / 6));
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Minimum Danger: " +minDanger.intValue);
            EditorGUILayout.LabelField("Minimum Value: " +minValue.intValue);
            EditorGUILayout.EndHorizontal();
            
            GUI.Box(GUILayoutUtility.GetLastRect(), "");
            EditorGUI.indentLevel++;
            showRollElements = EditorGUILayout.PropertyField(newEntryTable, new GUIContent("Roll Table"));// EditorGUILayout.Foldout(showRollElements, "Roll Table");
            GUI.Box(GUILayoutUtility.GetLastRect(), "");
            
            if (totalWeight != null)
                EditorGUILayout.LabelField("Total Weight: " + totalWeight.intValue);
            EditorGUI.indentLevel--;
        }
        
        
        if (EditorGUI.EndChangeCheck())        
            readyToPaint = false;
        
        if (addEntry.objectReferenceValue != null)
            targetTable.tableEntries.Add((Entry)addEntry.objectReferenceValue);

        if (entryArraySize.intValue != rollTable.Count)
        {           
            targetTable.ResizeTable();
            
            addEntry.objectReferenceValue = null;
            readyToPaint = false;
        }

        if (readyToPaint == false && Event.current.type == EventType.Layout)
        {
            TotalWeight();
            ResizeTypeList();
            readyToPaint = true;
        }
        
        EditorGUI.BeginChangeCheck();

        if (showRollElements)
        {            
            if (readyToPaint) 
                DrawRollElements(targetTable.TypeFilteredList(rollTable));            
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }

   

    /// <summary>
    /// Draws the roll element list
    /// </summary>
    void DrawRollElements(List<TableEntry> tableToDraw)
    {        
        EditorGUILayout.BeginVertical(GUILayout.MinWidth(TotalElementWidth().x), GUILayout.MaxWidth(Screen.width));
        {
            EditorGUI.BeginChangeCheck();//Check for changes in array to recalculate the total weight
            {
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("W:", GUILayout.MinWidth(elementIconLabel.x), GUILayout.MaxWidth(elementIconLabel.y ));
                    EditorGUILayout.LabelField("Base", GUILayout.MinWidth(weightField.x), GUILayout.MaxWidth(weightField.y));
                    EditorGUILayout.LabelField("Table ", GUILayout.MinWidth(weightField.x - 10), GUILayout.MaxWidth(weightField.y));
                    EditorGUILayout.LabelField("Chance", GUILayout.MinWidth(percentageField.x), GUILayout.MaxWidth(percentageField.y));
                    EditorGUILayout.LabelField("Will Drop#", GUILayout.MinWidth(willDropField.x), GUILayout.MaxWidth(willDropField.y));
                    EditorGUILayout.LabelField("Drop #", GUILayout.MinWidth(percentageField.x), GUILayout.MaxWidth(percentageField.y));
                    EditorGUILayout.LabelField("Value", GUILayout.MinWidth(percentageField.x), GUILayout.MaxWidth(percentageField.y));
                    EditorGUILayout.LabelField("Total Spent", GUILayout.MinWidth(percentageField.x), GUILayout.MaxWidth(percentageField.y*2));
                }
                EditorGUILayout.EndHorizontal();

                foreach (TableEntry e in tableToDraw)
                    DrawRollElement(e);
                EditorGUILayout.Space();
            }
            if (EditorGUI.EndChangeCheck())//If something changed in our loot table, recalculate total weight        
                TotalWeight();

            SafeRemoveButtons();
        }
        EditorGUILayout.EndVertical();

        EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), new Color(0.1f, 0.1f, 0.1f, 0.1f));
    }


    #region rollElementVariables
    readonly Vector2 elementIconLabel = new Vector2(40, 40);
    readonly Vector2 elementNameLabel = new Vector2(50, 200);
    readonly Vector2 weightField = new Vector2(30,40);
    readonly Vector2 willDropField = new Vector2(60, 80);
    readonly Vector2 percentageField = new Vector2(30, 50);
    readonly Vector2 propertyField = new Vector2(40, 40);
    readonly Vector2 buttonField = new Vector2(40, 40);

    /// <summary>
    /// Amalgam of all the input elements' width
    /// </summary>
    /// <returns></returns>
    Vector2 TotalElementWidth()
    {
        return elementNameLabel + weightField + willDropField +percentageField + propertyField + buttonField;
    }

    #endregion
    /// <summary>
    /// Draws a Roll Object with a link to its object, the weight and the percentage chance of appearing in this loot table
    /// </summary>    
    void DrawRollElement(TableEntry rollObject)
    {
        if (rollObject == null) return;
        if (rollObject.entry == null) { entriesToRemove.Add(rollObject); return; }
        if (totalWeight == null) return;

        int propertyWeight = rollObject.TotalWeight;

        double percentage = System.Math.Round((propertyWeight * 1.0f / totalWeight.intValue * 1.0f * 100f), 2);
       
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Box(new GUIContent(EditorGUIUtility.ObjectContent(rollObject.entry, rollObject.entry.GetType()).image), GUILayout.MinWidth(elementIconLabel.x), GUILayout.MaxWidth(elementIconLabel.y), GUILayout.MinHeight(elementIconLabel.x));
            //EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(),new Color(0.1f, 0.1f, 0.1f, 0.1f));
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.ObjectField(rollObject.entry, rollObject.entry.GetType(), false, GUILayout.MinWidth(rollObject.entry.name.Length * 5));
                    RemoveButton(rollObject);
                    // GUI.Box(GUILayoutUtility.GetLastRect(), "");
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {                  
                    EditorGUILayout.LabelField(rollObject.entry.weight.ToString(), GUILayout.MinWidth(weightField.x), GUILayout.MaxWidth(weightField.y));
                    rollObject.tableBonus = EditorGUILayout.IntField(rollObject.tableBonus, GUILayout.MinWidth(weightField.x), GUILayout.MaxWidth(weightField.y));
                    EditorGUILayout.LabelField(percentage.ToString() + "%", GUILayout.MinWidth(percentageField.x), GUILayout.MaxWidth(percentageField.y));
                    rollObject.guaranteeDrop = EditorGUILayout.IntField(rollObject.guaranteeDrop, GUILayout.MinWidth(willDropField.x), GUILayout.MaxWidth(willDropField.y));
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField(rollObject.dropTimes.ToString(), GUILayout.MinWidth(weightField.x), GUILayout.MaxWidth(weightField.y));
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField(rollObject.entry.Value().ToString(), GUILayout.MinWidth(weightField.x), GUILayout.MaxWidth(weightField.y));
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField(rollObject.totalResourcesSpent.ToString(), GUILayout.MinWidth(willDropField.x), GUILayout.MaxWidth(willDropField.y));
                   
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();

        Color propertyColor = ColorElement(rollObject);

        EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), propertyColor);
        
    }

    List<TableEntry> entriesToRemove = new List<TableEntry>();
    /// <summary>
    /// Removes the EntriesToRemove which has been populated by various functions at the end of the Inspector's update
    /// </summary>
    void SafeRemoveButtons()
    {
        if (entriesToRemove.Count < 1) return;
        foreach(TableEntry te in entriesToRemove)
            targetTable.RemoveRoll(te);
        ResizeEntryTable();
     
        entriesToRemove.Clear();
        
        EditorUtility.SetDirty(targetTable);
    }

    /// <summary>
    /// Draws the button that removes the element
    /// </summary>
    /// <param name="te"></param>
    void RemoveButton(TableEntry te)
    {
        if (!GUILayout.Button("X", GUILayout.Width(buttonField.x))) return;
        entriesToRemove.Add(te);
        readyToPaint = false;

    }

    /// <summary>
    /// Resizes the Table whenever we Delete something from the array
    /// </summary>
    void ResizeEntryTable()
    {
      
        SerializedProperty entryCopy = newEntryTable.Copy();

        //newEntryTable.ClearArray();
        int nullCount = 0;
        int newListCount = 0;
        for (int i = 0; i < entryArraySize.intValue; i++)
        {
            SerializedProperty entryCopyObj = entryCopy.GetArrayElementAtIndex(i);
            // SerializedObject rollObject = new SerializedObject(listObj.objectReferenceValue);
            // rollObject.Update();           

            if (entryCopyObj.objectReferenceValue == null) { nullCount++; continue; }
            //Debug.Log("List object: " + listObj.objectReferenceValue);
            newEntryTable.GetArrayElementAtIndex(newListCount).objectReferenceValue = entryCopyObj.objectReferenceValue;
            newListCount++;
        }
        entryArraySize.intValue -= nullCount;
        //entryListObject.ApplyModifiedProperties();

    }

    /// <summary>
    /// Calls to reroll the totalWeight
    /// </summary>
    void TotalWeight()
    {
        if (totalWeightMethod == null) return;
        if (targetObj == null) return;
        totalWeightMethod.Invoke(targetObj, null);
        InvokeDanger();
        InvokeValue();
    }


    //Colors the element if it has been rolled
    Color ColorElement(TableEntry property)
    {
        if (lastRolled == null|| property == null|| property.entry==null|| property.entry!= (Entry)lastRolled.objectReferenceValue)
            return new Color(0.1f, 0.1f, 0.1f, 0.1f);      
        else
            return new Color(0.1f, 1f, 0.1f, 0.1f);

    }
}
