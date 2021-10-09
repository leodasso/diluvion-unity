using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Diluvion.AI;
using Diluvion.Sonar;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A Faction is a collection of attitudes against signatures, will always add the signature to its adherent
/// </summary>
[CreateAssetMenu(fileName = "new Faction", menuName = "Diluvion/AI/Faction", order = 0)]
public class Faction : ScriptableObject 
{
	public List<SignatureValues> signatureRelations = new List<SignatureValues>();
	
    [InfoBox("Add a signature that corresponds to this faction", "InstanceShowParameterFunction")]
	public Signature factionSignature;
    
     
    #if UNITY_EDITOR
    [Button, ShowIf("InstanceShowParameterFunction")]
    void CreateFactionSignature()
    {
        string path = "Assets/Prefabs/Sonar/" + name + ".asset";
        Signature newSig = (Signature)AssetDatabase.LoadAssetAtPath(path, typeof(Signature));

        if (newSig == null)
        {
            newSig = CreateInstance<Signature>();
            newSig.faction = true;
            AssetDatabase.CreateAsset(newSig,path);
            AssetDatabase.SaveAssets();
        }
        factionSignature = newSig;
        signatureRelations.Add(new SignatureValues(factionSignature, 5));
    }
    #endif
    
    public bool InstanceShowParameterFunction()
    {
        return factionSignature == null;
    }
    
   
    Dictionary<ContextTarget, float> cachedRelations = new Dictionary<ContextTarget, float>();
    private List<ContextTarget> unInterestingCache= new List<ContextTarget>();
    
   
}
