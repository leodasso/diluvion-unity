using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[ExecuteInEditMode]
public class BoneColliderCaster : MonoBehaviour {

    public Vector3 direction;
    public LayerMask castLayerMask;

    public Collider hitCollider;

    public float minOffset = 1;
    public float maxOffset = 5;

    DynamicBoneCollider boneCol;

	// Use this for initialization
	void Start () {

        boneCol = GetComponent<DynamicBoneCollider>();
        if (boneCol == null)
        {
            Debug.LogWarning("Bone collider couldnt be found!");
            enabled = false;
        }
	}
	
	// Update is called once per frame
	void Update () {
        transform.rotation = Quaternion.identity;
        Cast();
	}

    void SetOffset(float offset)
    {

        //float finalOffset = Mathf.Clamp(offset, minOffset, maxOffset);
        float finalOffset = offset + boneCol.m_Radius;
        finalOffset = Mathf.Clamp(finalOffset, minOffset, maxOffset);
        //finalOffset += boneCol.m_Height / 2;

        boneCol.m_Center = new Vector3(boneCol.m_Center.x, finalOffset, boneCol.m_Center.z);
    }

    public void Cast()
    {
        Ray newRay = new Ray(transform.position, direction);
        RaycastHit hit;
        Physics.Raycast(newRay, out hit, 50, castLayerMask);

        if ( hit.collider == null ) {

            hitCollider = null;
            SetOffset(-50);
            return;
        }

        hitCollider = hit.collider;

        SetOffset(-hit.distance);
    }
}
