using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;  // Instancia estática

    private int currentLevel = 0;  // Nivel actual
    public string[] levels;  // Nombres o índices de las escenas

    private void Awake()
    {
        // Verificar si ya existe una instancia del GameManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // No destruir al cambiar de escena
        }
        else
        {
            Destroy(gameObject);  // Destruir el GameManager si ya existe una instancia
        }
    }

    public void BeginGame()
    {
        SceneManager.LoadScene(levels[0]);
    }

    // Método para cargar el siguiente nivel
    public void LoadNextLevel()
    {
        if (currentLevel < levels.Length - 1)
        {
            currentLevel++;
            SceneManager.LoadScene(levels[currentLevel]);
        }
        else
        {
            Debug.Log("Has completado todos los niveles.");
        }
    }

    // Método para reiniciar el nivel actual
    public void RestartLevel()
    {
        SceneManager.LoadScene(levels[currentLevel]);
    }
}