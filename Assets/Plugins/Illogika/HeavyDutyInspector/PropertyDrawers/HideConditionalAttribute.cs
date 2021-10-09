//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HeavyDutyInspector
{

	public class HideConditionalAttribute : PropertyAttribute {

		public enum ConditionType
		{
			IsNotNull,
			Bool,
			IntOrEnum,
			FloatRange,
			IntRange
		}

		public ConditionType conditionType
		{
			get;
			private set;
		}

		public bool hide
		{
			get;
			private set;
		}

		public string variableName
		{
			get;
			private set;
		}

		public string comment
		{
			get;
			private set;
		}

#if UNITY_EDITOR
		public MessageType messageType
		{
			get;
			private set;
		}
#endif

		public bool boolValue
		{
			get;
			private set;
		}

		public List<int> enumValues
		{
			get;
			private set;
		}

		public float minValue
		{
			get;
			private set;
		}

		public float maxValue
		{
			get;
			private set;
		}

		public bool isNotNull
		{
			get;
			private set;
		}

		/// <summary>
		/// Hides this variable until the value of another variable is not null or until a function returns true.
		/// </summary>
		/// <param name="conditionMemberName">The name of a class member whose value will be evaluated. This can be either a variable, or a parameterless function returning a boolean.</param>
		[System.Obsolete("You should use the new overload that takes a boolean as the first parameter.")]
		public HideConditionalAttribute(string conditionMemberName)
		{
			hide = true;
			conditionType = ConditionType.IsNotNull;
			variableName = conditionMemberName;
		}

		/// <summary>
		/// Hides this variable until a condition is met.
		/// </summary>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated.</param>
		/// <param name="visibleState">The state the condition variable has to be in for this variable to be shown in the Inspector.</param>
		[System.Obsolete("You should use the new overload that takes a boolean as the first parameter.")]
		public HideConditionalAttribute(string conditionVariableName, bool visibleState)
		{
			hide = true;
			conditionType = ConditionType.Bool;
			variableName = conditionVariableName;
			boolValue = visibleState;
		}

		/// <summary>
		/// Hides this variable until a condition is met.
		/// </summary>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated. Can be an int or an enum.</param>
		/// <param name="visibleStates">The states the condition variable can be in for this variable to be shown in the Inspector. This can also be used for Enums with an underlying integer type.</param>
		[System.Obsolete("You should use the new overload that takes a boolean as the first parameter.")]
		public HideConditionalAttribute(string conditionVariableName, params int[] visibleState)
		{
			hide = true;
			conditionType = ConditionType.IntOrEnum;
			variableName = conditionVariableName;
			enumValues = new List<int>();
			enumValues = visibleState.ToList();
		}

		/// <summary>
		/// Hides this variable until a condition is met.
		/// </summary>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated.</param>
		/// <param name="minValue">The minimum value the condition variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		/// <param name="maxValue">The maximum value this variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		[System.Obsolete("You should use the new overload that takes a boolean as the first parameter.")]
		public HideConditionalAttribute(string conditionVariableName, float minValue, float maxValue)
		{
			hide = true;
			conditionType = ConditionType.FloatRange;
			variableName = conditionVariableName;
			this.minValue = minValue;
			this.maxValue = maxValue;
		}

		/// <summary>
		/// Hides this variable until a condition is met.
		/// </summary>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated.</param>
		/// <param name="minValue">The minimum value the condition variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		/// <param name="maxValue">The maximum value this variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		[System.Obsolete("You should use the new overload that takes a boolean as the first parameter.")]
		public HideConditionalAttribute(string conditionVariableName, int minValue, int maxValue)
		{
			hide = true;
			conditionType = ConditionType.IntRange;
			variableName = conditionVariableName;
			this.minValue = (float)minValue;
			this.maxValue = (float)maxValue;
		}

		/// <summary>
		/// Hides this variable until the value of another variable is not null. Also displays a Comment over the variable if it is visible.
		/// </summary>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated.</param>
		/// <param name='comment'>The comment to display.</param>
		/// <param name='messageType'>The icon to be displayed next to the comment, if any.</param>
		[System.Obsolete("You should use the new overload that takes a boolean as the first parameter.")]
		public HideConditionalAttribute(string conditionVariableName, string comment, CommentType messageType)
		{
			hide = true;
			conditionType = ConditionType.IsNotNull;
			variableName = conditionVariableName;
			this.comment = comment;
			SetMessageType(messageType);
		}

		/// <summary>
		/// Hides this variable until a condition is met. Also displays a Comment over the variable if it is visible.
		/// </summary>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated.</param>
		/// <param name='comment'>The comment to display.</param>
		/// <param name='messageType'>The icon to be displayed next to the comment, if any.</param>
		/// <param name="visibleState">The state the condition variable has to be in for this variable to be shown in the Inspector.</param>
		[System.Obsolete("You should use the new overload that takes a boolean as the first parameter.")]
		public HideConditionalAttribute(string conditionVariableName, string comment, CommentType messageType, bool visibleState)
		{
			hide = true;
			conditionType = ConditionType.Bool;
			variableName = conditionVariableName;
			this.comment = comment;
			SetMessageType(messageType);
			boolValue = visibleState;
		}

		/// <summary>
		/// Hides this variable until a condition is met. Also displays a Comment over the variable if it is visible.
		/// </summary>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated. Can be an int or an enum.</param>
		/// <param name='comment'>The comment to display.</param>
		/// <param name='messageType'>The icon to be displayed next to the comment, if any.</param>
		/// <param name="visibleStates">The states the condition variable can be in for this variable to be shown in the Inspector. This can also be used for Enums with an underlying integer type.</param>
		[System.Obsolete("You should use the new overload that takes a boolean as the first parameter.")]
		public HideConditionalAttribute(string conditionVariableName, string comment, CommentType messageType, params int[] visibleState)
		{
			hide = true;
			conditionType = ConditionType.IntOrEnum;
			variableName = conditionVariableName;
			this.comment = comment;
			SetMessageType(messageType);
			enumValues = new List<int>();
			enumValues = visibleState.ToList();
		}

		/// <summary>
		/// Hides this variable until a condition is met. Also displays a Comment over the variable if it is visible.
		/// </summary>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated.</param>
		/// <param name='comment'>The comment to display.</param>
		/// <param name='messageType'>The icon to be displayed next to the comment, if any.</param>
		/// <param name="minValue">The minimum value the condition variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		/// <param name="maxValue">The maximum value this variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		[System.Obsolete("You should use the new overload that takes a boolean as the first parameter.")]
		public HideConditionalAttribute(string conditionVariableName, string comment, CommentType messageType, float minValue, float maxValue)
		{
			hide = true;
			conditionType = ConditionType.FloatRange;
			variableName = conditionVariableName;
			this.comment = comment;
			SetMessageType(messageType);
			this.minValue = minValue;
			this.maxValue = maxValue;
		}

		/// <summary>
		/// Hides this variable until a condition is met. Also displays a Comment over the variable if it is visible.
		/// </summary>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated.</param>
		/// <param name='comment'>The comment to display.</param>
		/// <param name='messageType'>The icon to be displayed next to the comment, if any.</param>
		/// <param name="minValue">The minimum value the condition variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		/// <param name="maxValue">The maximum value this variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		[System.Obsolete("You should use the new overload that takes a boolean as the first parameter.")]
		public HideConditionalAttribute(string conditionVariableName, string comment, CommentType messageType, int minValue, int maxValue)
		{
			hide = true;
			conditionType = ConditionType.IntRange;
			variableName = conditionVariableName;
			this.comment = comment;
			SetMessageType(messageType);
			this.minValue = (float)minValue;
			this.maxValue = (float)maxValue;
		}

		/// <summary>
		/// Hides or Shows this variable until the value of another variable is not null or until a function returns true.
		/// </summary>
		/// <param name="hide">Whether the variable should be hidden or displayed until the conditions are met.</param>
		/// <param name="conditionMemberName">The name of a class member whose value will be evaluated. This can be either a variable, or a parameterless function returning a boolean.</param>
		public HideConditionalAttribute(bool hide, string conditionMemberName)
		{
			this.hide = hide;
			conditionType = ConditionType.IsNotNull;
			variableName = conditionMemberName;
		}

		/// <summary>
		/// Hides or Shows this variable until a condition is met.
		/// </summary>
		/// <param name="hide">Whether the variable should be hidden or displayed until the conditions are met.</param>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated.</param>
		/// <param name="visibleState">The state the condition variable has to be in for this variable to be shown in the Inspector.</param>
		public HideConditionalAttribute(bool hide, string conditionVariableName, bool visibleState)
		{
			this.hide = hide;
			conditionType = ConditionType.Bool;
			variableName = conditionVariableName;
			boolValue = visibleState;
		}

		/// <summary>
		/// Hides or Shows this variable until a condition is met.
		/// </summary>
		/// <param name="hide">Whether the variable should be hidden or displayed until the conditions are met.</param>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated. Can be an int or an enum.</param>
		/// <param name="visibleStates">The states the condition variable can be in for this variable to be shown in the Inspector. This can also be used for Enums with an underlying integer type.</param>
		public HideConditionalAttribute(bool hide, string conditionVariableName, params int[] visibleState)
		{
			this.hide = hide;
			conditionType = ConditionType.IntOrEnum;
			variableName = conditionVariableName;
			enumValues = new List<int>();
			enumValues = visibleState.ToList();
		}

		/// <summary>
		/// Hides or Shows this variable until a condition is met.
		/// </summary>
		/// <param name="hide">Whether the variable should be hidden or displayed until the conditions are met.</param>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated.</param>
		/// <param name="minValue">The minimum value the condition variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		/// <param name="maxValue">The maximum value this variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		public HideConditionalAttribute(bool hide, string conditionVariableName, float minValue, float maxValue)
		{
			this.hide = hide;
			conditionType = ConditionType.FloatRange;
			variableName = conditionVariableName;
			this.minValue = minValue;
			this.maxValue = maxValue;
		}

		/// <summary>
		/// Hides or Shows this variable until a condition is met.
		/// </summary>
		/// <param name="hide">Whether the variable should be hidden or displayed until the conditions are met.</param>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated.</param>
		/// <param name="minValue">The minimum value the condition variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		/// <param name="maxValue">The maximum value this variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		public HideConditionalAttribute(bool hide, string conditionVariableName, int minValue, int maxValue)
		{
			this.hide = hide;
			conditionType = ConditionType.IntRange;
			variableName = conditionVariableName;
			this.minValue = (float)minValue;
			this.maxValue = (float)maxValue;
		}

		/// <summary>
		/// Hides or Shows this variable until the value of another variable is not null or until a function returns true. Also displays a Comment above the variable if it is visible.
		/// </summary>
		/// <param name="hide">Whether the variable should be hidden or displayed until the conditions are met.</param>
		/// <param name="conditionMemberName">The name of a class member whose value will be evaluated. This can be either a variable, or a parameterless function returning a boolean.</param>
		/// <param name='comment'>The comment to display.</param>
		/// <param name='messageType'>The icon to be displayed next to the comment, if any.</param>
		public HideConditionalAttribute(bool hide, string conditionMemberName, string comment, CommentType messageType)
		{
			this.hide = hide;
			conditionType = ConditionType.IsNotNull;
			variableName = conditionMemberName;
			this.comment = comment;
			SetMessageType(messageType);
		}

		/// <summary>
		/// Hides or Shows this variable until a condition is met. Also displays a Comment above the variable if it is visible.
		/// </summary>
		/// <param name="hide">Whether the variable should be hidden or displayed until the conditions are met.</param>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated.</param>
		/// <param name='comment'>The comment to display.</param>
		/// <param name='messageType'>The icon to be displayed next to the comment, if any.</param>
		/// <param name="visibleState">The state the condition variable has to be in for this variable to be shown in the Inspector.</param>
		public HideConditionalAttribute(bool hide, string conditionVariableName, string comment, CommentType messageType, bool visibleState)
		{
			this.hide = hide;
			conditionType = ConditionType.Bool;
			variableName = conditionVariableName;
			this.comment = comment;
			SetMessageType(messageType);
			boolValue = visibleState;
		}

		/// <summary>
		/// Hides or Shows this variable until a condition is met. Also displays a Comment above the variable if it is visible.
		/// </summary>
		/// <param name="hide">Whether the variable should be hidden or displayed until the conditions are met.</param>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated. Can be an int or an enum.</param>
		/// <param name='comment'>The comment to display.</param>
		/// <param name='messageType'>The icon to be displayed next to the comment, if any.</param>
		/// <param name="visibleStates">The states the condition variable can be in for this variable to be shown in the Inspector. This can also be used for Enums with an underlying integer type.</param>
		public HideConditionalAttribute(bool hide, string conditionVariableName, string comment, CommentType messageType, params int[] visibleState)
		{
			this.hide = hide;
			conditionType = ConditionType.IntOrEnum;
			variableName = conditionVariableName;
			this.comment = comment;
			SetMessageType(messageType);
			enumValues = new List<int>();
			enumValues = visibleState.ToList();
		}

		/// <summary>
		/// Hides or Shows this variable until a condition is met. Also displays a Comment above the variable if it is visible.
		/// </summary>
		/// <param name="hide">Whether the variable should be hidden or displayed until the conditions are met.</param>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated.</param>
		/// <param name='comment'>The comment to display.</param>
		/// <param name='messageType'>The icon to be displayed next to the comment, if any.</param>
		/// <param name="minValue">The minimum value the condition variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		/// <param name="maxValue">The maximum value this variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		public HideConditionalAttribute(bool hide, string conditionVariableName, string comment, CommentType messageType, float minValue, float maxValue)
		{
			this.hide = hide;
			conditionType = ConditionType.FloatRange;
			variableName = conditionVariableName;
			this.comment = comment;
			SetMessageType(messageType);
			this.minValue = minValue;
			this.maxValue = maxValue;
		}

		/// <summary>
		/// Hides or Shows this variable until a condition is met. Also displays a Comment above the variable if it is visible.
		/// </summary>
		/// <param name="hide">Whether the variable should be hidden or displayed until the conditions are met.</param>
		/// <param name="conditionVariableName">The name of the variable whose value will be evaluated.</param>
		/// <param name='comment'>The comment to display.</param>
		/// <param name='messageType'>The icon to be displayed next to the comment, if any.</param>
		/// <param name="minValue">The minimum value the condition variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		/// <param name="maxValue">The maximum value this variable can contain for this variable to be shown in the Inspector. Inclusive.</param>
		public HideConditionalAttribute(bool hide, string conditionVariableName, string comment, CommentType messageType, int minValue, int maxValue)
		{
			this.hide = hide;
			conditionType = ConditionType.IntRange;
			variableName = conditionVariableName;
			this.comment = comment;
			SetMessageType(messageType);
			this.minValue = (float)minValue;
			this.maxValue = (float)maxValue;
		}

		private void SetMessageType(CommentType commentType)
		{
#if UNITY_EDITOR
			switch(commentType)
			{
				case CommentType.Error:
					messageType = MessageType.Error;
					break;
				case CommentType.Info:
					messageType = MessageType.Info;
					break;
				case CommentType.None:
					messageType = MessageType.None;
					break;
				case CommentType.Warning:
					messageType = MessageType.Warning;
					break;
				default:
					break;
			}
#endif
		}
	}

}
