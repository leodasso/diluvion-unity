// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevTools.Extensions;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.DevToolsEditor;

namespace FluffyUnderware.CurvyEditor
{
    [CustomEditor(typeof(CurvyConnection))]
   public class CurvyConnectionEditor : CurvyEditorBase<CurvyConnection>
   {
        List<CurvySplineSegment> sorted;

        

        public new void OnSceneGUI()
        {
            CurvySplineSegment cp;
            var cam = SceneView.currentDrawingSceneView.camera;
            if (cam && Target)
            {
                List<string>samePos=new List<string>();
                
                for (int i = 0; i < Target.ControlPoints.Count; i++)
                {
                    cp = Target.ControlPoints[i];
                    Color c = Color.black;    
                    if (cp.ConnectionSyncPosition)
                    {
                        samePos.Add(cp.ToString());
                        c = new Color(1, 0, 0);
                    }
                    else
                    {
                        if (cp.ConnectionSyncRotation)
                            c = new Color(1, 1, 0);

                        DTGUI.PushColor(c);
                        Handles.Label(DTHandles.TranslateByPixel(cp.transform.position, 12, -12), new GUIContent(cp.ToString()));
                        DTGUI.PopColor();
                    }
                    
                }
                if (samePos.Count > 0)
                {
                    DTGUI.PushColor(Color.white);
                    var P1 = cam.ScreenToWorldPoint(cam.WorldToScreenPoint(Target.transform.position) + new Vector3(12, -12, 0));
                    Handles.Label(P1, new GUIContent(string.Join("\n",samePos.ToArray())));
                    DTGUI.PopColor();
                }

            }
        }

        protected override void OnCustomInspectorGUI()
        {
            GUILayout.Space(5);
            base.OnCustomInspectorGUI();
            var sel = DTSelection.GetAs<CurvySplineSegment>();

            if (sel)
            {
                sorted = new List<CurvySplineSegment>(Target.ControlPoints);
                sorted.Sort(delegate(CurvySplineSegment a, CurvySplineSegment b)
                {
                    if (a == sel || b==sel)
                        return (b==sel).CompareTo(a==sel);
                    else if (a.FollowUp==sel || b.FollowUp==sel)
                        return (b.FollowUp==sel).CompareTo(a.FollowUp==sel);
                    else
                    return a.ToString().CompareTo(b.ToString());
                });
            }
            else
                sorted = Target.ControlPoints;


            headerGUI(GUILayoutUtility.GetRect(0, 42));
            for (int i = 0; i < sorted.Count; i++)
            {
                itemGUI(GUILayoutUtility.GetRect(0, 42), i, true, true);
            }
            
            //if (Target.ControlPoints.Count == 2 && Target.ControlPoints[0].CanConnectTo(Target.ControlPoints[1],false))
                quickSetsGUI();
            if (GUILayout.Button("Delete Connection"))
            {
                Target.Delete();
                GUIUtility.ExitGUI();
            }
            GUILayout.Space(5);
        }

        Rect[] getRects(Rect rect, bool header=false)
        {
            var res = new Rect[9];
            float w1=rect.width-80;
            float w2 = (header) ? 40 : 0;
            res[0] = new Rect(rect.x, rect.y, w1, 21);
            res[1] = new Rect(rect.width-60-w2, rect.y, 20+w2*2, 21);
            res[8] = new Rect(rect.width - 40 - w2, rect.y, 20 + w2, 21);
            res[2] = new Rect(rect.xMax - 18, rect.y+2, 20, 36);
            float hl = (header) ? 17 : 21;
            res[3] = new Rect(rect.x, rect.y + hl, 60, 21);
            float w3 = 100;
            res[4] = new Rect(rect.x + 50, rect.y + hl, Mathf.Max(40, rect.width - w3-50), 21);
            res[5] = new Rect(rect.width - 58, res[4].y, 18, 21);
            res[6] = new Rect(rect.width - 40, res[4].y, 18, 21);
            res[7] = new Rect(rect.width - 22, res[4].y, 18, 21);
            
            return res;
        }

        void headerGUI(Rect rect)
        {
            var r = getRects(rect,true);

            if (Event.current.type == EventType.Repaint)
            {
                GUI.Box(rect, "");
            }

            GUI.Label(r[0], "Control Point",EditorStyles.boldLabel);
            GUI.Label(r[1], "Sync Pos / Rot", EditorStyles.boldLabel);


            GUI.Label(r[3], "Heading", EditorStyles.boldLabel);
            GUI.Label(r[5], "<", EditorStyles.boldLabel);
            GUI.Label(r[6], "@", EditorStyles.boldLabel);
            GUI.Label(r[7], ">", EditorStyles.boldLabel);
            
        }

