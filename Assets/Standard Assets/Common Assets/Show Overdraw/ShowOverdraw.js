@script ExecuteInEditMode
@script RequireComponent(Camera)

var fullOverdraw = false;
var shaderWithZ : Shader;
var shaderWithoutZ : Shader;

private var oldColor : Color;
private var oldClear : CameraClearFlags;

function OnPreCull()
{
	if (!enabled)
		return;
	oldColor = GetComponent.<Camera>().backgroundColor;
	oldClear = GetComponent.<Camera>().clearFlags;
	GetComponent.<Camera>().backgroundColor = Color(0,0,0,0);
	GetComponent.<Camera>().clearFlags = CameraClearFlags.SolidColor;
	GetComponent.<Camera>().SetReplacementShader (fullOverdraw ? shaderWithoutZ : shaderWithZ, "RenderType");
}

function OnPostRender() {
	if (!enabled)
		return;
	GetComponent.<Camera>().ResetReplacementShader();
	GetComponent.<Camera>().backgroundColor = oldColor;
	GetComponent.<Camera>().clearFlags = oldClear;
}
