using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Audio;

[System.Serializable]
public class Level
{
    // public int levelSceneIndex;
    public string levelName;
    public string tips;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
    }

    [Header("Level Select")]
    [SerializeField] private Level[] levels;
    [SerializeField] private Transform levelLayout;
    [SerializeField] private GameObject[] levelButtons;
    [SerializeField] private GameObject levelButton;

    [Header("Transitions")]
    [SerializeField] private Animator transitionAnim;

    [Header("Level UI")]
    [SerializeField] private TMP_Text levelTitle;
    [SerializeField] private TMP_Text levelTips;
    [SerializeField] private Button nextLevelButton;
    [Space]
    [SerializeField] private Image[] snakeSegments;
    [SerializeField] private TMP_Text segmentCount;
    [SerializeField] private Color segmentInactiveColor;
    private int currentLevel = -1;

    [Header("Audio")]
    [SerializeField] private Slider[] musicSliders;
    [SerializeField] private Slider[] sfxSliders;

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            SceneManager.LoadSceneAsync(1);
            PlayTransitionSound(false);
        }

        levelButtons = new GameObject[levels.Length];
        // Create level select buttons
        for (int i = 0; i < levels.Length; i++)
        {
            GameObject button = Instantiate(levelButton, levelLayout);
            levelButtons[i] = button;
            button.GetComponentInChildren<TMP_Text>().text = $"{i + 1} ";
            int sceneIndex = i;
            Button b = button.GetComponent<Button>();

            b.onClick.AddListener(() => EnterLevel(sceneIndex));
        }

        UpdateLevelButtons();
        InitializeAllButtonSounds();
        InitializeVolumeSliders();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlayTransitionSound(bool enter)
    {
        SoundManager.Instance.Play(enter ? 9 : 10, 0.5f, true);
    }

    private void InitializeAllButtonSounds()
    {
        foreach (Button b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            b.onClick.AddListener(() => SoundManager.Instance.Play(8, 0.6f, true));
        }
    }

    private void InitializeVolumeSliders()
    {
        foreach (Slider s in musicSliders)
        {
            s.onValueChanged.AddListener(SetMusicVolume);
        }
        foreach (Slider s in sfxSliders)
        {
            s.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    private void UpdateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool enabled = i <= PlayerPrefs.GetInt("completed");
            levelButtons[i].GetComponent<Button>().interactable = enabled;
            levelButtons[i].transform.GetChild(0).gameObject.SetActive(enabled);
        }
    }

    public void UnlockAllLevels()
    {
        PlayerPrefs.SetInt("completed", levels.Length);
        UpdateLevelButtons();
    }

    public void SetSnakeMeter(int maxSize, int currentSize)
    {
        foreach (Image segment in snakeSegments)
        {
            segment.gameObject.SetActive(false);
        }

        for (int i = 0; i < Mathf.Max(maxSize - currentSize, 1); i++)
        {
            snakeSegments[i].gameObject.SetActive(true);
            //snakeSegments[i].color = i < currentSize ? segmentInactiveColor : Color.white;
        }

        segmentCount.text = $"{maxSize - currentSize}";
    }

    private void EnterLevel(int levelIndex)
    {
        StartCoroutine(_EnterLevel(levelIndex));
    }

    private IEnumerator _EnterLevel(int levelIndex)
    {
        transitionAnim.SetTrigger("Start");
        PlayTransitionSound(true);
        yield return new WaitForSeconds(0.5f); // allow half second transition time

        SceneManager.LoadScene(levelIndex + 2); // the offset is pretty important
        currentLevel = levelIndex;
        levelTitle.text = $"{levelIndex + 1}. {levels[levelIndex].levelName}";
        levelTips.text = levels[levelIndex].tips;
        GUIManager.Instance.SetPage("game");

        transitionAnim.SetTrigger("End");
        PlayTransitionSound(false);
    }

    public void CompleteLevel()
    {
        PlayerPrefs.SetInt("completed", Mathf.Max(PlayerPrefs.GetInt("completed"), currentLevel + 1));
        UpdateLevelButtons();

        GUIManager.Instance.SetPage("level_end");
        nextLevelButton.gameObject.SetActive(currentLevel + 1 < levels.Length && currentLevel >= 0);
        if (!nextLevelButton.gameObject.activeInHierarchy)
        {
            GUIManager.Instance.SetPage("game_over");
        }
    }

    public void NextLevel()
    {
        currentLevel++;
        nextLevelButton.gameObject.SetActive(currentLevel + 1 < levels.Length && currentLevel >= 0);
        EnterLevel(currentLevel);
    }

    public void BackToMenu()
    {
        StartCoroutine(_BackToMenu());
    }

    private IEnumerator _BackToMenu()
    {
        transitionAnim.SetTrigger("Start");
        PlayTransitionSound(true);
        yield return new WaitForSeconds(0.5f); // allow half second transition time

        currentLevel = -1;
        SceneManager.LoadSceneAsync(1); // lol bad code... just assume 0 is the main menu or whatev
        GameObject.FindGameObjectWithTag("CameraParent").transform.position = Vector3.zero;
        GUIManager.Instance.SetPage("main");

        transitionAnim.SetTrigger("End");
        PlayTransitionSound(false);
    }

    public void SetMusicVolume(float value)
    {
        SoundManager.Instance.SetParameter("MusicVolume", value);
        foreach (Slider s in musicSliders)
        {
            s.value = value;
        }
    }

    public void SetSFXVolume(float value)
    {
        SoundManager.Instance.SetParameter("SFXVolume", value);
        foreach (Slider s in sfxSliders)
        {
            s.value = value;
        }
    }
}