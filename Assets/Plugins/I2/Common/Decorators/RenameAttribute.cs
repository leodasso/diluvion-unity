using UnityEngine;

namespace I2
{
	public class RenameAttribute : PropertyAttribute 
	{
		public readonly string Name, Tooltip;
		public readonly int HorizSpace;
		
		public RenameAttribute(int hspace, string name, string tooltip = default(string))
		{
			Name = name;
			Tooltip = tooltip;
			HorizSpace = hspace;
		}
		public RenameAttribute (string name, string tooltip = default(string)):this(0, name, tooltip){}
	}
}