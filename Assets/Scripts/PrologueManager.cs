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
        StartCoroutine(WaitForPopup());
    }

    // Update is called once per frame
    void Update()
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
        GuideText.text = $"'엇? 이게 뭐지?'";
        StartCoroutine(WaitForEffect());
    }

    IEnumerator WaitForPopup()
    {
        yield return new WaitForSeconds(3);

        popupPanel.SetActive(true);
    }

    IEnumerator WaitForEffect()
    {
        yield return new WaitForSeconds(3);

        sceneController.MoveOnToGame(1);
    }
}
