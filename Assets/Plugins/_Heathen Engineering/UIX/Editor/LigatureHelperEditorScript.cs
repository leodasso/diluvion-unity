using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Xml.Serialization;
using System.IO;
using System;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(HeathenEngineering.UIX.LigatureHelper))]
public class LigatureHelperEditorScript : Editor
{
    public override void OnInspectorGUI()
    {
        HeathenEngineering.UIX.LigatureHelper ligatureHelper = target as HeathenEngineering.UIX.LigatureHelper;
        
        EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
        DoOutputLink(ligatureHelper);
        EditorGUILayout.Space();
        ShowRows(ligatureHelper);

        if (GUI.changed)
            EditorUtility.SetDirty(target); 
    }

    void DoOutputLink(HeathenEngineering.UIX.LigatureHelper ligatureHelper)
    {
        ligatureHelper.linkedGameObject = EditorGUILayout.ObjectField("Linked GameObject", ligatureHelper.linkedGameObject, typeof(GameObject), true) as GameObject;

        if (ligatureHelper.linkedGameObject != null)
        {
            ligatureHelper.ValidateLinkedData();
            List<string> options = new List<string>();
            foreach (Component com in ligatureHelper.linkedBehaviours)
            {
                options.Add(com.GetType().ToString()); 

            }
            int indexOf = ligatureHelper.linkedBehaviours.IndexOf(ligatureHelper.linkedBehaviour);
            int newIndex = EditorGUILayout.Popup("On Behaviour", indexOf, options.ToArray());
            if (indexOf != newIndex)
            {
                ligatureHelper.linkedBehaviour = ligatureHelper.linkedBehaviours[newIndex];
                ligatureHelper.ValidateLinkedData();
                if (ligatureHelper.fields.Count <= 0)
                    return;
            }
            //Debug.Log("Found properties to list");
            indexOf = ligatureHelper.fields.IndexOf(ligatureHelper.field);
            newIndex = EditorGUILayout.Popup("For Property", indexOf, ligatureHelper.fields.ToArray());
            if (newIndex != indexOf)
            {
                ligatureHelper.field = ligatureHelper.fields[newIndex];
                EditorUtility.SetDirty(target); 
            }
        }
    }

