using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Game3Manager : MonoBehaviour
{
    public TMPro.TextMeshProUGUI GuideText;
    public Slider RemainingTimeSlider;
    public GameObject CongratsScreen, GameScreen, SuccessScreen;
    public GameObject SlidersPanel, FinishButtonPanel, FinishButton2Panel, PopupPanel, TimerPanel;
    public GameObject Ghost1, Ghost2, Ghost3, Ghost4, Ghost5;
    public Slider PitchSlider, FlangerSlider;
    public enum UserState
    {
        Ready,
        Playing,
        Success,
    }
    public UserState currentState = UserState.Ready;
    // private variables
    private GameObject currentGhost = null;
    private Ghost_Game3 ghost = null;
    private List<Ghost_Game3> ghosts = new List<Ghost_Game3>();
    private List<GameObject> ghostObjects;
    private List<bool> isManipulatedGhosts = new List<bool>
    {
        false, false, false, false, false
    };
    private float timeRemaining = 91.0f;

    private void Awake()
    {
        ghostObjects = new List<GameObject>
        {
            Ghost1, Ghost2, Ghost3, Ghost4, Ghost5
        };
        foreach (GameObject ghostObject in ghostObjects)
        {
            ghosts.Add(ghostObject.GetComponent<Ghost_Game3>());
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Timer Off
        if (timeRemaining <= 0)
        {
            if (currentState != UserState.Success)
            {
                FinishGame();
            }
        }
        else if (currentState == UserState.Playing)
        {
            if (TimerPanel.activeSelf)
            {
                // ~~~ Playing ~~~
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining, RemainingTimeSlider);
            }

            // Check Mission Clear
            if (isManipulatedGhosts.TrueForAll(x => x == true))
            {
                FinishButtonPanel.SetActive(true);
            }
            // Touch processing
            if (Input.touchCount > 0 && currentState == UserState.Playing)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (!SlidersPanel.activeSelf)
                    {
                        SlidersPanel.SetActive(true);
                    }
                    currentGhost = hit.transform.gameObject;
                    string currentIndex = currentGhost.name.Substring(5);
                    isManipulatedGhosts[int.Parse(currentIndex) - 1] = true;
                    ghost = currentGhost.GetComponent<Ghost_Game3>();
                    PitchSlider.value = ghost.pitchScale;
                    FlangerSlider.value = ghost.flangerScale;

                    // make it unvisible!
                    foreach (GameObject ghostObject in ghostObjects)
                    {
                        if (ghostObject.name == currentGhost.name)
                        {
                            ghostObject.GetComponent<Ghost_Game3>().select();
                        }
                        else
                        {
                            ghostObject.GetComponent<Ghost_Game3>().disable();
                        }
                    }
                }
                else if (EventSystem.current.currentSelectedGameObject == null)
                {
                    ResetSelection();
                }
            }
        }
    }

    void DisplayTime(float timeToDisplay, Slider RemainingTimeSlider)
    {
        float seconds = Mathf.Floor(timeToDisplay);
        RemainingTimeSlider.value = seconds;
    }

    void ResetSelection()
    {
        foreach (GameObject ghostObject in ghostObjects)
        {
            ghostObject.GetComponent<Ghost_Game3>().select();
        }
        SlidersPanel.SetActive(false);
        currentGhost = null;
        ghost = null;
    }

    public void DisplayFinish()
    {
        GameScreen.SetActive(true);
    }

    public void FinishGame()
    {
        currentState = UserState.Success;
        TimerPanel.SetActive(false);
        GuideText.text = "나만의 소리 유물이 완성되었군!";
        ResetSelection();
        //GameScreen.SetActive(false);
        FinishButton2Panel.SetActive(true);
        stopSinging();
        finalSong();
    }

    public void DisplayEnding()
    {
        stopSinging();
        GameScreen.SetActive(false);
        SuccessScreen.SetActive(true);
    }

    public void DisplayPositioningGuide()
    {
        GuideText.text = "나만의 유물함을 탭하여 소리 유물들을 불러오세요.";
    }

    public void PositioningCompleted()
    {
        stopBGM();
        TimerPanel.SetActive(true);
        GuideText.text = $"각 유령을 하나씩 선택하고 슬라이드를 조정하여 소리와 모양을 바꿔보세요.";
        for (int i = 0; i < ghostObjects.Count; i++)
        {
            GameObject ghostObject = ghostObjects[i];
            var fmodInstance = FMODUnity.RuntimeManager.CreateInstance($"event:/DIY/Sound{i + 1}");
            ghostObject.GetComponent<Ghost_Game3>().Init(fmodInstance);
        }
        startSinging();
        currentState = UserState.Playing;
    }

    public void onPitchScaleChange()
    {
        float pitchValue = PitchSlider.value;
        float sizeRatio = pitchValue + 0.5f;
        ghost.pitchScale = pitchValue;
        Vector3 newSize = new Vector3(ghost.changedScale.x, ghost.originalScale.y * sizeRatio, ghost.originalScale.z);
        ghost.changeScale(newSize);
    }

    public void onFlangerScaleChange()
    {
        float flangerValue = FlangerSlider.value;
        float sizeRatio = flangerValue + 0.5f;
        ghost.flangerScale = flangerValue;
        Vector3 newSize = new Vector3(ghost.originalScale.x * sizeRatio, ghost.changedScale.y, ghost.originalScale.z);
        ghost.changeScale(newSize);
    }

    void startSinging()
    {
        foreach (Ghost_Game3 ghost in ghosts)
        {
            ghost.play();
        }
    }

    IEnumerator WaitForFinalSong()
    {
        startSinging();
        yield return new WaitForSeconds(16);

        stopSinging();
        DisplayEnding();
    }

    void finalSong()
    {
        StartCoroutine(WaitForFinalSong());
    }

    public void stopSinging()
    {
        foreach (Ghost_Game3 ghost in ghosts)
        {
            ghost.stop();
        }
    }

    IEnumerator WaitForPopupClose()
    {
        yield return new WaitForSeconds(4);

        if (PopupPanel.activeSelf)
        {
            PopupPanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        StartCoroutine(WaitForPopupClose());
        PositioningCompleted();
    }

    void stopBGM()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("music");

    }
}
