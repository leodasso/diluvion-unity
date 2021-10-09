using Sirenix.OdinInspector;
using UnityEngine;
using Diluvion.SaveLoad;
using NodeCanvas.BehaviourTrees;

namespace Diluvion.Achievements
{
    
    public class DSaveAchievement : ScriptableObject
    {
        [SerializeField] protected bool debug;
        [ShowInInspector, ReadOnly] protected int progress;
        [ShowInInspector, ReadOnly] private string lastInspectedSaveData;
        
        [Button]
        void TestProgress()
        {
            if(!Application.isPlaying||DSave.current ==null)
                progress = Progress(GameManager.Get().saveData);
            else
            {
                progress = Progress(DSave.current);
            }
        }

        public virtual int Progress(DiluvionSaveData svd)
        {
            lastInspectedSaveData = svd.saveFileName;
            return progress = 0;
        }
    }
}