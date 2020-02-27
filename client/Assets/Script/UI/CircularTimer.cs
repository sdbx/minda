using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class CircularTimer : MonoBehaviour
{
    //sec
    public float wholeTime;
    public float leftTime;

    public bool isRunning;

    [SerializeField]
    private Image circleImage;
    [FormerlySerializedAs("BorderRound1")] [SerializeField]
    private RectTransform borderRound1;
    [FormerlySerializedAs("BorderRound2")] [SerializeField]
    private RectTransform borderRound2;
    [SerializeField]
    private Text timeText;
    [SerializeField]
    private float thickness;

    public bool displayText = true;

    private float _radius;

    private void Awake()
    {
        borderRound1.sizeDelta = new Vector2(thickness, thickness);
        borderRound2.sizeDelta = borderRound1.sizeDelta;
        _radius = circleImage.rectTransform.rect.height / 2;

        UpdateTimer();
    }

    public void Stop()
    {
        isRunning = false;
    }

    public void CountDown(float time)
    {
        isRunning = true;

        borderRound1.gameObject.SetActive(true);
        borderRound2.gameObject.SetActive(true);
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

        var angle = (float)(leftTime / wholeTime * 360);
        var positionAngle = (float)(angle * Mathf.Deg2Rad + Math.PI / 2);

        borderRound1.transform.localPosition = new Vector2(Mathf.Cos(positionAngle) * (_radius - thickness / 2), Mathf.Sin(positionAngle) * (_radius - thickness / 2));
        borderRound1.transform.rotation = Quaternion.Euler(0, 0, angle);

        circleImage.fillAmount = leftTime / wholeTime;

        if (leftTime <= 0)
        {
            Stop();
            borderRound1.gameObject.SetActive(false);
            borderRound2.gameObject.SetActive(false);
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
        var intTime = (int)leftTime;

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
