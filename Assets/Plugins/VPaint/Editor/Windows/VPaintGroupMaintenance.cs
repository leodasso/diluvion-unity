using UnityEngine;
using UnityEditor;
using Valkyrie.VPaint;
using System.Collections.Generic;
using System;
using System.Linq;

[InitializeOnLoad]
public class VPaintGroupMaintenance : VPaintWindowBase 
{		
	Vector2 leftPanelScroll;
	Vector2 rightPanelScroll;
	
	List<int> selection = new List<int>();
	
	public override bool CloseOnInvalid ()
	{
		return true;
	}
	
	public override bool LockSelection ()
	{
		return true;
	}
	
	public override void OnValidatedEnable ()
	{
		base.OnValidatedEnable ();
		name = "VPaint Object Manager";
	}
	
	public override bool Validate ()
	{
		return base.Validate () && VPaint.Instance.layerStack != null && VPaint.Instance.layerCache && VPaint.Instance.objectInfo != null;
	}
	
	public override void OnValidatedSelectionChange ()
	{
		base.OnValidatedSelectionChange ();
		selection.Clear();
		var goSelection = Selection.gameObjects;
		for(int i = 0; i < VPaint.Instance.objectInfo.Length; i++)
		{
			var oi = VPaint.Instance.objectInfo[i];
			if(!oi.vpaintObject) continue;
			if(goSelection.Contains(oi.vpaintObject.gameObject))
			{
				selection.Add(i);				
			}
		}
	}
	
	void CheckSelection ()
	{
		if(Event.current.type == EventType.Used)
		{
			var selectionInfo = new GameObject[selection.Count];
			for(int i = 0; i < selectionInfo.Length; i++)
			{
				selectionInfo[i] = VPaint.Instance.objectInfo[selection[i]].vpaintObject.gameObject;
			}
			Selection.objects = selectionInfo;
		}
	}
	
	bool selectionChanged = false;
	
	bool showOnlyErrors = false;
	
