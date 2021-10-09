using UnityEngine;
using SpiderWeb;
using Diluvion;
using Diluvion.SaveLoad;
using Sirenix.OdinInspector;

namespace Loot
{
    [CreateAssetMenu(fileName = "new decal item", menuName = "Diluvion/items/decal item")]
    public class DItemDecal : DItem
    {

        // texture 2D of the emblem
        [InlineEditor(InlineEditorModes.LargePreview)]
        public Texture2D emblemTexture;

        public override void Use()
        {
            base.Use();
            ApplyEmblem();
        }

        public override void AddToLibrary()
        {
            Localization.AddToKeyLib("item_" + locKey, niceName);
        }


        public override string LocalizedBody()
        {
            return Localization.GetFromLocLibrary("item_descr_emblem", description);
        }


        public void ApplyEmblem()
        {
            if (PlayerManager.pBridge == null) return;
            PlayerManager.pBridge.ApplyEmblem(this);

            if (DSave.current != null) DSave.current.playerShips[0].decalName = name;

            SpiderSound.MakeSound("Play_Emblem_Set", PlayerManager.PlayerShip());
        }


        public static DItemDecal GetEmblemItem(string emblemName)
        {
            if (string.IsNullOrEmpty(emblemName)) return null;
            return ItemsGlobal.GetItem(emblemName) as DItemDecal;
        }

        public override bool IsStealable()
        {
            return false;
        }
    }
}