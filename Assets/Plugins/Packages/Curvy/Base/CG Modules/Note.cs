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
    [ModuleInfo("Note", ModuleName="Note", Description = "Creates a note")]
    [HelpURL(CurvySpline.DOCLINK + "cgnote")]
    public class Note : CGModule, INoProcessing
    {

        [SerializeField, TextArea(3, 10)]
        string m_Note;

        public string NoteText
        {
            get { return m_Note; }
            set
            {
                if (m_Note != value)
                    m_Note = value;
            }
        }

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        protected override void OnEnable()
        {
            base.OnEnable();
            Properties.MinWidth = 200;
            Properties.LabelWidth = 50;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            NoteText = m_Note;
        }
#endif

        /*! \endcond */
        #endregion
      
    }
}
