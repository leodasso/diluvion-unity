using System.Collections.Generic;
using Valkyrie.VPaint;
using System;

namespace Valkyrie.VPaint
{
	public class VPaintObjectInfo 
	{
		public VPaintObject vpaintObject;
		
		public VPaintVertexCache vertexCache;
		
		public List<VPaintObjectError> errors = new List<VPaintObjectError>();
		
		public string Name {
			get{
				if(vpaintObject) return vpaintObject.name;
				return "[Missing]";
			}
		}
		
		public bool error {
			get{
				return errors.Count != 0;
			}
		}
		public string errorLog {
			get{
				if(!error) return "";
				if(errors.Count == 1)
				{
					return Enum.GetName(typeof(VPaintObjectError), errors[0]);
				}
				else return errors.Count + " errors";
			}
		}
	}
}
