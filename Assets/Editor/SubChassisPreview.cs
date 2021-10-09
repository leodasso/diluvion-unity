using UnityEngine;
using UnityEditor;
using System.Collections;
using Diluvion.Ships;

//[CustomPreview(typeof(SubChassis))]
public class SubChassisPreview : ObjectPreview {

    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        SubChassis chassis = (SubChassis)target;
        if (chassis == null) return;
        if (chassis.shipPrefab == null) return;

        Texture2D preview = AssetPreview.GetAssetPreview(chassis.shipPrefab);

        Rect rect1 = new Rect(r.x, r.y, r.width, r.height);
        Rect rect2 = new Rect(r.x, r.y + r.height * .6f, r.width, r.height * .4f);
        Rect rect3 = new Rect(r.x, rect2.y, r.width / 2, rect2.height);
        Rect rect4 = new Rect(r.x + r.width / 2, rect2.y, r.width / 2, rect2.height);

        Rect labelRect = new Rect(r.x, r.y, r.width, 16);
        Rect upgradeLabel = new Rect(rect4.x, rect4.y, rect4.width, 16);

        GUI.DrawTexture(rect1, preview, ScaleMode.ScaleAndCrop);

        GUI.Label(labelRect, "Level " + chassis.shipLevel);

        if (chassis.shipIcon == null) return;

        Texture2D iconPreview = AssetPreview.GetAssetPreview(chassis.shipIcon);

        if (chassis.hasUpgrade)
        {
            Texture2D upgradePreview = new Texture2D(1, 1);
            upgradePreview.SetPixel(0, 0, Color.red);
            upgradePreview.Apply();
            if (chassis.upgrade != null) 
                upgradePreview = AssetPreview.GetAssetPreview(chassis.upgrade.shipIcon);

            GUI.DrawTexture(rect3, iconPreview, ScaleMode.ScaleToFit);
            GUI.DrawTexture(rect4, upgradePreview, ScaleMode.ScaleToFit);
            GUI.Label(upgradeLabel, "upgrades to:");
        }
        else
            GUI.DrawTexture(rect2, iconPreview, ScaleMode.ScaleToFit);
    }
}
