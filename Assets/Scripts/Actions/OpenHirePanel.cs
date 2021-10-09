using UnityEngine;
using Object = UnityEngine.Object;
using DUI;

namespace Diluvion
{
    [CreateAssetMenu(fileName = "hire action", menuName = "Diluvion/actions/hire", order = 0)]
    /// <summary>
    /// Opens the hire panel on Do
    /// </summary>
    public class OpenHirePanel : Action
    {
        public bool mandatory;

        public override bool DoAction(Object o)
        {
            Character c = o as Character;

            if (c == null)
            {
                if (GetGameObject(o) == null) return false;
                c = GetGameObject(o).GetComponent<Character>();
            }
            if (c == null)
            {
                Debug.Log("Can't hire " + o.name + " because no crew component found!");
                return false;
            }

            HirePanel instance = UIManager.Create(UIManager.Get().hirePanel as HirePanel);
            instance.Init(c);
            instance.SetMandatory(mandatory);

            return true;
        }

        public override string ToString()
        {
            return "Opens hire panel. Mandatory: " + mandatory;
        }


        protected override void Test()
        {
            Debug.Log(ToString());
            DoAction(testObject);
        }
    }
}
