using UnityEngine;
using System.Collections;
using UnityEditor;
//[CanEditMultipleObjects]
[CustomEditor(typeof(Tile2D))]
public class Tile2DEditor : Editor {
	
	public override void OnInspectorGUI() {
		DrawCustomInspector();
		DrawDefaultInspector();
	}

	public void DrawCustomInspector() {
		Tile2D t = (Tile2D)target;
		EditorGUILayout.BeginHorizontal("Box");
		GUIStyle lab = new GUIStyle();
		lab.richText = true;
		GUILayout.Label("Correct sprite naming is required to auto-fill arrays\n\n<b>NAME_SEGMENT TYPE_NUMBER</b>\n\nExample:\nGRASS_C_4", lab);
		EditorGUILayout.EndHorizontal();
		if (GUILayout.Button("Find and apply sprite properties")) {
			
			t.transform.position = Vector3.one;
			Debug.Log(t);	
			if (t.tileTexture == null) {
				Debug.LogError("TileTool2D: Assign Texture containing multiple Sprites to [Tile Texture] property. (Texture must be in Resources folder)");
				return;
			}

			if (t.tileName == "") {
				Debug.LogWarning("TileTool2D: Assign Sprite Name to look for in the Tile Name property\nTrying to use texture name: " + t.tileTexture.name);
				t.tileName = t.tileTexture.name;
			}

			Texture tex = t.tileTexture;
			Sprite[] sprites = Resources.LoadAll<Sprite>(tex.name);
			Fill(sprites, t, true, true);
		}
	}

	public Sprite[] FindTiles(ArrayList sprites, string nam, string templateNum) {
		ArrayList tileList = new ArrayList();
		for (int i = 0; i < sprites.Count; i++) {
			Sprite s = (Sprite)sprites[i];
			string[] splitString = s.name.Split("_"[0]);
			if (splitString[1].ToUpper() == nam.ToUpper()) {
				tileList.Add(s);
			}
			if (splitString[1].ToUpper() == templateNum.ToUpper()) {
				tileList.Add(s);
			}
		}
		Sprite[] a = (Sprite[])tileList.ToArray(typeof(Sprite));
		if (a == null) a = new Sprite[0];
		return a;
	}

