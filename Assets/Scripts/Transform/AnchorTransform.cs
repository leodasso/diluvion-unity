using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Anchors transform parameters to the initial position
/// </summary>
public class AnchorTransform : MonoBehaviour
{

    public Space anchorSpace = Space.World;

    public bool anchorScale;
    public bool anchorRotation;
    public bool anchorPosition;

    Vector3 initScale;
    Vector3 initPos;
    Quaternion initRot;

    Vector3 initPosLocal;
    Quaternion initRotLocal;

    // Use this for initialization
    void Start ()
    {
        initScale = transform.localScale;
        initRot = transform.rotation;
        initPos = transform.position;

        initPosLocal = transform.localPosition;
        initRotLocal = transform.localRotation;
    }

    // Update is called once per frame
    void Update ()
    {

        if (anchorScale) transform.localScale = initScale;

            if (anchorSpace == Space.Self)
        {
            if (anchorPosition) transform.localPosition = initPosLocal;
            if (anchorRotation) transform.localRotation = initRotLocal;
        }else
        {
            if (anchorPosition) transform.position = initPos;
            if (anchorRotation) transform.rotation = initRot;
        }
    }
}
