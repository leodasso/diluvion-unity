// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.CurvyEditor.Controllers
{
    
    public class CurvyControllerEditor<T> : CurvyEditorBase<T> where T:CurvyController
    {
        bool mRunningInEditor;
        bool mPreviewFoldout;
        

        protected override void OnEnable()
        {
            base.OnEnable();
        
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            StopPreview();
        }

        protected override void OnReadNodes()
        {
            var node = Node.AddSection("Preview", ShowPreviewButtons);
            node.Expanded = false;
            node.SortOrder = 5000;
        }

        /// <summary>
        /// Start editor preview
        /// </summary>
        public virtual void StartPreview()
        {

            if (mRunningInEditor)
            {
                if (Target.IsPaused)
                    Target.Play();
                else
                    Target.Pause();
            } else
            {
                DTTime.InitializeEditorTime();
                Target.BeginPreview();
                EditorApplication.update -= editorUpdate;
                EditorApplication.update += editorUpdate;
                mRunningInEditor = true;
            }
        }

        /// <summary>
        /// Stop editor preview
        /// </summary>
        public virtual void StopPreview()
        {
            if (Target != null)
            {
                EditorApplication.update -= editorUpdate;
                if (mRunningInEditor)
                    Target.EndPreview();
            }
            mRunningInEditor = false;
            
        }

        void editorUpdate()
        {
            DTTime.UpdateEditorTime();
            Target.EditorUpdate();
            if (!Target.IsPlaying)
                StopPreview();
        }

        /// <summary>
        /// Show the preview buttons
        /// </summary>
        protected void ShowPreviewButtons(DTInspectorNode node)
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = !Application.isPlaying;
            if (GUILayout.Toggle(mRunningInEditor, new GUIContent(CurvyStyles.TexPlay, "Play/Pause in Editor"), GUI.skin.button) != mRunningInEditor)
                StartPreview();
            if (GUILayout.Button(new GUIContent(CurvyStyles.TexStop, "Stop/Reset")))
                StopPreview();
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
        
    }
}
