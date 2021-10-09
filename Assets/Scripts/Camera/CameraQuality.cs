using UnityEngine;
using UnityStandardAssets.ImageEffects;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Enables / disables the 
/// </summary>
public class CameraQuality : MonoBehaviour {

	int qualityLevel;
    public bool dofActive = true;

	// Use this for initialization
	void Start () {
	
		qualityLevel = QualitySettings.GetQualityLevel();
		SetQuality(qualityLevel);
	}

    /// <summary>
    /// Sets the camera quality of all cameras in scene
    /// </summary>
    /// <param name="qLevel"></param>
    public static void SetQualityAll(int qLevel)
    {
        List<CameraQuality> allCamQuality = GameObject.FindObjectsOfType<CameraQuality>().ToList<CameraQuality>();
        foreach ( CameraQuality cam in allCamQuality ) cam.SetQuality(qLevel);
    }

	public void SetQuality( int qLevel) {

		//Debug.Log( gameObject.name + " Setting quality to " + qLevel);
		
        // Fast quality
        if (qLevel == 0)
        {
            SetBloom(false);
            SetDOF(false);
            SetOverlay(false);
        }

        //good quality
        else if (qLevel == 1)
        {
            SetBloom(true);
            SetDOF(false);
            SetOverlay(true);
        }

        // great quality
        else if (qLevel == 2)
        {
            SetBloom(true);
            SetDOF(false);
            SetOverlay(true);
        }

        // beautiful quality
        else 
        {
            SetBloom(true);
            SetDOF(true);
            SetOverlay(true);
        }
	}

    void SetBloom(bool active)
    {
        SENaturalBloomAndDirtyLens bloom = GetComponent<SENaturalBloomAndDirtyLens>();
        if ( bloom == null ) return;

        bloom.enabled = active;
    }

    void SetDOF(bool active)
    {
        dofActive = active;

        DepthOfField dof = GetComponent<DepthOfField>();
        if (dof) dof.enabled = active;

        DepthOfFieldDeprecated dof34 = GetComponent<DepthOfFieldDeprecated>();
        if (dof34) dof34.enabled = active;
        
    }

    void SetOverlay(bool active)
    {
        //ScreenOverlay overlay = GetComponent<ScreenOverlay>();
       // if ( overlay == null ) return;

       // overlay.enabled = active;
    }
}
