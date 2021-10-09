// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

#if UNITY_5_3 || UNITY_5_2 || UNITY_5_1 || UNITY_5_0
#define PRE_UNITY_5_4
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using UnityEngine.Rendering;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Create/Mesh",ModuleName="Create Mesh")]
    [HelpURL(CurvySpline.DOCLINK + "cgcreatemesh")]
    public class CreateMesh : CGModule
    {
        [HideInInspector]
        [InputSlotInfo(typeof(CGVMesh),Array=true,Name="VMesh")]
        public CGModuleInputSlot InVMeshArray = new CGModuleInputSlot();

        [HideInInspector]
        [InputSlotInfo(typeof(CGSpots), Name = "Spots", Optional=true)]
        public CGModuleInputSlot InSpots = new CGModuleInputSlot();

        [SerializeField, CGResourceCollectionManager("Mesh", ShowCount = true)]
        CGMeshResourceCollection m_MeshResources = new CGMeshResourceCollection();

        #region ### Serialized Fields ###

        [Tab("General")]

        [Tooltip("Merge meshes")]
        [SerializeField]
        bool m_Combine;

        [Tooltip("Merge meshes sharing the same Index")]
        [FieldCondition("canGroupMeshes", true, Action = ConditionalAttribute.ActionEnum.Enable)]
        [SerializeField]
        bool m_GroupMeshes = true;

        [SerializeField]
        CGYesNoAuto m_AddNormals = CGYesNoAuto.Auto;

        [SerializeField]
        CGYesNoAuto m_AddTangents = CGYesNoAuto.No;
        [SerializeField]
        bool m_AddUV2 = true;


        [SerializeField]
        bool m_MakeStatic;
        [SerializeField]
        [Layer]
        int m_Layer;

        [Tab("Renderer")]
        [SerializeField]
        ShadowCastingMode m_CastShadows = ShadowCastingMode.On;
        [SerializeField]
        bool m_ReceiveShadows=true;
#if !PRE_UNITY_5_4
        [SerializeField]
        LightProbeUsage m_LightProbeUsage = LightProbeUsage.BlendProbes;

        [HideInInspector]
#endif
        [SerializeField]
        bool m_UseLightProbes=true;


        [SerializeField]
        ReflectionProbeUsage m_ReflectionProbes = ReflectionProbeUsage.BlendProbes;
        [SerializeField]
        Transform m_AnchorOverride;

        [Tab("Collider")]
        [SerializeField]
        CGColliderEnum m_Collider = CGColliderEnum.Mesh;
       
        [FieldCondition("m_Collider", CGColliderEnum.Mesh)]
        [SerializeField]
        bool m_Convex;

        [FieldCondition("m_Collider", CGColliderEnum.None, true, Action = ConditionalAttribute.ActionEnum.Enable)]
        [Label("Auto Update")]
        [SerializeField]
        bool m_AutoUpdateColliders = true;

        [FieldCondition("m_Collider", CGColliderEnum.None, true, Action = ConditionalAttribute.ActionEnum.Enable)]
        [SerializeField]
        PhysicMaterial m_Material;

#endregion

        #region ### Public Properties ###

        #region --- General ---
                public bool Combine
                {
                    get { return m_Combine; }
                    set
                    {
                        if (m_Combine != value)
                            m_Combine = value;
                        Dirty = true;
                    }
                }

                public bool GroupMeshes
                {
                    get { return m_GroupMeshes; }
                    set
                    {
                        if (m_GroupMeshes != value)
                            m_GroupMeshes = value;
                        Dirty = true;
                    }
                }

                public CGYesNoAuto AddNormals
                {
                    get { return m_AddNormals; }
                    set
                    {
                        if (m_AddNormals != value)
                            m_AddNormals = value;
                        Dirty = true;
                    }
                }

                public CGYesNoAuto AddTangents
                {
                    get { return m_AddTangents; }
                    set
                    {
                        if (m_AddTangents != value)
                            m_AddTangents = value;
                        Dirty = true;
                    }
                }

                public bool AddUV2
                {
                    get { return m_AddUV2; }
                    set
                    {
                        if (m_AddUV2 != value)
                            m_AddUV2 = value;
                        Dirty = true;
                    }
                }


                public int Layer
                {
                    get { return m_Layer; }
                    set
                    {
                        int v = Mathf.Clamp(value, 0, 32);
                        if (m_Layer != v)
                            m_Layer = v;
                        Dirty = true;
                    }
                }

                public bool MakeStatic
                {
                    get { return m_MakeStatic; }
                    set
                    {
                        if (m_MakeStatic != value)
                            m_MakeStatic = value;
                        Dirty = true;
                    }
                }
        #endregion

        #region --- Renderer ---

                public ShadowCastingMode CastShadows
                {
                    get { return m_CastShadows; }
                    set
                    {
                        if (m_CastShadows != value)
                            m_CastShadows = value;
                        Dirty = true;
                    }
                }

                public bool ReceiveShadows
                {
                    get { return m_ReceiveShadows; }
                    set
                    {
                        if (m_ReceiveShadows != value)
                            m_ReceiveShadows = value;
                        Dirty = true;
                    }
                }

                public bool UseLightProbes
                {
                    get { return m_UseLightProbes; }
                    set
                    {
                        if (m_UseLightProbes != value)
                            m_UseLightProbes = value;
                        Dirty = true;
                    }
                }

        #if !PRE_UNITY_5_4
                public LightProbeUsage LightProbeUsage
                {
                    get { return m_LightProbeUsage; }
                    set
                    {
                        if (m_LightProbeUsage != value)
                            m_LightProbeUsage = value;
                        Dirty = true;
                    }
                }
        #endif


                public ReflectionProbeUsage ReflectionProbes
                {
                    get { return m_ReflectionProbes; }
                    set
                    {
                        if (m_ReflectionProbes != value)
                            m_ReflectionProbes = value;
                        Dirty = true;
                    }
                }

                public Transform AnchorOverride
                {
                    get { return m_AnchorOverride; }
                    set
                    {
                        if (m_AnchorOverride != value)
                            m_AnchorOverride = value;
                        Dirty = true;
                    }
                }

        #endregion

        #region --- Collider ---

                public CGColliderEnum Collider
                {
                    get { return m_Collider; }
                    set
                    {
                        if (m_Collider != value)
                            m_Collider = value;
                        Dirty = true;
                    }
                }

                public bool AutoUpdateColliders
                {
                    get { return m_AutoUpdateColliders; }
                    set
                    {
                        if (m_AutoUpdateColliders != value)
                            m_AutoUpdateColliders = value;
                        Dirty = true;
                    }
                }

                public bool Convex
                {
                    get { return m_Convex; }
                    set
                    {
                        if (m_Convex != value)
                            m_Convex = value;
                        Dirty = true;
                    }
                }

                public PhysicMaterial Material
                {
                    get { return m_Material; }
                    set
                    {
                        if (m_Material != value)
                            m_Material = value;
                        Dirty = true;
                    }
                }

        #endregion

                public CGMeshResourceCollection Meshes
                {
                    get { return m_MeshResources; }
                }

                public int MeshCount
                {
                    get { return Meshes.Count; }
                }

                public int VertexCount { get; private set; }

        #endregion

        #region ### Private Fields & Properties ###

                int mCurrentMeshCount;

                bool canGroupMeshes
                {
                    get
                    {
                        return (InSpots.IsLinked && m_Combine);
                    }
                }
        

        #endregion

        #region ### Unity Callbacks ###
                /*! \cond UNITY */

        #if UNITY_EDITOR
                protected override void OnValidate()
                {
                    base.OnValidate();
                    AddNormals = m_AddNormals;
                    AddTangents = m_AddTangents;
                    Collider = m_Collider;
                }
        #endif

                public override void Reset()
                {
                    base.Reset();
                    Combine = false;
                    GroupMeshes = true;
                    AddNormals = CGYesNoAuto.Auto;
                    AddTangents = CGYesNoAuto.No;
                    MakeStatic = false;
                    Material = null;
                    Convex = false;
                    Layer = 0;
                    CastShadows = ShadowCastingMode.On;
                    ReceiveShadows = true;
                    UseLightProbes = true;
        #if !PRE_UNITY_5_4
                    LightProbeUsage = LightProbeUsage.BlendProbes;
        #endif
                    ReflectionProbes = ReflectionProbeUsage.BlendProbes;
                    AnchorOverride = null;
                    Collider = CGColliderEnum.Mesh;
                    AutoUpdateColliders = true;
                    Convex = false;
                    Clear();
            
                }

                /*! \endcond */
        #endregion

        #region ### Public Methods ###

                public override void OnTemplateCreated()
                {
                    Clear();
                }

                public void Clear()
                {
                    mCurrentMeshCount = 0;
                    removeUnusedResource();
                    Resources.UnloadUnusedAssets();
                }

                public override void OnStateChange()
                {
                    base.OnStateChange();
                    if (!IsConfigured)
                        Clear();

                }

                public override void Refresh()
                {
                    
                    base.Refresh();
                    if (!Application.isPlaying || !MakeStatic)
                    {
                        var VMeshes = InVMeshArray.GetAllData<CGVMesh>();
                        var Spots = InSpots.GetData<CGSpots>();

                        mCurrentMeshCount = 0;
                        VertexCount = 0;

                        if (VMeshes.Count > 0 && (!InSpots.IsLinked || (Spots != null && Spots.Count > 0)))
                        {
                            if (Spots != null && Spots.Count > 0)
                                createSpotMeshes(ref VMeshes, ref Spots, Combine);
                            else
                                createMeshes(ref VMeshes, Combine);
                        }
                        // Cleanup
                        removeUnusedResource();

                        // Update Colliders?
                        if (AutoUpdateColliders)
                            UpdateColliders();
                    }
                }

                public GameObject SaveToScene(Transform parent = null)
                {
                    List<Component> res;
                    List<string> names;
                    GetManagedResources(out res, out names);
                    if (res.Count == 0)
                        return null;

                    Transform root;
                    if (res.Count > 1)
                    {
                        root = new GameObject(ModuleName).transform;
                        root.transform.parent = (parent == null) ? Generator.transform.parent : parent;
                        for (int i = 0; i < res.Count; i++)
                        {
                            var orgF = res[i].GetComponent<MeshFilter>();
                            var subGO = res[i].gameObject.DuplicateGameObject(root.transform);
                            subGO.name = res[i].name;
                            subGO.GetComponent<CGMeshResource>().Destroy();
                            subGO.GetComponent<MeshFilter>().sharedMesh = Component.Instantiate<Mesh>(orgF.sharedMesh);
                        }
                        return root.gameObject;
                    }
                    else
                    {
                        var orgF = res[0].GetComponent<MeshFilter>();
                        var subGO = res[0].gameObject.DuplicateGameObject(parent);
                        subGO.name = res[0].name;
                        subGO.GetComponent<CGMeshResource>().Destroy();
                        subGO.GetComponent<MeshFilter>().sharedMesh = Component.Instantiate<Mesh>(orgF.sharedMesh);
                
                        return subGO;
                    }
                }

                public void UpdateColliders()
                {
                    bool success = true;
                    for (int r = 0; r < m_MeshResources.Count; r++)
                    {
                        if (!m_MeshResources.Items[r].UpdateCollider(Collider,Convex,Material))
                            success = false;
                    }
                    if (!success)
                        UIMessages.Add("Error setting collider!");
                }

        
        #endregion

        protected override bool UpgradeVersion(string oldVersion, string newVersion)
        {
            return true;
        }

        #region ### Privates ###
                /*! \cond PRIVATE */

                void createMeshes(ref List<CGVMesh> vMeshes, bool combine)
                {
                    int exceededVertexCount = 0;

                    if (combine)
                    {
                        sortByVertexCount(ref vMeshes);
                        // Skip meshes that exceeds Vertex limits
                        while (vMeshes[exceededVertexCount].Count > 65534)
                            exceededVertexCount++;

                        CGVMesh curVMesh = (vMeshes.Count == 1) ? vMeshes[0] : new CGVMesh(vMeshes[0]); // Just one mesh? No need to create a copy! Otherwise, take a copy!
                        for (int vm = exceededVertexCount + 1; vm < vMeshes.Count; vm++)
                        {
                            if (curVMesh.Count + vMeshes[vm].Count > 65534)
                            { // write curVMesh 
                                writeVMeshToMesh(ref curVMesh);
                                curVMesh = (vm < vMeshes.Count - 1) ? vMeshes[vm] : new CGVMesh(vMeshes[vm]); // just one mesh left? No need to create a copy! Otherwise, take a copy!
                            }
                            else // Add new vMesh to curVMesh
                                curVMesh.MergeVMesh(vMeshes[vm]);
                        }
                        writeVMeshToMesh(ref curVMesh);

                    }
                    else
                    {
                        for (int vm = 0; vm < vMeshes.Count; vm++)
                        {
                            if (vMeshes[vm].Count < 65535)
                            {
                                CGVMesh vmesh = vMeshes[vm];
                                writeVMeshToMesh(ref vmesh);
                            }
                            else
                                exceededVertexCount++;
                        }
                    }
                    if (exceededVertexCount > 0)
                        UIMessages.Add(string.Format("{0} meshes skipped (VertexCount>65534)", exceededVertexCount));
                }

                void createSpotMeshes(ref List<CGVMesh> vMeshes, ref CGSpots spots, bool combine)
                {
                    int exceededVertexCount = 0;
                    int vmCount = vMeshes.Count;
                    CGSpot spot;

                    CGMeshResource res;
                    if (combine)
                    {
                        var sortedSpots = new List<CGSpot>(spots.Points);
                        if (GroupMeshes)
                            sortedSpots.Sort((CGSpot a, CGSpot b) => a.Index.CompareTo(b.Index));

                        spot = sortedSpots[0];
                        CGVMesh curVMesh = new CGVMesh(vMeshes[spot.Index]);
                        if (spot.Position != Vector3.zero || spot.Rotation != Quaternion.identity || spot.Scale != Vector3.one)
                            curVMesh.TRS(spot.Matrix);
                        for (int s = 1; s < sortedSpots.Count; s++)
                        {
                            spot = sortedSpots[s];
                            // Filter spot.index not in vMeshes[]
                            if (spot.Index > -1 && spot.Index < vmCount)
                            {
                                if (curVMesh.Count + vMeshes[spot.Index].Count > 65534 || (GroupMeshes && spot.Index != sortedSpots[s - 1].Index))
                                { // write curVMesh 
                                    writeVMeshToMesh(ref curVMesh);
                                    curVMesh = new CGVMesh(vMeshes[spot.Index]);
                                    if (!spot.Matrix.isIdentity)
                                        curVMesh.TRS(spot.Matrix);
                                }
                                else
                                { // Add new vMesh to curVMesh
                                    if (!spot.Matrix.isIdentity)
                                        curVMesh.MergeVMesh(vMeshes[spot.Index], spot.Matrix);
                                    else
                                        curVMesh.MergeVMesh(vMeshes[spot.Index]);
                                }
                            }
                        }
                        writeVMeshToMesh(ref curVMesh);
                    }
                    else
                    {
                        for (int s = 0; s < spots.Count; s++)
                        {
                            spot = spots.Points[s];
                            // Filter spot.index not in vMeshes[]
                            if (spot.Index > -1 && spot.Index < vmCount)
                            {
                                // Don't touch vertices, TRS Resource instead
                                if (vMeshes[spot.Index].Count < 65535)
                                {
                                    CGVMesh vmesh = vMeshes[spot.Index];
                                    res = writeVMeshToMesh(ref vmesh);
                                    if (spot.Position != Vector3.zero || spot.Rotation != Quaternion.identity || spot.Scale != Vector3.one)
                                        spot.ToTransform(res.Filter.transform);
                                }
                                else
                                    exceededVertexCount++;
                            }
                        }
                    }

                    if (exceededVertexCount > 0)
                        UIMessages.Add(string.Format("{0} meshes skipped (VertexCount>65534)", exceededVertexCount));
                }

                /// <summary>
                /// create a mesh resource and copy vmesh data to the mesh!
                /// </summary>
                /// <param name="vmesh"></param>
                CGMeshResource writeVMeshToMesh(ref CGVMesh vmesh)
                {
                    CGMeshResource res;
                    Mesh mesh;

                    bool wantNormals = (AddNormals != CGYesNoAuto.No);
                    bool wantTangents = (AddTangents != CGYesNoAuto.No);
                    res = getNewMesh();
        #if UNITY_EDITOR
                    if (!Application.isPlaying)
                        res.Filter.gameObject.isStatic = false;
        #endif
                    mesh = res.Prepare();
                    res.gameObject.layer = Layer;
                    vmesh.ToMesh(ref mesh);
                    VertexCount += vmesh.Count;
                    if (AddUV2 && !vmesh.HasUV2)
                        mesh.uv2 = CGUtility.CalculateUV2(vmesh.UV);
                    if (wantNormals && !vmesh.HasNormals)
                        mesh.RecalculateNormals();
                    if (wantTangents && !vmesh.HasTangents)
                        res.Filter.CalculateTangents();


                    // Reset Transform
                    res.Filter.transform.localPosition = Vector3.zero;
                    res.Filter.transform.localRotation = Quaternion.identity;
                    res.Filter.transform.localScale = Vector3.one;
        #if UNITY_EDITOR
                    if (!Application.isPlaying)
                        res.Filter.gameObject.isStatic = MakeStatic;
        #endif
                    res.Renderer.sharedMaterials = vmesh.GetMaterials();


                    return res;
                }

                void sortByVertexCount(ref List<CGVMesh> vMeshes)
                {
                    vMeshes.Sort(delegate(CGVMesh a, CGVMesh b) { return -a.Count.CompareTo(b.Count); });
                }

                /// <summary>
                /// remove all mesh resources not currently used (>=mCurrentMeshCount)
                /// </summary>
                void removeUnusedResource()
                {
                    for (int r = mCurrentMeshCount; r < Meshes.Count; r++)
                        DeleteManagedResource("Mesh", Meshes.Items[r]);
                    Meshes.Items.RemoveRange(mCurrentMeshCount, Meshes.Count - mCurrentMeshCount);

                }

                /// <summary>
                /// gets a new mesh resource and increase mCurrentMeshCount
                /// </summary>
                CGMeshResource getNewMesh()
                {
                    CGMeshResource r;
                    // Reuse existing resources
                    if (mCurrentMeshCount < Meshes.Count)
                    {
                        r = Meshes.Items[mCurrentMeshCount];
                        if (r == null)
                        {
                            r = ((CGMeshResource)AddManagedResource("Mesh", "", mCurrentMeshCount));
                            Meshes.Items[mCurrentMeshCount] = r;
                        }
                    }
                    else
                    {
                        r = ((CGMeshResource)AddManagedResource("Mesh", "", mCurrentMeshCount));
                        Meshes.Items.Add(r);
                    }

                    // Renderer settings
                    r.Renderer.shadowCastingMode = CastShadows;
                    r.Renderer.receiveShadows = ReceiveShadows;
        #if PRE_UNITY_5_4
                    r.Renderer.useLightProbes = UseLightProbes;
        #else
                    r.Renderer.lightProbeUsage = LightProbeUsage;
        #endif
                    r.Renderer.reflectionProbeUsage = ReflectionProbes;

                    r.Renderer.probeAnchor = AnchorOverride;

                    if (!r.ColliderMatches(Collider))
                        r.RemoveCollider();
            
                    //RenameResource("Mesh", r, mCurrentMeshCount);
                    mCurrentMeshCount++;
                    return r;
                }
       

                /*! \endcond */
        #endregion

        
    }
    
}