    void ShowRows(HeathenEngineering.UIX.LigatureHelper ligatureHelper)
    {
        if (ligatureHelper.map == null)
        {
            ligatureHelper.map = new List<HeathenEngineering.UIX.Serialization.LigatureReference>();
            EditorUtility.SetDirty(ligatureHelper); 
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Map", EditorStyles.boldLabel);
        if(drawButton("+", 15))
        {
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference());
            EditorUtility.SetDirty(ligatureHelper); 
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Add Standard Ligatures");
        EditorGUILayout.BeginHorizontal();
        if (drawButton("Common", 125))
        {
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(C)", "©"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(c)", "©"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(SC)", "℗"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(sc)", "℗"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(Sc)", "℗"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(sC)", "℗"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(TM)", "™"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(tm)", "™"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(Tm)", "™"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(tM)", "™"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(RTM)", "®"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(rtm)", "®"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(rTm)", "®"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(Rtm)", "®"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(RTm)", "®"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(RtM)", "®"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(rTM)", "®"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(rtM)", "®"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(sm)", "℠"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(SM)", "℠"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(Sm)", "℠"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(sM)", "℠"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("`a", "à"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("`A", "À"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("`e", "è"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("`E", "È"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("`i", "ì"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("`I", "Ì"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("`o", "ò"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("`O", "Ò"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("`u", "ù"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("`U", "Ù"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(No)", "№"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(#)", "№"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("(deg)", "°"));
        }
        if (drawButton("Super Script", 125))
        {
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^1", "¹"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^2", "²"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^3", "³"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^4", "⁴"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^5", "⁵"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^6", "⁶"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^7", "⁷"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^8", "⁸"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^9", "⁹"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^0", "⁰"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^+", "⁺"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^-", "⁻"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^=", "⁼"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^(", "⁽"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^)", "⁾"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^a", "ᵃ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^b", "ᵇ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^c", "ᶜ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^d", "ᵈ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^e", "ᵉ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^f", "ᶠ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^g", "ᵍ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^h", "ʰ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^i", "ⁱ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^j", "ʲ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^k", "ᵏ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^l", "ˡ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^m", "ᵐ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^n", "ⁿ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^o", "ᵒ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^p", "ᵖ"));
            //ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^q", "ʳ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^r", "ʳ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^s", "ˢ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^t", "ᵗ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^u", "ᵘ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^v", "ᵛ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^w", "ʷ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^x", "ˣ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^y", "ʸ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^z", "ᶻ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^A", "ᴬ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^B", "ᴮ"));
            //ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^C", ""));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^D", "ᴰ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^E", "ᴱ"));
            //ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^F", ""));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^G", "ᴳ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^H", "ᴴ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^I", "ᴵ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^J", "ᴶ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^K", "ᴷ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^L", "ᴸ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^M", "ᴹ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^N", "ᴺ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^O", "ᴼ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^P", "ᴾ"));
            //ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^Q", ""));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^R", "ᴿ"));
            //ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^S", ""));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^T", "ᵀ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^U", "ᵁ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^V", "ⱽ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^W", "ᵂ"));
            //ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^X", ""));
            //ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^Y", ""));
            //ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("^Z", ""));
        }
        if (drawButton("Fraction", 125))
        {
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("1/4", "¼"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("2/4", "½"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("1/2", "½"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("3/4", "¾"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("1/10", "⅒"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("2/10", "⅕"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("4/10", "⅖"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("5/10", "½"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("6/10", "⅗"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("8/10", "⅘"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("1/3", "⅓"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("2/3", "⅔"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("1/5", "⅕"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("2/5", "⅖"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("3/5", "⅗"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("4/5", "⅘"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("1/6", "⅙"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("2/6", "⅓"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("3/6", "½"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("4/6", "⅔"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("5/6", "⅚"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("1/8", "⅛"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("2/8", "¼"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("3/8", "⅜"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("4/8", "½"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("5/8", "⅝"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("6/8", "¾"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("7/8", "⅞"));
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (drawButton("Katakana カタカナ", 125))
        {
            //A
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ア=", "ァ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ア-", "ァ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ka", "カ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("sa", "サ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ta", "タ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("na", "ナ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ha", "ハ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ma", "マ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ya", "ヤ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ヤ=", "ャ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ヤ-", "ャ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ra", "ラ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("wa", "ワ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ga", "ガ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("za", "ザ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("da", "ダ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ba", "バ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("pa", "パ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ンa", "ナ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("a", "ア"));
            //I
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("イ=", "ィ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("イ-", "ィ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ki", "キ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("shi", "シ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("chi", "チ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ni", "ニ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("hi", "ヒ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("mi", "ミ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ri", "リ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("wi", "ヰ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("gi", "ギ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ji", "ジ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("di", "ヂ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ジ=", "ヂ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ジ-", "ヂ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("bi", "ビ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("pi", "ピ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("i", "イ"));
            //U
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ウ=", "ゥ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ウ-", "ゥ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ku", "ク"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("tス", "ツ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("tsu", "ツ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("su", "ス"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("nu", "ヌ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("fu", "フ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("mu", "ム"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("yu", "ユ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ユ=", "ュ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ユ-", "ュ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ru", "ル"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("gu", "グ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("zu", "ズ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("du", "ヅ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ズ=", "ヅ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ズ-", "ヅ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("bu", "ブ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("bu", "プ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("u", "ウ"));
            //E
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("エ=", "ェ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("エ-", "ェ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ke", "ケ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("se", "セ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("te", "テ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ne", "ネ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("he", "ヘ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("me", "メ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("re", "レ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("we", "ヱ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ge", "ゲ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ze", "ゼ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("de", "デ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("be", "ベ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("pe", "ペ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("e", "エ"));
            //O
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("オ=", "ォ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("オ-", "ォ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ko", "コ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("so", "ソ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("to", "ト"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("no", "ノ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ho", "ホ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("mo", "モ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("yo", "ヨ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ヨ=", "ョ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ヨ-", "ョ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("ro", "ロ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("wo", "ヲ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("go", "ゴ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("zo", "ゾ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("do", "ド"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("bo", "ボ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("po", "ポ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("o", "オ"));
            //Alt
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("v", "ヴ"));
            ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference("n", "ン"));
        }
        if (drawButton("Korean 한국어", 125))
        {
            string[] set1 = { "ㅏ", "ㅐ", "ㅑ", "ㅒ", "ㅓ", "ㅔ", "ㅕ", "ㅖ", "ㅗ", "ㅘ", "ㅙ", "ㅚ", "ㅛ", "ㅜ", "ㅝ", "ㅞ", "ㅟ", "ㅠ", "ㅡ", "ㅢ", "ㅣ" };
            string[] set2 = { "ㄱ", "ㄲ", "ㄴ", "ㄷ", "ㄸ", "ㄹ", "ㅁ", "ㅂ", "ㅃ", "ㅅ", "ㅆ", "ㅇ", "ㅈ", "ㅉ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ" };
            string[][] ligSet = {   new string[]{"가", "까", "나", "다", "따", "라", "마", "바", "빠", "사", "싸", "아", "자", "짜", "차", "카", "타", "파", "하"},
                                    new string[]{"개", "깨", "내", "대", "때", "래", "매", "배", "빼", "새", "쌔", "애", "재", "째", "채", "캐", "태", "패", "해"},
                                    new string[]{"갸", "꺄", "냐", "댜", "땨", "랴", "먀", "뱌", "뺘", "샤", "쌰", "야", "쟈", "쨔", "챠", "캬", "탸", "퍄", "햐"},
                                    new string[]{"걔", "꺠", "냬", "댸", "떄", "럐", "먜", "뱨", "뺴", "섀", "썌", "얘", "쟤", "쨰", "챼", "컈", "턔", "퍠", "햬"},
                                    new string[]{"거", "꺼", "너", "더", "떠", "러", "머", "버", "뻐", "서", "써", "어", "저", "쩌", "처", "커", "터", "퍼", "허"},
                                    new string[]{"게", "께", "네", "데", "떼", "레", "메", "베", "뻬", "세", "쎄", "에", "제", "쩨", "체", "케", "테", "페", "헤"},
                                    new string[]{"겨", "껴", "녀", "뎌", "뗘", "려", "며", "벼", "뼈", "셔", "쎠", "여", "져", "쪄", "쳐", "켜", "텨", "폐", "혀"},
                                    new string[]{"계", "꼐", "녜", "뎨", "뗴", "례", "몌", "볘", "뼤", "셰", "쎼", "예", "졔", "쪠", "쳬", "켸", "톄", "폐", "혜"},
                                    new string[]{"고", "	꼬", "노", "도", "또", "로", "모", "보", "뽀", "소", "쏘", "오", "조", "쪼", "초", "코", "토", "포", "호"},
                                    new string[]{"과", "꽈", "놔", "돠", "똬", "롸", "뫄", "봐", "뽜", "솨", "쏴", "와", "좌", "쫘", "촤", "콰", "톼", "퐈", "화"},
                                    new string[]{"괘", "꽤", "놰", "돼", "뙈", "뢔", "뫠", "봬", "뽸", "쇄", "쐐", "왜", "좨", "쫴", "쵀", "쾌", "퇘", "퐤", "홰"},
                                    new string[]{"괴", "꾀", "뇌", "되", "뙤", "뢰", "뫼", "뵈", "뾔", "쇠", "쐬", "외", "죄", "쬐", "최", "쾨", "퇴", "푀", "회"},
                                    new string[]{"교", "꾜", "뇨", "됴", "뚀", "료", "묘", "뵤", "뾰", "쇼", "쑈", "요", "죠", "쬬", "쵸", "쿄", "툐", "표", "효"},
                                    new string[]{"구", "꾸", "누", "두", "뚜", "루", "무", "부", "뿌", "수", "쑤", "우", "주", "쭈", "추", "쿠", "투", "푸", "후"},
                                    new string[]{"궈", "꿔", "눠", "둬", "뚸", "뤄", "뭐", "붜", "뿨", "숴", "쒀", "워", "줘", "쭤", "춰", "쿼", "퉈", "풔", "훠"},
                                    new string[]{"궤", "꿰", "눼", "뒈", "뛔", "뤠", "뭬", "붸", "쀄", "쉐", "쒜", "웨", "줴", "쮀", "췌", "퀘", "퉤", "풰", "훼"},
                                    new string[]{"귀", "뀌", "뉘", "뒤", "뛰", "뤼", "뮈", "뷔", "쀠", "쉬", "쒸", "위", "쥐", "쮜", "취", "퀴", "튀", "퓌", "휘"},
                                    new string[]{"규", "뀨", "뉴", "듀", "뜌", "류", "뮤", "뷰", "쀼", "슈", "쓔", "유", "쥬", "쮸", "츄", "큐", "튜", "퓨", "휴"},
                                    new string[]{"그", "끄", "느", "드", "뜨", "르", "므", "브", "쁘", "스", "쓰", "으", "즈", "쯔", "츠", "크", "트", "프", "흐"},
                                    new string[]{"긔", "끠", "늬", "듸", "띄", "릐", "믜", "븨", "쁴", "싀", "씌", "의", "즤", "쯰", "츼", "킈", "틔", "픠", "희"},
                                    new string[]{"기", "끼", "니", "디", "띠", "리", "미", "비", "삐", "시", "씨", "이", "지", "찌", "치", "키", "티", "피", "히" }};

            for (int i = 0; i < set2.Length; i++)
            {
                for (int ii = 0; ii < set1.Length; ii++)
                {
                    ligatureHelper.map.Add(new HeathenEngineering.UIX.Serialization.LigatureReference(set2[i] + set1[ii], ligSet[ii][i]));
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("User String  -->  Ligature");
        List<HeathenEngineering.UIX.Serialization.LigatureReference> toRemove = new List<HeathenEngineering.UIX.Serialization.LigatureReference>();
        foreach(HeathenEngineering.UIX.Serialization.LigatureReference lig in ligatureHelper.map)
        {
            EditorGUILayout.BeginHorizontal();
            if (drawButton("-", 15))
            {
                toRemove.Add(lig);
                EditorUtility.SetDirty(ligatureHelper); 
            }
            lig.Characters = EditorGUILayout.TextField(GUIContent.none, lig.Characters, GUILayout.Width(70f));
            EditorGUILayout.LabelField(" --> ", GUILayout.Width(32f));
            lig.Ligature = EditorGUILayout.TextField(GUIContent.none, lig.Ligature, GUILayout.Width(70f));
            EditorGUILayout.EndHorizontal();
        }
        foreach (HeathenEngineering.UIX.Serialization.LigatureReference lig in toRemove)
        {
            if (ligatureHelper.map.Contains(lig))
                ligatureHelper.map.Remove(lig);
        }
    }

    bool drawButton(string label, float width)
    {
        Rect r = EditorGUILayout.BeginHorizontal("Button", GUILayout.Width(width));
        if (GUI.Button(r, GUIContent.none))
            return true;
        GUILayout.Label(label);
        EditorGUILayout.EndHorizontal();
        return false;
    }

    bool drawButton(string label)
    {
        Rect r = EditorGUILayout.BeginHorizontal("Button");
        if (GUI.Button(r, GUIContent.none))
            return true;
        GUILayout.Label(label);
        EditorGUILayout.EndHorizontal();
        return false;
    }

}

