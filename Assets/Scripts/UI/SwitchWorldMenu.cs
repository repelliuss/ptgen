using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class SwitchWorldMenu : MonoBehaviour
{
    static string[] scenes;
    static bool isPaused;

    public GameObject menu;
    public Button switchWorld;
    public Button randomSeed;
    public Text randomSeedText;
    public Dropdown dropdown;
    public Text dropdownLabel;
    public InputField seedInput;
    public Button switchCamera;
    public Text switchCameraText;
    public Button exit;

    Player player;

    bool changeCursor;

    void Start()
    {
        player = FindObjectOfType<Player>();

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        int dropValue = 0;
        dropdown.options.Clear();

        for (int i = 0; i < sceneCount; i++)
        {
            string path = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
            dropdown.options.Add(new Dropdown.OptionData(path));
            if(path == SceneManager.GetActiveScene().name)
            {
                dropValue = i;
            }
        }

        seedInput.characterLimit = 7;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        dropdown.value = dropValue;

        switchCameraText.text = "SWITCH TO " + player.GetCameraName();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    void Resume()
    {
        changeCursor = true;
        menu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause()
    {
        changeCursor = true;
        menu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    void OnGUI()
    {
        if (changeCursor)
        {
            if (menu.activeSelf)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            changeCursor = false;
        }
    }

    public void SwitchCamera()
    {
        player.SwitchCamera();
        switchCameraText.text = "SWITCH TO " + player.GetCameraName();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SwitchWorld()
    {
        int seed = ProceduralTerrain.worldSeed;
        if (seedInput.text.Length == 0 || int.TryParse(seedInput.text, out seed))
        {
            ProceduralTerrain.worldSeed = seed;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
            isPaused = false;
            SceneManager.LoadScene(dropdown.value);
            var chunker = FindObjectOfType<TerrainChunker>();
            if(chunker != null)
            {
                chunker.ResetPlayerPos();
            }
        }
    }

    public void PutRandomSeed()
    {
        seedInput.text = Random.Range(int.MinValue, int.MaxValue).ToString();
    }
}
