using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Fog/AlphaSortedFog")]

public class AlphaSortedFog : MonoBehaviour
{

    void Start()
    {
        camera = Camera.main;
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        DrawFog(source, destination);
    }
    //fog script
    public enum FogMode
    {
        AbsoluteYAndDistance = 0,
        AbsoluteY = 1,
        Distance = 2,
        RelativeYAndDistance = 3,
    }
    public bool bEnableFog = true;
    public FogMode fogMode = FogMode.AbsoluteYAndDistance;

    private float CAMERA_NEAR = 0.5f;
    private float CAMERA_FAR = 50.0f;
    private float CAMERA_FOV = 60.0f;
    private float CAMERA_ASPECT_RATIO = 1.333333f;

    public float startDistance = 200.0f;
    public float globalDensity = 1.0f;
    public float heightScale = 100.0f;
    public float height = 0.0f;

    public Color globalFogColor = Color.grey;

    public Shader fogShader;
    private Material fogMaterial = null;
    new Camera camera;


    void DrawFog(RenderTexture source, RenderTexture destination)
    {
        if (!bEnableFog)
        {
            Graphics.Blit(source, destination);
            return;
        }
        if (fogMaterial == null)
        {
            fogMaterial = new Material(fogShader);
        }
        CAMERA_NEAR = camera.nearClipPlane;
        CAMERA_FAR = camera.farClipPlane;
        CAMERA_FOV = camera.fieldOfView;
        CAMERA_ASPECT_RATIO = camera.aspect;

        Matrix4x4 frustumCorners = Matrix4x4.identity;
        //Vector4 vec;
        //Vector3 corner;

        float fovWHalf = CAMERA_FOV * 0.5f;

        Vector3 toRight = camera.transform.right * CAMERA_NEAR * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * CAMERA_ASPECT_RATIO;
        Vector3 toTop = camera.transform.up * CAMERA_NEAR * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

        Vector3 topLeft = (camera.transform.forward * CAMERA_NEAR - toRight + toTop);
        float CAMERA_SCALE = topLeft.magnitude * CAMERA_FAR / CAMERA_NEAR;

        topLeft.Normalize();
        topLeft *= CAMERA_SCALE;

        Vector3 topRight = (camera.transform.forward * CAMERA_NEAR + toRight + toTop);
        topRight.Normalize();
        topRight *= CAMERA_SCALE;

        Vector3 bottomRight = (camera.transform.forward * CAMERA_NEAR + toRight - toTop);
        bottomRight.Normalize();
        bottomRight *= CAMERA_SCALE;

        Vector3 bottomLeft = (camera.transform.forward * CAMERA_NEAR - toRight - toTop);
        bottomLeft.Normalize();
        bottomLeft *= CAMERA_SCALE;

        frustumCorners.SetRow(0, topLeft);
        frustumCorners.SetRow(1, topRight);
        frustumCorners.SetRow(2, bottomLeft);
        frustumCorners.SetRow(3, bottomRight);

        fogMaterial.SetMatrix("_FrustumCornersWS", frustumCorners);
        fogMaterial.SetVector("_CameraWS", camera.transform.position);
        fogMaterial.SetVector("_StartDistance", new Vector4(1.0f / startDistance, (CAMERA_SCALE - startDistance)));
        fogMaterial.SetVector("_Y", new Vector4(height, 1.0f / heightScale));

        fogMaterial.SetFloat("_GlobalDensity", globalDensity * 0.01f);
        fogMaterial.SetColor("_FogColor", globalFogColor);
        CustomGraphicsBlit(source, destination, fogMaterial, (int)fogMode);
    }

    static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr)
    {
        Graphics.Blit(source, dest, fxMaterial, passNr);
    }
}