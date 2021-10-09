// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Modifier/TRS Shape", ModuleName="TRS Shape", Description = "Transform,Rotate,Scale a Shape")]
    [HelpURL(CurvySpline.DOCLINK + "cgtrsshape")]
    public class ModifierTRSShape : TRSModuleBase, IOnRequestPath
    {
        [HideInInspector]
        [InputSlotInfo(typeof(CGShape), Name = "Shape A", ModifiesData = true)]
        public CGModuleInputSlot InShape = new CGModuleInputSlot();

        [HideInInspector]
        [OutputSlotInfo(typeof(CGShape))]
        public CGModuleOutputSlot OutShape = new CGModuleOutputSlot();

        #region ### Public Properties ###

        public float PathLength
        {
            get
            {
                return (IsConfigured) ? InShape.SourceSlot().OnRequestPathModule.PathLength : 0;
            }
        }

        public bool PathIsClosed
        {
            get
            {
                return (IsConfigured) ? InShape.SourceSlot().OnRequestPathModule.PathIsClosed : false;
            }
        }

        #endregion

        #region ### IOnRequestProcessing ###

        public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
        {
            if (requestedSlot == OutShape)
            {
                var Data = InShape.GetData<CGShape>(requests);

                Matrix4x4 mat = Matrix;
                for (int i = 0; i < Data.Count; i++)
                    Data.Position[i] = mat.MultiplyPoint3x4(Data.Position[i]);

                Data.Recalculate();
                return new CGData[1] { Data };

            }
            return null;
        }

        #endregion



      
        
    }
}
