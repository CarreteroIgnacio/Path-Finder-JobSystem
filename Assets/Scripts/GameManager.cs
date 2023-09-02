using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        winUI.SetActive(false);
        Time.timeScale = 1;
    }


    public GameObject winUI;
    public void Win()
    {
        Time.timeScale = 0;
        winUI.SetActive(true);
    }

    public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
