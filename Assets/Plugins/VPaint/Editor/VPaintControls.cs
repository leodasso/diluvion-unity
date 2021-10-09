using UnityEditor;
using UnityEngine;

namespace Valkyrie.VPaint
{
	public static class VertexEditorControls
	{
		static bool _useR = true;
		public static bool useR {
			get{ return _useR; }
			set{
				_useR = value;
				EditorPrefs.SetBool("VP_UseR", _useR);
			}
		}
		
		static bool _useG = true;
		public static bool useG {
			get{ return _useG; }
			set{
				_useG = value;
				EditorPrefs.SetBool("VP_UseG", _useG);
			}
		}
		
		static bool _useB = true;
		public static bool useB {
			get{ return _useB; }
			set{
				_useB = value;
				EditorPrefs.SetBool("VP_UseB", _useB);
			}
		}
		
		static bool _useA = true;
		public static bool useA {
			get{ return _useA; }
			set{
				_useA = value;
				EditorPrefs.SetBool("VP_UseA", _useA);
			}
		}
		
		static float _radius = 2f;
		public static float radius {
			get{ return _radius; }
			set{ 
				_radius = value; 
				EditorPrefs.SetFloat("VP_Radius", _radius);
			}
		}
		
		static float _strength = 100f;
		public static float strength {
			get{ return _strength; }
			set{ 
				_strength = value; 
				EditorPrefs.SetFloat("VP_Strength", _strength);
			}
		}
		
		static float _falloff = 0f;
		public static float falloff {
			get{ return _falloff; }
			set{ 
				_falloff = value; 
				EditorPrefs.SetFloat("VP_Falloff", _falloff);
			}
		}
		
		static bool _cordoneEnabled = false;
		public static bool cordoneEnabled {
			get{ return _cordoneEnabled; }
			set{
				_cordoneEnabled = value;
				EditorPrefs.SetBool("VP_Cordone_Enabled", _cordoneEnabled);
			}
		}
		
		static float _cordoneSizeX = 0f;
		static float _cordoneSizeY = 0f;
		static float _cordoneSizeZ = 0f;
		public static Vector3 cordoneSize {
			get{ return new Vector3(_cordoneSizeX, _cordoneSizeY, _cordoneSizeZ); }
			set{
				_cordoneSizeX = value.x;
				_cordoneSizeY = value.y;
				_cordoneSizeZ = value.z;
				EditorPrefs.SetFloat("VP_Cordone_SizeX", _cordoneSizeX);
				EditorPrefs.SetFloat("VP_Cordone_SizeY", _cordoneSizeY);
				EditorPrefs.SetFloat("VP_Cordone_SizeZ", _cordoneSizeZ);
			}
		}
		
		static float _cordonePositionX = 0f;
		static float _cordonePositionY = 0f;
		static float _cordonePositionZ = 0f;
		public static Vector3 cordonePosition {
			get{ return new Vector3(_cordonePositionX, _cordonePositionY, _cordonePositionZ); }
			set{
				_cordonePositionX = value.x;
				_cordonePositionY = value.y;
				_cordonePositionZ = value.z;
				EditorPrefs.SetFloat("VP_Cordone_PosX", _cordonePositionX);
				EditorPrefs.SetFloat("VP_Cordone_PosY", _cordonePositionY);
				EditorPrefs.SetFloat("VP_Cordone_PosZ", _cordonePositionZ );
			}
		}
		public static Bounds GetCordoneBounds () 
		{
			return new Bounds(cordonePosition, cordoneSize);
		}
		
		
		public static Color targetColor {
			get{
				return VertexEditorColors.colors[selectedColor]; 
			}
			set{
				VertexEditorColors.colors[selectedColor] = value;
				VertexEditorColors.Serialize();
			}
		}
		
		static int _selectedColor;
		public static int selectedColor {
			get{
				return _selectedColor;
			}
			set{
				_selectedColor = value;
				EditorPrefs.SetInt("VP_SelectedColor", _selectedColor);
			}
		}
		
		public static void Load ()
		{
			_useR = EditorPrefs.GetBool("VP_UseR", true);
			_useG = EditorPrefs.GetBool("VP_UseG", true);
			_useB = EditorPrefs.GetBool("VP_UseB", true);
			_useA = EditorPrefs.GetBool("VP_UseA", true);
			_radius = EditorPrefs.GetFloat("VP_Radius", 2f);
			_strength = EditorPrefs.GetFloat("VP_Strength", 100f);
			_falloff = EditorPrefs.GetFloat("VP_Falloff", 0f);
			_selectedColor = EditorPrefs.GetInt("VP_SelectedColor", _selectedColor);
			
			_cordoneEnabled = EditorPrefs.GetBool("VP_Cordone_Enabled", false);
			
			_cordoneSizeX = EditorPrefs.GetFloat("VP_Cordone_SizeX", 1f);
			_cordoneSizeY = EditorPrefs.GetFloat("VP_Cordone_SizeY", 1f);
			_cordoneSizeZ = EditorPrefs.GetFloat("VP_Cordone_SizeZ", 1f);
			
			_cordonePositionX = EditorPrefs.GetFloat("VP_Cordone_PosX", 0f);
			_cordonePositionY = EditorPrefs.GetFloat("VP_Cordone_PosY", 0f);
			_cordonePositionZ = EditorPrefs.GetFloat("VP_Cordone_PosZ", 0f);
		}
		
		public static void Reset ()
		{
			useR = true;
			useG = true;
			useB = true;
			useA = true;
			radius = 2f;
			strength = 100f;
			falloff = 0f;
			selectedColor = 0;
			cordoneEnabled = false;
			cordoneSize = Vector3.one;
			cordonePosition = Vector3.zero;
		}
	}
}
