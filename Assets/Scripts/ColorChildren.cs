using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using BlendModes;
//using SpiderWeb;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SpriteRenderer)), ExecuteInEditMode]
public class ColorChildren : MonoBehaviour
{

    [OnValueChanged("ApplyBlendMode")]
    public BlendMode blendMode = BlendMode.Normal;

    [ToggleLeft]
    public bool colorNewChildren        = false;

    [ToggleLeft]
    public bool overrideParentColor     = false;

    [ToggleLeft]
    public bool overrideParentSorting   = false;

    [ToggleLeft]
    public bool includeLineRenderer = false;

    [ToggleLeft]
    public bool sortOnStart = false;

    BlendModes.RenderMode renderMode = BlendModes.RenderMode.Framebuffer;

    int children = 0;

    /// <summary>
    /// Removes blending from all children. Editor only.
    /// </summary>
    public void RemoveBlending ()
    {
#if UNITY_EDITOR
        Material defaultSprite = Resources.Load("default sprite") as Material;

        foreach (BlendModeEffect bm in GetComponentsInChildren<BlendModeEffect>())
        {
            if (bm.GetComponent<ColorChildren>()) continue;
            DestroyImmediate(bm);
        }


        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr.GetComponent<ColorChildren>()) continue;
            sr.sharedMaterial = defaultSprite;
        }
#endif
    }

    public void ApplyBlendMode ()
    {
        if (blendMode == BlendMode.Normal)
        {
            RemoveBlending();
            return;
        }

        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr.GetComponent<ColorChildren>()) continue;

            BlendModeEffect bm = sr.GetComponent<BlendModeEffect>();
#if UNITY_EDITOR
            if (!bm) bm = Undo.AddComponent<BlendModeEffect>(sr.gameObject);
#endif

            bm.SetBlendMode(blendMode, renderMode);
        }
    }

    private void Start ()
    {
        if (Application.isPlaying) return;

        if (sortOnStart)
        {
            SetSorting();
        }
    }

    private void Update ()
    {
        if (Application.isPlaying) return;

        // update for editing
        if (children != transform.childCount)
        {
            if (colorNewChildren) SetColor();
        }

        children = transform.childCount;
    }

    [ButtonGroup]
    public void SetColor ()
    {

        SpriteRenderer mySr = GetComponent<SpriteRenderer>();

        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {

            if (sr.GetComponent<ColorChildren>()) continue;
            sr.color = mySr.color;
        }

        foreach (TextMeshPro tmPro in GetComponentsInChildren<TextMeshPro>())
        {
            tmPro.color = mySr.color;
        }

        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            if (mr.GetComponent<ColorChildren>()) continue;
            //mr.material.color = mySr.color;
        }

        if (includeLineRenderer)
        {
            LineRenderer line = GetComponent<LineRenderer>();
            if (line)
            {
                line.startColor = mySr.color;
                line.endColor = mySr.color;
            }
        }

        foreach (ColorChildren cc in GetComponentsInChildren<ColorChildren>())
        {
            if (cc.overrideParentColor && cc != this) cc.SetColor();
        }
    }

    /// <summary>
    /// Sets the color of all children sprites to the given color.
    /// </summary>
    public void SetColor (Color newColor)
    {
        SpriteRenderer mySr = GetComponent<SpriteRenderer>();
        mySr.color = newColor;

        SetColor();
    }

    [ButtonGroup]
    void SetSorting ()
    {

        SpriteRenderer mySr = GetComponent<SpriteRenderer>();

        int j = 0;
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr.GetComponent<ColorChildren>()) continue;
            sr.sortingLayerName = mySr.sortingLayerName;
            sr.sortingOrder = mySr.sortingOrder + j;
            j++;
        }



        foreach (ColorChildren cc in GetComponentsInChildren<ColorChildren>())
        {
            if (cc.overrideParentSorting && cc != this) cc.SetSorting();
        }
    }
}
