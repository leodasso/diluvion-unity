using UnityEngine;
using UnityEditor;
using Loot;

/*
[CustomPreview(typeof(DItem))]
public class CustomItemPreview : ObjectPreview {

    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        SerializedObject newObj = new SerializedObject(target);
        Sprite sprite = newObj.FindProperty("icon").objectReferenceValue as Sprite;

        Texture2D preview = AssetPreview.GetAssetPreview(sprite);
        GUI.DrawTexture(r, preview, ScaleMode.ScaleToFit);

        if (newObj.FindProperty("keyItem").boolValue)
            EditorGUI.LabelField(r, "key item");
    }
}

[CustomPreview(typeof(DItemCaptainTime))]
public class CustomCaptTimeItemPreview : CustomItemPreview {}

[CustomPreview(typeof(DItemDecal))]
public class CustomDecalItemPreview : CustomItemPreview { }

[CustomPreview(typeof(DItemMap))]
public class CustomMapItemPreview : CustomItemPreview { }

[CustomPreview(typeof(DItemRepair))]
public class CustomRepairItemPreview : CustomItemPreview { }

[CustomPreview(typeof(DItemStatBuff))]
public class CustomBuffItemPreview : CustomItemPreview { }

[CustomPreview(typeof(DItemWeapon))]
public class CustomWeapItemPreview : CustomItemPreview { }
*/