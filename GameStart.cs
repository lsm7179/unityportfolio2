using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Proyecto26;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class GameStart : MonoBehaviour
{
    [SerializeField]
    Image progressBar;

    //씬 넘기기
    public void OnSceneLoad()
    {
        StartCoroutine(LoadScene());
    }

    public void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    IEnumerator LoadScene()
    {
        progressBar.gameObject.SetActive(true);
        yield return null;
        AsyncOperation op = SceneManager.LoadSceneAsync("SampleScene");
        op.allowSceneActivation = false;
        float timer = 0.0f;
        while (!op.isDone)

        {
            yield return null;
            timer += Time.deltaTime;
            if (op.progress < 0.9f)

            {
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, op.progress, timer);
                if (progressBar.fillAmount >= op.progress)

                {
                    timer = 0f;
                }
            }
            else

            {
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, 1f, timer);
                if (progressBar.fillAmount == 1.0f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}
