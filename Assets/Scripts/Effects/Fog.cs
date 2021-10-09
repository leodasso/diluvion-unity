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
public class Fog : MonoBehaviour
{
    #region Public Properties
   
   // [SerializeField]
   // public FogMode _fogMode = FogMode.Linear;


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
    
    [SerializeField]
    float _outlineDistance = 1;
    
    public float outlineDistance
    {
        get { return _outlineDistance; }
        set { _outlineDistance = value; }
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
    
    [SerializeField]
    bool _useDistance;

    public bool useDistance
    {
        get { return _useDistance; }
        set { _useDistance = value; }
    }
    #endregion

    #region Private Properties

    [SerializeField] 
    public Shader _shader;
    [SerializeField] 
    public Material _material;
    bool changedColor;
    Color savedColor;
    Camera cam;
    private Vector3[] frustumCorners;
    Vector4[] vectorArray;
    #endregion

    #region MonoBehaviour Functions

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = DepthTextureMode.Depth;
    }

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
            if (_shader == null) return;
            _material = new Material(_shader);
            _material.hideFlags = HideFlags.DontSave;
        }

        _startDistance = Mathf.Max(_startDistance, 0.0f);
        _material.SetFloat("_DistanceOffset", _offsetDistance);

        var start = _startDistance;
        var end = _endDistance;
        var invDiff = 1.0f / Mathf.Max(end - start, 1.0e-6f);
        _material.SetFloat("_LinearGrad", -invDiff);
        _material.SetFloat("_LinearOffs", end * invDiff);
        _material.SetFloat("_OutLineDist", _outlineDistance);
        _material.DisableKeyword("DisableKeyword");
        //_material.FOG_EXP("FOG_EXP2");
     

        if (_useRadialDistance)
            _material.EnableKeyword("RADIAL_DIST");
        else
            _material.DisableKeyword("RADIAL_DIST");

        if (_fadeToSkybox)
        {
            _material.EnableKeyword("USE_SKYBOX");
            // Transfer the skybox parameters.
            var skybox = RenderSettings.skybox;
            _material.SetColor("_Color1", skybox.GetColor("_Color1"));
            _material.SetColor("_Color2", skybox.GetColor("_Color2"));
            _material.SetColor("_Color3", skybox.GetColor("_Color3"));
            _material.SetFloat("_Exponent1", skybox.GetFloat("_Exponent1"));
            _material.SetFloat("_Exponent2", skybox.GetFloat("_Exponent2"));
            _material.SetFloat("_Intensity", skybox.GetFloat("_Intensity"));

        }
        else
        {
        _material.DisableKeyword("USE_SKYBOX");
        _material.SetColor("_FogColor", _fogColor);
        }

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


        if (_useDistance)
        {
            _material.EnableKeyword("FOG_DISTANCE");
         
        }
        else
        {
            _material.DisableKeyword("FOG_DISTANCE");
            
        }
        // Draw screen quad.
        RenderTexture.active = destination;

        _material.SetTexture("_MainTex", source);
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
