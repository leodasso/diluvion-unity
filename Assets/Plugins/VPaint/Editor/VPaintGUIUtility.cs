using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.VPaint
{
	public class VPaintGUIUtility
	{		
		public static bool FoldoutMenu ()
		{
			bool b = false;
			Rect r = EditorGUILayout.BeginHorizontal(GUILayout.Width(16));
			EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
			GUILayout.Label("");
			EditorGUILayout.EndHorizontal();
			if(r.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				b = true;
				Event.current.Use();
			}
			EditorGUI.Foldout(r, true, GUIContent.none);
			return b;
		}
		
		public static Rect BoxArea (float margin, float border, int boxCount, Action contents)
		{
			return BoxArea(Vector4.one * margin, Vector4.one * border, boxCount, contents);
		}
		
		public static Rect BoxArea (Vector2 margin, Vector2 border, int boxCount, Action contents)
		{
			return BoxArea(new Vector4(margin.x, margin.x, margin.y, margin.y), new Vector4(border.x, border.x, border.y, border.y), boxCount, contents);
		}
		
		public static Rect BoxArea (Vector4 margin, Vector4 border, int boxCount, Action contents)
		{
			return Area(margin, ()=>{
				Rect r = EditorGUILayout.BeginHorizontal();
				int bc = EditorGUIUtility.isProSkin ? boxCount : (int)Mathf.Pow(boxCount, 2);
				for(int i = 0; i < bc; i++) GUI.Box(r, GUIContent.none);
				GUILayout.Space(border.x);
				EditorGUILayout.BeginVertical();
				GUILayout.Space(border.z);
				contents();
				GUILayout.Space(border.w);
				EditorGUILayout.EndVertical();
				GUILayout.Space(border.y);
				EditorGUILayout.EndHorizontal();
			});
		}
		
		public static Rect Area (float margin, Action contents)
		{
			return Area(Vector4.one * margin, contents);
		}
		
		public static Rect Area (Vector2 margin, Action contents)
		{
			return Area(new Vector4(margin.x, margin.x, margin.y, margin.y), contents);
		}
		
		public static Rect Area (Vector4 border, Action contents)
		{
			Rect r = EditorGUILayout.BeginHorizontal();
			GUILayout.Space(border.x);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(border.z);
			contents();
			GUILayout.Space(border.w);
			EditorGUILayout.EndVertical();
			GUILayout.Space(border.y);
			EditorGUILayout.EndHorizontal();
			return r;
		}
		
		static float columnWidth;
		public static int columnViewBoxCount = 1;
		public static bool vAlignRows = true;
		public static void BeginColumnView (float width)
		{
			columnWidth = width;
		}
		public static Rect DrawColumnRow (float height, params Action[] items)
		{
			Action<Rect>[] rectItems = new Action<Rect>[items.Length];
			for(int i = 0; i < rectItems.Length; i++)
			{
				Action a = items[i];
				rectItems[i] = (r)=>{ a(); };
			}
			return DrawColumnRow(height, rectItems);
		}
		
		public static Rect DrawColumnRow (float height, params Action<Rect>[] items)
		{
			float width = columnWidth/items.Length - 3*(items.Length-1) - ((items.Length % 2) * 3);// - 2*items.Length;
			
			Rect r = EditorGUILayout.BeginHorizontal();
			
			GUILayout.FlexibleSpace();
			
			for(int i = 0; i < items.Length; i++)
			{
				BoxArea(0, 4, columnViewBoxCount, ()=>{
					Rect rect = EditorGUILayout.BeginVertical(GUILayout.Height(height), GUILayout.Width(width));
					bool doVAlign = vAlignRows;
					if(doVAlign) GUILayout.FlexibleSpace();
					EditorGUILayout.BeginHorizontal();
					items[i](rect);
					EditorGUILayout.EndHorizontal();
					if(doVAlign) GUILayout.FlexibleSpace();
					EditorGUILayout.EndVertical();
				});
				if(i != items.Length-1) GUILayout.Space(4);
			}
			
			GUILayout.FlexibleSpace();
			
			EditorGUILayout.EndHorizontal();
			
			GUILayout.Space(2);
			
			return r;
		}
		
		public static void SelectionGroup (string title, List<VPaintObject> objects, ref Vector2 scroll)
		{
			VPaintGUIUtility.DrawColumnRow(24,
			()=>{
				GUILayout.Label(title);
				if(VPaintGUIUtility.FoldoutMenu())
				{
					AddMenu(objects);
				}
			});
			
			bool doVAlign = vAlignRows;
			vAlignRows = false;
			
			var s = scroll;
			VPaintGUIUtility.DrawColumnRow(200,
			()=>{
				s = EditorGUILayout.BeginScrollView(s);
				for(int i = 0; i < objects.Count; i++)
				{
					var obj = objects[i];
					var r = EditorGUILayout.BeginHorizontal();
					GUILayout.Label(obj.name);
					GUILayout.FlexibleSpace();
					if(GUILayout.Button("X"))
					{
						objects.RemoveAt(i--);
					}
					EditorGUILayout.EndHorizontal();
					if(r.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
						EditorGUIUtility.PingObject(obj);
				}
				EditorGUILayout.EndScrollView();
			});
			scroll = s;
			
			vAlignRows = doVAlign;
			
			VPaintGUIUtility.DrawColumnRow(1,()=>{});
		}
		
		public static void DualSelectionGroup (
			string leftTitle, List<VPaintObject> leftObjects, ref Vector2 leftScroll,
			string rightTitle, List<VPaintObject> rightObjects, ref Vector2 rightScroll)
		{		
			VPaintGUIUtility.DrawColumnRow(24,
			()=>{
				GUILayout.Label(leftTitle);
				GUILayout.FlexibleSpace();
				if(VPaintGUIUtility.FoldoutMenu())
				{
					AddMenu(leftObjects);
				}
			}, 
			()=>{
				if(VPaintGUIUtility.FoldoutMenu())
				{
					AddMenu(rightObjects);
				}
				GUILayout.FlexibleSpace();
				GUILayout.Label(rightTitle);
			});
		
			var ls = leftScroll;
			var rs = rightScroll;
			
			bool doVAlign = vAlignRows;
			vAlignRows = false;
			
			VPaintGUIUtility.DrawColumnRow(200,
			()=>{
				ls = EditorGUILayout.BeginScrollView(ls);
				for(int i = 0; i < leftObjects.Count; i++)
				{
					var obj = leftObjects[i];
					var r = EditorGUILayout.BeginHorizontal();
					GUILayout.Label(obj.name);
					GUILayout.FlexibleSpace();
					if(GUILayout.Button("X"))
					{
						leftObjects.RemoveAt(i--);
					}
					EditorGUILayout.EndHorizontal();
					if(r.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
						EditorGUIUtility.PingObject(obj);
				}
				EditorGUILayout.EndScrollView();
			},
			()=>{
				rs = EditorGUILayout.BeginScrollView(rs);
				for(int i = 0; i < rightObjects.Count; i++)
				{
					var obj = rightObjects[i];
					var r = EditorGUILayout.BeginHorizontal();
					GUILayout.Label(obj.name);
					GUILayout.FlexibleSpace();
					if(GUILayout.Button("X"))
					{
						rightObjects.RemoveAt(i--);
					}
					EditorGUILayout.EndHorizontal();
					
					if(r.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
						EditorGUIUtility.PingObject(obj);
				}
				EditorGUILayout.EndScrollView();
			});
			
			vAlignRows = doVAlign;
			
			leftScroll = ls;
			rightScroll = rs;

			VPaintGUIUtility.DrawColumnRow(1,()=>{},()=>{});
		}
		static void AddMenu (List<VPaintObject> objects)
		{
			var menu = new GenericMenu();
			
			var instance = global::VPaint.Instance;
			
			menu.AddItem(new GUIContent("Add All"), false, ()=>{
				foreach(var vp in instance.currentEditingContents)
				{
					if(!objects.Contains(vp)) objects.Add(vp);
				}
			});
			menu.AddItem(new GUIContent("Add Selected"), false, ()=>{
				foreach(var go in Selection.gameObjects)
				{
					var vp = go.GetComponent<VPaintObject>();
					if(vp && !objects.Contains(vp) 
					&& instance.currentEditingContents.Contains(vp))
					{
						objects.Add(vp);
					}
				}
			});

			menu.AddItem(new GUIContent("Add Selected + Children"), false, ()=>{
				foreach(var go in Selection.gameObjects)
				{
					var vps = go.GetComponentsInChildren<VPaintObject>();
					foreach(var vp in vps)
					{
						if(!objects.Contains(vp)
						&& instance.currentEditingContents.Contains(vp)) objects.Add(vp);
					}
				}
			});
			menu.AddItem(new GUIContent("Remove Selected"), false, ()=>{
				foreach(var go in Selection.gameObjects)
				{
					var vp = go.GetComponent<VPaintObject>();
					if(vp) objects.Remove(vp);
				}
			});
			menu.AddItem(new GUIContent("Remove Selected + Children"), false, ()=>{
				foreach(var go in Selection.gameObjects)
				{
					var vps = go.GetComponentsInChildren<VPaintObject>();
					foreach(var vp in vps)
					{
						objects.Remove(vp);
					}
				}
			});
			menu.AddItem(new GUIContent("Clear"), false, ()=>{
				objects.Clear();
			});
			
			menu.ShowAsContext();
		}
	}
}
