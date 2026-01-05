using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToTItle : MonoBehaviour
{
    public GameObject cam;
    public float returnTime = 90f;

    public void Start()
    {
        StartCoroutine(ReturnToTitle());
    }

    public IEnumerator ReturnToTitle()
    {
        yield return new WaitForSeconds(returnTime);
        SceneManager.LoadScene("0_Title");
        Destroy(GameObject.Find("ScreenFader").gameObject);
        Destroy(cam.gameObject);
    }
}
