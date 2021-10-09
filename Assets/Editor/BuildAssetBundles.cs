using UnityEditor;
using UnityEngine;
public class BuildAssetBundles
{

	[MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        Debug.Log("active build target : " +  EditorUserBuildSettings.activeBuildTarget);
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles", BuildAssetBundleOptions.UncompressedAssetBundle, EditorUserBuildSettings.activeBuildTarget);
    }
}
