
using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees
{

    [Category("Composites")]
    [Description("Execute the child nodes in order or randomly and return Success if all children return Success, else return Failure.\nAll the children are executed, regardless of their return status.")]
    [Icon("StepIterator")]
    [Color("bf7fff")]
    public class Serial : BTComposite
    {
        public bool random;

        int lastRunningNodeIndex = 0;
        new Status status;

        public override string name
        {
            get { return base.name.ToUpper(); }
        }

        protected override Status OnExecute(Component agent, IBlackboard blackboard)
        {
            for (var i = lastRunningNodeIndex; i < outConnections.Count; i++)
            {
                var status = outConnections[i].Execute(agent, blackboard);
 
                switch (status)
                {
                    case Status.Running:
                        lastRunningNodeIndex = i;
                        return Status.Running;
 
                    case Status.Failure:
                        if (this.status == Status.Success)
                            this.status = Status.Failure;
                        continue;
 
                    case Status.Error:
                        this.status = Status.Error;
                        continue;
                }
            }
 
            return status;
        }
 
            protected override void OnReset()
    {
        lastRunningNodeIndex = 0;
        status = Status.Success;
        if (random)
            outConnections = Shuffle(outConnections);
    }

    public override void OnChildDisconnected(int index)
    {
        if (index != 0 && index == lastRunningNodeIndex)
            lastRunningNodeIndex--;
    }

    public override void OnGraphStarted()
    {
        OnReset();
    }

    //Fisher-Yates shuffle algorithm
    private List<Connection> Shuffle(List<Connection> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = (int)Mathf.Floor(Random.value * (i + 1));
            var temp = list[i];
                    list[i] = list[j];
                    list[j] = temp;
                }
 
                return list;
            }
 
            /////////////////////////////////////////
            /////////GUI AND EDITOR STUFF////////////
            /////////////////////////////////////////
    #if UNITY_EDITOR
 
            protected override void OnNodeGUI()
    {
        if (random)
            GUILayout.Label("<b>RANDOM</b>");
    }
 
    #endif
    }
}
 