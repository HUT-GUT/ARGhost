using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Game3Manager : MonoBehaviour
{
    public TMPro.TextMeshProUGUI GuideText;
    public GameObject CongratsScreen, GameScreen, MissionClearScreen, EndingScreen;
    public GameObject SlidersPanel, FinishButtonPanel;
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
    private Ghost ghost = null;
    private List<Ghost> ghosts = new List<Ghost>();
    private List<GameObject> ghostObjects;
    private List<bool> isManipulatedGhosts = new List<bool>
    {
        false, false, false, false, false
    };

    private void Awake()
    {
        ghostObjects = new List<GameObject>
        {
            Ghost1, Ghost2, Ghost3, Ghost4, Ghost5
        };
        foreach (GameObject ghostObject in ghostObjects)
        {
            ghosts.Add(ghostObject.GetComponent<Ghost>());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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
                ghost = currentGhost.GetComponent<Ghost>();
                PitchSlider.value = ghost.pitchScale;
                FlangerSlider.value = ghost.flangerScale;

                // make it unvisible!
                foreach (GameObject ghostObject in ghostObjects)
                {
                    if (ghostObject.name == currentGhost.name)
                    {
                        ghostObject.GetComponent<Ghost>().select();
                    }
                    else
                    {
                        ghostObject.GetComponent<Ghost>().disable();
                    }
                }
            }
            else if (EventSystem.current.currentSelectedGameObject == null)
            {
                ResetSelection();
            }
        }
    }

    void ResetSelection()
    {
        foreach (GameObject ghostObject in ghostObjects)
        {
            ghostObject.GetComponent<Ghost>().select();
        }
        currentGhost = null;
        ghost = null;
    }

    public void DisplayFinish()
    {
        GameScreen.SetActive(true);
    }

    public void FinishGame()
    {
        GuideText.text = $"당신의 소리 유물이 완성되었습니다!";
        GameScreen.SetActive(false);
        MissionClearScreen.SetActive(true);
        stopSinging();
        finalSong();
    }

    public void DisplayEnding()
    {
        stopSinging();
        MissionClearScreen.SetActive(false);
        EndingScreen.SetActive(true);
    }

    public void DisplayPositioningGuide()
    {
        GuideText.text = "나만의 유물함을 탭하여 소리 유물들을 불러오세요.";
    }

    public void PositioningCompleted()
    {
        GuideText.text = $"두 손가락으로 유령들의 방향을 회전시킬 수 있습니다.\n 각 유령을 선택하고 슬라이드를 조정하여 소리와 모양을 바꿔보세요.";
        for (int i = 0; i < ghostObjects.Count; i++)
        {
            GameObject ghostObject = ghostObjects[i];
            var fmodInstance = FMODUnity.RuntimeManager.CreateInstance($"event:/Choir/Ghost{i + 1}");
            ghostObject.GetComponent<Ghost>().Init(fmodInstance);
        }
        startSinging();
        currentState = UserState.Playing;
    }

    public void onPitchScaleChange()
    {
        float pitchValue = PitchSlider.value;
        float sizeRatio = (pitchValue * 1 / 6) + 0.5f;
        ghost.pitchScale = pitchValue;
        Vector3 newSize = new Vector3(ghost.changedScale.x, ghost.originalScale.y * sizeRatio, ghost.originalScale.z);
        ghost.changeScale(newSize);
    }

    public void onFlangerScaleChange()
    {
        float flangerValue = FlangerSlider.value;
        float sizeRatio = (flangerValue * 1 / 8) + 0.5f;
        ghost.flangerScale = flangerValue;
        Vector3 newSize = new Vector3(ghost.originalScale.x * sizeRatio, ghost.changedScale.y, ghost.originalScale.z);
        ghost.changeScale(newSize);
    }

    void startSinging()
    {
        foreach (Ghost ghost in ghosts)
        {
            ghost.play();
        }
    }

    public void finalSong()
    {
        foreach (Ghost ghost in ghosts)
        {
            ghost.playOnce();
        }
    }

    public void stopSinging()
    {
        foreach (Ghost ghost in ghosts)
        {
            ghost.stop();
        }
    }
}
