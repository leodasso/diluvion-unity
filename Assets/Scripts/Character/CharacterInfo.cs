using UnityEngine;
using System.Collections.Generic;
using Diluvion.Ships;
using Sirenix.OdinInspector;

namespace Diluvion
{

    [CreateAssetMenu(fileName = "character info", menuName = "Diluvion/Characters/character info")]
    public class CharacterInfo : ScriptableObject
    {
        public string niceName;

        [Tooltip("For most this can be the same template. It can be unique for special characters such as jay that will have quest components.")]
        public GameObject prefab;
        public Appearance appearance;

        [AssetList(Path = "Prefabs/Dialogue/")]
        public Dialogue dialogue;
        public ShipModule skill;
        public Gender gender;
        public VoiceType voiceType = VoiceType.Somber;

        /// <summary>
        /// Whether pre-set or random generated, returns the language localized name of this instance.
        /// </summary>
        [ButtonGroup("Loc")]
        public string GetLocalizedName()
        {
            return SpiderWeb.Localization.GetFromLocLibrary(CharactersGlobal.namePrefix + niceName, niceName);
        }

        [ButtonGroup("Loc")]
        public void AddLoc()
        {
            SpiderWeb.Localization.AddToKeyLib(CharactersGlobal.namePrefix + niceName, niceName);
        }

        public bool IsValid()
        {
            if (!prefab) return false;
            if (!dialogue) return false;
            return true;
        }

        /// <summary>
        /// Returns an instance with this character info.
        /// </summary>
        [Button]
        public Character InstantiateCharacter()
        {
            GameObject instance = Instantiate(prefab);
            Character ch = instance.GetComponent<Character>();
            if (ch == null)
            {
                Debug.LogError("Character component couldn't be found on prefab when trying to instantiate " + niceName, this);
                return null;
            }
            ch.characterInfo = this;
            if (appearance)
            {
                if (ch.GetComponent<Animator>() && appearance.animController)
                    ch.GetComponent<Animator>().runtimeAnimatorController = appearance.animController;
            }

            instance.name = name;

            return ch;
        }

        [Button, ShowIf("IsPlaying")]
        public void AddToPlayerCrew()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Must be in play mode!");
                return;
            }

            PlayerManager.PlayerCrew().AddCrewman(InstantiateCharacter());
        }

        bool IsPlaying()
        {
            return Application.isPlaying;
        }
    }
}