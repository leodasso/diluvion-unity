using Diluvion;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

public class LoadSceneAsync : MonoBehaviour
{
	public string sceneName;

	[ReadOnly, ShowInInspector]
	float _unloadBufferTimer;

	[ReadOnly, ShowInInspector]
	bool _pendingUnload;

	AsyncOperation _loadOperation;
	
	#if UNITY_EDITOR
	[Button]
	void TestLoad()
	{
		GameZone.LoadSceneIntoEditor(sceneName);
	}
	#endif

	public void Load()
	{
		_pendingUnload = false;
		SafeLoad();
	}

	public void Unload()
	{
		// If it's not loaded, take no action.
		if (!IsSceneLoaded(sceneName)) return;
		_pendingUnload = true;
	} 

	void Update()
	{
		// Don't allow any load/unload operations until the current operation is complete.
		if (_loadOperation != null)
		{
			if (!_loadOperation.isDone) return;
		}
		
		// -- nothing below this point will happen until the current load operation is complete --

		// Get the load buffer from the game mode
		float loadBuffer = 5;
		if (GameManager.Mode()) loadBuffer = GameManager.Mode().cosmeticSceneCushionTime;

		if (_pendingUnload) _unloadBufferTimer += Time.deltaTime;
		else _unloadBufferTimer = 0;
		
		if (_unloadBufferTimer >= loadBuffer) SafeUnload();
	}

	void SafeLoad()
	{
		if (IsSceneLoaded(sceneName)) return;
		_loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
	}

	void SafeUnload()
	{
		if (!IsSceneLoaded(sceneName)) return;
		_loadOperation = SceneManager.UnloadSceneAsync(sceneName);
	}
	
	/// <summary>
	/// Returns true if the scene with the given name is currently loaded.
	/// </summary>
	bool IsSceneLoaded(string sceneName) 
	{
		for(int i = 0; i<SceneManager.sceneCount; ++i) 
		{
			Scene scene = SceneManager.GetSceneAt(i);
			if(scene.name == sceneName) return true;
		}
		return false;
	}
}