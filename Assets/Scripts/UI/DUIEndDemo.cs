using UnityEngine;
using System.Collections;
using Diluvion;

public class DUIEndDemo : MonoBehaviour
{
    public float endTime = 15;   

    public void Start()
    {
        GameManager.Freeze(this);
        Cursor.visible = true;
        if(Application.isEditor) Cursor.lockState = CursorLockMode.None;
        else Cursor.lockState = CursorLockMode.Confined;        
        StartCoroutine(EndDemotimer(endTime));
    }

    IEnumerator EndDemotimer(float time)
    {
        yield return new WaitForSeconds(time);
        EndGameToMenu();
    }
    
    public void EndGameToMenu()
    {
        //StageManager.Get().RestartGame();
    }

	public void PlayEndingMusic() {
		///Music.Get().AddMusic(musictrack);   
	}
}
