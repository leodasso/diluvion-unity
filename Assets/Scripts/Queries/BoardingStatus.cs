using UnityEngine;
using Diluvion;
using CharacterInfo = Diluvion.CharacterInfo;

namespace Queries
{
    [CreateAssetMenu(fileName = "boarding status", menuName = "Diluvion/queries/boarding status", order = 0)]
    public class BoardingStatus : Query
    {
        public enum CrewBoardingStatus { boarded = 0, off = 1, any = 2 };
        [Space]
        public CrewBoardingStatus boardingStatus;

        public CharacterInfo character;

        public override bool IsTrue(Object o)
        {
            //if (o) Debug.Log("checking boarding status on " + o.name, o);
            
            if (boardingStatus == CrewBoardingStatus.any) return true;
            if (PlayerManager.PlayerCrew() == null)
            {
                //Debug.LogError("No player crew manager could be found.", this);
                return false;
            }

            bool hasCrew = false;
            
            // Check for the object as a character info
            CharacterInfo info = QueriedCharacter(o);
            if (info) hasCrew = PlayerManager.PlayerCrew().HasCharacter(QueriedCharacter(o));
            
            // Check if the object is a character component
            else if (GetCharacter(o))
                hasCrew = PlayerManager.PlayerCrew().HasCrewmember(GetCharacter(o));
            
            if (hasCrew && boardingStatus == CrewBoardingStatus.boarded) return true;
            if (!hasCrew && boardingStatus == CrewBoardingStatus.off) return true;

            return false;
        }

        
        Character GetCharacter(Object original)
        {
            if (original is Character) return original as Character;
            
            GameObject GO = GetGameObject(original);
            if (!GO) return null;

            return GO.GetComponent<Character>();
        }

        CharacterInfo QueriedCharacter(Object inputObject)
        {
            if (character != null) return character;

            // check if the input object is straight up a character info
            return inputObject as CharacterInfo;
        }

        protected override void Test()
        {
            base.Test();
            bool result = IsTrue(testingObject);
            Debug.Log("True if " + ToString() + ": " + result);
        }

        public override string ToString()
        {
            string n = "generic crew";
            return  n + " has boarding status " + boardingStatus.ToString() + " ";
        }
    }
}