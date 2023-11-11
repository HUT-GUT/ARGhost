using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{

    public void MoveOnToGame(int gameNumber)
    {
        SceneManager.LoadScene($"Game {gameNumber}");
    }

    public void MoveOnToPrologue()
    {
        SceneManager.LoadScene("Prologue");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Opening");
    }

    public void RestartCurrentGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void JumpToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
