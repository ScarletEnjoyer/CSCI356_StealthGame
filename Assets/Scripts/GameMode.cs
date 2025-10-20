using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;

public class GameMode : MonoBehaviour
{
    public static GameMode Instance { get; private set; }

    private GameObject deadMask;
    public GameObject winMask;
    public TMP_Text winText;

    private AudioSource audioSrc;
    public UnityEvent OnAlarmEvents;

    private float runStartTime;

    void Start()
    {
        Instance = this;
        runStartTime = Time.time;

        var canvas = GameObject.Find("Canvas")?.transform;
        if (canvas != null)
        {
            var dm = canvas.Find("DeadMask");
            if (dm) deadMask = dm.gameObject;
            if (winMask == null)
            {
                var wm = canvas.Find("WinMask");
                if (wm) winMask = wm.gameObject;
            }
        }

        if (deadMask) deadMask.SetActive(false);
        if (winMask) winMask.SetActive(false);

        audioSrc = GetComponent<AudioSource>();
        if (OnAlarmEvents == null) OnAlarmEvents = new UnityEvent();
    }

    public void GameOver()
    {
        if (deadMask) deadMask.SetActive(true);
    }

    public void RestartLevel()
    {
        string cur = SceneManager.GetActiveScene().name;
        Time.timeScale = 1f;
        SceneManager.LoadScene(cur);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Alarm()
    {
        if (audioSrc != null && !audioSrc.isPlaying)
        {
            AudioClip clip = Resources.Load<AudioClip>("Sound/Alarm");
            if (clip != null)
            {
                audioSrc.clip = clip;
                audioSrc.Play();
            }
        }
        OnAlarmEvents?.Invoke();
    }

    public void Victory()
    {
        float used = Time.time - runStartTime;

        if (winMask)
        {
            winMask.SetActive(true);
            winMask.transform.SetAsLastSibling();
        }

        if (winText)
        {
            winText.text =
                $"MISSION COMPLETE\n" +
                $"Time: {FormatTime(used)}";
        }

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        float s = seconds % 60f;
        return $"{m:00}:{s:00.00}";
    }
}
