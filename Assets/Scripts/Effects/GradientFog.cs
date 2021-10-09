//
// KinoFog - Deferred fog effect
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Kino Image Effects/Fog")]
public class GradientFog : MonoBehaviour
{
    #region Public Properties
   
    [SerializeField]
    public FogMode _fogMode = FogMode.Linear;

    [SerializeField]
    Gradient _gradient;
    public Gradient gradient
    {
        get { return _gradient; }
        set { _gradient = value; }
    }

    [SerializeField]
    int _gradientGranularity = 5;
    public int gradientGranularity
    {
        get { return _gradientGranularity; }
        set { _gradientGranularity = value; }
    }

    [SerializeField]
    Color _fogColor = Color.blue;
    public Color fogColor
    {
        get { return _fogColor; }
        set { _fogColor = value; }
    }

    // Start distance
    [SerializeField]
    float _startDistance = 20;

    public float startDistance {
        get { return _startDistance; }
        set { _startDistance = value; }
    }

    [SerializeField]
    float _endDistance = 100;

    public float endDistance
    {
        get { return _endDistance; }
        set { _endDistance = value; }
    }

    [SerializeField]
    float _offsetDistance = 50;

    public float offsetDistance
    {
        get { return _offsetDistance; }
        set { _offsetDistance = value; }
    }

    // Use radial distance
    [SerializeField]
    bool _useRadialDistance;

    public bool useRadialDistance {
        get { return _useRadialDistance; }
        set { _useRadialDistance = value; }
    }

    // Fade-to-skybox flag
    [SerializeField]
    bool _fadeToSkybox;

    public bool fadeToSkybox
    {
        get { return _fadeToSkybox; }
        set { _fadeToSkybox = value; }
    }

    #endregion

    #region Private Properties

    [SerializeField] 
    public Shader _shader;
    [SerializeField] 
    public Material _material;

    private Texture2D gradientBlit;
    bool changedColor;
    Color savedColor;
    Camera cam;
    #endregion

    #region MonoBehaviour Functions

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = DepthTextureMode.Depth;
        
        gradientBlit = new Texture2D(gradientGranularity, 1,TextureFormat.ARGB32, false);
        gradientBlit.anisoLevel = 0;
        gradientBlit.filterMode = FilterMode.Bilinear;
        gradientBlit.wrapMode = TextureWrapMode.Clamp;
        Color[] colours = new Color[gradientGranularity];
        for (int i = 0; i < gradientGranularity; i++)
        {
            colours[i] = gradient.Evaluate((float)i /gradientGranularity);
        }

        gradientBlit.SetPixels(colours);
        gradientBlit.Apply(false);

    }

    //public void BlitIt()
    //{
    //    gradientBlit = new Texture2D(gradientGranularity, 1);

    //}

    void Update()
    {
        if (savedColor != fogColor)
        { 
            savedColor = fogColor;
            cam.backgroundColor = savedColor;
        }         
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_material == null)
        {
            _material = new Material(_shader);
            _material.hideFlags = HideFlags.DontSave;
        }

        _startDistance = Mathf.Max(_startDistance, 0.0f);
        _material.SetFloat("_DistanceOffset", _offsetDistance);

        FogMode mode = _fogMode;
        if (mode == FogMode.Linear)
        {
            var start = _startDistance;
            var end = _endDistance;
            var invDiff = 1.0f / Mathf.Max(end - start, 1.0e-6f);
            _material.SetFloat("_LinearGrad", -invDiff);
            _material.SetFloat("_LinearOffs", end * invDiff);
            _material.DisableKeyword("DisableKeyword");
            //_material.FOG_EXP("FOG_EXP2");
        }
        else if (mode == FogMode.Exponential)
        {
            const float coeff = 1.4426950408f; // 1/ln(2)
            var density = RenderSettings.fogDensity;
            _material.SetFloat("_Density", coeff * density);
            _material.EnableKeyword("FOG_EXP");
            _material.DisableKeyword("FOG_EXP2");
        }
        else // FogMode.ExponentialSquared
        {
            const float coeff = 1.2011224087f; // 1/sqrt(ln(2))
            var density = RenderSettings.fogDensity;
            _material.SetFloat("_Density", coeff * density);
            _material.DisableKeyword("FOG_EXP");
            _material.EnableKeyword("FOG_EXP2");
        }

        _material.EnableKeyword("RADIAL_DIST");    

        _material.DisableKeyword("USE_SKYBOX");
        _material.SetColor("_FogColor", _fogColor);
      

        // Calculate vectors towards frustum corners.
        var cam = GetComponent<Camera>();
        var camtr = cam.transform;
        var camNear = cam.nearClipPlane;
        var camFar = cam.farClipPlane;

        var tanHalfFov = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad / 2);
        var toRight = camtr.right * camNear * tanHalfFov * cam.aspect;
        var toTop = camtr.up * camNear * tanHalfFov;

        var v_tl = camtr.forward * camNear - toRight + toTop;
        var v_tr = camtr.forward * camNear + toRight + toTop;
        var v_br = camtr.forward * camNear + toRight - toTop;
        var v_bl = camtr.forward * camNear - toRight - toTop;

        var v_s = v_tl.magnitude * camFar / camNear;

        // Draw screen quad.
        RenderTexture.active = destination;

        _material.SetTexture("_MainTex", source);
        _material.SetTexture("_Gradient", gradientBlit);
        _material.SetPass(0);

        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Begin(GL.QUADS);

        GL.MultiTexCoord2(0, 0, 0);
        GL.MultiTexCoord(1, v_bl.normalized * v_s);
        GL.Vertex3(0, 0, 0.1f);

        GL.MultiTexCoord2(0, 1, 0);
        GL.MultiTexCoord(1, v_br.normalized * v_s);
        GL.Vertex3(1, 0, 0.1f);

        GL.MultiTexCoord2(0, 1, 1);
        GL.MultiTexCoord(1, v_tr.normalized * v_s);
        GL.Vertex3(1, 1, 0.1f);

        GL.MultiTexCoord2(0, 0, 1);
        GL.MultiTexCoord(1, v_tl.normalized * v_s);
        GL.Vertex3(0, 1, 0.1f);

        GL.End();
        GL.PopMatrix();
    }

    #endregion
}
