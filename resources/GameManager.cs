using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class IntEvent : UnityEvent<int> {}

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Button playButton;
    [SerializeField]
    private GameObject titleScreen;
    [SerializeField]
    private GameObject gameScreen;
    [SerializeField]
    private GameObject pauseScreen;
    [SerializeField]
    private GameObject gameOverScreen;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private TextMeshProUGUI timeText;
    [SerializeField]
    private TextMeshProUGUI scoreText;

    private PlayerController playerControllerScript;
    private SpawnManager spawnManagerScript;
    private IntEvent jointsUpdated;
    private bool paused;
    private float time;
    private int score;

    public int jointEnemies;
    public bool gameIsActive = false;

    void Start()
    {
        playerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>();
        spawnManagerScript = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
        playButton.onClick.AddListener(StartGame);
    }

    private void Update()
    {
        if (gameIsActive)
        {
            jointsUpdated.Invoke(jointEnemies);
            score++;
            scoreText.text = "Score: " + score;
            timeText.text = "Time: " + Mathf.Round(time);
            time += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
            {
                CheckForPaused();
            }
        }
    }

    private void StartGame()
    {
        titleScreen.SetActive(false);
        gameScreen.SetActive(true);
        gameIsActive = true;
        paused = false;
        jointEnemies = 1;
        score = 0;
        time = 0;
        spawnManagerScript.StartSpawning();

        jointsUpdated = new IntEvent();
        jointsUpdated.AddListener(SetPlayerSpeed);
    }

    private void SetPlayerSpeed(int value)
    {
        if (gameIsActive && jointEnemies >= 5)
        {
            StartCoroutine(SwanSong());
        }

        if (gameIsActive && jointEnemies >= 2)
        {
            float playerSpeed = 1000.0f / value;
            playerControllerScript.playerSpeed = playerSpeed;
        }

        else
        {
            playerControllerScript.playerSpeed = 800.0f;
        }
    }

    private IEnumerator SwanSong()
    {
        yield return new WaitForSeconds(3.0f);

        if (gameIsActive && jointEnemies >=5)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        gameIsActive = false;
        // Stop player movement
        player.GetComponent<PlayerController>().enabled = false;
        // Spawn 20 new elfs to swarm player
        spawnManagerScript.SpawnEnemyWave(100);
        // Make view fogged and bring restart menu on top
        gameOverScreen.SetActive(true);
    }

    public void AddKillToScore(int scoreToAdd)
    {
        score += scoreToAdd;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1;
    }

    private void CheckForPaused()
    {
        if (!paused)
        {
            paused = true;
            pauseScreen.SetActive(true);
            Time.timeScale = 0;
        }

        else
        {
            paused = false;
            pauseScreen.SetActive(false);
            Time.timeScale = 1;
        }
    }
}