	public void Fill(Sprite[] sprites, Tile2D tile2DTarget, bool useName, bool warnings) {
		tile2DTarget.cacheRenderer = tile2DTarget.GetComponent<SpriteRenderer>();
		ArrayList fooList = new ArrayList();
		for (int i = 0; i < sprites.Length; i++) {
			string[] splitString = sprites[i].name.Split("_"[0]);
			if (!useName || splitString[0].ToUpper() == tile2DTarget.tileName.ToUpper()) {
				fooList.Add(sprites[i]);
			}
		}
		if (tile2DTarget.tileType == "") tile2DTarget.tileType = tile2DTarget.tileName;
		string log = "TileTool2D: Couldn't find MAIN tile types :";
		string olog = "TileTool2D: Couldn't find OPTIONAL tile types :";
		if (FindTiles(fooList, "A", "0").Length > 0) tile2DTarget.tileA = FindTiles(fooList, "A", "0");
		if (tile2DTarget.tileA.Length == 0) log = log + " A";
		tile2DTarget.tileC = FindTiles(fooList, "C", "4");
		if (tile2DTarget.tileC.Length == 0) log = log + " - C";
		tile2DTarget.tileCE = FindTiles(fooList, "CE", "15");
		if (tile2DTarget.tileCE.Length == 0) log = log + " - CE";
		tile2DTarget.tileCN = FindTiles(fooList, "CN", "5");
		if (tile2DTarget.tileCN.Length == 0) log = log + " - CN";
		tile2DTarget.tileCS = FindTiles(fooList, "CS", "6");
		if (tile2DTarget.tileCS.Length == 0) log = log + " - CS";
		tile2DTarget.tileCW = FindTiles(fooList, "CW", "14");
		if (tile2DTarget.tileCW.Length == 0) log = log + " - CW";
		tile2DTarget.tileE = FindTiles(fooList, "E", "3");
		if (tile2DTarget.tileE.Length == 0) log = log + " - E";
		tile2DTarget.tileN = FindTiles(fooList, "N", "9");
		if (tile2DTarget.tileN.Length == 0) log = log + " - N";
		tile2DTarget.tileNE = FindTiles(fooList, "NE", "8");
		if (tile2DTarget.tileNE.Length == 0) log = log + " - NE";
		tile2DTarget.tileNS = FindTiles(fooList, "NS", "2");
		if (tile2DTarget.tileNS.Length == 0) log = log + " - NS";
		tile2DTarget.tileNW = FindTiles(fooList, "NW", "7");
		if (tile2DTarget.tileNW.Length == 0) log = log + " - NW";
		tile2DTarget.tileS = FindTiles(fooList, "S", "23");
		if (tile2DTarget.tileS.Length == 0) log = log + " - S";
		tile2DTarget.tileSE = FindTiles(fooList, "SE", "22");
		if (tile2DTarget.tileSE.Length == 0) log = log + " - SE";
		tile2DTarget.tileSW = FindTiles(fooList, "SW", "21");
		if (tile2DTarget.tileSW.Length == 0) log = log + " - SW";
		tile2DTarget.tileW = FindTiles(fooList, "W", "1");
		if (tile2DTarget.tileW.Length == 0) log = log + " - W";
		tile2DTarget.tileWE = FindTiles(fooList, "WE", "16");
		if (tile2DTarget.tileWE.Length == 0) log = log + " - WE";
		tile2DTarget.tileCNW = FindTiles(fooList, "CNW", "26");
		if (tile2DTarget.tileCNW.Length == 0) olog = olog + " CNW";
		tile2DTarget.tileCNE = FindTiles(fooList, "CNE", "24");
		if (tile2DTarget.tileCNE.Length == 0) olog = olog + " - CNE";
		tile2DTarget.tileCSW = FindTiles(fooList, "CSW", "12");
		if (tile2DTarget.tileCSW.Length == 0) olog = olog + " - CSW";
		tile2DTarget.tileCSE = FindTiles(fooList, "CSE", "10");
		if (tile2DTarget.tileCSE.Length == 0) olog = olog + " - CSE";
		tile2DTarget.tileCNWE = FindTiles(fooList, "CNWE", "25");
		if (tile2DTarget.tileCNWE.Length == 0) olog = olog + " - CNWE";
		tile2DTarget.tileCENS = FindTiles(fooList, "CENS", "17");
		if (tile2DTarget.tileCENS.Length == 0) olog = olog + " - CENS";
		tile2DTarget.tileCSWE = FindTiles(fooList, "CSWE", "11");
		if (tile2DTarget.tileCSWE.Length == 0) olog = olog + " - CSWE";
		tile2DTarget.tileCWNS = FindTiles(fooList, "CWNS", "19");
		if (tile2DTarget.tileCWNS.Length == 0) olog = olog + " - CWNS";
		tile2DTarget.tileCNSWE = FindTiles(fooList, "CNSWE", "18");
		if (tile2DTarget.tileCNSWE.Length == 0) olog = olog + " - CNSWE";
		tile2DTarget.tileCWE = FindTiles(fooList, "CWE", "13");
		if (tile2DTarget.tileCWE.Length == 0) olog = olog + " - CWE";
		tile2DTarget.tileCNS = FindTiles(fooList, "CNS", "27");
		if (tile2DTarget.tileCNS.Length == 0) olog = olog + " - CNS";
		tile2DTarget.tileCSN = FindTiles(fooList, "CSN", "34");
		if (tile2DTarget.tileCSN.Length == 0) olog = olog + " - CSN";
		tile2DTarget.tileCEW = FindTiles(fooList, "CEW", "20");
		if (tile2DTarget.tileCEW.Length == 0) olog = olog + " - CEW";
		tile2DTarget.tileCWNN = FindTiles(fooList, "CWNN", "37");
		if (tile2DTarget.tileCWNN.Length == 0) olog = olog + " - CWNN";
		tile2DTarget.tileCNEE = FindTiles(fooList, "CNEE", "32");
		if (tile2DTarget.tileCNEE.Length == 0) olog = olog + " - CNEE";
		tile2DTarget.tileCSWW = FindTiles(fooList, "CSWW", "40");
		if (tile2DTarget.tileCSWW.Length == 0) olog = olog + " - CSWW";
		tile2DTarget.tileCESS = FindTiles(fooList, "CESS", "31");
		if (tile2DTarget.tileCESS.Length == 0) olog = olog + " - CESS";
		tile2DTarget.tileCNSS = FindTiles(fooList, "CNSS", "33");
		if (tile2DTarget.tileCNSS.Length == 0) olog = olog + " - CNSS";
		tile2DTarget.tileCENN = FindTiles(fooList, "CENN", "38");
		if (tile2DTarget.tileCENN.Length == 0) olog = olog + " - CENN";
		tile2DTarget.tileCWSS = FindTiles(fooList, "CWSS", "30");
		if (tile2DTarget.tileCWSS.Length == 0) olog = olog + " - CWSS";
		tile2DTarget.tileCSEE = FindTiles(fooList, "CSEE", "39");
		if (tile2DTarget.tileCSEE.Length == 0) olog = olog + " - CSEE";
		tile2DTarget.tileSENW = FindTiles(fooList, "SENW", "36");
		if (tile2DTarget.tileSENW.Length == 0) olog = olog + " - SENW";
		tile2DTarget.tileSWNE = FindTiles(fooList, "SWNE", "35");
		if (tile2DTarget.tileSWNE.Length == 0) olog = olog + " - SWNE";
		tile2DTarget.tileNESW = FindTiles(fooList, "NESW", "29");
		if (tile2DTarget.tileNESW.Length == 0) olog = olog + " - NESW";
		tile2DTarget.tileNWSE = FindTiles(fooList, "NWSE", "28");
		if (tile2DTarget.tileNWSE.Length == 0) olog = olog + " - NWSE";
		tile2DTarget.tileCENSW = FindTiles(fooList, "CENSW", "43");
		if (tile2DTarget.tileCENSW.Length == 0) olog = olog + " - CENSW";
		tile2DTarget.tileCNWES = FindTiles(fooList, "CNWES", "44");
		if (tile2DTarget.tileCNWES.Length == 0) olog = olog + " - CNWES";
		tile2DTarget.tileCSWEN = FindTiles(fooList, "CSWEN", "45");
		if (tile2DTarget.tileCSWEN.Length == 0) olog = olog + " - CSWEN";
		tile2DTarget.tileCWNSE = FindTiles(fooList, "CWNSE", "46");
		if (tile2DTarget.tileCWNSE.Length == 0) olog = olog + " - CWNSE";
		tile2DTarget.tileCNWSE = FindTiles(fooList, "CNWSE", "41");
		if (tile2DTarget.tileCNWSE.Length == 0) olog = olog + " - CNWSE";
		tile2DTarget.tileCNESW = FindTiles(fooList, "CNESW", "42");
		if (tile2DTarget.tileCNESW.Length == 0) olog = olog + " - CNESW";
		if (log != "TileTool2D: Couldn't find MAIN tile types :") Debug.LogWarning(log);
		if (warnings && log != "TileTool2D: Couldn't find MAIN tile types :") Debug.LogWarning("\n<b> (Textures must be in the resource folder while auto detecting)</b>");
		if (olog != "TileTool2D: Couldn't find OPTIONAL tile types :") Debug.LogWarning(olog);
		if (tile2DTarget.tileA.Length > 0) tile2DTarget.cacheRenderer.sprite = tile2DTarget.tileA[0];
		else if (tile2DTarget.tileCN.Length > 0) tile2DTarget.cacheRenderer.sprite = tile2DTarget.tileCN[0];
		else if (tile2DTarget.tileCE.Length > 0) tile2DTarget.cacheRenderer.sprite = tile2DTarget.tileCE[0];
		else if (tile2DTarget.tileCS.Length > 0) tile2DTarget.cacheRenderer.sprite = tile2DTarget.tileCS[0];		
		else if (tile2DTarget.tileCW.Length > 0) tile2DTarget.cacheRenderer.sprite = tile2DTarget.tileCW[0];
		else if (tile2DTarget.tileC.Length > 0) tile2DTarget.cacheRenderer.sprite = tile2DTarget.tileC[0];
		else if(warnings) Debug.LogWarning("Please assign default sprite to tile manually");
		EditorUtility.SetDirty(tile2DTarget.gameObject);
		string spritePath = AssetDatabase.GetAssetPath(tile2DTarget.gameObject);
		AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
	}
}