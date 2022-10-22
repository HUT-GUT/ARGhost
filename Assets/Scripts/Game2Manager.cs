using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class Game2Manager : MonoBehaviour
{
    public TMPro.TextMeshProUGUI GuideText;
    public Slider RemainingTimeSlider;
    public GameObject TimerPanel, SlidersPanel;
    public GameObject GameScreen, GameOverScreen, GameSuccessScreen;
    public GameObject Ghost1, Ghost2, AudioHint1, AudioHint2;
    public Slider PitchSlider, FlangerSlider;

    // private variables
    private enum UserState
    {
        Ready,
        Playing,
        Success,
        GameOver
    }
    private UserState currentState = UserState.Ready;
    private List<bool> isRestoredGhosts = new List<bool>
    {
        false, false
    };
    private int currentIndex = 0;
    private GameObject currentGhost = null;
    private Ghost ghost = null;
    private float timeRemaining = 60.0f;
    private List<Slider> PitchSliders, FlangerSliders;
    private List<GameObject> Ghosts;
    private List<GameObject> audioHints;
    private SceneController sceneController;

    private void Awake()
    {
        Ghosts = new List<GameObject>
        {
            Ghost1, Ghost2
        };
        audioHints = new List<GameObject>
        {
            AudioHint1, AudioHint2
        };
        sceneController = FindObjectOfType<SceneController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        currentState = UserState.Ready;
    }

    // Update is called once per frame
    void Update()
    {
        // Timer Off
        if (timeRemaining <= 0)
        {
            if (currentState != UserState.GameOver)
            {
                ghost.stop();
                currentState = UserState.GameOver;
                TimerPanel.SetActive(false);
                GameScreen.SetActive(false);
                GameOverScreen.SetActive(true);
            }
        }
        else if (currentState == UserState.Playing)
        {
            // ~~~ Playing ~~~
            timeRemaining -= Time.deltaTime;
            DisplayTime(timeRemaining, RemainingTimeSlider);

            // Check Mission Clear
            if (isRestoredGhosts.TrueForAll(x => x == true))
            {
                MissionClear();
            }
            // Check Reset
            else if (currentGhost == null && currentIndex < Ghosts.Count)
            {
                StartGhostRestoring(currentIndex);
            }
            else
            {
                if (currentIndex == 0 && ghost.pitchScale == 2 && ghost.flangerScale == 5)
                {
                    // Restore Success!!
                    GuideText.text = $"{currentIndex+1}번째 유령의 복원이 무사히 완료되었습니다! \n";
                    ghost.stop();
                    isRestoredGhosts[currentIndex] = true;
                    ResetForNextRound();
                }
                else if (currentIndex == 1 && ghost.pitchScale == 6 && ghost.flangerScale == 5)
                {
                    // Restore Success!!
                    GuideText.text = $"{currentIndex + 1}번째 유령의 복원도 무사히 완료되었습니다! \n";
                    ghost.stop();
                    isRestoredGhosts[currentIndex] = true;
                    ResetForNextRound();
                }
            }

            // Touch processing
            if (Input.touchCount > 0)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    ghost.select();
                    if (!SlidersPanel.activeSelf)
                    {
                        SlidersPanel.SetActive(true);
                    }
                    GuideText.text = $"슬라이더를 움직여 유령의 모양을 바꿔서 원래 소리대로 맞춰보세요.\n";

                    if (!ghost.isPlaying)
                    {
                        ghost.play();
                    }
                }
            }
        }
    }

    void StartGhostRestoring(int ghostIndex)
    {
        audioHints[ghostIndex].SetActive(true);
        currentGhost = Ghosts[ghostIndex];
        var fmodInstance = FMODUnity.RuntimeManager.CreateInstance($"event:/3D/Ghost{currentIndex + 1}");
        ghost = currentGhost.GetComponent<Ghost>();
        ghost.Init(fmodInstance);
        currentGhost.SetActive(true);
        ghost.disable();
        PitchSlider.value = ghost.pitchScale;
        FlangerSlider.value = ghost.flangerScale;
        GuideText.text = $"{ghostIndex + 1}번째 유령을 탭하여 소리를 들어보세요.\n";
    }

    void DisplayTime(float timeToDisplay, Slider RemainingTimeSlider)
    {
        float seconds = Mathf.Floor(timeToDisplay);
        RemainingTimeSlider.value = seconds;
    }
    

    void ResetForNextRound()
    {
        audioHints[currentIndex].SetActive(false);
        currentGhost = null;
        ghost = null;
        currentIndex++;
        SlidersPanel.SetActive(false);
    }

    void MissionClear()
    {
        GuideText.text = "축하합니다!";
        currentState = UserState.Success;
        GameScreen.SetActive(false);
        GameSuccessScreen.SetActive(true);
    }

    public void startPlaying()
    {
        if (currentGhost != null && ghost.isPlaying == false)
        {
            ghost.play();
            ghost.select();
        }
    }

    public void stopPlaying()
    {
        if (currentGhost != null && ghost.isPlaying == true)
        {
            ghost.stop();
            ghost.disable();
        }
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

    public void GameStart()
    {        
        TimerPanel.SetActive(true);
        currentState = UserState.Playing;
    }
}
