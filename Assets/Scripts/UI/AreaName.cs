using TMPro;
using SpiderWeb;
using Diluvion;

namespace DUI
{
    public class AreaName : DUIPanel
    {
        public TextMeshProUGUI areaName;
        public TextMeshProUGUI zoneName;

        public static void CreateAreaName(LocTerm areaName)
        {
            UIManager.Create(UIManager.Get().areaName as AreaName).InitAreaName(areaName);
        }


        void InitAreaName(LocTerm areaName)
        {
            // Display the zone name
            zoneName.text = GameManager.CurrentZone().zoneName.LocalizedText();

            // Display the area name
            areaName.text = areaName.LocalizedText();
        }
    }
}
