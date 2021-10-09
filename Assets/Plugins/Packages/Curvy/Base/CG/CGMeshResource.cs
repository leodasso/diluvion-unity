// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using System.Collections.Generic;

namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// Mesh Resource Component used by Curvy Generator
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class CGMeshResource : DuplicateEditorMesh, IPoolable
    {
        MeshRenderer mRenderer;
        Collider mCollider;

        public MeshRenderer Renderer
        {
            get
            {
                if (mRenderer == null)
                    mRenderer = GetComponent<MeshRenderer>();
                return mRenderer;
            }
        }

        public Collider Collider
        {
            get
            {
                if (mCollider == null)
                    mCollider = GetComponent<Collider>();
                return mCollider;
            }
            
        }

        public Mesh Prepare()
        {
            return Filter.PrepareNewShared();
        }

        public bool ColliderMatches(CGColliderEnum type)
        {
            if (Collider == null && type == CGColliderEnum.None)
                return true;
            if (Collider is MeshCollider && type == CGColliderEnum.Mesh)
                return true;
            if (Collider is BoxCollider && type == CGColliderEnum.Box)
                return true;
            if (Collider is SphereCollider && type == CGColliderEnum.Sphere)
                return true;

            return false;
        }
      
        public void RemoveCollider()
        {
            if (Collider){
                if (Application.isPlaying)
                    Destroy(mCollider);
                else
                    DestroyImmediate(mCollider);
                mCollider = null;
            }
        }

        public bool UpdateCollider(CGColliderEnum mode, bool convex, PhysicMaterial material)
        {
            if (Collider == null)
            {
                switch (mode)
                {
                    case CGColliderEnum.Mesh:
                        if (canAddMeshCollider(Filter.sharedMesh.bounds))
                        {
                            var mc = gameObject.AddComponent<MeshCollider>();
                            mc.convex = convex;
                            mCollider = mc;
                        }
                        break;
                    case CGColliderEnum.Box:
                        gameObject.AddComponent<BoxCollider>();
                        break;
                    case CGColliderEnum.Sphere:
                        gameObject.AddComponent<SphereCollider>();
                        break;
                }
            }

            if (Collider != null)
            {
                switch (mode)
                {
                    case CGColliderEnum.Mesh:
                        var mc = Collider as MeshCollider;
                        if (mc != null)
                        {
                            mc.sharedMesh = null;
                            if (!canAddMeshCollider(Filter.sharedMesh.bounds))
                                return false;
                            try
                            {
                                mc.sharedMesh = Filter.sharedMesh;
                            }
                            catch
                            {
                                return false;
                            }
                        }
                        break;
                    case CGColliderEnum.Box:
                        var bc = Collider as BoxCollider;
                        if (bc != null)
                        {
                            bc.center = Filter.sharedMesh.bounds.center;
                            bc.size = Filter.sharedMesh.bounds.size;
                        }
                        break;
                    case CGColliderEnum.Sphere:
                        var sc = Collider as SphereCollider;
                        if (sc != null)
                        {
                            sc.center = Filter.sharedMesh.bounds.center;
                            sc.radius = Filter.sharedMesh.bounds.extents.magnitude;
                        }
                        break;
                }
                Collider.material = material;
            }
            return true;

            
        }

        bool canAddMeshCollider(Bounds b)
        {
            return b.extents.x > float.Epsilon && b.extents.y > float.Epsilon && b.extents.z > float.Epsilon;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public void OnBeforePush()
        {
        }

        public void OnAfterPop()
        {
        }
    }

    /// <summary>
    /// Collection of Mesh Resources
    /// </summary>
    [System.Serializable]
    public class CGMeshResourceCollection : ICGResourceCollection
    {
        public List<CGMeshResource> Items = new List<CGMeshResource>();

        public int Count
        {
            get
            {
                return Items.Count;
            }
        }

        public Component[] ItemsArray
        {
            get { return Items.ToArray(); }
        }

    }
}
