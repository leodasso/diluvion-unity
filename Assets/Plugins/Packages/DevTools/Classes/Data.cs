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

namespace FluffyUnderware.DevTools
{
    [System.Serializable]
    public class TTransform
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
        public Matrix4x4 localToWorldMatrix;
        public Quaternion rotation;
        public Vector3 position;

        public Vector3 up
        {
            get
            {
                return rotation * Vector3.up;
            }
        }

        public TTransform(Transform t)
        {
            FromTransform(t);
        }

        /// <summary>
        /// Sets the TTransform to the transform
        /// </summary>
        /// <param name="t"></param>
        public void FromTransform(Transform t)
        {
            position = t.position;
            localPosition = t.localPosition;
            localRotation = t.localRotation;
            localScale = t.localScale;
            localToWorldMatrix = t.localToWorldMatrix;
            rotation = t.rotation;
        }
        /// <summary>
        /// Sets the transform to match TTransform
        /// </summary>
        /// <param name="t">the transform to set</param>
        /// <param name="space">the space use</param>
        public void ToTransform(Transform t, Space space=Space.Self)
        {
            switch (space)
            {
                case Space.World:
                    t.position = position;
                    t.rotation = rotation;
                    t.localScale = localScale;
                    break;
                default:
                    t.localPosition = localPosition;
                    t.localRotation = localRotation;
                    t.localScale = localScale;
                    break;
            }
            
        }

        /// <summary>
        /// Updates TTransform if neccessary
        /// </summary>
        /// <param name="transform">the Transform to check against</param>
        /// <returns>true if Transform has changed</returns>
        public bool Changed(Transform transform)
        {
            if (this == transform)
                return false;
            else
            {
                FromTransform(transform);
                return true;
            }
        }

        public override int GetHashCode()
        {
            return localPosition.GetHashCode() ^ localRotation.GetHashCode() << 2 ^ localScale.GetHashCode() >> 2 ^ rotation.GetHashCode() >> 1;
        }
        public override bool Equals(object other)
        {
            if (!(other is TTransform))
            {
                return false;
            }
            TTransform r = (TTransform)other;
            return localPosition.Equals(r.localPosition) && localRotation.Equals(r.localRotation) && localScale.Equals(r.localScale) && rotation.Equals(r.rotation);
        }
      
        public static bool operator == (TTransform a, Transform b)
        {
            return a.localPosition==b.localPosition &&
                   a.localRotation==b.localRotation &&
                   a.localScale==b.localScale &&
                   a.rotation==b.rotation;
        }

        public static bool operator ==(TTransform a, TTransform b)
        {
            return a.localPosition == b.localPosition &&
                   a.localRotation == b.localRotation &&
                   a.localScale == b.localScale &&
                   a.rotation==b.rotation;
        }

        public static bool operator !=(TTransform a, Transform b)
        {
            return a.localPosition != b.localPosition ||
                   a.localRotation != b.localRotation ||
                   a.localScale != b.localScale ||
                   a.rotation!=b.rotation;
        }

        public static bool operator !=(TTransform a, TTransform b)
        {
            return a.localPosition != b.localPosition ||
                   a.localRotation != b.localRotation ||
                   a.localScale != b.localScale ||
                   a.rotation!=b.rotation;
        }