	public override void OnValidatedGUI ()
	{
		base.OnValidatedGUI ();
		
		selectionChanged = false;
		
		Rect r;
		
		float totalWidth = position.width - 20;
		float width;
		
		VPaintGUIUtility.BeginColumnView(totalWidth);
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUILayout.Label("VPaint Group Maintenance");
		});
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			if(GUILayout.Button("Add Objects"))
			{
				var menu = new GenericMenu();
				
				menu.AddItem(new GUIContent("Add Selected"), false, ()=>{
					VPaint.Instance.PushUndo("Add Selected");
					foreach(var go in Selection.gameObjects)
					{
						var vp = go.GetComponent<VPaintObject>();
						if(!vp)
						{
							if(go.GetComponent<MeshFilter>() && go.GetComponent<MeshRenderer>())
								vp = go.AddComponent<VPaintObject>();
						}
						if(vp) VPaint.Instance.layerCache.AddColorer(vp);
					}
					VPaint.Instance.RefreshObjects();
					selection.Clear();
					selectionChanged = true;
				});
				menu.AddItem(new GUIContent("Add Selected + Children"), false, ()=>{
					VPaint.Instance.PushUndo("Add Selected + Children");
					foreach(var go in Selection.gameObjects)
					{
						var all = go.GetComponentsInChildren<Transform>();
						foreach(var tr in all)
						{
							var vp = tr.GetComponent<VPaintObject>();
							if(!vp)
							{
								if(tr.GetComponent<MeshFilter>() && tr.GetComponent<MeshRenderer>())
								{
									vp = tr.gameObject.AddComponent<VPaintObject>();
								}
							}
							if(vp) VPaint.Instance.layerCache.AddColorer(vp);
						}
					}
					VPaint.Instance.RefreshObjects();
					selection.Clear();
					selectionChanged = true;
				});
					
				menu.ShowAsContext();
			}
			if(GUILayout.Button("Remove Objects"))
			{
				var menu = new GenericMenu();
				
				menu.AddItem(new GUIContent("Remove Selected"), false, ()=>{
					VPaint.Instance.PushUndo("Remove Selected");
					foreach(var go in Selection.gameObjects)
					{
						VPaint.Instance.layerCache.RemoveColorer(go.GetComponent<VPaintObject>());
					}
					VPaint.Instance.RefreshObjects();
					selection.Clear();
					selectionChanged = true;
				});
				menu.AddItem(new GUIContent("Remove Selected + Children"), false, ()=>{
					VPaint.Instance.PushUndo("Remove Selected + Children");
					foreach(var go in Selection.gameObjects)
					{
						foreach(var vc in go.GetComponentsInChildren<VPaintObject>())
						{
							VPaint.Instance.layerCache.RemoveColorer(vc);
						}
					}
					VPaint.Instance.RefreshObjects();
					selection.Clear();
					selectionChanged = true;
				});
				
				menu.ShowAsContext();
			}
		});
		
		VPaintGUIUtility.DrawColumnRow(24,
		()=>{
			GUILayout.Label("Filter:");
		}, ()=>{
			GUILayout.Label("Errors:");
			GUILayout.FlexibleSpace();
			showOnlyErrors = GUILayout.Toggle(showOnlyErrors,GUIContent.none);
		}, ()=>{}, ()=>{});
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		
		width = totalWidth*0.4f;
		r = EditorGUILayout.BeginVertical(GUILayout.Width(width));
		GUI.Box(r, GUIContent.none);
		
		LeftPanel(width);
		
		EditorGUILayout.EndVertical();
		
		GUILayout.Space(2);
		
		width = totalWidth*0.6f;
		r = EditorGUILayout.BeginVertical(GUILayout.Width(width));
		GUI.Box(r, GUIContent.none);
		
		RightPanel(width);
		
		EditorGUILayout.EndVertical();
		
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space(10);
		
		if(Event.current.type == EventType.KeyDown && Event.current.control && Event.current.keyCode == KeyCode.A)
		{
			selectionChanged = true;
			selection.Clear();
			for(int i = 0; i < VPaint.Instance.objectInfo.Length; i++)
			{
				selection.Add(i);
			}
			Event.current.Use();
		}
		
		if(selectionChanged)
		{
			var selectionInfo = new GameObject[selection.Count];
			for(int i = 0; i < selectionInfo.Length; i++)
			{
				var si = VPaint.Instance.objectInfo[selection[i]];
				if(!si.vpaintObject) continue;
				selectionInfo[i] = si.vpaintObject.gameObject;
			}
			Selection.objects = selectionInfo;
		}
	}
	
	public void LeftPanel (float width)
	{		
		Rect panelRect = EditorGUILayout.BeginVertical();
		
		GUILayout.Space(2);
		
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Select All", GUILayout.Height(14)))
		{
			selectionChanged = true;
			selection.Clear();
			for(int i = 0; i < VPaint.Instance.objectInfo.Length; i++)
			{
				selection.Add(i);
			}
		}
		if(GUILayout.Button("Deselect All", GUILayout.Height(14)))
		{
			selectionChanged = true;
			selection.Clear();
		}
		if(VPaintGUIUtility.FoldoutMenu())
		{
			var menu = new GenericMenu();
			foreach(var type in VPaint.Instance.errorTypes)
			{
				VPaintObjectError error = type;
				menu.AddItem(new GUIContent("Select Errors/"+Enum.GetName(typeof(VPaintObjectError), error)), false, ()=>{
					for(int i = 0; i < VPaint.Instance.objectInfo.Length; i++)
					{
						var info = VPaint.Instance.objectInfo[i];
						if(info.errors.Contains(error))
						{
							if(!selection.Contains(i))
							{
								selection.Add(i);
							}
						}
					}
				});
			}
			menu.ShowAsContext();
		}
		EditorGUILayout.EndHorizontal();
		
		leftPanelScroll = EditorGUILayout.BeginScrollView(leftPanelScroll);
		
		for(int i = 0; i < VPaint.Instance.objectInfo.Length; i++)
		{
			var info = VPaint.Instance.objectInfo[i];
			
			if(showOnlyErrors)
			{
				if(!info.error)
				{
					selection.Remove(i);
					continue;
				}
			}
			
			Rect r = EditorGUILayout.BeginHorizontal();
			r.height+=2;
			
			int boxCount = 0;
			if(selection.Contains(i)) boxCount = 1;
			for(int b = 0; b < boxCount; b++) GUI.Box(r, GUIContent.none);
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && r.Contains(Event.current.mousePosition))
			{
				if(!Event.current.shift && !Event.current.control) selection.Clear();
				
				if(!selection.Contains(i)){
					selection.Add(i);
					selectionChanged = true;
				}
				else if(!Event.current.shift && Event.current.control)
				{
					selection.Remove(i);
					selectionChanged = true;
				}
				
				Event.current.Use();
			}
			
			GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
			if(info.error) labelStyle.normal.textColor = Color.red;
			
			GUILayout.Label(info.Name, labelStyle);
			
			GUILayout.FlexibleSpace();
			
			string errorLog = info.errorLog;
			if(errorLog != "") errorLog = "[" + errorLog + "]";
			GUILayout.Label(errorLog, labelStyle);
			
			EditorGUILayout.EndHorizontal();
			
			GUILayout.Space(2);
		}
		EditorGUILayout.EndScrollView();
		
		if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && panelRect.Contains(Event.current.mousePosition))
		{
			if(!Event.current.shift)
			{
				selection.Clear();
				selectionChanged = true;
				Event.current.Use();
			}
		}
		
		EditorGUILayout.EndVertical();
	}
	
	public void RightPanel (float width)
	{				
		rightPanelScroll = EditorGUILayout.BeginScrollView(rightPanelScroll);
		
		GUILayout.Space(5);
		
		if(selection.Count == 0)
		{
			GUILayout.Label(" ");
		}
		else
		{
			EditorGUILayout.BeginHorizontal();
			
			string title = "";
			
			VPaintObjectInfo[] selectionInfo = new VPaintObjectInfo[selection.Count];
			Dictionary<VPaintObjectError, List<VPaintObjectInfo>> errors = new Dictionary<VPaintObjectError, List<VPaintObjectInfo>>();
			
			for(int i = 0; i < selection.Count; i++)
			{
				var index = selection[i];
				var info = VPaint.Instance.objectInfo[index];
				selectionInfo[i] = info;
				
				foreach(var err in info.errors)
				{
					if(!errors.ContainsKey(err))
					{
						errors.Add(err, new List<VPaintObjectInfo>());
					}
					errors[err].Add(info);
				}
				
				if(i == 0) title = info.Name;
				else title += ", " + info.Name;
			}
			
			GUIStyle style = new GUIStyle(GUI.skin.label);
			style.fontSize = 20;
			
			var titleSize = style.CalcSize(new GUIContent(title));
			if(width-42 < titleSize.x) title = title.Substring(0, (int)(title.Length*((width-42)/titleSize.x))) + "...";
			
			GUILayout.Label(title, style);
			
			EditorGUILayout.EndHorizontal();
			
			VPaintGUIUtility.BeginColumnView(width-24);
			
			GUILayout.Space(10);
			
			if(selection.Count != 0) MultiInfoPanel(selectionInfo);
			
			GUILayout.Space(10);
			
			if(selection.Count == 1) SingleInfoPanel(selectionInfo[0]);
			
			GUILayout.Space(10);
			
			if(errors.Count != 0)
			{
				GUILayout.Label("The selected objects contain the following errors:");
				GUILayout.Space(10);
				foreach(var kvp in errors)
				{
					var panel = GetErrorPanel(kvp.Key);
					if(panel != null) panel(kvp.Value.ToArray());
					GUILayout.Space(10);
				}
			}
		}		
		
		EditorGUILayout.EndScrollView();
	}
	
	void MultiInfoPanel (VPaintObjectInfo[] grp)
	{		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUILayout.Label("Colors:");
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Clear Selected Layer"))
			{
				VPaint.Instance.PushUndo("Clear Colors");
				foreach(var info in grp)
				{
					if(currentLayer != null) currentLayer.Remove(info.vpaintObject);
				}
				VPaint.Instance.BuildObjectInfo();
			}
			if(GUILayout.Button("Clear All Layers"))
			{
				VPaint.Instance.PushUndo("Clear Colors");
				foreach(var info in grp)
				{
					foreach(var layer in VPaint.Instance.layerStack.layers)
					{
						layer.Remove(info.vpaintObject);
					}
				}
				VPaint.Instance.BuildObjectInfo();
			}
		});
		
		if(grp.Length != 1)
		{
			bool meshesValid = true;
			bool allInstanced = true;
			foreach(var info in grp)
			{
				meshesValid &= info.vpaintObject.originalMesh;
				if(!meshesValid)
				{
					allInstanced = false;
					break;
				}
				allInstanced &= !EditorUtility.IsPersistent(info.vpaintObject.originalMesh);
			}
			VPaintGUIUtility.DrawColumnRow(24, ()=>{
				GUILayout.Label("Instancing:");
				GUILayout.FlexibleSpace();
				GUI.enabled = meshesValid && !allInstanced;
				if(GUILayout.Button("Break Instances"))
				{
					foreach(var info in grp)
					{
						VPaint.Instance.BreakInstance(info.vpaintObject);
					}
				}
				GUI.enabled = true;
			});
		}
	}
	
	void SingleInfoPanel (VPaintObjectInfo info)
	{
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUILayout.Label("Mesh Reference:");
			GUILayout.FlexibleSpace();
			if(!info.vpaintObject.originalMesh)
			{
				GUILayout.Label("MISSING");
			}
			else
			{
				bool isPersistent = EditorUtility.IsPersistent(info.vpaintObject.originalMesh);
				if(isPersistent)
				{
					GUILayout.Label("Instanced");
					if(GUILayout.Button("Break Instance"))
					{
						VPaint.Instance.BreakInstance(info.vpaintObject);
					}
				}
				else
				{
					GUILayout.Label("Unique");
					GUI.enabled = false;
					GUILayout.Button("Break Instance");
					GUI.enabled = true;
				}
			}
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUILayout.Label("Mesh Vertices:");
			GUILayout.FlexibleSpace();
			if(info.errors.Contains(VPaintObjectError.MissingMesh))
			{
				GUILayout.Label("MISSING");
			}
			else
			{
				GUILayout.Label(info.vpaintObject.GetMeshInstance().vertices.Length.ToString());
			}
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUILayout.Label("Cached Vertices:");
			GUILayout.FlexibleSpace();
			if(info.vertexCache == null)
			{
				GUILayout.Label("None");
			}
			else
			{
				GUILayout.Label(info.vertexCache.vertices.Length.ToString());
			}
		});
	}
	
	Action<VPaintObjectInfo[]> GetErrorPanel (VPaintObjectError error)
	{
		switch(error)
		{
			case VPaintObjectError.InvalidVertexCount:
				return InvalidVertexCountPanel;
			case VPaintObjectError.MissingMesh:
				return MissingMeshPanel;
			case VPaintObjectError.MissingMeshFilter:
				return MissingMeshFilterPanel;
			case VPaintObjectError.MissingMeshRenderer:
				return MissingMeshRendererPanel;
			case VPaintObjectError.MissingObject:
				return MissingObjectPanel;
		}
		return null;
	}
	
	enum VertexTransferType
	{
		NearestVert,
		RadialSample,
	}
	VertexTransferType vertexTransferType = VertexTransferType.RadialSample;
	float radialSampleRadius = 1f;
	float radialSampleFalloff = 1f;
	
	Vector3 vertexTransferOffset = Vector3.zero;
	Vector3 vertexTransferRotate = Vector3.zero;
	Vector3 vertexTransferScale = Vector3.one;
	
	void InvalidVertexCountPanel (VPaintObjectInfo[] grp)
	{
		ErrorTitle("INVALID VERTEX COUNT");
		
		bool hasVertexCache = true;
		foreach(var info in grp)
		{
			hasVertexCache &= info.vertexCache != null;
		}
		bool validTransfer = true;
		string err = "";
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{			
			EditorGUILayout.BeginVertical();
			vertexTransferType = (VertexTransferType)EditorGUILayout.EnumPopup("Fix:", vertexTransferType);
			if(!hasVertexCache)
			{
				err = "Some or all objects are missing vertex cache data. Cannot transfer color data.";
				validTransfer = false;
				return;
			}
			switch(vertexTransferType)
			{
				case VertexTransferType.NearestVert:
					break;
				case VertexTransferType.RadialSample:
					radialSampleRadius = EditorGUILayout.FloatField("Sample Radius:", radialSampleRadius);
					radialSampleFalloff = EditorGUILayout.FloatField("Sample Falloff:", radialSampleFalloff);
					break;
			}
			if(validTransfer)
			{
				vertexTransferOffset = EditorGUILayout.Vector3Field("Offset:", vertexTransferOffset);
				vertexTransferRotate = EditorGUILayout.Vector3Field("Rotate:", vertexTransferRotate);
				vertexTransferScale = EditorGUILayout.Vector3Field("Scale:", vertexTransferScale);
			}
			EditorGUILayout.EndVertical();
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>{			
			GUI.enabled = validTransfer;
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label(err);
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Transfer"))
			{
				VPaint.Instance.PushUndo("Transfer Old Colors");
				switch(vertexTransferType)
				{
					case VertexTransferType.NearestVert:
						foreach(var info in grp) NearestVertTransfer(info);
						break;
					case VertexTransferType.RadialSample:
						foreach(var info in grp) RadialSampleTransfer(info, radialSampleRadius, radialSampleFalloff);
						break;
				}
				VPaint.Instance.RefreshObjects();
				VPaint.Instance.ReloadLayers();
			}
			EditorGUILayout.EndHorizontal();
			
			GUI.enabled = true;
		});
	}
	
	Mesh fixMesh;
	void MissingMeshPanel (VPaintObjectInfo[] grp)
	{
		ErrorTitle("MISSING MESH");
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			fixMesh = EditorGUILayout.ObjectField("Assign Mesh:", fixMesh, typeof(Mesh), false) as Mesh;
			GUI.enabled = fixMesh;
			if(GUILayout.Button("Fix", GUILayout.Width(40)))
			{
				foreach(var obj in grp)
				{
					obj.vpaintObject.originalMesh = fixMesh;
					obj.vpaintObject.ResetInstances();
				}
				VPaint.Instance.BuildObjectInfo();
			}
		});
	}
	
	void MissingMeshRendererPanel (VPaintObjectInfo[] grp)
	{
		ErrorTitle("MISSING MESH RENDERER");
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUILayout.Label("Objects are missing a mesh renderer.");
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Fix"))
			{
				foreach(var obj in grp)
				{
					obj.vpaintObject.gameObject.AddComponent<MeshRenderer>();
				}
				VPaint.Instance.BuildObjectInfo();
			}
		});
	}
	
	void MissingMeshFilterPanel (VPaintObjectInfo[] grp)
	{
		ErrorTitle("MISSING MESH FILTER");
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUILayout.Label("Objects are missing a mesh filter.");
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Fix"))
			{
				foreach(var obj in grp)
				{
					obj.vpaintObject.gameObject.AddComponent<MeshFilter>();
				}
				VPaint.Instance.BuildObjectInfo();
			}
		});
	}
	
	void MissingObjectPanel (VPaintObjectInfo[] grp)
	{
		ErrorTitle("MISSING OBJECT");
	}
	
	void ErrorTitle (string title)
	{
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUIStyle style = new GUIStyle(GUI.skin.label);
			style.normal.textColor = Color.red;
			GUILayout.Label(title, style);
		});
	}
	
	void NearestVertTransfer (VPaintObjectInfo info)
	{
		var mesh = info.vpaintObject.originalMesh;
		if(!mesh) mesh = info.vpaintObject.GetComponent<MeshFilter>().sharedMesh;
		var verts = mesh.vertices;
		var oldVerts = info.vertexCache.vertices;
		
		Matrix4x4 transformMatrix = Matrix4x4.TRS(vertexTransferOffset, Quaternion.Euler(vertexTransferRotate), vertexTransferScale);
		
		for(int i = 0; i < VPaint.Instance.layerStack.layers.Count; i++)
		{
			var layer = VPaint.Instance.layerStack.layers[i];
			var pd = layer.Get(info.vpaintObject);
			if(pd == null) continue;
			if(pd.colors.Length == verts.Length) continue;
			var oldColors = pd.colors;
			var oldTrans = pd.transparency;
			if(oldVerts.Length != oldColors.Length)
			{
				Debug.LogWarning("Color data associated with " + info.vpaintObject.name + " does not match the vertex cache. Transfer could not be performed.");
				continue;
			}
			
			pd.colors = new Color[verts.Length];
			pd.transparency = new float[verts.Length];
			
			for(int v = 0; v < verts.Length; v++)
			{
				var vert = verts[v];
				float closestDist = Mathf.Infinity;
				Color closestColor = Color.black;
				float closestTrans = 0f;
				
				for(int a = 0; a < oldVerts.Length; a++)
				{
					var oldVert = transformMatrix.MultiplyPoint(oldVerts[a]);
					var dist = Vector3.Distance(oldVert, vert);
					if(dist < closestDist)
					{
						closestDist = dist;
						closestColor = oldColors[a];
						closestTrans = oldTrans[a];
					}
				}
				
				pd.colors[v] = closestColor;
				pd.transparency[v] = closestTrans;
			}
		}
	}
	
	void RadialSampleTransfer (VPaintObjectInfo info, float radius, float falloff)
	{
		var mesh = info.vpaintObject.originalMesh;
		if(!mesh) mesh = info.vpaintObject.GetComponent<MeshFilter>().sharedMesh;
		var verts = mesh.vertices;
		var oldVerts = info.vertexCache.vertices;
		
		Matrix4x4 transformMatrix = Matrix4x4.TRS(vertexTransferOffset, Quaternion.Euler(vertexTransferRotate), vertexTransferScale);
		
		for(int i = 0; i < VPaint.Instance.layerStack.layers.Count; i++)
		{
			var layer = VPaint.Instance.layerStack.layers[i];
			
			var pd = layer.Get(info.vpaintObject);
			if(pd == null) continue;
			if(pd.colors.Length == verts.Length) continue;
			var oldColors = pd.colors;
			var oldTrans = pd.transparency;
			
			if(oldVerts.Length != oldColors.Length)
			{
				Debug.LogWarning("Color data associated with " + info.vpaintObject.name + " does not match the vertex cache. Transfer could not be performed.");
				continue;
			}
			
			pd.colors = new Color[verts.Length];
			pd.transparency = new float[verts.Length];
			
			for(int v = 0; v < verts.Length; v++)
			{
				var vert = verts[v];
				
				Vector4 col = Vector4.zero;
				float tr = 0;
				float fac = 0;
				
				for(int o = 0; o < oldVerts.Length; o++)
				{
					var oldVert = transformMatrix.MultiplyPoint(oldVerts[o]);
					var oldColor = oldColors[o];
					var oldTr = oldTrans[o];
					
					float f = Mathf.Pow(1 - Mathf.Clamp01(Vector3.Distance(oldVert, vert) / radius), falloff);
					
					col += (Vector4)oldColor * f;
					tr += oldTr * f;
					fac += f;
				}
				
				if(fac != 0)
				{
					col /= fac;
					tr /= fac;
				}
				
				pd.colors[v] = (Color)col;
				pd.transparency[v] = tr;
				
			}
		}
	}
	
}
