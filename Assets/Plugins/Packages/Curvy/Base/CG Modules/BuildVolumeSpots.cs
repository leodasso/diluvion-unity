// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;
using System.Collections.Generic;


namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Build/Volume Spots",ModuleName="Volume Spots", Description="Generate spots along a path/volume", UsesRandom=true)]
    [HelpURL(CurvySpline.DOCLINK + "cgvolumespots")]
    public class BuildVolumeSpots : CGModule
    {
        [HideInInspector]
        [InputSlotInfo(typeof(CGPath), Name="Path/Volume")]
        public CGModuleInputSlot InPath = new CGModuleInputSlot();
        
        [HideInInspector]
        [InputSlotInfo(typeof(CGBounds),Array=true)]
        public CGModuleInputSlot InBounds = new CGModuleInputSlot();
        
        [HideInInspector]
        [OutputSlotInfo(typeof(CGSpots))]
        public CGModuleOutputSlot OutSpots = new CGModuleOutputSlot();

        #region ### Serialized Fields ###

        [Tab("General")]
        [FloatRegion(RegionOptionsPropertyName = "RangeOptions", Precision = 4)]
        [SerializeField]
        FloatRegion m_Range = FloatRegion.ZeroOne;
        [SerializeField]
        bool m_UseVolume;
        [Tooltip("Dry run without actually creating spots?")]
        [SerializeField]
        bool m_Simulate;
        [Section("Default/General/Cross")]
        [SerializeField]
        [RangeEx(-1, 1)]
        float m_CrossBase;
        [SerializeField]
        AnimationCurve m_CrossCurve = AnimationCurve.Linear(0, 0, 1, 0);

        [Tab("Groups")]
        [ArrayEx(Space = 10)]
        [SerializeField]
        List<CGBoundsGroup> m_Groups = new List<CGBoundsGroup>();

        [IntRegion(UseSlider = false, RegionOptionsPropertyName = "RepeatingGroupsOptions", Options = AttributeOptionsFlags.Compact)]
        [SerializeField]
        IntRegion m_RepeatingGroups;
        [SerializeField]
        CurvyRepeatingOrderEnum m_RepeatingOrder = CurvyRepeatingOrderEnum.Row;

        [SerializeField]
        bool m_FitEnd;
        
        #endregion

        #region ### Public Properties ###

        #region - General Tab -

        public FloatRegion Range
        {
            get { return m_Range; }
            set
            {
                if (m_Range != value)
                    m_Range = value;
                Dirty = true;
            }
        }

        public bool UseVolume
        {
            get { return m_UseVolume; }
            set
            {
                if (m_UseVolume != value)
                    m_UseVolume = value;
                Dirty = true;
            }
        }

        public bool Simulate
        {
            get { return m_Simulate; }
            set
            {
                if (m_Simulate != value)
                    m_Simulate = value;
                Dirty = true;
            }
        }

        public float CrossBase
        {
            get { return m_CrossBase; }
            set
            {
                float v = Mathf.Repeat(value, 1);
                if (m_CrossBase != v)
                    m_CrossBase = v;
                Dirty = true;
            }
        }

        public AnimationCurve CrossCurve
        {
            get { return m_CrossCurve; }
            set
            {
                if (m_CrossCurve != value)
                    m_CrossCurve = value;
                Dirty = true;
            }
        }


        #endregion

        public List<CGBoundsGroup> Groups
        {
            get { return m_Groups; }
            set
            {
                if (m_Groups != value)
                    m_Groups = value;
            }
        }

        public CurvyRepeatingOrderEnum RepeatingOrder
        {
            get { return m_RepeatingOrder; }
            set
            {
                if (m_RepeatingOrder != value)
                    m_RepeatingOrder = value;
                Dirty = true;
            }
        }

        public int FirstRepeating
        {
            get { return m_RepeatingGroups.From; }
            set
            {
                int v = Mathf.Clamp(value, 0, Mathf.Max(0, GroupCount - 1));
                if (m_RepeatingGroups.From != v)
                    m_RepeatingGroups.From = v;

                Dirty = true;
            }
        }

        public int LastRepeating
        {
            get { return m_RepeatingGroups.To; }
            set
            {
                int v = Mathf.Clamp(value, FirstRepeating, Mathf.Max(0, GroupCount - 1));
                if (m_RepeatingGroups.To != v)
                    m_RepeatingGroups.To = v;
                Dirty = true;
            }
        }

        public bool FitEnd
        {
            get { return m_FitEnd; }
            set
            {
                if (m_FitEnd != value)
                    m_FitEnd = value;
                Dirty = true;
            }
        }


        public int GroupCount { get { return Groups.Count; } }

        public GUIContent[] BoundsNames
        {
            get
            {
                if (mBounds == null)
                    return new GUIContent[0];
                GUIContent[] v = new GUIContent[mBounds.Count];
                for (int i = 0; i < mBounds.Count; i++)
                    v[i] = new GUIContent(i.ToString()+":"+mBounds[i].Name);
                return v;
            }
        }

        public int[] BoundsIndices
        {
            get
            {
                if (mBounds == null)
                    return new int[0];
                int[] v = new int[mBounds.Count];
                for (int i = 0; i < mBounds.Count; i++)
                    v[i] = i;
                return v;
            }
        }

        public int Count { get; private set; }

        public CGSpots SimulatedSpots;


        #endregion

        #region ### Private Fields & Properties ###

        /// <summary>
        /// holds data to be rebased (end groups!)
        /// </summary>
        class GroupSet
        {
            public CGBoundsGroup Group;
            public float Length;
            public List<int> Items = new List<int>();
            public List<float> Distances = new List<float>();
        }

        WeightedRandom<int> mGroupBag;
        List<CGBounds> mBounds;
        bool mGroupsHaveDepth;

        int lastGroupIndex { get { return Mathf.Max(0, GroupCount - 1); } }

        RegionOptions<float> RangeOptions
        {
            get
            {
                return RegionOptions<float>.MinMax(0, 1);
            }
        }

        RegionOptions<int> RepeatingGroupsOptions
        {
            get
            {
                return RegionOptions<int>.MinMax(0, Mathf.Max(0, GroupCount - 1));
            }
        }

        CGPath Path
        {
            get;
            set;
        }

        CGVolume Volume
        {
            get { return Path as CGVolume; }
        }

        bool UsePath
        {
            get { return (Volume == null) || UseVolume; }
        }

        float Length
        {
            get { return (Path != null) ? Path.Length * m_Range.Length : 0; }
        }

        float StartDistance
        {
            get;
            set;
        }

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        protected override void OnEnable()
        {
            base.OnEnable();
            Properties.MinWidth = 350;
            //Properties.LabelWidth = 80;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            RepeatingOrder = m_RepeatingOrder;
        }
#endif

        public override void Reset()
        {
            base.Reset();
            m_Range = FloatRegion.ZeroOne;
            UseVolume = false;
            Simulate = false;
            CrossBase = 0;
            CrossCurve = AnimationCurve.Linear(0, 0, 1, 0);
            RepeatingOrder = CurvyRepeatingOrderEnum.Row;
            FirstRepeating = 0;
            LastRepeating = 0;
            FitEnd = false;

            Groups.Clear();
            AddGroup("Group");
        }

        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        public override void OnStateChange()
        {
            base.OnStateChange();
            if (!IsConfigured)
                Clear();
        }

        public void Clear()
        {
            Count = 0;
            SimulatedSpots = new CGSpots();
            OutSpots.SetData(SimulatedSpots);
        }

        public override void Refresh()
        {
            base.Refresh();
            mBounds = InBounds.GetAllData<CGBounds>();
            Path = InPath.GetData<CGPath>();
            if (Path != null && Volume == null && UseVolume)
                m_UseVolume = false;
            List<CGSpot> spots = new List<CGSpot>();
            List<GroupSet> endGroups = null;
            prepare();

            if (Path && mBounds.Count > 0 && mGroupsHaveDepth)
            {
                float remainingLength = Length;
                StartDistance = Path.FToDistance(m_Range.Low);
                float currentDistance = StartDistance;

                // Fixed start group(s)
                for (int g = 0; g < FirstRepeating; g++)
                {
                    addGroupItems(Groups[g], ref spots, ref remainingLength, ref currentDistance);
                    if (remainingLength <= 0)
                        break;
                }
                // Fixed end group(s)
                if (GroupCount - LastRepeating - 1 > 0)
                {
                    endGroups = new List<GroupSet>();
                    float endDist = 0;
                    for (int g = LastRepeating + 1; g < GroupCount; g++)
                    {
                        endGroups.Add(addGroupItems(Groups[g], ref spots, ref remainingLength, ref endDist, true));
                    }
                }
                // Mid-Groups, either in row or random
                if (RepeatingOrder == CurvyRepeatingOrderEnum.Row)
                {
                    int g = FirstRepeating;
                    while (remainingLength > 0)
                    {
                        addGroupItems(Groups[g++], ref spots, ref remainingLength, ref currentDistance);
                        if (g > LastRepeating)
                            g = FirstRepeating;
                    }
                }
                else
                {
                    while (remainingLength > 0)
                        addGroupItems(Groups[mGroupBag.Next()], ref spots, ref remainingLength, ref currentDistance);
                }
                // If we have previously generated endgroups data, shift them now
                if (endGroups != null)
                {
                    rebase(ref spots, ref endGroups, currentDistance, remainingLength);
                }

            }

            Count = spots.Count;

            SimulatedSpots = new CGSpots(spots);
            if (Simulate)
                OutSpots.SetData(new CGSpots());
            else
                OutSpots.SetData(SimulatedSpots);

        }

        public CGBoundsGroup AddGroup(string name)
        {
            var grp = new CGBoundsGroup(name);
            Groups.Add(grp);
            Dirty = true;
            return grp;
        }

        public void RemoveGroup(CGBoundsGroup group)
        {
            Groups.Remove(group);
            Dirty = true;
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */

        GroupSet addGroupItems(CGBoundsGroup group, ref List<CGSpot> spots, ref float remainingLength, ref float currentDistance, bool calcLengthOnly = false)
        {

            if (group.ItemCount == 0)
                return null;

            CGBounds currentBounds;
            int itemID;
            int added = 0;

            float gapBefore = group.SpaceBefore.Next;
            float gapAfter = group.SpaceAfter.Next;
            float remain = remainingLength - gapBefore;
            GroupSet vGroup = null;
            GroupSet endItems = new GroupSet();

            float dist = currentDistance + gapBefore;



            if (calcLengthOnly)
            {
                vGroup = new GroupSet();
                vGroup.Group = group;
                vGroup.Length = gapBefore + gapAfter;

            }
            // Fixed Start Item(s)

            for (int i = 0; i < group.FirstRepeating; i++)
            {
                itemID = group.Items[i].Index;
                currentBounds = getItemBounds(itemID);
                if (currentBounds)
                {
                    remain -= currentBounds.Depth;
                    if (remain > 0)
                    {
                        if (calcLengthOnly)
                        {
                            vGroup.Length += currentBounds.Depth;
                            vGroup.Items.Add(itemID);
                            vGroup.Distances.Add(dist);
                        }
                        else
                            spots.Add(getSpot(itemID, ref group, ref currentBounds, dist, remainingLength));

                        dist += currentBounds.Depth;
                        added++;
                    }
                    else
                    {
                        if (group.KeepTogether && added > 0)
                        {
                            spots.RemoveRange(spots.Count - added, added);
                        }
                        break;
                    }
                }
            }
            if (remain > 0)
            {
                float endDist = 0;
                // Fixed End Item(s)
                for (int i = group.LastRepeating + 1; i < group.ItemCount; i++)
                {
                    itemID = group.Items[i].Index;
                    currentBounds = getItemBounds(itemID);
                    if (currentBounds)
                    {
                        remain -= currentBounds.Depth;
                        if (remain > 0)
                        {
                            endItems.Length += currentBounds.Depth;
                            endItems.Items.Add(itemID);
                            endItems.Distances.Add(endDist);
                            endDist += currentBounds.Depth;
                        }
                        else
                            break;
                    }
                }


                if (remain > 0)
                {
                    // Mid Items
                    int itemIdx;
                    for (int i = group.FirstRepeating; i <= group.LastRepeating; i++)
                    {
                        itemIdx = (group.RepeatingOrder == CurvyRepeatingOrderEnum.Row) ? i : group.getRandomItemINTERNAL();
                        itemID = group.Items[itemIdx].Index;

                        currentBounds = getItemBounds(itemID);
                        if (currentBounds)
                        {
                            remain -= currentBounds.Depth;
                            if (remain > 0)
                            {
                                if (calcLengthOnly)
                                {
                                    vGroup.Length += currentBounds.Depth;
                                    vGroup.Items.Add(itemID);
                                    vGroup.Distances.Add(dist);
                                }
                                else
                                    spots.Add(getSpot(itemID, ref group, ref currentBounds, dist, remainingLength));

                                dist += currentBounds.Depth;
                                added++;
                            }
                            else
                            {
                                if (group.KeepTogether && added > 0)
                                {
                                    spots.RemoveRange(spots.Count - added, added);
                                }
                                break;
                            }
                        }
                    }
                }

                if (remain > 0 || !group.KeepTogether)
                {
                    for (int e = 0; e < endItems.Items.Count; e++)
                    {
                        var b = getItemBounds(endItems.Items[e]);
                        spots.Add(getSpot(endItems.Items[e], ref group, ref b, dist + endItems.Distances[e], remainingLength));
                        dist += b.Depth;
                    }
                }
            }

            remainingLength = remain - gapAfter;
            currentDistance = dist + gapAfter;
            return vGroup;
        }

        void rebase(ref List<CGSpot> spots, ref List<GroupSet> sets, float currentDistance, float remainingLength)
        {
            if (FitEnd)
            {
                currentDistance = Path.FToDistance(m_Range.To);
                for (int s = 0; s < sets.Count; s++)
                    currentDistance -= sets[s].Length;
            }
            GroupSet set;
            CGBounds bounds;
            for (int s = 0; s < sets.Count; s++)
            {
                set = sets[s];
                for (int i = 0; i < set.Items.Count; i++)
                {
                    bounds = getItemBounds(set.Items[i]);
                    spots.Add(getSpot(set.Items[i], ref set.Group, ref bounds, currentDistance + set.Distances[i], remainingLength));
                }
            }
        }

        CGSpot getSpot(int itemID, ref CGBoundsGroup group, ref CGBounds bounds, float startDist, float remainingDistance)
        {
            var spot = new CGSpot(itemID);
            float pathF = Path.DistanceToF(startDist + bounds.Depth / 2);
            Vector3 pos = Vector3.zero;
            Vector3 tan = Vector3.forward;
            Vector3 up = Vector3.up;

            float crossF = getCrossValue((startDist - StartDistance) / Length, group);
            if (group.RotationMode != CGBoundsGroup.RotationModeEnum.Independent)
            {
                if (UseVolume)
                    Volume.InterpolateVolume(pathF, crossF, out pos, out tan, out up);
                else
                    Path.Interpolate(pathF, crossF, out pos, out tan, out up);
                switch (group.RotationMode)
                {
                    case CGBoundsGroup.RotationModeEnum.Direction:
                        up = Vector3.up;
                        break;
                    case CGBoundsGroup.RotationModeEnum.Horizontal:
                        up = Vector3.up;
                        tan.y = 0;
                        break;
                }
            }
            else
                pos = (UseVolume) ? Volume.InterpolateVolumePosition(pathF, crossF) : Path.InterpolatePosition(pathF);

            // Orientation Setup
            if (Path.SourceIsManaged)
            {
                spot.Rotation = Quaternion.LookRotation(tan, up) *
                                Quaternion.Euler(group.RotationOffset.x + group.RotationScatter.x * Random.Range(-1,1),
                                                 group.RotationOffset.y + group.RotationScatter.y * Random.Range(-1, 1),
                                                 group.RotationOffset.z + group.RotationScatter.z * Random.Range(-1, 1));
                spot.Position = pos + spot.Rotation * new Vector3(0, group.Height.Next, 0);
            }
            else
            {

                spot.Rotation = Quaternion.LookRotation(tan, up) *
                                Quaternion.Euler(group.RotationOffset.x + group.RotationScatter.x * Random.Range(-1, 1),
                                                 group.RotationOffset.y + group.RotationScatter.y * Random.Range(-1, 1),
                                                 group.RotationOffset.z + group.RotationScatter.z * Random.Range(-1, 1));
                spot.Position = pos + spot.Rotation * new Vector3(0, group.Height.Next, 0);
                
            }
            return spot;
        }

        void prepare()
        {
            mGroupsHaveDepth = false;
            m_RepeatingGroups.MakePositive();
            m_RepeatingGroups.Clamp(0, GroupCount - 1);
            // Groups
            if (mGroupBag == null)
                mGroupBag = new WeightedRandom<int>();
            else
                mGroupBag.Clear();
            if (RepeatingOrder == CurvyRepeatingOrderEnum.Random)
            {
                for (int g = FirstRepeating; g <= LastRepeating; g++)
                    mGroupBag.Add(g, (int)(Groups[g].Weight * 10));
            }
            // Prepare Groups & ItemBags
            for (int g = 0; g < Groups.Count; g++)
            {
                Groups[g].PrepareINTERNAL();
                mGroupsHaveDepth = mGroupsHaveDepth || (getMinGroupDepth(Groups[g]) > 0);
            }

        }

        float getMinGroupDepth(CGBoundsGroup group)
        {
            float r = group.SpaceBefore.Low + group.SpaceAfter.Low;
            CGBounds b;
            for (int i = 0; i < group.ItemCount; i++)
            {
                b = getItemBounds(group.Items[i].Index);
                if (b)
                    r += b.Depth;
            }
            return r;
        }

        CGBounds getItemBounds(int itemIndex)
        {
            return (itemIndex >= 0 && itemIndex < mBounds.Count) ? mBounds[itemIndex] : null;
        }

        float getCrossValue(float globalF, CGBoundsGroup group)
        {
            switch (group.DistributionMode)
            {
                case CGBoundsGroup.DistributionModeEnum.Parent:
                    return DTMath.MapValue(-0.5f, 0.5f, CrossBase + m_CrossCurve.Evaluate(globalF) + group.PositionOffset.Next);
                case CGBoundsGroup.DistributionModeEnum.Self:
                    return DTMath.MapValue(-0.5f, 0.5f, group.PositionOffset.Next);
            }
            return 0;
        }

        /*! \endcond */
        #endregion

   


    }
}
