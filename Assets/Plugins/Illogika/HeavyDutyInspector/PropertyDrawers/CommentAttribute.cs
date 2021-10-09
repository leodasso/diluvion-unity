//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2013 - 2015  Illogika
//----------------------------------------------
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace HeavyDutyInspector
{

	public enum CommentType { Error,
							  Info,
							  None,
							  Warning }

	[AttributeUsage (AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
	public class CommentAttribute : PropertyAttribute {
		
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

		/// <summary>
		/// Adds a comment before this variable.
		/// </summary>
		/// <param name='comment'>The comment to display.</param>
		/// <param name='messageType'>The icon to be displayed next to the comment, if any.</param>
		public CommentAttribute(string comment, CommentType messageType = CommentType.None, int order = 0)
		{
			this.comment = comment;
			this.order = order;

	#if UNITY_EDITOR
			switch(messageType)
			{
			case CommentType.Error:
				this.messageType = MessageType.Error;
				break;
			case CommentType.Info:
				this.messageType = MessageType.Info;
				break;
			case CommentType.None:
				this.messageType = MessageType.None;
				break;
			case CommentType.Warning:
				this.messageType = MessageType.Warning;
				break;
			default:
				break;
			}
	#endif
		}
	}

}
