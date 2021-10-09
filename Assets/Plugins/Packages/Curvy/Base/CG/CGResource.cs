// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Reflection;
using FluffyUnderware.Curvy.Shapes;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;

namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// Resource attribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ResourceLoaderAttribute : System.Attribute
    {
        public readonly string ResourceName;

        public ResourceLoaderAttribute(string resName)
        {
            ResourceName = resName;
        }
    }

    /// <summary>
    /// Resource Helper class used by Curvy Generator
    /// </summary>
    public class CGResourceHandler
    {
        static Dictionary<string, ICGResourceLoader> Loader = new Dictionary<string, ICGResourceLoader>();

        public static Component CreateResource(CGModule module, string resName, string context)
        {
            if (Loader.Count == 0)
                getLoaders();
            if (Loader.ContainsKey(resName))
            {
                var loader = Loader[resName];
                return loader.Create(module,context);
            }
            else
            {
                Debug.LogError("CGResourceHandler: Missing Loader for resource '" + resName + "'");
                return null;
            }

        }

        public static void DestroyResource(CGModule module, string resName, Component obj, string context, bool kill)
        {
            if (Loader.Count == 0)
                getLoaders();
            if (Loader.ContainsKey(resName))
            {
                var loader = Loader[resName];
                loader.Destroy(module, obj, context, kill);
            }
            else
                Debug.LogError("CGResourceHandler: Missing Loader for resource '" + resName + "'");
        }

        static void getLoaders()
        {
            var tt = typeof(CGModule).GetAllTypes();
            foreach (System.Type T in tt)
            {
#if NETFX_CORE
                object[] at = (object[])T.GetTypeInfo().GetCustomAttributes(typeof(ResourceLoaderAttribute), true);
#else
                object[] at = (object[])T.GetCustomAttributes(typeof(ResourceLoaderAttribute), true);
#endif
                if (at.Length > 0)
                {
                    var o = (ICGResourceLoader)System.Activator.CreateInstance(T);
                    if (o != null)
                        Loader.Add(((ResourceLoaderAttribute)at[0]).ResourceName, o);
                }
            }

        }
    }

    /// <summary>
    /// Spline resource loader class
    /// </summary>
    [ResourceLoader("Spline")]
    public class CGSplineResourceLoader : ICGResourceLoader
    {

        public Component Create(CGModule module, string context)
        {
            var spl = CurvySpline.Create();
            spl.transform.position = Vector3.zero;
            spl.Closed = true;
            spl.Add(new Vector3(0, 0, 0), new Vector3(5, 0, 10), new Vector3(-5, 0, 10));
            return spl;
        }

        public void Destroy(CGModule module, Component obj, string context, bool kill)
        {
            if (obj != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.DestroyObjectImmediate(obj.gameObject);
                else
#endif
                    GameObject.Destroy(obj);
            }
        }
    }

    /// <summary>
    /// Shape (2D spline) resource loader class
    /// </summary>
    [ResourceLoader("Shape")]
    public class CGShapeResourceLoader : ICGResourceLoader
    {

        public Component Create(CGModule module, string context)
        {
            var spl = CurvySpline.Create();
            spl.transform.position = Vector3.zero;
            spl.RestrictTo2D = true;
            spl.Closed = true;
            spl.Orientation = CurvyOrientation.None;
            spl.gameObject.AddComponent<CSCircle>().Refresh();
            return spl;
        }

        public void Destroy(CGModule module, Component obj, string context, bool kill)
        {
            if (obj != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.DestroyObjectImmediate(obj.gameObject);
                else
#endif
                    GameObject.Destroy(obj);
            }

        }
    }

    /// <summary>
    /// Mesh resource loader class
    /// </summary>
    [ResourceLoader("Mesh")]
    public class CGMeshResourceLoader : ICGResourceLoader
    {


        public Component Create(CGModule module, string context)
        {
            var cmp=module.Generator.PoolManager.GetComponentPool<CGMeshResource>().Pop();
            return cmp;
        }

        public void Destroy(CGModule module, Component obj, string context, bool kill)
        {
            if (obj != null)
            {
                if (kill)
                {
                    if (Application.isPlaying)
                        GameObject.Destroy(obj.gameObject);
                    else
                        GameObject.DestroyImmediate(obj.gameObject);
                }
                else
                {
                    obj.StripComponents(typeof(CGMeshResource), typeof(MeshFilter), typeof(MeshRenderer));
                    module.Generator.PoolManager.GetComponentPool<CGMeshResource>().Push(obj);
                }
            }
        }
    }

    /// <summary>
    /// GameObject resource loader class
    /// </summary>
    [ResourceLoader("GameObject")]
    public class CGGameObjectResourceLoader : ICGResourceLoader
    {
        public Component Create(CGModule module, string context)
        {
            var go=module.Generator.PoolManager.GetPrefabPool(context).Pop();
            return go.transform;

        }

        public void Destroy(CGModule module, Component obj, string context, bool kill)
        {
            if (obj != null)
            {
                if (kill)
                {
                    if (Application.isPlaying)
                        GameObject.Destroy(obj.gameObject);
                    else
                        GameObject.DestroyImmediate(obj.gameObject);
                }
                else
                {
                    module.Generator.PoolManager.GetPrefabPool(context).Push(obj.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Collection of GameObject resources
    /// </summary>
    [System.Serializable]
    public class CGGameObjectResourceCollection : ICGResourceCollection
    {
        public List<Transform> Items = new List<Transform>();
        public List<string> PoolNames = new List<string>();

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
