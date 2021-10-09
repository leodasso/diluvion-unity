// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================


using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;
using FluffyUnderware.Curvy.Utils;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Create/Path Line Renderer",ModuleName="Create Path Line Renderer", Description="Feeds a Line Renderer with a Path")]
    public class CreatePathLineRenderer : CGModule
    {
        
        [HideInInspector]
        [InputSlotInfo(typeof(CGPath))]
        public CGModuleInputSlot InPath = new CGModuleInputSlot();

        #region ### Serialized Fields ###

        public LineRenderer LineRenderer
        {
            get
            {
                if (mLineRenderer == null)
                    mLineRenderer = GetComponent<LineRenderer>();
                return mLineRenderer;
            }
        }

		#endregion
        #region ### Public Properties ###

		// Gets whether the module is properly configured i.e. has everything needed to work like intended
        public override bool IsConfigured
        {
            get
            {
                return base.IsConfigured;
            }
        }

		// Gets whether the module and all its dependencies are fully initialized
        public override bool IsInitialized
        {
            get
            {
                return base.IsInitialized;
            }
        }

        #endregion

        LineRenderer mLineRenderer;

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        protected override void Awake()
        {
            base.Awake();
            createLR();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            //Properties.MinWidth = 250;
            //Properties.LabelWidth = 80;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif

		public override void Reset()
		{
			base.Reset();
		}

		/*! \endcond */
		#endregion

		#region ### Module Overrides ###
		
        public override void Refresh()
        {
            base.Refresh();
            var path = InPath.GetData<CGPath>();
#if UNITY_5_6_OR_NEWER
            if (path != null)
            {
                LineRenderer.positionCount = path.Position.Length;
                LineRenderer.SetPositions(path.Position);
            }
            else
                LineRenderer.positionCount = 0;
#else
            if (path != null)
            {
                LineRenderer.numPositions = path.Position.Length;
                for (int v = 0; v < path.Position.Length; v++)
                    LineRenderer.SetPosition(v, path.Position[v]);
            }
            else
                LineRenderer.numPositions = 0;
#endif
            
        }

        // Called when a module's state changes (Link added/removed, Active toggles etc..)
        //public override void OnStateChange()
        //{
        //    base.OnStateChange();
        //}

        // Called after a module was copied to a template
        //public override void OnTemplateCreated() 
        //{
        //	base.OnTemplateCreated();
        //}


#endregion

        void createLR()
        {
            if (LineRenderer == null)
            {
                mLineRenderer = gameObject.AddComponent<LineRenderer>();
                mLineRenderer.useWorldSpace = false;
#if UNITY_5_6_OR_NEWER
                mLineRenderer.textureMode = LineTextureMode.Tile;
#endif
                mLineRenderer.sharedMaterial= CurvyUtility.GetDefaultMaterial();
            }
        }

    }
}
