using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;
using System.Linq;


namespace NodeCanvas.Tasks.Actions{

	[Category("✫ Script Control/Standalone Only")]
	[Description("Set a property on a script")]
	public class SetProperty : ActionTask, ISubParametersContainer {

		[SerializeField] /*[IncludeParseVariables]*/
		protected ReflectedActionWrapper functionWrapper;

		BBParameter[] ISubParametersContainer.GetIncludeParseParameters(){
			return functionWrapper != null? functionWrapper.GetVariables() : null;
		}

		private MethodInfo targetMethod{
			get {return functionWrapper != null? functionWrapper.GetMethod() : null;}
		}

		public override System.Type agentType{
			get {return targetMethod != null? targetMethod.RTReflectedType() : typeof(Transform);}
		}

		protected override string info{
			get
			{
				if (functionWrapper == null)
					return "No Property Selected";
				if (targetMethod == null)
					return string.Format("<color=#ff6457>* {0} *</color>", functionWrapper.GetMethodString() );
				return string.Format("{0}.{1} = {2}", agentInfo, targetMethod.Name, functionWrapper.GetVariables()[0] );
			}
		}

		public override void OnValidate(ITaskSystem ownerSystem){
			if (functionWrapper != null && functionWrapper.HasChanged()){	
				SetMethod(functionWrapper.GetMethod());
			}
			if (functionWrapper != null && targetMethod == null){
				Error(string.Format("Missing Property '{0}'", functionWrapper.GetMethodString()));
			}
		}

		//store the method info on init for performance
		protected override string OnInit(){

			if (functionWrapper == null){
				return "No Property selected";
			}
			if (targetMethod == null){
				return string.Format("Missing Property '{0}'", functionWrapper.GetMethodString());
			}

			try
			{
				functionWrapper.Init(agent);
				return null;
			}
			catch {return "SetProperty Error";}
		}

		//do it by invoking method
		protected override void OnExecute(){

			if (functionWrapper == null){
				EndAction(false);
				return;
			}

			functionWrapper.Call();
			EndAction();
		}

		void SetMethod(MethodInfo method){
			if (method != null){
				functionWrapper = ReflectedActionWrapper.Create(method, blackboard);
			}
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR

		protected override void OnTaskInspectorGUI(){

			if (!Application.isPlaying && GUILayout.Button("Select Property")){
				var menu = new UnityEditor.GenericMenu();
				if (agent != null){
					foreach(var comp in agent.GetComponents(typeof(Component)).Where(c => c.hideFlags != HideFlags.HideInInspector) ){
						menu = EditorUtils.GetMethodSelectionMenu(comp.GetType(), typeof(void), typeof(object), SetMethod, 1, true, false, menu);
					}
					menu.AddSeparator("/");
				}
				foreach (var t in UserTypePrefs.GetPreferedTypesList(typeof(Component))){
					menu = EditorUtils.GetMethodSelectionMenu(t, typeof(void), typeof(object), SetMethod, 1, true, false, menu);
				}
				if ( NodeCanvas.Editor.NCPrefs.useBrowser){ menu.ShowAsBrowser("Select Property", this.GetType()); }
				else { menu.ShowAsContext(); }
				Event.current.Use();
			}

			if (targetMethod != null){
				GUILayout.BeginVertical("box");
				UnityEditor.EditorGUILayout.LabelField("Type", agentType.FriendlyName());
				UnityEditor.EditorGUILayout.LabelField("Property", targetMethod.Name);
				UnityEditor.EditorGUILayout.LabelField("Set Type", functionWrapper.GetVariables()[0].varType.FriendlyName() );
				GUILayout.EndVertical();
				EditorUtils.BBParameterField("Set Value", functionWrapper.GetVariables()[0]);
			}
		}

		#endif
	}
}