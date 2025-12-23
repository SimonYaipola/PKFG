using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneButton : MonoBehaviour
{
    [SerializeField] private string sceneName = "TownScene";

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
