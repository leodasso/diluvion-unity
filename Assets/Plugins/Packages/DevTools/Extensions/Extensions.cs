// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Text;
using System.IO;
#if NETFX_CORE
using System.Linq;
#endif


namespace FluffyUnderware.DevTools.Extensions
{
    public static class TransformExt
    {
        public static void CopyFrom(this Transform t, Transform other)
        {
            t.position = other.position;
            t.rotation = other.rotation;
            t.localScale = other.localScale;
        }

        public static void SetChildrenHideFlags(this Transform t, HideFlags flags)
        {
            if (t != null)
            {
                int cnt = t.childCount;
                for (int i = 0; i < cnt; i++)
                    t.GetChild(i).hideFlags = flags;
            }

        }
    }

    public static class ObjectExt
    {
        public static void Destroy(this UnityEngine.Object c)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.DestroyObjectImmediate(c);
            else
#endif
                UnityEngine.Object.Destroy(c);
        }


        public static string ToDumpString(this object o)
        {
            return new DTObjectDump(o).ToString();
        }

    }

    public static class Vector2Ext
    {
        public static Vector2 Snap(this Vector2 v, float snapX, float snapY = -1)
        {
            if (snapY == -1)
                snapY = snapX;
            return (new Vector2(v.x - v.x % snapX, v.y - v.y % snapY));
        }

        public static float AngleSigned(this Vector2 a, Vector2 b)
        {
            var sign = Mathf.Sign(a.x * b.y - a.y * b.x);
            return Vector2.Angle(a, b) * sign;
        }

        public static Vector2 LeftNormal(this Vector2 v)
        {
            return new Vector2(-v.y, v.x);
        }
        public static Vector2 RightNormal(this Vector2 v)
        {
            return new Vector2(v.y, -v.x);
        }

        public static Vector2 Rotate(this Vector2 v, float degree)
        {
            var rad = degree * Mathf.Deg2Rad;
            var c = Mathf.Cos(rad);
            var s = Mathf.Sin(rad);
            return new Vector2(c * v.x - s * v.y, s * v.x + c * v.y);
        }

        public static Vector2 ToVector3(this Vector2 v)
        {
            return new Vector3(v.x, v.y, 0);
        }


    }

    public static class Vector3Ext
    {
        public static float AngleSigned(this Vector3 a, Vector3 b, Vector3 normal)
        {
            return Mathf.Atan2(Vector3.Dot(normal, Vector3.Cross(a, b)), Vector3.Dot(a, b)) * Mathf.Rad2Deg;
        }

        public static Vector3 RotateAround(this Vector3 point, Vector3 origin, Quaternion rotation)
        {
            var dir = point - origin;
            dir = rotation * dir;
            return origin + dir;
        }

        public static Vector2 ToVector2(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

    }

    public static class CameraExt
    {

        static Plane[] camPlanes;
        static Vector3 camPos;
        static Vector3 camForward;
        static float fov;
        static float screenW;
        static float screenH;


        public static bool BoundsInView(this Camera c, Bounds bounds)
        {
            if (camPos != c.transform.position ||
                camForward != c.transform.forward ||
                screenW != Screen.width ||
                screenH != Screen.height ||
                fov != c.fieldOfView)
            {
                camPos = c.transform.position;
                camForward = c.transform.forward;
                screenW = Screen.width;
                screenH = Screen.height;
                fov = c.fieldOfView;
                camPlanes = GeometryUtility.CalculateFrustumPlanes(c);
            }
            return GeometryUtility.TestPlanesAABB(camPlanes, bounds);
        }

        public static bool BoundsPartiallyInView(this Camera c, Bounds bounds)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(c);
            Vector3 v3Corner = Vector3.zero;
            Vector3 v3Center = bounds.center;
            Vector3 v3Extents = bounds.extents;

            v3Corner.Set(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top left corner
            if (GeometryUtility.TestPlanesAABB(planes, new Bounds(v3Corner, new Vector3(0.1f, 0.1f, 0.1f))))
                return true;
            v3Corner.Set(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top right corner
            if (GeometryUtility.TestPlanesAABB(planes, new Bounds(v3Corner, new Vector3(0.1f, 0.1f, 0.1f))))
                return true;
            v3Corner.Set(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom left corner
            if (GeometryUtility.TestPlanesAABB(planes, new Bounds(v3Corner, new Vector3(0.1f, 0.1f, 0.1f))))
                return true;
            v3Corner.Set(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom right corner
            if (GeometryUtility.TestPlanesAABB(planes, new Bounds(v3Corner, new Vector3(0.1f, 0.1f, 0.1f))))
                return true;
            v3Corner.Set(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top left corner
            if (GeometryUtility.TestPlanesAABB(planes, new Bounds(v3Corner, new Vector3(0.1f, 0.1f, 0.1f))))
                return true;
            v3Corner.Set(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top right corner
            if (GeometryUtility.TestPlanesAABB(planes, new Bounds(v3Corner, new Vector3(0.1f, 0.1f, 0.1f))))
                return true;
            v3Corner.Set(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom left corner
            if (GeometryUtility.TestPlanesAABB(planes, new Bounds(v3Corner, new Vector3(0.1f, 0.1f, 0.1f))))
                return true;
            v3Corner.Set(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom right corner
            if (GeometryUtility.TestPlanesAABB(planes, new Bounds(v3Corner, new Vector3(0.1f, 0.1f, 0.1f))))
                return true;

            return false;

        }
    }

    public static class GameObjectExt
    {


        /// <summary>
        /// Duplicates a GameObject
        /// </summary>
        /// <param name="source">a component being part of the source GameObject</param>
        /// <returns>the component from the cloned GameObject</returns>
        public static GameObject DuplicateGameObject(this GameObject source, Transform newParent, bool keepPrefabReference = false)
        {
            if (!source)
                return null;

            GameObject newGO;
#if UNITY_EDITOR
            UnityEngine.Object prefabRoot = PrefabUtility.GetPrefabParent(source.gameObject);

            if (prefabRoot != null && keepPrefabReference)
                newGO = PrefabUtility.InstantiatePrefab(prefabRoot) as GameObject;
            else
#endif
                newGO = GameObject.Instantiate(source.gameObject) as GameObject;

            if (newGO)
                newGO.transform.parent = newParent;

            return newGO;
        }

        public static void StripComponents(this GameObject go, params Type[] toKeep)
        {
            var keep = new List<Type>(toKeep);
            keep.Add(typeof(Transform));
            keep.Add(typeof(RectTransform));
            var cmps = go.GetComponents<Component>();
            for (int i = 0; i < cmps.Length; i++)
                if (!keep.Contains(cmps[i].GetType()))
                {
                    if (!Application.isPlaying)
                        Component.DestroyImmediate(cmps[i]);
                    else
                        Component.Destroy(cmps[i]);
                }

        }
    }

    public static class ComponentExt
    {
        public static void StripComponents(this Component c, params Type[] toKeep)
        {
            if (toKeep.Length == 0)
                c.gameObject.StripComponents(c.GetType());
            else
                c.gameObject.StripComponents(toKeep);
        }

        public static GameObject AddChildGameObject(this Component c, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(c.transform);
            return go;
        }

        public static T AddChildGameObject<T>(this Component c, string name) where T : Component
        {
            var go = new GameObject(name);
            if (go)
            {
                go.transform.SetParent(c.transform);
                return go.AddComponent<T>();
            }
            else
                return null;
        }

        /// <summary>
        /// Duplicates the GameObject of a component, returning the component
        /// </summary>
        /// <param name="source">a component being part of the source GameObject</param>
        /// <returns>the component from the cloned GameObject</returns>
        public static T DuplicateGameObject<T>(this Component source, Transform newParent, bool keepPrefabConnection = false) where T : Component
        {
            if (!source || !source.gameObject)
                return null;

            var cmps = new List<Component>(source.gameObject.GetComponents<Component>());
            int sourceIdx = cmps.IndexOf(source);
            GameObject newGO;
#if UNITY_EDITOR
            UnityEngine.Object prefabRoot = PrefabUtility.GetPrefabParent(source.gameObject);

            if (prefabRoot != null && keepPrefabConnection)
                newGO = PrefabUtility.InstantiatePrefab(prefabRoot) as GameObject;
            else
#endif
                newGO = GameObject.Instantiate<GameObject>(source.gameObject);

            if (newGO)
            {
                newGO.transform.SetParent(newParent, false);
                var newCmps = newGO.GetComponents<Component>();
                return newCmps[sourceIdx] as T;
            }
            else
                return null;
        }

        /// <summary>
        /// Duplicates the GameObject of a component, returning the component
        /// </summary>
        /// <param name="source">a component being part of the source GameObject</param>
        /// <returns>the component from the cloned GameObject</returns>
        public static Component DuplicateGameObject(this Component source, Transform newParent, bool keepPrefabConnection = false)
        {
            if (!source || !source.gameObject || !newParent)
                return null;

            var cmps = new List<Component>(source.gameObject.GetComponents<Component>());
            int sourceIdx = cmps.IndexOf(source);
            GameObject newGO;
#if UNITY_EDITOR
            UnityEngine.Object prefabRoot = PrefabUtility.GetPrefabParent(source.gameObject);

            if (prefabRoot != null && keepPrefabConnection)
                newGO = PrefabUtility.InstantiatePrefab(prefabRoot) as GameObject;
            else
#endif
                newGO = GameObject.Instantiate<GameObject>(source.gameObject);

            if (newGO)
            {
                newGO.transform.SetParent(newParent, false);
                var newCmps = newGO.GetComponents<Component>();
                return newCmps[sourceIdx];
            }
            else
                return null;
        }

    }

    public static class ColorExt
    {
        public static string ToHtml(this Color c)
        {
            Color32 col = c;
            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", new object[] { col.r, col.g, col.b, col.a });
        }


    }

    public static class EnumExt
    {

        /// <summary>
        /// Checks if at least one of the provided flags is set in variable
        /// </summary>
        public static bool HasFlag(this Enum variable, params Enum[] flags)
        {
            if (flags.Length == 0)
                throw new ArgumentNullException("flags");

            int varInt = Convert.ToInt32(variable);

            Type T = variable.GetType();
            for (int i = 0; i < flags.Length; i++)
            {
                if (!Enum.IsDefined(T, flags[i]))
                {
                    throw new ArgumentException(string.Format(
                    "Enumeration type mismatch.  The flag is of type '{0}', was expecting '{1}'.",
                    flags[i].GetType(), T));
                }
                int num = Convert.ToInt32(flags[i]);
                if ((varInt & num) == num)
                    return true;
            }
            return false;
        }

        public static bool HasFlag<T>(this T value, T flag) where T : struct
        {
            long lValue = Convert.ToInt64(value);
            long lFlag = Convert.ToInt64(flag);
            return (lValue & lFlag) != 0;
        }

        /// <summary>
        /// Sets a flag
        /// </summary>
        public static T Set<T>(this Enum value, T append) { return Set(value, append, true); }
        /// <summary>
        /// Sets a flag
        /// </summary>
        /// <param name="OnOff">whether to set or unset the value</param>
        public static T Set<T>(this Enum value, T append, bool OnOff)
        {
            if (append == null)
                throw new ArgumentNullException("append");

            Type type = value.GetType();
            //return the final value
            if (OnOff)
                return (T)Enum.Parse(type, (Convert.ToUInt64(value) | Convert.ToUInt64(append)).ToString());
            else
                return (T)Enum.Parse(type, (Convert.ToUInt64(value) & ~Convert.ToUInt64(append)).ToString());
        }

        /// <summary>
        /// Toggles a flag
        /// </summary>
        public static T Toggle<T>(this Enum value, T toggleValue)
        {
            if (toggleValue == null)
                throw new ArgumentNullException("toggleValue");

            Type type = value.GetType();
            //return the final value
            var intValue = Convert.ToUInt64(value);
            return (T)Enum.Parse(type, (intValue ^ intValue).ToString());
        }

        public static T SetAll<T>(this Enum value)
        {
            Type type = value.GetType();
            object result = value;
            string[] names = Enum.GetNames(type);
            foreach (var name in names)
            {
                ((Enum)result).Set(Enum.Parse(type, name));
            }

            return (T)result;
        }

    }

    public static class RectExt
    {

        public static Rect Set(this Rect rect, Vector2 pos, Vector2 size)
        {
            rect.Set(pos.x, pos.y, size.x, size.y);
            return new Rect(rect);
        }

        public static Rect SetBetween(this Rect rect, Vector2 pos, Vector2 pos2)
        {
            rect.Set(pos.x, pos.y, pos2.x - pos.x, pos2.y - pos.y);
            return new Rect(rect);
        }

        /// <summary>
        /// Sets x/y
        /// </summary>
        public static Rect SetPosition(this Rect rect, Vector2 pos)
        {
            rect.x = pos.x;
            rect.y = pos.y;
            return new Rect(rect);
        }

        /// <summary>
        /// Sets x/y
        /// </summary>
        public static Rect SetPosition(this Rect rect, float x, float y)
        {
            rect.x = x;
            rect.y = y;
            return new Rect(rect);
        }

        /// <summary>
        /// gets width/height as Vector2
        /// </summary>
        public static Vector2 GetSize(this Rect rect)
        {
            return new Vector2(rect.width, rect.height);
        }
        /// <summary>
        /// Sets width/height
        /// </summary>
        public static Rect SetSize(this Rect rect, Vector2 size)
        {
            rect.width = size.x;
            rect.height = size.y;
            return new Rect(rect);
        }


        /// <summary>
        /// Grow/Shrink a rect
        /// </summary>
        public static Rect ScaleBy(this Rect rect, int pixel) { return ScaleBy(rect, pixel, pixel); }
        /// <summary>
        /// Grow/Shrink a rect
        /// </summary>
        public static Rect ScaleBy(this Rect rect, int x, int y)
        {
            rect.x -= (float)x;
            rect.y -= (float)y;
            rect.width += (float)x * 2;
            rect.height += (float)y * 2;
            return new Rect(rect);
        }

        public static Rect ShiftBy(this Rect rect, int x, int y)
        {
            rect.x += (float)x;
            rect.y += (float)y;
            return new Rect(rect);
        }

        public static Rect Include(this Rect rect, Rect other)
        {
            Rect r = new Rect();
            r.xMin = Mathf.Min(rect.xMin, other.xMin);
            r.xMax = Mathf.Max(rect.xMax, other.xMax);
            r.yMin = Mathf.Min(rect.yMin, other.yMin);
            r.yMax = Mathf.Max(rect.yMax, other.yMax);
            return r;
        }

    }

    public static class StringExt
    {
        /// <summary>
        /// Converts a HTML color endcoded string int a color
        /// </summary>
        /// <param name="hexString">html color of type [#]rrggbb[aa]</param>
        /// <returns>a Color</returns>
        public static Color ColorFromHtml(this string hexString)
        {
            if (hexString.Length < 9)
                hexString += "FF";
            if (hexString.StartsWith("#") && hexString.Length == 9)
            {
                int[] rgba = new int[4];
                try
                {
                    rgba[0] = int.Parse(hexString.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                    rgba[1] = int.Parse(hexString.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                    rgba[2] = int.Parse(hexString.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                    rgba[3] = int.Parse(hexString.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
                    return new Color(rgba[0] / 255f, rgba[1] / 255f, rgba[2] / 255f, rgba[3] / 255f);
                }
                catch
                {
                    return Color.white;
                }
            }
            return Color.white;
        }

        public static string TrimStart(this string s, string trim, StringComparison compare = StringComparison.CurrentCultureIgnoreCase)
        {
            if (!s.StartsWith(trim, compare))
                return s;
            else
            {
                return s.Substring(trim.Length);
            }
        }

        public static string TrimEnd(this string s, string trim, StringComparison compare = StringComparison.CurrentCultureIgnoreCase)
        {
            if (!s.EndsWith(trim, compare))
                return s;
            else
            {
                return s.Substring(0, s.Length - trim.Length);
            }
        }


    }

    public static class IEnumerableExt
    {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie)
            {
                action(i);
            }
        }
    }

    public static class ArrayExt
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            length = Mathf.Clamp(length, 0, data.Length - index);
            T[] result = new T[length];
            if (length > 0)
                Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            T[] dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

        public static T[] InsertAt<T>(this T[] source, int index)
        {
            T[] dest = new T[source.Length + 1];
            index = Mathf.Clamp(index, 0, source.Length - 1);

            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            Array.Copy(source, index, dest, index + 1, source.Length - index);

            return dest;
        }

        public static T[] Swap<T>(this T[] source, int index, int with)
        {
            index = Mathf.Clamp(index, 0, source.Length - 1);
            with = Mathf.Clamp(index, 0, source.Length - 1);
            T tmp = source[index];
            source[index] = source[with];
            source[with] = tmp;
            return source;
        }

        public static T[] Add<T>(this T[] source, T item)
        {
            Array.Resize<T>(ref source, source.Length + 1);
            source[source.Length - 1] = item;
            return source;
        }

        public static T[] AddRange<T>(this T[] source, T[] items)
        {
            Array.Resize<T>(ref source, source.Length + items.Length);
            System.Array.Copy(items, 0, source, source.Length - items.Length, items.Length);
            return source;
        }

        public static T[] RemoveDuplicates<T>(this T[] source)
        {
            List<T> res = new List<T>();
            HashSet<T> hash = new HashSet<T>();
            foreach (var p in source)
            {
                if (hash.Add(p))
                {
                    res.Add(p);
                }
            }
            return res.ToArray();
        }

        public static int IndexOf<T>(this T[] source, T item)
        {
            for (int i = 0; i < source.Length; i++)
                if (source[i].Equals(item))
                    return i;
            return -1;
        }

        public static T[] Remove<T>(this T[] source, T item)
        {
            int idx = source.IndexOf<T>(item);
            if (idx > -1)
                return source.RemoveAt<T>(idx);
            else
                return source;
        }
    }

    public static class MeshFilterExt
    {
        /// <summary>
        /// Returns a shared mesh to work with. If existing, it will be cleared
        /// </summary>
        public static Mesh PrepareNewShared(this MeshFilter m, string name = "Mesh")
        {
            if (m == null)
                return null;
            if (m.sharedMesh == null)
            {
                var msh = new Mesh();
                msh.MarkDynamic();
                msh.name = name;
                m.sharedMesh = msh;
            }
            else
            {
                m.sharedMesh.Clear();
                m.sharedMesh.name = name;
                m.sharedMesh.subMeshCount = 0;
            }
            return m.sharedMesh;
        }

        public static void CalculateTangents(this MeshFilter m)
        {
            //speed up math by copying the mesh arrays
            int[] triangles = m.sharedMesh.triangles;
            Vector3[] vertices = m.sharedMesh.vertices;
            Vector2[] uv = m.sharedMesh.uv;
            Vector3[] normals = m.sharedMesh.normals;

            if (uv.Length == 0)
                return;

            //variable definitions
            int triangleCount = triangles.Length;
            int vertexCount = vertices.Length;

            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];

            Vector4[] tangents = new Vector4[vertexCount];

            for (long a = 0; a < triangleCount; a += 3)
            {
                long i1 = triangles[a + 0];
                long i2 = triangles[a + 1];
                long i3 = triangles[a + 2];

                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 v3 = vertices[i3];

                Vector2 w1 = uv[i1];
                Vector2 w2 = uv[i2];
                Vector2 w3 = uv[i3];

                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;

                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;

                float div = s1 * t2 - s2 * t1;
                float r = div == 0.0f ? 0.0f : 1.0f / div;

                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }


            for (long a = 0; a < vertexCount; ++a)
            {
                Vector3 n = normals[a];
                Vector3 t = tan1[a];
                Vector3.OrthoNormalize(ref n, ref t);
                tangents[a].x = t.x;
                tangents[a].y = t.y;
                tangents[a].z = t.z;

                tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
            }

            m.sharedMesh.tangents = tangents;
        }
    }

    public static class TypeExt
    {

        /// <summary>
        /// Gets all types (Crossplatform)
        /// </summary>
        /// <param name="typeFromAssembly">a type being part of the assembly to use</param>
        public static Type[] GetAllTypes(this Type typeFromAssembly)
        {
#if NETFX_CORE
            var ti=typeFromAssembly.GetTypeInfo().Assembly.DefinedTypes;
            var tt=new List<Type>();
            foreach (var t in ti)
                tt.Add(t.AsType());
            return tt.ToArray();
#else
            return typeFromAssembly.Assembly.GetTypes();
#endif
        }

        /// <summary>
        /// Gets all Types T that have an attribute U
        /// </summary>
        public static Dictionary<U, Type> GetAllTypesWithAttribute<U>(this Type type)
        {
            var res = new Dictionary<U, Type>();
            var tt = type.GetAllTypes();
            foreach (Type t in tt)
            {
#if NETFX_CORE
                if (t.GetTypeInfo().IsSubclassOf(type))
#else
                if (t.IsSubclassOf(type))
#endif
                {
#if NETFX_CORE
                    object[] attribs = (object[])t.GetTypeInfo().GetCustomAttributes(typeof(U), false);
#else
                    object[] attribs = t.GetCustomAttributes(typeof(U), false);
#endif

                    if (attribs.Length > 0)
                    {
                        res.Add((U)attribs[0], t);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Gets all fields of a type that have a certain attribute
        /// </summary>
        public static List<FieldInfo> GetFieldsWithAttribute<T>(this Type type, bool includeInherited = false, bool includePrivate = false) where T : Attribute
        {
            var flds = type.GetAllFields(includeInherited, includePrivate);
            var res = new List<FieldInfo>();
            foreach (FieldInfo fi in flds)
            {
                if (fi.GetCustomAttribute<T>() != null)
                    res.Add(fi);
            }
            return res;
        }

        /// <summary>
        /// Gets a custom attribute from a type (Crossplatform)
        /// </summary>
        public static T GetCustomAttribute<T>(this Type type) where T : Attribute
        {
#if NETFX_CORE
            object[] at = (object[])type.GetTypeInfo().GetCustomAttributes(typeof(T), true);
#else
            object[] at = (object[])type.GetCustomAttributes(typeof(T), true);
#endif
            return (at.Length > 0) ? (T)at[0] : null;

        }

        /// <summary>
        /// Finds a Method (Crossplatform)
        /// </summary>
        /// <param name="type">type containing the method</param>
        /// <param name="name">Name of method</param>
        /// <param name="includeInherited">Whether methods of base classes should be returned as well</param>
        /// <param name="includePrivate">Whether private methods should be returned as well</param>
        public static MethodInfo MethodByName(this Type type, string name, bool includeInherited = false, bool includePrivate = false)
        {
#if NETFX_CORE
            MethodInfo res=type.GetRuntimeMethods().Where(mi => (mi.IsPublic || includePrivate) && mi.DeclaringType==type && mi.Name==name).FirstOrDefault();
            if (res==null && includeInherited)
            {
                type=type.GetTypeInfo().BaseType;
                while (res==null && type!=typeof(object))
                {
                    res=type.GetRuntimeMethods().Where(mi => (mi.IsPublic || includePrivate) && mi.DeclaringType==type && mi.Name==name).FirstOrDefault();
                    type=type.GetTypeInfo().BaseType;
                }
            }
            return res;
#else
            var flags = BindingFlags.Public | BindingFlags.Instance;
            if (includePrivate)
                flags = flags | BindingFlags.NonPublic;

            if (includeInherited)
                return type.GetMethodIncludingBaseClasses(name, flags);
            else
                return type.GetMethod(name, flags);
#endif
        }
        /// <summary>
        /// Finds a Field (Crossplatform)
        /// </summary>
        /// <param name="type">type containing the field</param>
        /// <param name="name">Name of field</param>
        /// <param name="includeInherited">Whether fields of base classes should be returned as well</param>
        /// <param name="includePrivate">Whether private fields should be returned as well</param>
        public static FieldInfo FieldByName(this Type type, string name, bool includeInherited = false, bool includePrivate = false)
        {
#if NETFX_CORE
            FieldInfo res=type.GetRuntimeFields().Where(fi => (fi.IsPublic || includePrivate) && fi.DeclaringType==type && fi.Name==name).FirstOrDefault();
            if (res==null && includeInherited)
            {
                type=type.GetTypeInfo().BaseType;
                while (res==null && type!=typeof(object))
                {
                    res=type.GetRuntimeFields().Where(fi => (fi.IsPublic || includePrivate) && fi.DeclaringType==type && fi.Name==name).FirstOrDefault();
                    type=type.GetTypeInfo().BaseType;
                }
            }
            return res;
#else
            var flags = BindingFlags.Public | BindingFlags.Instance;
            if (includePrivate)
                flags = flags | BindingFlags.NonPublic;

            if (includeInherited)
                return type.GetFieldIncludingBaseClasses(name, flags);
            else
                return type.GetField(name, flags);
#endif
        }
        /// <summary>
        /// Finds a Property (Crossplatform)
        /// </summary>
        /// <param name="type">type containing the property</param>
        /// <param name="name">Name of property</param>
        /// <param name="includeInherited">Whether properties of base classes should be returned as well</param>
        /// <param name="includePrivate">Whether private properties should be returned as well</param>
        public static PropertyInfo PropertyByName(this Type type, string name, bool includeInherited = false, bool includePrivate = false)
        {
#if NETFX_CORE
            PropertyInfo res=type.GetRuntimeProperties().Where(pi => ((pi.GetMethod!=null && pi.GetMethod.IsPublic) || includePrivate) && pi.DeclaringType==type && pi.Name==name).FirstOrDefault();
            if (res==null && includeInherited)
            {
                type=type.GetTypeInfo().BaseType;
                while (res==null && type!=typeof(object))
                {
                    res=type.GetRuntimeProperties().Where(pi => ((pi.GetMethod!=null && pi.GetMethod.IsPublic) || includePrivate) && pi.DeclaringType==type && pi.Name==name).FirstOrDefault();
                    type=type.GetTypeInfo().BaseType;
                }
            }
            return res;
#else
            var flags = BindingFlags.Public | BindingFlags.Instance;
            if (includePrivate)
                flags = flags | BindingFlags.NonPublic;

            if (includeInherited)
                return type.GetPropertyIncludingBaseClasses(name, flags);
            else
                return type.GetProperty(name, flags);
#endif
        }
        /// <summary>
        /// Gets all fields (Crossplatform)
        /// </summary>
        /// <param name="type">type to reflect</param>
        /// <param name="includeInherited">Whether fields of base classes should be returned as well</param>
        /// <param name="includePrivate">Whether private fields should be returned as well</param>
        public static FieldInfo[] GetAllFields(this Type type, bool includeInherited = false, bool includePrivate = false)
        {
#if NETFX_CORE

            List<FieldInfo> res=type.GetRuntimeFields().Where(fi => (fi.IsPublic || includePrivate) && fi.DeclaringType==type).ToList();
            if (includeInherited)
            {
                type=type.GetTypeInfo().BaseType;
                while (type!=typeof(object))
                {
                    res.AddRange(type.GetRuntimeFields().Where(fi => (fi.IsPublic || includePrivate) && fi.DeclaringType==type).ToArray());
                    type=type.GetTypeInfo().BaseType;
                }
            }
            return res.ToArray();
#else
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            if (includePrivate)
                flags = flags | BindingFlags.NonPublic;

            if (includeInherited)
            {
                var currentType = type;
                var res = new List<FieldInfo>();
                while (currentType != typeof(object))
                {
                    res.AddRange(currentType.GetFields(flags));
                    currentType = currentType.BaseType;
                }
                return res.ToArray();
            }
            else
                return type.GetFields(flags);
#endif
        }
        /// <summary>
        /// Gets all properties (Crossplatform)
        /// </summary>
        /// <param name="type">type to reflect</param>
        /// <param name="includeInherited">Whether properties of base classes should be returned as well</param>
        /// <param name="includePrivate">Whether private properties should be returned as well</param>
        public static PropertyInfo[] GetAllProperties(this Type type, bool includeInherited = false, bool includePrivate = false)
        {
#if NETFX_CORE
            List<PropertyInfo> res=type.GetRuntimeProperties().Where(pi => ((pi.GetMethod!=null && pi.GetMethod.IsPublic) || includePrivate) && pi.DeclaringType==type).ToList();
            if (includeInherited)
            {
                type=type.GetTypeInfo().BaseType;
                while (type!=typeof(object))
                {
                    res.AddRange(type.GetRuntimeProperties().Where(pi => ((pi.GetMethod!=null && pi.GetMethod.IsPublic) || includePrivate) && pi.DeclaringType==type).ToArray());
                    type=type.GetTypeInfo().BaseType;
                }
            }
            return res.ToArray();
#else
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            if (includePrivate)
                flags = flags | BindingFlags.NonPublic;

            if (includeInherited)
            {
                var currentType = type;
                var res = new List<PropertyInfo>();
                while (currentType != typeof(object))
                {
                    res.AddRange(currentType.GetProperties(flags));
                    currentType = currentType.BaseType;
                }
                return res.ToArray();
            }
            else
                return type.GetProperties(flags);
#endif
        }

        /// <summary>
        /// Whether the type is a framework type, i.e. a primitive, string or DateTime (Crossplatform)
        /// </summary>
        public static bool IsFrameworkType(this Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().IsPrimitive || type.Equals(typeof(string)) || type.Equals(typeof(DateTime));
#else
            return type.IsPrimitive || type.Equals(typeof(string)) || type.Equals(typeof(DateTime));
#endif
        }

        public static bool IsArrayOrList(this Type type)
        {
            return (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)));
        }


#if !NETFX_CORE
        // not NETFX_CORE compliant
        public static Type GetEnumerableType(this Type t)
        {
            Type ienum = FindIEnumerable(t);
            if (ienum == null) return t;
            return ienum.GetGenericArguments()[0];
        }

        // not NETFX_CORE compliant
        static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
                return null;
            if (seqType.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());

            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }
            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null) return ienum;
                }
            }

            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
            }
            return null;
        }

        // not NETFX_CORE compliant
        static MethodInfo GetMethodIncludingBaseClasses(this Type type, string name, BindingFlags bindingFlags)
        {
            // If this class doesn't have a base, don't waste any time
            MethodInfo mi = type.GetMethod(name, bindingFlags);
            if (type.BaseType == typeof(object))
            {
                return mi;
            }
            else
            {   // Otherwise, collect all types up to the furthest base class
                var currentType = type;
                while (currentType != typeof(object))
                {
                    mi = currentType.GetMethod(name, bindingFlags);
                    if (mi != null)
                        return mi;
                    currentType = currentType.BaseType;
                }
                return null;
            }
        }
        // not NETFX_CORE compliant
        static FieldInfo GetFieldIncludingBaseClasses(this Type type, string name, BindingFlags bindingFlags)
        {
            FieldInfo fieldInfo = type.GetField(name, bindingFlags);

            // If this class doesn't have a base, don't waste any time
            if (type.BaseType == typeof(object))
            {
                return fieldInfo;
            }
            else
            {   // Otherwise, collect all types up to the furthest base class
                var currentType = type;
                while (currentType != typeof(object))
                {
                    fieldInfo = currentType.GetField(name, bindingFlags);
                    if (fieldInfo != null)
                        return fieldInfo;
                    currentType = currentType.BaseType;
                }
                return null;
            }
        }
        // not NETFX_CORE compliant
        static PropertyInfo GetPropertyIncludingBaseClasses(this Type type, string name, BindingFlags bindingFlags)
        {
            PropertyInfo propertyInfo = type.GetProperty(name, bindingFlags);

            // If this class doesn't have a base, don't waste any time
            if (type.BaseType == typeof(object))
            {
                return propertyInfo;
            }
            else
            {   // Otherwise, collect all types up to the furthest base class
                var currentType = type;
                while (currentType != typeof(object))
                {
                    propertyInfo = currentType.GetProperty(name, bindingFlags);
                    if (propertyInfo != null)
                        return propertyInfo;
                    currentType = currentType.BaseType;
                }
                return null;
            }
        }

#endif

        public static bool Matches(this Type type, params Type[] types)
        {
#if NETFX_CORE
            var ti = type.GetTypeInfo();
#endif
            foreach (var t in types)
#if NETFX_CORE
                if (type == t || ti.IsAssignableFrom(t.GetTypeInfo()))
#else
                if (type == t || type.IsAssignableFrom(t))
#endif
                    return true;

            return false;
        }




    }

    public static class FieldInfoExt
    {
#if !NETFX_CORE
        /// <summary>
        /// Gets a custom attribute (CrossPlatform)
        /// </summary>
        public static T GetCustomAttribute<T>(this FieldInfo field) where T : Attribute
        {
            var at = field.GetCustomAttributes(typeof(T), true);
            return (at.Length > 0) ? (T)at[0] : null;

        }
#endif
    }
}
