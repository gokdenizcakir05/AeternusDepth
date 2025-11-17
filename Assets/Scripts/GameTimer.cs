using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameTimer : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public string timeFormat = "{0:00}:{1:00}"; // DAKİKA:SANİYE

    [Header("Timer Settings")]
    public bool timerRunning = true;

    private float elapsedTime = 0f;
    private int minutes = 0;
    private int seconds = 0;
    private bool isPaused = false;
    private static GameTimer instance;

    public static GameTimer Instance => instance;
    public float ElapsedTime => elapsedTime;
    public string FormattedTime => string.Format(timeFormat, minutes, seconds);

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (timerText == null)
        {
            timerText = GetComponentInChildren<TextMeshProUGUI>();
        }

        StartTimer();
        Debug.Log("⏰ Oyun sayacı başladı!");
    }

    void Update()
    {
        if (timerRunning && !isPaused)
        {
            UpdateTimer();
        }
    }

    void UpdateTimer()
    {
        elapsedTime += Time.deltaTime;

        minutes = Mathf.FloorToInt(elapsedTime / 60f);
        seconds = Mathf.FloorToInt(elapsedTime % 60f);

        if (timerText != null)
        {
            timerText.text = string.Format(timeFormat, minutes, seconds);
        }
    }

    public void StartTimer()
    {
        timerRunning = true;
        isPaused = false;
    }

    public void PauseTimer()
    {
        isPaused = true;
    }

    public void ResumeTimer()
    {
        isPaused = false;
    }

    public void StopTimer()
    {
        timerRunning = false;
        isPaused = false;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        minutes = 0;
        seconds = 0;

        if (timerText != null)
        {
            timerText.text = string.Format(timeFormat, 0, 0);
        }
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }
}