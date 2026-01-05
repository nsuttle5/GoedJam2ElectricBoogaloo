using System.Collections;
using TMPro;
using UnityEngine;

public class StartTimer : MonoBehaviour
{
    public TMP_Text text;
    public bool isNegative;
    private int truetime;

    void Start()
    {
        StartCountDown(60);
    }
    public void StartCountDown(int time)
    {
        truetime = time;
        SetTime();
        StopAllCoroutines();
        StartCoroutine(Count());
    }
    void SetTime()
    {
        string setTime = "0"+((truetime-truetime%60)/60).ToString() + ":" + (((truetime%60) < 10) ? "0" : "") + (truetime%60).ToString();
        text.text = (isNegative ? "-" : "") + setTime;
    }

    IEnumerator Count()
    {
        yield return new WaitForSeconds(1);
        truetime += isNegative ? 1 : -1;
        SetTime();
        StartCoroutine(Count());
    }
}
