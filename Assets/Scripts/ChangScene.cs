using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ChangScene : MonoBehaviour
{
    public string Scenes;
    public void ESC()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(Scenes);
        }
    }
    public void CS(string Scene)
    {
        SceneManager.LoadScene(Scene);
    }
}
