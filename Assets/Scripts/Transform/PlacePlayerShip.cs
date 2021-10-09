using UnityEngine;
using System.Collections;
using Diluvion.Ships;
using Diluvion;

/// <summary>
/// Places the current instance of the player ship as a child of this transform. Zeros out the local rotation and position
/// of the player ship once it's a child.
/// </summary>
public class PlacePlayerShip : MonoBehaviour {

    public bool placeOnStart = true;
    public bool setToMyLayer;
    public bool deParent;

    Bridge playerShip;

    bool placed = false;

	// Use this for initialization
	void Start () {

        if ( deParent ) transform.SetParent(null);

        if ( placeOnStart ) Place();
	}

    void Place()
    {
        playerShip = PlayerManager.pBridge;
        if ( playerShip == null ) return;

        playerShip.GetComponent<DockControl>().BreakDocking();

        // clear suimono surface components
        if (playerShip.GetComponentInChildren<SurfaceSub>())
        {
            Debug.Log("Found surface sub!");
            playerShip.GetComponentInChildren<SurfaceSub>().RemoveBuoyancy();
        }

        // Clear the docking rope TODO
        //if (playerShip.helm.endingDock != null)
          //  playerShip.helm.endingDock();

        playerShip.GetComponent<Rigidbody>().isKinematic = true;

        // Set the ship as a child and zero out the coords.
        playerShip.transform.parent = transform;
        playerShip.transform.localPosition = playerShip.transform.localEulerAngles = Vector3.zero;

        if (setToMyLayer)
        {
            foreach (Transform t in GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = gameObject.layer;
            }
        }

        playerShip.GetComponent<Rigidbody>().velocity = Vector3.zero;

        placed = true;
    }

    void Update()
    {
        if (placed && playerShip != null)
        {
            playerShip.transform.localPosition = Vector3.zero;
        }
    }
}
