using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class CrucialColliderProfile : ScriptableObject 
{
	[Range(0, 1)]
    public float overallAlpha = 1.0f;
	[ToggleLeft]
    public bool drawFill = true;
	[ShowIf("drawFill"), Indent()]
	public Color fillColor = new Color(.6f, .6f, 1f, .5f);
	[ToggleLeft]
    public bool drawWire = true;
	[ShowIf("drawWire"), Indent()]
	public Color wireColor = new Color(.6f,.7f,1f,.1f);
	[ToggleLeft]
    public bool drawCenter;
	[ShowIf("drawCenter"), Indent()]
    public float centerMarkerRadius = 1.0f;
	[ShowIf("drawCenter"), Indent()]
	public Color centerColor = new Color(.6f,.7f,1f,.7f);
	
    public float edgePointMarkerRadius = .5f;
	
    public float collider2D_ZDepth = 2.0f;
	
    
}
