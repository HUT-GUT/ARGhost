using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private float defaultTimeRemaining = 91.0f;
    private float timeRemaining;
    private List<GameObject> Ghosts;
    private List<GameObject> audioHints;
    private float userPitchScale;
    private float userFlangerScale;
    private bool isWaiting = false;

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
        timeRemaining = defaultTimeRemaining;
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
            if (TimerPanel.activeSelf)
            {
                // ~~~ Playing ~~~
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining, RemainingTimeSlider);
            }

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
                // Restore Success!!
                if (currentIndex == 0 && userPitchScale == 2 && !isWaiting)
                {
                    StartCoroutine(GhostRestoreSuccess(currentIndex));
                }
                else if (currentIndex == 1 && userFlangerScale == 5 && !isWaiting)
                {
                    StartCoroutine(GhostRestoreSuccess(currentIndex));
                }
            }

            // Touch processing
            if (Input.touchCount > 0)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (!TimerPanel.activeSelf)
                    {
                        TimerPanel.SetActive(true);
                    }
                    if (!SlidersPanel.activeSelf)
                    {
                        SlidersPanel.SetActive(true);
                    }
                    ghost.select();
                    GuideText.text = $"힌트 버튼을 눌러 유령의 원래 소리를 잘 듣고 기억하세요. \n슬라이더를 조정하여 유령의 모양과 소리를 바꿔서 원래 소리에 맞게 복원해보세요.";

                    if (!ghost.isPlaying)
                    {
                        ghost.play();
                    }  
                }
                else if (EventSystem.current.currentSelectedGameObject != null)
                {
                    if (Input.touches[0].phase == TouchPhase.Ended)
                    {
                        userPitchScale = ghost.pitchScale;
                        userFlangerScale = ghost.flangerScale;
                    }
                }
            }
        }
    }

    IEnumerator GhostRestoreSuccess(int ghostIndex)
    {
        GuideText.text = $"축하합니다! {ghostIndex + 1}번째 유령의 복원이 무사히 완료되었습니다! \n";
        ghost.stop();
        isRestoredGhosts[ghostIndex] = true;
        isWaiting = true;
        yield return new WaitForSeconds(2);

        isWaiting = false;
        ResetForNextRound();
    }

    void StartGhostRestoring(int ghostIndex)
    {
        if (currentIndex == 0)
        {
            PitchSlider.gameObject.SetActive(true);
            FlangerSlider.gameObject.SetActive(false);
        }
        else if (currentIndex == 1)
        {
            PitchSlider.gameObject.SetActive(false);
            FlangerSlider.gameObject.SetActive(true);
        }

        audioHints[ghostIndex].SetActive(true);
        currentGhost = Ghosts[ghostIndex];
        var fmodInstance = FMODUnity.RuntimeManager.CreateInstance($"event:/3D/Ghost{currentIndex + 1}");
        ghost = currentGhost.GetComponent<Ghost>();
        ghost.Init(fmodInstance);
        currentGhost.SetActive(true);
        ghost.disable();
        PitchSlider.value = ghost.pitchScale;
        FlangerSlider.value = ghost.flangerScale;
        GuideText.text = $"화면에서 투명한 유령을 클릭하여 유령이 내는 소리를 들어보세요.\n";
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
        TimerPanel.SetActive(false);
        timeRemaining = defaultTimeRemaining;
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
        Vector3 newSize = new Vector3(ghost.originalScale.x, ghost.changedScale.y, ghost.originalScale.z * sizeRatio);
        ghost.changeScale(newSize);
    }

    public void GameStart()
    {
        currentState = UserState.Playing;
    }

}