        void itemGUI(Rect rect, int index, bool isActive, bool isFocused)
        {
            var item=sorted[index];
            var r = getRects(rect);

            if (item.gameObject == Selection.activeGameObject)
                DTHandles.DrawSolidRectangleWithOutline(rect.ScaleBy(4, 2), new Color(0, 0, 0.2f, 0.1f), new Color(.24f,.37f,.59f));
            else if (index>0)
            {
                DTHandles.PushHandlesColor(new Color(0.1f,0.1f,0.1f));
                Handles.DrawLine(new Vector2(rect.xMin-5, rect.yMin), new Vector2(rect.xMax+4, rect.yMin));
                DTHandles.PopHandlesColor();
            }

            bool repaint=false;
            var cnt=new GUIContent(item.ToString(),"Click to select");
            r[0].width = DTStyles.HtmlLinkLabel.CalcSize(cnt).x;
            if (DTGUI.LinkButton(r[0], cnt, ref repaint))// EditorStyles.label))
            {
                DTSelection.SetGameObjects(sorted[index]);
            }
            if (repaint)
                Repaint();
            
            item.ConnectionSyncPosition=GUI.Toggle(r[1], item.ConnectionSyncPosition, "");
            item.ConnectionSyncRotation = GUI.Toggle(r[8], item.ConnectionSyncRotation, "");
            if (GUI.Button(r[2], new GUIContent(CurvyStyles.DeleteSmallTexture, "Delete"),CurvyStyles.ImageButton))
            {
                item.Disconnect();
                CurvyProject.Instance.ScanConnections();
                GUIUtility.ExitGUI();
                
            }
            if (item.CanHaveFollowUp)
            {
                GUI.Label(r[3], "Head to");
                doFollowUpPopup(r[4], item);
                bool b = item.FollowUpHeading == ConnectionHeadingEnum.Minus;
                if (GUI.Toggle(r[5], b, new GUIContent("", "Head to spline start")) != b)
                    item.FollowUpHeading = (!b) ? ConnectionHeadingEnum.Minus : ConnectionHeadingEnum.Auto;
                b = item.FollowUpHeading == ConnectionHeadingEnum.Sharp;
                if (GUI.Toggle(r[6], b, new GUIContent("", "No Heading")) != b)
                    item.FollowUpHeading = (!b) ? ConnectionHeadingEnum.Sharp : ConnectionHeadingEnum.Auto;
                b = item.FollowUpHeading == ConnectionHeadingEnum.Plus;
                if (GUI.Toggle(r[7], b, new GUIContent("", "Head to spline end")) != b)
                    item.FollowUpHeading = (!b) ? ConnectionHeadingEnum.Plus : ConnectionHeadingEnum.Auto;
            }
        }

        void quickSetsGUI()
        {
            //DrawSection("connection_quicksets", "Presets");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(CurvyStyles.TexConnection, "None")))
                setConType(false, false);

            if (GUILayout.Button(new GUIContent(CurvyStyles.TexConnectionPos, "Pos")))
                setConType(true, false);

            if (GUILayout.Button(new GUIContent(CurvyStyles.TexConnectionRot, "Rot")))
                setConType(false, true);
            if (GUILayout.Button(new GUIContent(CurvyStyles.TexConnectionFull, "Full")))
            {
                setConType(true,true);
            }
            EditorGUILayout.EndHorizontal();
            
        }

        void setConType(bool syncPos, bool syncRot)
        {
            foreach (var cp in Target.ControlPoints)
            {
                cp.ConnectionSyncPosition = syncPos;
                cp.ConnectionSyncRotation = syncRot;
                cp.RefreshTransform(true,true);
                Target.AutoSetFollowUp();
            }

        }

        void doFollowUpPopup(Rect r, CurvySplineSegment item)
        {
            var possibleTargets = (from cp in Target.ControlPoints
                                  where cp != item
                                  select cp).ToList();
            int sel = possibleTargets.IndexOf(item.FollowUp)+1;
            
            var content = (from cp in possibleTargets
                           select cp.ToString()).ToList();
            content.Insert(0, " ");
            EditorGUI.BeginChangeCheck();

            if (item.FollowUp && item.FollowUp.gameObject == Selection.activeGameObject)
                DTHandles.DrawSolidRectangleWithOutline(r.ScaleBy(2, 0).ShiftBy(1, -2), new Color(0f, 0, 0, 0.1f), new Color(.24f, .37f, .59f));
                
            sel=EditorGUI.Popup(r, sel, content.ToArray());

            if (EditorGUI.EndChangeCheck())
            {
                
                if (sel > 0)
                    item.SetFollowUp(possibleTargets[sel-1]);
                else
                    item.SetFollowUp(null);
                
            }
        }

        
   }

}
