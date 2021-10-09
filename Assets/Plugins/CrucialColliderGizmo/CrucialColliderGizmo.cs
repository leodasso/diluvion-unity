using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

//*****************************************************************************
//       Crucial Collider Gizmo by TimeFloat. Thanks for purchasing!         **
//*****************************************************************************


// Simply attach this script to any game object that has a collider that you would like
// to be drawn on screen and you are good to go!

public class CrucialColliderGizmo : MonoBehaviour
{
	[AssetsOnly]
	public CrucialColliderProfile settings;

	[ToggleLeft]
	public bool includeChildColliders ;
	
	
	void OnDrawGizmos()
	{
		if(!enabled || !settings)
		{
			return;
		}

		DrawColliders(this.gameObject);
		
		Transform[] allTransforms = gameObject.GetComponentsInChildren<Transform>();

		if (!includeChildColliders) return;
		
		foreach (Transform t in allTransforms)
		{
			if(t.gameObject == gameObject)
			{
				continue;
			}
			DrawColliders(t.gameObject);
		}
	}
	
	delegate void Drawer(CubeDrawer cubeDrawer, SphereDrawer sphereDrawer, LineDrawer linedrawer);
	
	delegate void CubeDrawer(Vector3 center, Vector3 size);
	delegate void SphereDrawer(Vector3 center, float radius);
	delegate void LineDrawer(Vector3 posOne, Vector3 pos2);
	
	void DrawCollider(SphereDrawer drawer, EdgeCollider2D collider, Vector3 position, Vector3 scale, Transform targetTran, LineDrawer lineDrawer)
	{
		if (!collider || !settings) return;

		Vector3 prev = Vector2.zero;
		for (int i = 0; i < collider.points.Length; i++)
		{
			var colPoint = collider.points[i];
			Vector3 pos = new Vector3(colPoint.x * scale.x, colPoint.y * scale.y, 0);
			Vector3 rotated = targetTran.rotation * pos;

			if (i != 0) {
				lineDrawer(position + prev, position + rotated);
			}

			prev = rotated;


			drawer(position + rotated, settings.edgePointMarkerRadius);
		}
	}

	void DrawCollider(CubeDrawer drawer, BoxCollider2D collider, Transform targetTran)
	{
		if (!collider || !settings) return;
		Vector3 newColliderLocation = new Vector3(targetTran.position.x + collider.offset.x, targetTran.position.y + collider.offset.y, targetTran.position.z);
		Vector3 newColliderSize = new Vector3(collider.bounds.size.x, collider.bounds.size.y, settings.collider2D_ZDepth);
		drawer(newColliderLocation, newColliderSize);
	}
	
	void DrawCollider(CubeDrawer drawer, BoxCollider collider, Transform targetTran)
	{
		if (!collider || !settings) return;
		Gizmos.matrix = Matrix4x4.TRS(targetTran.position, targetTran.rotation, targetTran.lossyScale);
		drawer(collider.center, collider.size);
		Gizmos.matrix = Matrix4x4.identity;
	}
	
	void DrawCollider(SphereDrawer drawer, CircleCollider2D collider, Vector3 position, Vector3 scale)
	{
		if (!collider || !settings) return;
		drawer(position + new Vector3(collider.offset.x, collider.offset.y, 0.0f), collider.radius * Mathf.Max(scale.x, scale.y));
	}
	
	void DrawCollider(SphereDrawer drawer, SphereCollider collider, Vector3 position, Vector3 scale)
	{
		if (!collider || !settings) return;
		drawer(position + new Vector3(collider.center.x, collider.center.y, 0.0f), collider.radius * Mathf.Max(scale.x, scale.y, scale.z));
	}
	
	void DrawColliders(GameObject hostGameObject)
	{
		if (!settings) return;
		
		Transform targetTran = hostGameObject.transform;
		Vector3 position = targetTran.position;
		Vector3 trueScale = targetTran.localScale;
		while(targetTran.parent != null)
		{
			targetTran = targetTran.parent;
			trueScale = new Vector3(targetTran.localScale.x * trueScale.x, targetTran.localScale.y * trueScale.y, targetTran.localScale.z * trueScale.z);
		}
		
		Drawer draw = (CubeDrawer cubeDrawer, SphereDrawer sphereDrawer, LineDrawer linedrawer) =>
			
		{
			DrawCollider(sphereDrawer, hostGameObject.GetComponent<EdgeCollider2D>(), position, trueScale, hostGameObject.transform, linedrawer);
			DrawCollider(cubeDrawer, hostGameObject.GetComponent<BoxCollider2D>(), hostGameObject.transform);
			DrawCollider(cubeDrawer, hostGameObject.GetComponent<BoxCollider>(), hostGameObject.transform);
			DrawCollider(sphereDrawer, hostGameObject.GetComponent<CircleCollider2D>(), position, trueScale);
			DrawCollider(sphereDrawer, hostGameObject.GetComponent<SphereCollider>(), position, trueScale);
		};
		
		Gizmos.color = new Color(settings.wireColor.r, settings.wireColor.g, settings.wireColor.b, settings.wireColor.a * settings.overallAlpha);
		if (settings.drawWire)
		{
			draw(Gizmos.DrawWireCube, Gizmos.DrawWireSphere, Gizmos.DrawLine);
		}
		
		Gizmos.color = new Color(settings.fillColor.r, settings.fillColor.g, settings.fillColor.b, settings.fillColor.a * settings.overallAlpha);
		if (settings.drawFill)
		{
			draw(Gizmos.DrawCube, Gizmos.DrawSphere, Gizmos.DrawLine);
		}
		
		if(settings.drawCenter)
		{
			Gizmos.color = new Color(settings.centerColor.r, settings.centerColor.g, settings.centerColor.b, settings.centerColor.a * settings.overallAlpha);
			Gizmos.DrawSphere(hostGameObject.transform.position, settings.centerMarkerRadius );
		}
	}
}