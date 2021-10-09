using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Diluvion;
using CharacterInfo = Diluvion.CharacterInfo;

/*

[CustomEditor(typeof(Convo))]
public class ConversationInspector : Editor {

    public static bool otherCrap;

    Convo convo;
    Speech speechToRemove = null;

    SerializedProperty postConvoActions;
    SerializedProperty convoQueries;
    SerializedProperty readTheseFirst;
    SerializedProperty triggerQuest;
    SerializedProperty triggerObjective;
    SerializedProperty dontLog;
    SerializedProperty questToStart;
    SerializedProperty tQuestStatus;
    SerializedProperty tObjStatus;
    SerializedProperty manualAdd;


    public void OnEnable()
    {
        convo = (Convo)target;

        convoQueries = serializedObject.FindProperty("conversationQueries");
        postConvoActions = serializedObject.FindProperty("postConvoActions");
        readTheseFirst = serializedObject.FindProperty("readTheseFirst");
        triggerQuest = serializedObject.FindProperty("triggeringQuest");
        triggerObjective = serializedObject.FindProperty("triggeringObjective");
        dontLog = serializedObject.FindProperty("dontLog");
        questToStart = serializedObject.FindProperty("questToStart");
        tQuestStatus = serializedObject.FindProperty("triggeringQuestStatus");
        tObjStatus = serializedObject.FindProperty("triggeringObjectiveStatus");
        manualAdd = serializedObject.FindProperty("manualAdd");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //set color
        Color bgColor = Color.white;
        if (convo.urgent) bgColor = Color.green;

        string showsChatter = " [shows chatter]";
        if (!convo.showChatter) showsChatter = "";

        EditorGUILayout.BeginHorizontal();
        convo.active = EditorGUILayout.Toggle(convo.active, GUILayout.MaxWidth(20));
        if (convo.active)
            EditorGUILayout.LabelField(convo.titleText + showsChatter, EditorStyles.boldLabel);
        else EditorGUILayout.LabelField(convo.titleText + showsChatter);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(manualAdd);
        EditorGUILayout.PropertyField(dontLog);
        EditorGUILayout.PropertyField(triggerQuest);
        if (triggerQuest.objectReferenceValue != null)
        {
            EditorGUILayout.PropertyField(tQuestStatus);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(triggerObjective);
            if (triggerObjective.objectReferenceValue != null)
                EditorGUILayout.PropertyField(tObjStatus);
        }

        EditorGUILayout.PropertyField(convoQueries, true);
        EditorGUILayout.PropertyField(readTheseFirst, true);

        EditorGUILayout.BeginHorizontal();

        //selecting if urgent or not
        GUIContent urgentLabel = new GUIContent("Urgent", "Urgent dialogue will have the character call attention to themselves.  If character" +
            "is on your ship and you're 3D view, he will send a notification.");
        EditorGUILayout.LabelField(urgentLabel, GUILayout.MaxWidth(70));
        convo.urgent = EditorGUILayout.Toggle(convo.urgent, GUILayout.MaxWidth(10));

        //Is this dialogue trigger only?
        EditorGUILayout.LabelField("      Triggers only", GUILayout.MaxWidth(140));
        convo.triggerOnly = EditorGUILayout.Toggle(convo.triggerOnly, GUILayout.MaxWidth(10));

        //Does it show chatter?
        EditorGUILayout.LabelField("      Shows Chatter", GUILayout.MaxWidth(120));
        convo.showChatter = EditorGUILayout.Toggle(convo.showChatter, GUILayout.MaxWidth(10));


        EditorGUILayout.EndHorizontal();


        //Dialogue triggers
        if (convo.triggerOnly)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Triggers:", "Separate triggers with a space"), GUILayout.MaxWidth(100));
            convo.textTriggers = EditorGUILayout.TextField(convo.textTriggers, GUILayout.MinWidth(100));
            EditorGUILayout.EndHorizontal();
        }

        //Cooldown
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Chatter cooldown: (sec) ", GUILayout.MaxWidth(140));
        convo.cooldown = EditorGUILayout.Slider(convo.cooldown, 0, 200, GUILayout.MaxWidth(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();


        //Dialogue title
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Chatter ", GUILayout.MaxWidth(100));
        convo.titleText = EditorGUILayout.TextField(convo.titleText, GUILayout.MinWidth(100));
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Player question", GUILayout.MaxWidth(100));
        convo.question = EditorGUILayout.TextField(convo.question, GUILayout.MinWidth(100));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        bool addingSpeech = false;
        int newSpeechIndex = 0;
        int i = 0;

        //Show 'insert speech' button 
        if (GUILayout.Button("Insert Speech"))
        {
            newSpeechIndex = 0;
            addingSpeech = true;
        }


        //Display the inspector for each speech
        foreach (Speech speech in convo.speeches)
        {
            DisplaySpeech(speech, i, convo.locKey);

            //Show 'insert speech' button after each speech 
            if (GUILayout.Button("Insert Speech"))
            {
                newSpeechIndex = i + 1;
                addingSpeech = true;
            }

            i++;
        }

        //add a new speech
        if (addingSpeech)
        {
            Speech newSpeech = new Speech();
            convo.speeches.Insert(newSpeechIndex, newSpeech);
        }

        EditorGUILayout.Space();

        // Post convo actions
        EditorGUILayout.PropertyField(questToStart);
        EditorGUILayout.PropertyField(postConvoActions, true);

        otherCrap = EditorGUILayout.Foldout(otherCrap, "other crap");

        #region other crap
        if (otherCrap)
        {
            // If convo starts quest
            convo.startsQuest = EditorGUILayout.ToggleLeft("Starts quest", convo.startsQuest);

            if (convo.startsQuest) EditorGUILayout.LabelField(convo.questLocKey);

            // If convo completes a quest
            convo.completesQuest = EditorGUILayout.ToggleLeft("completes quest", convo.completesQuest);

            if (convo.completesQuest) EditorGUILayout.LabelField(convo.completeQuestKey);

            convo.requireState = EditorGUILayout.ToggleLeft("Require Story State", convo.requireState);
            if (convo.requireState) convo.requiredState = (StoryState)EditorGUILayout.EnumPopup(convo.requiredState);

            convo.addGuestOnShip = EditorGUILayout.ToggleLeft("Add as Guest", convo.addGuestOnShip);
        }
        #endregion

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(target);

        convo.speeches.Remove(speechToRemove);

        speechToRemove = null;
    }


    void DisplaySpeech(Speech speech, int index, string convoLocKey)
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("Speech " + index.ToString());

        EditorGUI.indentLevel += 2;

        if (speech.animTool == null) speech.animTool = new CrewAnimationTool();

        DisplayAnimationTool(speech.animTool);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Subject", GUILayout.MaxWidth(100));

        speech.speaker = (CharacterInfo)EditorGUILayout.ObjectField(speech.speaker, typeof(CharacterInfo), false);
        EditorGUILayout.EndHorizontal();

        //Speech text
        EditorStyles.textField.wordWrap = true;
        speech.text = EditorGUILayout.TextField(speech.text, GUILayout.MinHeight(70));

        EditorGUILayout.Space();

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Remove Speech", GUILayout.MaxWidth(150)))
            speechToRemove = speech;

        GUI.backgroundColor = Color.white;

        EditorGUI.indentLevel -= 2;

        EditorGUILayout.Space();

        EditorGUILayout.EndVertical();

        GUI.Box(GUILayoutUtility.GetLastRect(), "");

        EditorGUILayout.Space();
    }


    void DisplayAnimationTool(CrewAnimationTool animTool)
    {
        animTool.animationType = (AnimationType)EditorGUILayout.EnumPopup("animation type:", animTool.animationType);

        // Display options
        if (animTool.animationType != AnimationType.none)
        {

            animTool.animTag = EditorGUILayout.TextField("animation trigger:", animTool.animTag);
            animTool.animTime = EditorGUILayout.FloatField("duration:", animTool.animTime);
            animTool.setAsDefault = EditorGUILayout.ToggleLeft("Set as default:", animTool.setAsDefault);
        }
    }
}
*/