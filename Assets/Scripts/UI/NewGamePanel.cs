using Diluvion;
using UnityEngine;
using Diluvion.SaveLoad;
using TMPro;
using UnityEngine.SceneManagement;

namespace DUI
{

	public class NewGamePanel : DUIView
	{
		
		public DUIShipSelect shipSelectPanel;
		public IntroScene introScene;
		public TMP_InputField inputField;

		[Space] 
		public PopupObject nameExistsWarning;
		public PopupObject nameTooLong;
		public PopupObject nameTooShort;
		
		/// <summary>
		/// Begins a new game by opening up the starting cutscene and creating a new save file.
		/// </summary>
		public void BeginNewGame()
		{
			BeginNewGame(inputField.text);
		}
		
		/// <summary>
		/// Overload of new game for directly setting the save file name. Useful for demos where we don't want the player
		/// to create a name.
		/// </summary>
		void BeginNewGame(string newName)
		{
			// check if name is valid
			if (!NameIsValid(newName)) return;
			
			// deactivate all buttons
			group.interactable = false;

			GameManager.LoadNewGame(newName);

			SetSaveID();
			Hide();
			

			FadeOverlay.FadeInThenOut(2, Color.black, StartingCutscene);
		}
		
		void StartingCutscene()
		{
			SceneManager.LoadScene("The Legend intro");
		}

		
		void SetSaveID()
		{
			int standardseed = Random.seed;

			Random.seed = Mathf.RoundToInt(Time.realtimeSinceStartup);
			float seedrandom = Random.Range(0, 999999999);
			string saveFileID = SystemInfo.deviceUniqueIdentifier + seedrandom.ToString();
			Random.seed = standardseed;
			DSave.current.SetSaveFileID(saveFileID);
		}
		
		/// <summary>
		/// Checks to see if the user input name is valid. If it's not, makes the 'begin' button
		/// non-interactable.
		/// </summary>
		public bool NameIsValid(string newName)
		{

			if (newName.Length < 1)
			{
				nameTooShort.CreateUI();
				return false;
			}

			if (DSave.CheckForSaveName(newName))
			{
				// popup for 'name already exists'
				nameExistsWarning.CreateUI();
				return false;
			}

			if (newName.Length > DSave.maxNameLength)
			{
				// popup for 'name too long'
				nameTooLong.CreateUI();
				return false;
			}

			return true;
		}
	}
}