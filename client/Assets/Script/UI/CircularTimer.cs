using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CircularTimer : MonoBehaviour
{
    //sec
    public float wholeTime;
    public float leftTime;

    public bool isRunning;

    [SerializeField]
    private Image circleImage;
    [SerializeField]
    private RectTransform mask;
    [SerializeField]
    private RectTransform BorderRound1;
    [SerializeField]
    private RectTransform BorderRound2;
    [SerializeField]
    private Text timeText;
    [SerializeField]
    private float thickness;

    public bool displayText = true;

    private float radius;

    private void Awake()
    {
        BorderRound1.sizeDelta = new Vector2(thickness, thickness);
        BorderRound2.sizeDelta = BorderRound1.sizeDelta;
        mask.sizeDelta = circleImage.rectTransform.rect.size - new Vector2(thickness * 2, thickness * 2);
        radius = circleImage.rectTransform.rect.height / 2;

        UpdateTimer();
    }

    public void Stop()
    {
        isRunning = false;
    }

    public void CountDown(float time)
    {
        isRunning = true;
        
        BorderRound1.gameObject.SetActive(true);
        BorderRound2.gameObject.SetActive(true);
        StartCoroutine(CountdownCorutine(time));
    }

    public void SetValue(float leftTime)
    {
        this.leftTime = leftTime;
        UpdateTimer();
    }

    private IEnumerator CountdownCorutine(float time)
    {
        float totalTime = 0;

        while (isRunning && totalTime <= time)
        {
            totalTime += Time.deltaTime;
            leftTime -= Time.deltaTime;

            UpdateTimer();

            if (leftTime <= 0)
            {
                yield break;
            }
            yield return null;
        }
    }

    public void UpdateTimer()
    {
        timeText.text = MakeTimeStr(leftTime);

        float angle = (float)(leftTime / wholeTime * 360);
        float positionAngle = (float)(angle * Mathf.Deg2Rad + Math.PI / 2);

        BorderRound1.transform.localPosition = new Vector2(Mathf.Cos(positionAngle) * radius, Mathf.Sin(positionAngle) * radius);
        BorderRound1.transform.rotation = Quaternion.Euler(0, 0, angle);

        circleImage.fillAmount = leftTime / wholeTime;

        if (leftTime <= 0)
        {
            Stop();
            BorderRound1.gameObject.SetActive(false);
            BorderRound2.gameObject.SetActive(false);
            timeText.text = "00 : 00";
        }

        if (displayText)
        {
            timeText.gameObject.SetActive(true);
        }
        else
        {
            timeText.gameObject.SetActive(false);
        }
    }

    private string MakeTimeStr(float time)
    {
        var intTime = (int)leftTime + 1;

        var min = Mathf.CeilToInt(intTime / 60);
        string minStr, secStr;

        if (min < 10)
            minStr = "0" + min;
        else
            minStr = min.ToString();

        var sec = intTime - min * 60;

        if (sec < 10)
            secStr = "0" + sec;
        else
            secStr = sec.ToString();

        return $"{minStr} : {secStr}";

    }
}
