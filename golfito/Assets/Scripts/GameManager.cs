using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;

public class GameManager : MonoBehaviour
{
    // Clase para guardar la puntuacion del player
    private class ScoreEntry
    {
        public string Name { get; }
        public int Score { get; }

        public ScoreEntry(string name, int score)
        {
            Name = name;
            Score = score;
        }
    }
    
    public static GameManager Instance;  // Instancia estática
    public string[] levels;  // Nombres o índices de las escenas
    public TMP_Text scoreBoardText;
    public TMP_InputField playerUsernameField;
    
    private FirebaseFirestore db;
    private int currentLevel = 0;  // Nivel actual
    private int throwedBallCount = 0; // Contador de veces que la bola ha sido lanzada
    private String playerUsername;
    private bool dbReady = false;
    private bool needUpdate = true;
    
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
    
    private void Start()
    {
        // Hacemos la conexión con Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                // En caso de que la conexión este disponible
                // Hacemos la instancia de el Firestore
                db = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firebase inicializado con éxito.");
                dbReady = true;
            }
            else
            {
                Debug.LogError("No se pudo inicializar Firebase.");
            }
        });
    }

    public void BeginGame()
    {
        // En caso de que el player no haya puesto su username no le dejaremos empezar la partida
        if (playerUsernameField.text == "" || playerUsernameField.text == null)
            return;
        
        // Guardamos el username en el GameManager
        playerUsername = playerUsernameField.text;
        // Reiniciamos el InputField del username para la siguiente partida
        playerUsernameField.text = "";
        // Reiniciamos las veces que se ha lanzado la pelota en caso de que sigan guardadas de la anterior partida
        throwedBallCount = 0;
        // Cargamos el primer nivel
        SceneManager.LoadScene(levels[0]);
    }

    // Método para cargar el siguiente nivel
    public void LoadNextLevel()
    {
        // Comprobamos si siguen quedando msa niveles
        if (currentLevel < levels.Length - 1)
        {
            // Sumamos la cuenta del nivel por el que vamos
            currentLevel++;
            // Cargamos el siguiente nivel
            SceneManager.LoadScene(levels[currentLevel]);
        }
        else
        {
            // Acabamos la partida ya que no quedan mas niveles
            EndGame();
        }
    }

    // Método para reiniciar el nivel actual
    public void RestartLevel()
    {
        SceneManager.LoadScene(levels[currentLevel]);
    }

    // Metodo que se ejecuta una vez el player a completado todos los niveles
    public void EndGame()
    {
        SaveScore(playerUsername, throwedBallCount);
        SceneManager.LoadScene(0);
        needUpdate = true;
    }
    
    // Metodo para poder contar las veces que sea a tirado la pelota en la partida
    public void ThrowedBall()
    {
        throwedBallCount++;
    }
    
    // Metodo para guardar la score del player
    private void SaveScore(string playerName, int score)
    {
        // Referencia al documento del jugador
        DocumentReference docRef = db.Collection("scores").Document(playerName);

        // Obtiene el documento para comprobar si ya existe una puntuación guardada
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task => 
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    // Si el documento existe, obtenemos la puntuación almacenada
                    int existingScore = snapshot.GetValue<int>("score");

                    // Comprobamos si la nueva puntuación es menor que la existente
                    if (score < existingScore)
                    {
                        // Crea un objeto con los datos a guardar solo si la nueva puntuación es menor
                        Dictionary<string, object> playerData = new Dictionary<string, object>
                        {
                            { "name", playerName },
                            { "score", score }
                        };

                        // Guarda los nuevos datos en Firestore
                        docRef.SetAsync(playerData).ContinueWithOnMainThread(saveTask => 
                        {
                            if (saveTask.IsCompleted)
                            {
                                Debug.Log("Puntuación guardada con éxito.");
                            }
                            else
                            {
                                Debug.LogError("Error al guardar la puntuación: " + saveTask.Exception);
                            }
                        });
                    }
                    else
                    {
                        Debug.Log("La nueva puntuación no es menor que la puntuación existente. No se guarda.");
                    }
                }
                else
                {
                    // Si no existe puntuación previa, guardamos la nueva
                    Dictionary<string, object> playerData = new Dictionary<string, object>
                    {
                        { "name", playerName },
                        { "score", score }
                    };

                    docRef.SetAsync(playerData).ContinueWithOnMainThread(saveTask => 
                    {
                        if (saveTask.IsCompleted)
                        {
                            Debug.Log("Puntuación guardada con éxito.");
                        }
                        else
                        {
                            Debug.LogError("Error al guardar la puntuación: " + saveTask.Exception);
                        }
                    });
                }
            }
            else
            {
                Debug.LogError("Error al obtener la puntuación del jugador: " + task.Exception);
            }
        });
    }
    
    // Metodo para obtener los 5 mejores scores que hay en la Firestore
    private async Task<ScoreEntry[]> GetTop5Scores()
    {
        List<ScoreEntry> topScores = new List<ScoreEntry>();

        // Consulta para obtener los 5 jugadores con menor puntuación
        QuerySnapshot snapshot = await db.Collection("scores")
            .OrderBy("score")  // Ordenar por puntuación de menor a mayor
            .Limit(5)               // Limitar a los 5 mejores
            .GetSnapshotAsync();

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            string name = document.GetValue<string>("name");
            int score = document.GetValue<int>("score");
            topScores.Add(new ScoreEntry(name, score));
        }

        return topScores.ToArray();
    }

    // Metodo para actualizar la scoreboard del menu principal
    public async void UpdateTop5Score()
    {
        if (!dbReady)
            return;
        
        // En caso de que el scoreBoardText sea null lo buscamos por tag
        // (esto se hace para que cuando volvamos al menu no tengamos probelmas para poner el score, etc)
        if (scoreBoardText == null)
            scoreBoardText = GameObject.FindGameObjectWithTag("Scoreboard")?.GetComponent<TMP_Text>();

        // En caso de que el playerUsernameField sea null lo buscamos por tag
        // (esto se hace para que cuando volvamos al menu no tengamos probelmas para poner el score, etc)
        if (playerUsernameField == null)
            playerUsernameField = GameObject.FindGameObjectWithTag("Username")?.GetComponent<TMP_InputField>();
        
        ScoreEntry[] topScores = await GetTop5Scores();
        scoreBoardText.text = "";

        foreach (var score in topScores)
        {
            scoreBoardText.text += $"{score.Name}: {score.Score}\n";
        }

        needUpdate = false;
    }

    public bool getNeedUpdate()
    {
        return needUpdate;
    }
}