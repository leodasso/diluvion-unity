using UnityEngine;
using UnityEditor;
using System.Collections;
using Diluvion.Roll;

[CustomPreview(typeof(ExplorableEntry))]
public class SpawnableEntryPreview : ObjectPreview
{

    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        ExplorableEntry spawn = (ExplorableEntry)target;
   

        if (spawn == null) return;
        if (spawn.prefab == null) return;
        // Debug.Log("Have the target spawn");  
        // Debug.Log("Have the target spawnPrefab" + spawn.prefab);

        Texture2D preview = AssetPreview.GetAssetPreview(spawn.prefab.gameObject);
        if (!AssetPreview.IsLoadingAssetPreview(spawn.GetInstanceID()))
        {

            Rect rect1 = new Rect(r.x, r.y, r.width, r.height);


            GUI.DrawTexture(rect1, preview, ScaleMode.ScaleAndCrop);
        }
        //else
           // GUILayout.Label("LOADING PREVIEW");
        
    }
}
