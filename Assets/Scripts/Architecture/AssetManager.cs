using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Diluvion;
using Diluvion.Ships;
using Diluvion.SaveLoad;

public class AssetManager : MonoBehaviour
{
    [SerializeField]
    List<SubChassis> dlcShips = new List<SubChassis>();
    /// <summary>
    /// List of DLC Ships
    /// </summary>
    public List<SubChassis> DLCShips
    {
        get
        {
            return dlcShips;
        }
        private set
        {
            dlcShips = value;
        }
    }

    public bool loadingships = true;

    public static AssetManager assetManager;  

    public static AssetManager Get()
    {
        if (assetManager != null) return assetManager;
        assetManager = FindObjectOfType<AssetManager>();
        if (assetManager == null)
        {
            GameObject assman = new GameObject("AssetManager");
            assetManager = assman.AddComponent<AssetManager>();
        }
        return assetManager;
    }

    /// <summary>
    /// Returns a list of DLC ships that are not in the input list
    /// </summary>
    /// <param name="currentShips"></param>
    /// <returns></returns>
    public List<SubChassis> UniqueDLCShips(List<SubChassis> currentShips)
    {
        List<SubChassis> returnList = new List<SubChassis>(dlcShips);
        foreach (SubChassis sc in currentShips)
        {
            if (!returnList.Contains(sc)) continue;            
            returnList.Remove(sc);         
        }
        return returnList;
    }

    public SubChassis SavedShipInDLC(ShipSave ss)
    {
        if (ss == null) return null;
        if (dlcShips.Count < 1) return null;       
        foreach(SubChassis lsd in dlcShips)
            if(ss.currentShipType==lsd.name)            
                return lsd;            
        return null;
    }

    public string BundleDirectory()
    {
        string returnPath = Application.streamingAssetsPath;
        if (!Directory.Exists(returnPath))
            Directory.CreateDirectory(returnPath);
        return returnPath;
    }

    public List<FileInfo> GetKickstarterBundles()
    {
        List<FileInfo> tempList = new List<FileInfo>();

        string[] fileEntries = Directory.GetFiles(BundleDirectory());
        foreach (string s in fileEntries)
        {        
            if (s.EndsWith("-ks")|| s.EndsWith("-preorder")||s.EndsWith("-special"))//Only show DLC files
                tempList.Add(new FileInfo(s));
        }
        return tempList;
    }
   
    //Loads assetbundles and the bridges within
    IEnumerator LoadChassis(string name)
    {
        var bundleLoadRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, name));
        yield return bundleLoadRequest;    

        var myLoadedAssetBundle = bundleLoadRequest.assetBundle;
        if (myLoadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            yield break;
        }

        var assetLoadRequest = myLoadedAssetBundle.LoadAllAssetsAsync<SubChassis>();
        yield return assetLoadRequest;
        if (assetLoadRequest == null)
        {
            Debug.Log("Failed to load Gameobjects!");
            yield break;
        }

        foreach (SubChassis go in assetLoadRequest.allAssets)
        {
            dlcShips.Add(go);
        }    

        myLoadedAssetBundle.Unload(false);
    }

    public IEnumerator LoadAllShips()
    {
        //Load Kickstarter bundles
        foreach (FileInfo f in GetKickstarterBundles())
        {
            yield return LoadChassis(f.Name);
            Debug.Log("Done loading dlc ship: " + f.Name);
        }
        loadingships = false;
    }       

    void Awake()
    {     
        DontDestroyOnLoad(gameObject);
        StartCoroutine(LoadAllShips());
    }

    bool AlreadyLoaded (SubChassis dlcShip)
    {        
        if (dlcShips == null) return false;
        foreach (SubChassis ls in dlcShips)
            if (ls == dlcShip)
                return true;

        return false;
    }
    
}