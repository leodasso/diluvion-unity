using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Diluvion;
using Diluvion.SaveLoad;

namespace DUI
{

    public class Startmenu : DUIView
    {
        [Space]
        public QualitySettings settingsWindowPrefab;
        public IntroScene introScene;
        public CanvasGroup parentGroup;

        [Space] 
        public Transform settingsParent;
        public RectTransform creditsParent;
        public RectTransform newGameParent;
        
        [Space]
        public LoadMenu loadMenu;
        public DUIView creditsPrefab;
        public DUIView newGamePrefab;
        [ReadOnly]
        public DUIView creditsScroll;
        [ReadOnly]
        public DUIView newGameWindow;
        public GameObject loadButton;
        public GameObject beginDemoButton;
        
        
        static Startmenu _startMenu;
        public static Startmenu Get()
        {
            if (_startMenu != null) return _startMenu;
            _startMenu = FindObjectOfType<Startmenu>();
            return _startMenu;
        }

        public static bool Exists()
        {
            return _startMenu != null;
        }

        protected override void Start()
        {
            base.Start();

            Cursor.visible = true;
            Cursor.lockState = Application.isEditor ? CursorLockMode.None : CursorLockMode.Confined;

            Time.timeScale = 1;

            // Transition in
            FadeOverlay.FadeOut(2, Color.black);

            // If there's no save files, don't allow clicking of the load button.
            
            //if (!loadMenu.GetComponent<LoadMenu>().GetSavesList())
            //    GrayOutLoadingButtons();

            // Clear out any previous dSave file
            DSave.current = null;
            
            GameManager.FullUnFreeze();
        }


        protected override void Update()
        {
            // hard override for resetting inputs
            if (Input.GetKeyDown(KeyCode.F8))
            {
                var mapper = UIManager.GetControlMapper();
                mapper.Open();
                mapper.TryRestoreDefaults();

                mapper.ScreenClosedEvent += OnMapperClosed;

                parentGroup.interactable = false;
            }
            
            base.Update();

            Cursor.lockState = Application.isEditor ? CursorLockMode.None : CursorLockMode.Confined;
            Cursor.visible = true;
        }

        void OnMapperClosed()
        {
            Debug.Log("Mapper closed; returning start menu interactivity");
            parentGroup.interactable = true;
        }


        protected override void SetDefaultSelectable()
        {
            // Try to set default selection to new game.
            if (defaultSelection != null)
            {
                if (defaultSelection.activeInHierarchy)
                {
                    SetCurrentSelectable(defaultSelection);
                    return;
                }
            }

            // if new game is off, try to set to demo button
            if (beginDemoButton) SetCurrentSelectable(beginDemoButton);
        }


        public void GrayOutLoadingButtons()
        {
            loadButton.GetComponent<Button>().interactable = false;
        }

        #region ButtonCommands

        /// <summary>
        /// Open the new game window
        /// </summary>
        public void NewGame()
        {
            if (newGameWindow != null) Destroy(newGameWindow);
            newGameWindow = InstantiateAndShowPanel(newGamePrefab, newGameParent);
        }

        public void ShowCredits()
        {
            // instantiate the credits
            if (creditsScroll != null) Destroy(creditsScroll);
            creditsScroll = InstantiateAndShowPanel(creditsPrefab, creditsParent);
        }

        DUIView InstantiateAndShowPanel(DUIView panelPrefab, RectTransform parent)
        {
            // instantiate the credits
            DUIView panelInstance = Instantiate(panelPrefab, parent);
            panelInstance.backTarget = this;
            
            ShowAnother(panelInstance);
            return panelInstance;
        }

        /// <summary>
        /// Displays the load menu
        /// </summary>
        public void Load()
        {
            loadMenu = UIManager.Create(UIManager.Get().loadGamePanel as LoadMenu);
            ShowAnother(loadMenu);
            loadMenu.backTarget = this;
        }

        /// <summary>
        /// Display the settings panel
        /// </summary>
        public void Settings()
        {
            Debug.Log(name + " is creating settings menu.");
            QualitySettings settingsWindow = Instantiate(settingsWindowPrefab);
            settingsWindow.backTarget = this;
            
            Debug.Log("settings window back target: " + settingsWindow.backTarget.name);

            ShowAnother(settingsWindow);
        }

        protected override void FadeoutComplete()
        {
            gameObject.SetActive(false);
        }

        //Quits game
        public void EndGame()
        {
            if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
            //Application.Quit();
        }
        #endregion
    }
}