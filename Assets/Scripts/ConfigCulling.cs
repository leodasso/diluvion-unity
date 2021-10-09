using UnityEngine;
using Diluvion;

public enum ConfigCondition
{
    DisableIfConfig,
    DisableIfNotConfig
}

/// <summary>
/// Turns the game object on or off on awake, based on the game mode settings.
/// </summary>
public class ConfigCulling : MonoBehaviour {

    public GameMode validGameMode;
	//public BuildConfigContainer configToCheck;
    public ConfigCondition condition;

    public void Awake()
    {
        if (GameManager.Mode() == validGameMode && condition == ConfigCondition.DisableIfConfig)
            gameObject.SetActive(false);

        if (GameManager.Mode() != validGameMode && condition == ConfigCondition.DisableIfNotConfig)
            gameObject.SetActive(false);
    }
}
