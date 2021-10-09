// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Modifier/TRS Mesh", ModuleName = "TRS Mesh", Description = "Transform,Rotate,Scale a VMesh")]
    [HelpURL(CurvySpline.DOCLINK + "cgtrsmesh")]
    public class ModifierTRSMesh : TRSModuleBase
    {
        [HideInInspector]
        [InputSlotInfo(typeof(CGVMesh),Array=true,ModifiesData=true)]
        public CGModuleInputSlot InVMesh = new CGModuleInputSlot();
        
        [HideInInspector]
        [OutputSlotInfo(typeof(CGVMesh),Array=true)]
        public CGModuleOutputSlot OutVMesh = new CGModuleOutputSlot();

       

        #region ### Public Methods ###

        public override void Refresh()
        {
            base.Refresh();
            if (OutVMesh.IsLinked)
            {
                var vMesh = InVMesh.GetAllData<CGVMesh>();
                var mat = Matrix;
                for (int i = 0; i < vMesh.Count; i++)
                    vMesh[i].TRS(mat);

                OutVMesh.SetData(vMesh);
            }

        }

        #endregion

      

        

       
    
    }
}