        public static implicit operator bool(TTransform a)
        {
            return !object.ReferenceEquals(a, null);
        }
    }

    [System.Serializable]
    public struct FloatRegion
    {
        public float From;
        public float To;
        public bool SimpleValue;

        public FloatRegion(float value)
        {
            From = value;
            To = value;
            SimpleValue = true;
        }

        public FloatRegion(float A, float B)
        {
            this.From = A;
            this.To = B;
            SimpleValue = false;
        }

        public static FloatRegion ZeroOne
        { 
            get { return new FloatRegion(0,1); }
        }

        public void MakePositive()
        {
            if (To < From)
            {
                float temp = To;
                To = From;
                From = temp;
            }
        }

        public void Clamp(float low, float high)
        {
            Low = Mathf.Clamp(Low, low, high);
            High = Mathf.Clamp(High, low, high);
        }

        public bool Positive
        {
            get { return From <= To; }
        }

        public float Low
        {
            get { return (Positive) ? From : To; }
            set
            {
                if (Positive)
                    From = value;
                else
                    To = value;
            }
        }

        public float High
        {
            get { return (Positive) ? To : From; }
            set
            {
                if (Positive)
                    To = value;
                else
                    From = value;
            }
        }

        public float Random
        {
            get
            {
                return UnityEngine.Random.Range(From, To);
            }
        }

        public float Next
        {
            get
            {
                if (SimpleValue)
                    return From;
                else
                    return Random;
            }
        }

        public float Length
        {
            get
            {
                return To - From;
            }
        }

        public float LengthPositive
        {
            get { return (Positive) ? To - From : From - To; }
        }

        public override string ToString()
        {
            return string.Format("({0:F1}-{1:F1})", From,To);
        }
       
        public override int GetHashCode()
        {
            return From.GetHashCode() ^ To.GetHashCode() << 2;
        }
        public override bool Equals(object other)
        {
            if (!(other is FloatRegion))
            {
                return false;
            }
            FloatRegion r = (FloatRegion)other;
            return From.Equals(r.From) && To.Equals(r.To);
        }

        public static FloatRegion operator +(FloatRegion a, FloatRegion b)
        {
            return new FloatRegion(a.From + b.From, a.To + b.To);
        }

        public static FloatRegion operator -(FloatRegion a, FloatRegion b)
        {
            return new FloatRegion(a.From - b.From, a.To - b.To);
        }

        public static FloatRegion operator -(FloatRegion a)
        {
            return new FloatRegion(-a.From, -a.To);
        }

        public static FloatRegion operator *(FloatRegion a, float v)
        {
            return new FloatRegion(a.From * v, a.To * v);
        }

        public static FloatRegion operator *(float v,FloatRegion a)
        {
            return new FloatRegion(a.From * v, a.To * v);
        }

        public static FloatRegion operator /(FloatRegion a, float v)
        {
            return new FloatRegion(a.From / v, a.To / v);
        }

        public static bool operator ==(FloatRegion lhs, FloatRegion rhs)
        {
            return lhs.SimpleValue == rhs.SimpleValue && Mathf.Approximately(lhs.From, rhs.From) && Mathf.Approximately(lhs.To, rhs.To);
        }
        public static bool operator !=(FloatRegion lhs, FloatRegion rhs)
        {
            return lhs.SimpleValue != rhs.SimpleValue || !Mathf.Approximately(lhs.From, rhs.From) || !Mathf.Approximately(lhs.To, rhs.To);
        }
    }

    [System.Serializable]
    public struct IntRegion
    {
        public int From;
        public int To;
        public bool SimpleValue;
        

        public IntRegion(int value)
        {
            From = value;
            To = value;
            SimpleValue = true;
        
        }

        public IntRegion(int A, int B)
        {
            this.From = A;
            this.To = B;
            SimpleValue = false;
        
        }
        
        public static IntRegion ZeroOne
        {
            get { return new IntRegion(0, 1); }
        }

        public void MakePositive()
        {
            if (To < From)
            {
                int temp = To;
                To = From;
                From = temp;
            }
        }

        public void Clamp(int low, int high) 
        {
            Low = Mathf.Clamp(Low, low, high);
            High = Mathf.Clamp(High, low, high);
        }

        public bool Positive
        {
            get { return From <= To; }
        }

        public int Low
        {
            get { return (Positive) ? From : To; }
            set
            {
                if (Positive)
                    From = value;
                else
                    To = value;
            }
        }

        public int High
        {
            get { return (Positive) ? To : From; }
            set
            {
                if (Positive)
                    To = value;
                else
                    From = value;
            }
        }

        public int Random
        {
            get
            {
                return UnityEngine.Random.Range(From, To);
            }
        }

        public int Length
        {
            get
            {
                return To - From;
            }
        }

        public int LengthPositive
        {
            get { return (Positive) ? To - From : From - To; }
        }

        public override string ToString()
        {
            return string.Format("({0}-{1})", From, To);
        }

        public override int GetHashCode()
        {
            return From.GetHashCode() ^ To.GetHashCode() << 2;
        }
        public override bool Equals(object other)
        {
            if (!(other is IntRegion))
            {
                return false;
            }
            IntRegion r = (IntRegion)other;
            return From.Equals(r.From) && To.Equals(r.To);
        }

        public static IntRegion operator +(IntRegion a, IntRegion b)
        {
            return new IntRegion(a.From + b.From, a.To + b.To);
        }

        public static IntRegion operator -(IntRegion a, IntRegion b)
        {
            return new IntRegion(a.From - b.From, a.To - b.To);
        }

        public static IntRegion operator -(IntRegion a)
        {
            return new IntRegion(-a.From, -a.To);
        }

        public static IntRegion operator *(IntRegion a, int v)
        {
            return new IntRegion(a.From * v, a.To * v);
        }

        public static IntRegion operator *(int v, IntRegion a)
        {
            return new IntRegion(a.From * v, a.To * v);
        }

        public static IntRegion operator /(IntRegion a, int v)
        {
            return new IntRegion(a.From / v, a.To / v);
        }

        public static bool operator ==(IntRegion lhs, IntRegion rhs)
        {
            return lhs.From == rhs.From && lhs.To == rhs.To && lhs.SimpleValue!=rhs.SimpleValue;
        }
        public static bool operator !=(IntRegion lhs, IntRegion rhs)
        {
            return lhs.From != rhs.From || lhs.To != rhs.To || lhs.SimpleValue != rhs.SimpleValue;
        }
    }

    /// <summary>
    /// Simple but effective ShuffleBag<T> implementation
    /// </summary>
    public class WeightedRandom<T>
    {
        List<T> mData;
        int mCurrentPosition = -1;
        T mCurrentItem;

        public int Seed { get; set; }

        public bool RandomizeSeed { get; set; }

        private int Capacity { get { return mData.Capacity; } }
        public int Size { get { return mData.Count; } }

        public WeightedRandom(int initCapacity = 0)
        {
            mData = new List<T>(initCapacity);
#if PRE_UNITY_5_4
            Seed = Random.seed;
#endif
        }

        /// <summary>
        /// Adds items to the bag
        /// </summary>
        /// <param name="item">the item</param>
        /// <param name="amount">number of times this item should be added</param>
        public void Add(T item, int amount)
        {
            for (int i = 0; i < amount; i++)
                mData.Add(item);
            mCurrentPosition = Size - 1;
        }

        /// <summary>
        /// Gets a random item from the bag
        /// </summary>
        /// <returns>an item</returns>
        public T Next()
        {
            if (mCurrentPosition < 1)
            {
                mCurrentPosition = Size - 1;
                mCurrentItem = mData[0];
                return mCurrentItem;
            }
#if PRE_UNITY_5_4
            int s = Random.seed;
            if (RandomizeSeed)
                Seed = Random.Range(0, int.MaxValue);
            Random.seed = Seed;
            int idx = Random.Range(0, mCurrentPosition);
            Random.seed = s;
#else
            Random.State s=Random.state;
            if (RandomizeSeed)
                Seed = Random.Range(0, int.MaxValue);
            Random.InitState(Seed);
            int idx = Random.Range(0, mCurrentPosition);
            Random.state = s;
#endif
            mCurrentItem = mData[idx];
            mData[idx] = mData[mCurrentPosition];
            mData[mCurrentPosition] = mCurrentItem;
            mCurrentPosition--;
            return mCurrentItem;
        }

        /// <summary>
        /// Refill the bag
        /// </summary>
        public void Reset()
        {
            mCurrentPosition = Size - 1;
        }

        /// <summary>
        /// Clear the bag
        /// </summary>
        public void Clear()
        {
            mData.Clear();
            mCurrentPosition = -1;
        }

    }

    public class Ring<T> : IList<T>
    {
        List<T> mList;
        public int Size { get; private set; }

        int mIndex;

        public Ring(int size)
        {
            mList = new List<T>(size);
            Size = size;
        }

        public void Add(T item)
        {
            if (mList.Count == Size)
            {
                mList[mIndex++] = item;
                if (mIndex == mList.Count)
                    mIndex = 0;
            }
            else
                mList.Add(item);
        }

        public void Clear()
        {
            mList.Clear();
            mIndex = 0;
        }


        public int IndexOf(T item)
        {
            return mList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            throw new System.NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new System.NotSupportedException();
        }

        public T this[int index]
        {
            get
            {
                return mList[index];
            }
            set
            {
                mList[index] = value;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return mList.GetEnumerator();
        }


        public bool Contains(T item)
        {
            return mList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            mList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return mList.Count; }
        }

        public bool IsReadOnly
        {
            get { throw new System.NotSupportedException(); }
        }

        public bool Remove(T item)
        {
            return mList.Remove(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }

    public class Pool<T> : IPool
    {
        List<T> mObjects = new List<T>();

        public string Identifier { get; set; }

        public PoolSettings Settings { get; protected set; }

        public System.Type Type
        {
            get { return typeof(T); }
        }

        double mLastTime;
        double mDeltaTime;


        public Pool(PoolSettings settings = null)
        {
            Settings = settings ?? new PoolSettings();
            Identifier = typeof(T).FullName;
            mLastTime = DTTime.TimeSinceStartup + UnityEngine.Random.Range(0, Settings.Speed);
            if (Settings.Prewarm)
                Reset();
        }

        public void Update()
        {
            mDeltaTime += DTTime.TimeSinceStartup - mLastTime;
            mLastTime = DTTime.TimeSinceStartup;

            if (Settings.Speed > 0)
            {
                int c = (int)(mDeltaTime / Settings.Speed);
                mDeltaTime -= c;

                if (Count > Settings.Threshold)
                {
                    c = Mathf.Min(c, Count - Settings.Threshold);
                    while (c-- > 0)
                    {
                        destroy(mObjects[0]);
                        mObjects.RemoveAt(0);
                        log("Threshold exceeded: Deleting item");
                    }
                }
                else if (Count < Settings.MinItems)
                {
                    c = Mathf.Min(c, Settings.MinItems - Count);
                    while (c-- > 0)
                    {
                        mObjects.Add(create());
                        log("Below MinItems: Adding item");
                    }
                }
            }
            else
                mDeltaTime = 0;
        }

        public void Reset()
        {
            if (Application.isPlaying)
            {
                while (Count < Settings.MinItems)
                {
                    mObjects.Add(create());
                }
                while (Count > Settings.Threshold)
                {
                    destroy(mObjects[0]);
                    mObjects.RemoveAt(0);
                }
                log("Prewarm/Reset");
            }
        }

        public void Clear()
        {
            log("Clear");
            for (int i = 0; i < Count; i++)
                destroy(mObjects[i]);
            mObjects.Clear();
        }

        public int Count
        {
            get { return mObjects.Count; }
        }

        public virtual T Pop(Transform parent = null)
        {
            T item = default(T);
            if (Count > 0)
            {
                item = mObjects[0];
                mObjects.RemoveAt(0);

            }
            else
            {
                if (Settings.AutoCreate || !Application.isPlaying)
                {
                    log("Auto create item");
                    item = create();

                }
            }
            if (item != null)
            {
                sendAfterPop(item);
                setParent(item, parent);
                log("Pop " + item);
            }

            return item;
        }

        public virtual void Push(T item)
        {
            log("Push " + item);
            if (Application.isPlaying && item != null)
            {
                sendBeforePush(item);
                mObjects.Add(item);
            }
        }

        protected virtual void sendBeforePush(T item)
        {
            if (item is IPoolable)
                ((IPoolable)item).OnBeforePush();
        }

        protected virtual void sendAfterPop(T item)
        {
            if (item is IPoolable)
                ((IPoolable)item).OnAfterPop();
        }

        protected virtual void setParent(T item, Transform parent)
        {
        }

        protected virtual T create()
        {
            return System.Activator.CreateInstance<T>();
        }

        protected virtual void destroy(T item)
        {
        }

        void log(string msg)
        {
            if (Settings.Debug)
                Debug.Log(string.Format("[{0}] ({1} items) {2}", Identifier, Count, msg));
        }
    }


    [System.Serializable]
    public class PoolSettings
    {
        [SerializeField]
        bool m_Prewarm = false;
        [SerializeField]
        bool m_AutoCreate = true;
        [SerializeField]
        bool m_AutoEnableDisable = true;
        [Positive]
        [SerializeField]
        int m_MinItems = 0;

        [Positive]
        [SerializeField]
        int m_Threshold = 0;

        [Positive]
        [SerializeField]
        float m_Speed = 1;




        public bool Debug;

        public PoolSettings() { }

        public PoolSettings(PoolSettings src)
        {
            Prewarm = src.Prewarm;
            AutoCreate = src.AutoCreate;
            MinItems = src.MinItems;
            Threshold = src.Threshold;
            Speed = src.Speed;
            Debug = src.Debug;
        }

        public bool Prewarm
        {
            get { return m_Prewarm; }
            set
            {
                if (m_Prewarm != value)
                    m_Prewarm = value;
            }
        }

        public bool AutoCreate
        {
            get { return m_AutoCreate; }
            set
            {
                if (m_AutoCreate != value)
                    m_AutoCreate = value;
            }
        }

        public bool AutoEnableDisable
        {
            get { return m_AutoEnableDisable; }
            set
            {
                if (m_AutoEnableDisable != value)
                    m_AutoEnableDisable = value;
            }
        }

        public int MinItems
        {
            get { return m_MinItems; }
            set
            {
                int v = Mathf.Max(0, value);
                if (m_MinItems != v)
                    m_MinItems = v;
            }
        }

        public int Threshold
        {
            get { return m_Threshold; }
            set
            {
                int v = Mathf.Max(MinItems, value);
                if (m_Threshold != v)
                    m_Threshold = v;
            }
        }

        public float Speed
        {
            get { return m_Speed; }
            set
            {
                float v = Mathf.Max(0, value);
                if (m_Speed != v)
                    m_Speed = v;
            }
        }

        public void OnValidate()
        {
            MinItems = m_MinItems;
            Threshold = m_Threshold;
            Speed = m_Speed;
        }
    }


    public interface IPool
    {
        string Identifier { get; set; }
        PoolSettings Settings { get; }
        void Clear();
        void Reset();
        void Update();
        int Count { get; }
    }

    public interface IPoolable
    {
        void OnBeforePush();
        void OnAfterPop();
    }
}
