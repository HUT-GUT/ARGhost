using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrologueManager : MonoBehaviour
{
    public TMPro.TextMeshProUGUI GuideText;
    public GameObject popupPanel, modelTarget;

    private SceneController sceneController;

    private void Awake()
    {
        sceneController = FindObjectOfType<SceneController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void StartDetection()
    {
        modelTarget.SetActive(true);
    }

    public void WaitAndMoveOnToGame1()
    {
        StartCoroutine(WaitForEffect());
    }

    public void TargetDetected()
    {
        stopBGM();
        playDetectionSound();
        GuideText.text = $"'엇? 이게 뭐지?'";
        StartCoroutine(WaitForEffect());
    }

    public void YoureInvited()
    {
        StartCoroutine(WaitForPopup());
    }

    IEnumerator WaitForPopup()
    {
        yield return new WaitForSeconds(3);

        popupPanel.SetActive(true);
        StartCoroutine(WaitForPopupClose());
    }

    IEnumerator WaitForPopupClose()
    {
        yield return new WaitForSeconds(4);

        if (popupPanel.activeSelf)
        {
            popupPanel.SetActive(false);
            StartDetection();
        }
    }

    IEnumerator WaitForEffect()
    {
        yield return new WaitForSeconds(3);
        sceneController.JumpToScene("SpinningGrandma");
    }

    void stopBGM()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("music");
        Destroy(obj);
    }

    void playDetectionSound()
    {
        var emitter = this.GetComponent<FMODUnity.StudioEventEmitter>();
        emitter.Play();
    }
}
