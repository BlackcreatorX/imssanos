using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
public void Jugar()
{
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); 
}
public void Salir()
{
    Application.Quit();
    Debug.Log("Saliendo del juego...");
}
}

