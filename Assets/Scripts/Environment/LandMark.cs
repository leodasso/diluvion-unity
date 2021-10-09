using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SpiderWeb;
using Diluvion.SaveLoad;
using DUI;
using Sirenix.OdinInspector;

public class LandMark : MonoBehaviour 
{
    static List<LandMark> allLandmarks = new List<LandMark>();

    public string landmarkLocKey;
    [DisplayAsString]
    public string niceName;
    [InlineEditor(InlineEditorModes.LargePreview)]
    public Sprite icon;
    public Color color = Color.white;
	public GameObject discoveryParticle;

    void Awake()
    {
        allLandmarks.Add(this);
    }

    /// <summary>
    /// Returns the list of all landmarks currently in the scene.
    /// </summary>
    public static List<LandMark> AllLandmarks()
    {
        // Clear out any null entries
        allLandmarks = allLandmarks.Where(x => x != null).ToList();
        return allLandmarks;
    }


    /// <summary>
    /// Returns a list of all landmarks that the player's discovered.
    /// </summary>
    public static List<LandMark> DiscoveredLandmarks()
    {
        List<LandMark> returnList = new List<LandMark>();
        if (DSave.current == null) return returnList;

        foreach (LandMark lm in AllLandmarks())
        {
            if (DSave.current.knownLandmarks.Contains(lm.landmarkLocKey))
                returnList.Add(lm);
        }

        return returnList;
    }

    #region loc
    public void SetLocToNiceName()
    {
        landmarkLocKey = "lm_" + niceName.ToLower();
    }


    public void AddToKeyLib()
    {
		Localization.AddToKeyLib(landmarkLocKey, niceName);
    }


	public string LocalizedName() {
		return Localization.GetFromLocLibrary(landmarkLocKey, niceName);
	}
#endregion

    public void PlayLandmarkDiscoveryMusic()
    {
        SpiderSound.MakeSound("Play_MUS_New_Landmark_Found", gameObject);
    }


    public void Discover()
    {
        if (DSave.current == null) return;

        // If this was a new discovery, tell the playaa
        if (DSave.current.AddLandmark(this))
        {
            string locString = Localization.GetFromLocLibrary("GUI/foundLandmark", "_lm found");
            Notifier.DisplayNotification(locString + " - " + LocalizedName(), Color.cyan);

            // Create nifty particle
            if (!discoveryParticle) return;
            GameObject newParticle = Instantiate(discoveryParticle, transform.position, transform.rotation);

            Destroy(newParticle, 6);
        }
        
        else Debug.Log("This landmark is already known!");
	}
}