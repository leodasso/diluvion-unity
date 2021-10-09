using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace I2.Loc
{
	#if !UNITY_5_0 && !UNITY_5_1
    [AddComponentMenu("I2/Localization/Localize Dropdown")]
	public class LocalizeDropdown : MonoBehaviour
	{
        public List<string> _Terms = new List<string>();
		
		public void Start()
		{
			LocalizationManager.OnLocalizeEvent += OnLocalize;
            OnLocalize();
		}
		
		public void OnDestroy()
		{
			LocalizationManager.OnLocalizeEvent -= OnLocalize;
		}

        void OnEnable()
        {
            OnLocalize ();
        }
		
		public void OnLocalize()
		{
            if (!enabled || gameObject==null || !gameObject.activeInHierarchy)
                return;

            if (string.IsNullOrEmpty(LocalizationManager.CurrentLanguage))
                return;
            
			UpdateLocalization();
		}
		
		public void UpdateLocalization()
		{
			var _Dropdown = GetComponent<Dropdown>();
			if (_Dropdown==null)
				return;
			
			_Dropdown.options.Clear();
			foreach (var term in _Terms)
			{
                var translation = LocalizationManager.GetTermTranslation(term);
				_Dropdown.options.Add( new Dropdown.OptionData( translation ) );
			}
            _Dropdown.RefreshShownValue();
		}
	}
	#endif
}