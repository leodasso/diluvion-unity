using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Valkyrie.VPaint
{

	public static class VPaintUtility
	{
		public static void MergeColors (
			Color[] source, 
			float[] sourceTransparency, 
			Color[] target, 
			float[] targetTransparency, 
			VPaintBlendMode blendMode, 
			float opacity, 
			bool maskR, 
			bool maskG, 
			bool maskB, 
			bool maskA
		){
			if(source.Length != target.Length) return;;
			
			for(int i = 0; i < source.Length; i++)
			{
				Color c = source[i];
				Color t = c;
				Color targ = target[i];
				
				float trans = Mathf.Clamp01(targetTransparency[i] * opacity);
				
				switch(blendMode)
				{
					case VPaintBlendMode.Additive:
						t = c + targ;
						break;
					case VPaintBlendMode.Multiply:
						t = c * targ;
						break;
					case VPaintBlendMode.Opaque:
						t = targ;
						break;
					case VPaintBlendMode.Overlay:
						t = c * targ * 2;
						break;
				}
				
				t = Color.Lerp(c, t, trans);
				
				if(maskR) c.r = t.r;
				if(maskG) c.g = t.g;
				if(maskB) c.b = t.b;
				if(maskA) c.a = t.a;

				source[i] = c;
				sourceTransparency[i] = Mathf.Clamp01(sourceTransparency[i] + trans);
			}
		}
		
		/// <summary>
		/// Blends colors between objects using the radial method.
		/// </summary>
		/// <param name='layers'>
		/// The layers to affect. If acting on a VPaintLayerStack, use VPaintLayerStack.layers.ToArray()
		/// </param>
		/// <param name='blendObjects'>
		/// The objects to blend
		/// </param>
		/// <param name='blendTargets'>
		/// The objects to blend towards
		/// </param>
		/// <param name='radius'>
		/// The radius to use in the blend operation
		/// </param>
		/// <param name='intensity'>
		/// (Optional) The intensity of the blend operation
		/// </param>
		/// <param name='bounds'>
		/// (Optional) The bounding box to limit the blend operation to.
		/// </param>
		public static void BlendRadial (
			VPaintLayer[] layers,
			VPaintObject[] blendObjects,
			VPaintObject[] blendTargets,
			float radius,
			float intensity = 1f,
			Bounds? bounds = null
		){
			var async = BlendRadialAsync(layers, blendObjects, blendTargets, radius, intensity, bounds);
			while(async.MoveNext());
		}
		
		/// <summary>
		/// Blends colors between objects using the radial method.
		/// </summary>
		/// <param name='layers'>
		/// The layers to affect. If acting on a VPaintLayerStack, use VPaintLayerStack.layers.ToArray()
		/// </param>
		/// <param name='blendObjects'>
		/// The objects to blend
		/// </param>
		/// <param name='blendTargets'>
		/// The objects to blend towards
		/// </param>
		/// <param name='radius'>
		/// The radius to use in the blend operation
		/// </param>
		/// <param name='intensity'>
		/// (Optional) The intensity of the blend operation
		/// </param>
		/// <param name='bounds'>
		/// (Optional) The bounding box to limit the blend operation to.
		/// </param>
		public static IEnumerator<VPaintProgress> BlendRadialAsync (
			VPaintLayer[] layers,
			VPaintObject[] blendObjects,
			VPaintObject[] blendTargets,
			float radius,
			float intensity = 1f,
			Bounds? bounds = null
		){
			var validTargets = new List<VPaintObject>();
			foreach(var vc in blendTargets)
			{
				if(Application.isPlaying && !vc.isDynamic)
				{
					throw new VPaintObjectNotDynamicException();
				}
				if(bounds.HasValue)
				{
					if(!bounds.Value.Intersects(vc.editorCollider.bounds))
						continue;
				}
				validTargets.Add(vc);
			}
			
			for(int l = 0; l < layers.Length; l++)
			{
				var layer = layers[l];
				
				for(int i = 0; i < blendObjects.Length; i++)
				{
					var vc = blendObjects[i];
					
					yield return new VPaintProgress()
					{
						message = "Blending " + vc.name + " on layer " + layer.name,
						progress = (float)i/blendObjects.Length
					};
					
					Mesh m = vc.GetMeshInstance();
					Vector3[] vertices = m.vertices;
					
					var paintData = layer.GetOrCreate(vc);
					Color[] colors = paintData.colors;
					float[] trans = paintData.transparency;
					
					Transform t = vc.transform;
					
					Vector4[] colTotal = new Vector4[colors.Length];
					float[] transTotal = new float[trans.Length];
					float[] facTotal = new float[trans.Length];
					float[] distTotal = new float[trans.Length];
					float[] countTotal = new float[trans.Length];
					for(int d = 0; d < distTotal.Length; d++) distTotal[d] = radius;
					
					for(int tg = 0; tg < validTargets.Count; tg++)
					{
						var targ = validTargets[tg];
						
						var targPaintData = layer.GetOrCreate(targ);
						if(targPaintData == null) continue;
						
						Mesh targMesh = targ.GetMeshInstance();
						
						Vector3[] targVerts = targMesh.vertices;
						Color[] targCols = targPaintData.colors;
						float[] targTrans = targPaintData.transparency;
						Transform transform = targ.transform;
						
						for(int v1 = 0; v1 < vertices.Length; v1++)
						{						
							Vector4 avg = Vector4.zero;
							float avgTrans = 0f;
							float shortestDist = radius;
							float fac = 0f;
							int total = 0;
							
							var vert = t.TransformPoint(vertices[v1]);					
							
							for(int v2 = 0; v2 < targVerts.Length; v2++)
							{
								Vector3 targVert = transform.TransformPoint(targVerts[v2]);
								
								if(bounds.HasValue)
								{
									if(!bounds.Value.Contains(targVert)) continue;
								}
								
								float dist = Vector3.Distance(vert, targVert);
								
								if(radius < dist) continue;
								
								float factor = 1 - dist/radius;
								
								fac += factor;
								avg += (Vector4)(targCols[v2] * factor);
								avgTrans += targTrans[v2] * factor;
								total++;
								shortestDist = Mathf.Min(dist, shortestDist);
							}
							
							colTotal[v1] += avg;
							transTotal[v1] += avgTrans;
							facTotal[v1] += fac;
							countTotal[v1] += total;
							distTotal[v1] = Mathf.Min(distTotal[v1], shortestDist);
						}
				
					} 
					
					for(int v = 0; v < colors.Length; v++)
					{
						float fac = facTotal[v];
						Color col = fac == 0 ? new Color() : (Color)colTotal[v]/fac;
						float tr = fac == 0 ? 0 : transTotal[v]/fac;
						
						var count = countTotal[v];
						float lerp = count == 0 ? 0 : 1 - distTotal[v]/radius;
						lerp *= intensity;
						
						paintData.colors[v] = Color.Lerp(paintData.colors[v], col, lerp);
						paintData.transparency[v] = Mathf.Lerp(paintData.transparency[v], tr, lerp);
					}
				}
			}
		}
		
		/// <summary>
		/// Blends colors between objects using the radial method.
		/// </summary>
		/// <param name='layers'>
		/// The layers to affect. If acting on a VPaintLayerStack, use VPaintLayerStack.layers.ToArray()
		/// </param>
		/// <param name='blendObjects'>
		/// The objects to blend
		/// </param>
		/// <param name='blendTargets'>
		/// The objects to blend towards
		/// </param>
		/// <param name='direction'>
		/// The direction to apply the sample
		/// </param>
		/// <param name='distance'>
		/// The distance of the sample
		/// </param>
		/// <param name='intensity'>
		/// (Optional) The intensity of the blend operation
		/// </param>
		/// <param name='falloff'>
		/// (Optional) The falloff of the blend opreation
		/// </param>
		/// <param name='offset'>
		/// (Optional) the offset of the blend sample. Use this to avoid blend samples "missing" when the object intersects the target.
		/// </param>
		/// <param name='bounds'>
		/// (Optional) the bounds to constrain this blend operation to
		/// </param>
		public static void BlendDirectional (
			VPaintLayer[] layers,
			VPaintObject[] blendObjects,
			VPaintObject[] blendTargets,
			Vector3 direction,
			float distance,
			float intensity = 1,
			float falloff = 1,
			Vector3? offset = null,
			Bounds? bounds = null
		){
			var async = BlendDirectionalAsync(layers, blendObjects, blendTargets, direction, distance, intensity, falloff, offset, bounds);
			while(async.MoveNext());
		}
		
		/// <summary>
		/// Blends colors between objects using the radial method.
		/// </summary>
		/// <param name='layers'>
		/// The layers to affect. If acting on a VPaintLayerStack, use VPaintLayerStack.layers.ToArray()
		/// </param>
		/// <param name='blendObjects'>
		/// The objects to blend
		/// </param>
		/// <param name='blendTargets'>
		/// The objects to blend towards
		/// </param>
		/// <param name='direction'>
		/// The direction to apply the sample
		/// </param>
		/// <param name='distance'>
		/// The distance of the sample
		/// </param>
		/// <param name='intensity'>
		/// (Optional) The intensity of the blend operation
		/// </param>
		/// <param name='falloff'>
		/// (Optional) The falloff of the blend opreation
		/// </param>
		/// <param name='offset'>
		/// (Optional) the offset of the blend sample. Use this to avoid blend samples "missing" when the object intersects the target.
		/// </param>
		/// <param name='bounds'>
		/// (Optional) the bounds to constrain this blend operation to
		/// </param>
		public static IEnumerator<VPaintProgress> BlendDirectionalAsync (
			VPaintLayer[] layers,
			VPaintObject[] blendObjects,
			VPaintObject[] blendTargets,
			Vector3 direction,
			float distance,
			float intensity = 1,
			float falloff = 1,
			Vector3? offset = null,
			Bounds? bounds = null
		){	
			if(!offset.HasValue) offset = Vector3.zero;
			
			var validTargets = new List<VPaintObject>();
			foreach(var vc in blendTargets)
			{
				if(Application.isPlaying && !vc.isDynamic)
				{
					throw new VPaintObjectNotDynamicException();
				}
				if(bounds.HasValue)
				{
					if(!bounds.Value.Intersects(vc.editorCollider.bounds))
						continue;
				}
				validTargets.Add(vc);
			}
			
			for(int l = 0; l < layers.Length; l++)
			{
				var layer = layers[l];
				for(int i = 0; i < blendObjects.Length; i++)
				{					
					var vc = blendObjects[i];
					string message = "Blending " + vc.name + " on layer " + layer.name;
					
					float progressBase = (float)i/blendObjects.Length;
					float progressRange = ((float)(i+1)/blendObjects.Length - progressBase)/1;
					
					Mesh m = vc.GetMeshInstance();
					Vector3[] vertices = m.vertices;
					
					var paintData = layer.GetOrCreate(vc);
					Color[] colors = paintData.colors;
					float[] trans = paintData.transparency;
					
					Transform t = vc.transform;
					
					float offsetMagnitude = offset.Value.magnitude;
					
					for(int v = 0; v < vertices.Length; v++)
					{
						yield return new VPaintProgress()
						{
							message = message,
							progress = progressBase + (progressRange * (float)v/vertices.Length)
						};
						
						var vert = t.TransformPoint(vertices[v]);
						
						if(bounds.HasValue)
						{
							if(!bounds.Value.Contains(vert)) continue;
						}
						
						Vector4 avgCol = Vector4.zero;
						float avgTran = 0;
						float fac = 0;
						float count = 0;
						
						foreach(var target in validTargets)
						{
							var mc = target.editorCollider;
							Ray r = new Ray(vert + offset.Value, direction);
							RaycastHit hit;
							if(mc.Raycast(r, out hit, distance))
							{
								if(bounds.HasValue)
								{
									if(!bounds.Value.Contains(hit.point))
										continue;
								}
								Mesh mi = target.GetMeshInstance();
								if(!mi) continue;
								int[] triangles = mi.triangles;
								VPaintVertexData pd = layer.GetOrCreate(target);
								
								int t1 = triangles[(hit.triangleIndex*3)+0];
								int t2 = triangles[(hit.triangleIndex*3)+1];
								int t3 = triangles[(hit.triangleIndex*3)+2];
				
								//Colors
								Color c1 = pd.colors[t1];
								Color c2 = pd.colors[t2];
								Color c3 = pd.colors[t3];
								
								float tr1 = pd.transparency[t1];
								float tr2 = pd.transparency[t2];
								float tr3 = pd.transparency[t3];
				
								Vector3 bc = hit.barycentricCoordinate;
				
								Color sampleColor = c1 * bc.x + c2 * bc.y + c3 * bc.z;
								float sampleTran = tr1 * bc.x + tr2 * bc.y + tr3 * bc.z;
								
								float factor = Mathf.Pow(1 - (hit.distance - offsetMagnitude) / distance, falloff);
								
								fac += factor;
								avgTran += sampleTran * factor;
								avgCol += (Vector4)sampleColor * factor;
								count++;
							}
						}
						
						Color col = (Color)(avgCol/fac);
						float tran = avgTran/fac;
						if(fac != 0)
						{
							float lerp = intensity * (fac/count);
							colors[v] = Color.Lerp(colors[v], col, lerp);
							trans[v] = Mathf.Lerp(trans[v], tran, lerp);
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Lerps colors from one set to another
		/// </summary>
		/// <returns>
		/// A coroutine which will lerp colors from one set to another.
		/// </returns>
		/// <param name='obj'>
		/// The VPaint object to lerp
		/// </param>
		/// <param name='start'>
		/// The starting color set
		/// </param>
		/// <param name='end'>
		/// The ending color set
		/// </param>
		/// <param name='time'>
		/// The time the lerp operation should take
		/// </param>
		public static IEnumerator LerpColors (VPaintObject obj, Color[] start, Color[] end, float time)
		{
			float t = 0;
			int len = start.Length;
			Color[] lerp = new Color[len];
			while(t < time)
			{
				float f = t/time;
				
				for(int i = 0; i < len; i++)
				{
					lerp[i] = Color.Lerp(start[i], end[i], f);
				}
				
				obj.SetColors(lerp);
				
				yield return null;
				
				t += Time.deltaTime;
			}
			obj.SetColors(end);
		}
		
		/// <summary>
		/// Calculates ambient occlusion on a group of objects
		/// </summary>
		/// <param name='objects'>
		/// The objects to calculate ambient occlusion on
		/// </param>
		/// <param name='radius'>
		/// The radius of the AO samples
		/// </param>
		/// <param name='intensity'>
		/// The intensity of the AO
		/// </param>
		/// <param name='sampleCount'>
		/// Sample count.
		/// </param>
		///	<param name='darkColor'>
		/// The color to use when fully occluded
		/// </param>
		///	<param name='lightColor'>
		/// The color to use when fully unoccluded
		/// </param>
		/// <param name='bounds'>
		/// The bounds to constrain the calculation to. Null (or undefined) means unconstrained
		/// </param>
		public static VPaintLayer CalculateAmbientOcclusion (
			VPaintObject[] objects,
			float radius,
			float intensity,
			int sampleCount,
			Color darkColor,
			Color lightColor,
			Bounds? bounds = null
		){
			var async = CalculateAmbientOcclusionAsync(objects, radius, intensity, sampleCount, darkColor, lightColor, bounds);
			while(async.MoveNext());
			return async.Current.result;
		}
		
		public static IEnumerator<VPaintLayerProgress> CalculateAmbientOcclusionAsync (
			VPaintObject[] objects, 
			float radius, 
			float intensity, 
			int sampleCount,
			Color darkColor,
			Color lightColor,
			Bounds? bounds = null)
		{
			VPaintLayer layer = new VPaintLayer();
			
			//Used for valid targets to sample through (avoid realloc)
			List<VPaintObject> targets = new List<VPaintObject>();
			
			for(int i = 0; i < objects.Length; i++)
			{
				var vc = objects[i];
				
				//Grab the bounds of this object
				var checkBounds = vc.GetBounds();
				//Expand the bounds by radius, so account for sample length
				checkBounds.Expand(radius);
				
				//Does this object even intersect with the given bounds? (if any)
				if(bounds != null && !bounds.Value.Intersects(checkBounds))
					continue;
				
				//Grab mesh and duplicate arrays
				var mesh = vc.GetMeshInstance();
				var verts = mesh.vertices;
				var norms = mesh.normals;
				
				//grab another normals array copy,
				//	so that if we recalculate, we can reset the mesh's normals back the way they were
				var originalNorms = mesh.normals;
				
				//AO needs normals to work, so if they don't exist then recalculate them
				if(norms.Length != verts.Length)
				{
					mesh.RecalculateNormals();
					norms = mesh.normals;
				}
				
				//Color buffer to fill with AO data
				var colors = new Color[verts.Length];
				
				//Cull out objects which don't intersect with this one
				targets.Clear();
				foreach(var targ in objects)
				{
					//must have an editor collider!
					if(Application.isPlaying && !targ.isDynamic)
						continue;
					
					if(checkBounds.Intersects(targ.GetBounds()))
						targets.Add(targ);
				}
				
				//Message for the async process
				string msg = "Sampling AO on " + vc.name + " for vert ";
				
				//Loop through all verts and sample AO
				for(int v = 0; v < verts.Length; v++)
				{	
					var localVert = verts[v];
					
					//Grab vert and transform it to the world position
					var vert = vc.transform.TransformPoint(localVert);
					
					//don't do calculations on vert not within the bounds!
					if(bounds != null && !bounds.Value.Contains(vert))
						continue;
					
					//grab normal
					var norm = norms[v];
					
					//Displace the vert by its normal
					var vertDisp = vc.transform.TransformPoint(localVert + norm);
					
					//calc world normal
					var worldNormal = (vertDisp-vert).normalized;
					
					//collated occlusion factor
					float occlusion = 0f;
					
					//sample each object
					for(int h = 0; h < sampleCount; h++)
					{	
						//randomness for sample
						//	(is there a better way to do this?)
						var rayDir = 
							Quaternion.FromToRotation(Vector3.up, worldNormal)						
							* Quaternion.Euler(UnityEngine.Random.Range(-90f,90f), UnityEngine.Random.Range(-90f,90f), UnityEngine.Random.Range(-90f,90f)) * Vector3.up;
						
						//world normal reflection
						var offset = Vector3.Reflect( rayDir, worldNormal );
						
						//make sure the ray length lines up with the input radius
						rayDir = rayDir * (radius / rayDir.magnitude);
						
						//build ray
						Ray ray = new Ray( vert-(offset*0.1f), rayDir);//vert + sample, (vert - sample).normalized);
						
						//initialize empty raycast hit so we can do a champion test
						RaycastHit closestHit = new RaycastHit();
						closestHit.distance = Mathf.Infinity;
						bool used = false;
						
						RaycastHit hit;
						foreach(var targ in targets)
						{
							var collider = targ.editorCollider;
							
							//cast the ray
							if(collider.Raycast(ray, out hit, radius))
							{
								//if the ray hit, only use it if it's closer than the most recent hit
								if(hit.distance < closestHit.distance)
								{
									closestHit = hit;
									used = true;
								}
							}
						}
						
						if(used)
						{
							//if we found something, add to the occlusion
							occlusion += Mathf.Clamp01(1 - (closestHit.distance / radius));
						}
					}
					
					//normalize occlusion
					occlusion = Mathf.Clamp01(1 - ((occlusion*intensity)/sampleCount));
					
					//lerp in the color based on the occlusion factor
					colors[v] = Color.Lerp(darkColor, lightColor, occlusion);
					
					if(v%10 == 0)
					{
						//yield out a message for the async op
						yield return new VPaintLayerProgress()
						{
							progress = (float)v/verts.Length,
							message = msg + v + "/" + verts.Length,
							result = null
						};
					}
				}
				
				// Create paint data in the layer for this object
				var pd = layer.GetOrCreate(vc);
				pd.colors = colors;
				pd.transparency = new float[pd.colors.Length];
				for(int v = 0; v < pd.transparency.Length; v++) pd.transparency[v] = 1f;
				
				mesh.normals = originalNorms;
			}
			
			yield return new VPaintLayerProgress()
			{
				progress = 1,
				message = "Finished",
				result = layer
			};
		}
	}
	
}