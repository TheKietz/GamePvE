using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingStage1 : MonoBehaviour
{
    public void NextScence()
    {
        SceneManager.LoadScene("Stage 1");
    }
}
