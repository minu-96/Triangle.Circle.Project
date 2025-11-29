using UnityEngine;
using UnityEngine.SceneManagement;
public class ChangScene : MonoBehaviour
{
    public void CS(string Scene)
    {
        SceneManager.LoadScene(Scene);
    }
}
