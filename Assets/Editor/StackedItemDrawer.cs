using UnityEngine;
using UnityEditor;
using System.Collections;
using Diluvion;

[CustomPropertyDrawer(typeof(StackedItem))]
public class StackedItemDrawer : PropertyDrawer {

	public override void OnGUI (Rect rect, SerializedProperty property, GUIContent label)
	{
		//base.OnGUI (position, property, label);
		const float qtyLabelWidth = 50;
		const float qtyWidth = 60;

		SerializedProperty item = property.FindPropertyRelative("item");
		SerializedProperty qty 	= property.FindPropertyRelative("qty");

		// create rects
		Rect itemRect = new Rect(rect.position, new Vector2(rect.width - qtyWidth - qtyLabelWidth, rect.height));

		Rect qtyLabelRect = new Rect(
			new Vector2(rect.position.x + rect.width - qtyWidth - qtyLabelWidth, rect.position.y), 
			new Vector2(qtyLabelWidth, rect.height));
		
		Rect qtyRect = new Rect(
			new Vector2(rect.position.x + rect.width - qtyWidth, rect.position.y), 
			new Vector2(qtyWidth, rect.height));


		EditorGUI.PropertyField(itemRect, item);

		EditorGUI.LabelField(qtyLabelRect, "QTY");

		qty.intValue = Mathf.Clamp(EditorGUI.IntField(qtyRect, qty.intValue), 1, 99999);
		//EditorGUI.PropertyField(qtyRect, qty);
	}
}
