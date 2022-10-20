using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class Game2Manager : MonoBehaviour
{
    public TMPro.TextMeshProUGUI GuideText, RemainingTimeDisplay;
    public GameObject TimerPanel, Ghost1Panel, Ghost2Panel, GameScreen, GameOverScreen, GameSuccessScreen, HintsPanel;
    public GameObject Ghost1, Ghost2;
    public Slider Ghost1PitchSlider, Ghost1FlangerSlider, Ghost2PitchSlider, Ghost2FlangerSlider;
    public enum UserState
    {
        Ready,
        Playing,
        Success,
        GameOver
    }
    public UserState currentState = UserState.Ready;

    // private variables
    private List<bool> isRestoredGhosts = new List<bool>
    {
        false, false
    };
    private List<string> ghostNames = new List<string>
    {
        "Ghost1_Game2", "Ghost2_Game2"
    };
    private int currentIndex = 0;
    private GameObject currentGhost = null;
    private Ghost ghost = null;
    private float timeRemaining = 60.0f;
    private List<GameObject> ghostPanels;
    private List<Slider> PitchSliders, FlangerSliders;
    private List<GameObject> Ghosts;

    private void Awake()
    {
        ghostPanels = new List<GameObject>
        {
            Ghost1Panel, Ghost2Panel
        };
        PitchSliders = new List<Slider>
        {
            Ghost1PitchSlider, Ghost2PitchSlider
        };
        FlangerSliders = new List<Slider>
        {
            Ghost1FlangerSlider, Ghost2FlangerSlider
        };
        Ghosts = new List<GameObject>
        {
            Ghost1, Ghost2
        };
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
                currentGhost.GetComponent<Ghost>().stop();
                currentState = UserState.GameOver;
                TimerPanel.SetActive(false);
                // option
                GuideText.text = "Changed state to GameOver!";
                GameScreen.SetActive(false);
                GameOverScreen.SetActive(true);
            }
        }
        else if (currentState == UserState.Playing)
        {
            // ~~~ Playing ~~~
            timeRemaining -= Time.deltaTime;
            DisplayTime(timeRemaining, RemainingTimeDisplay);

            // Check Mission Clear
            if (isRestoredGhosts.TrueForAll(x => x == true))
            {
                if (ghost.isPlaying)
                {
                    ghost.stop();
                }
                MissionClear();
            }
            // Check Reset
            else if (currentGhost == null && currentIndex < ghostNames.Count)
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
                    GuideText.text = $"슬라이더를 움직여 유령의 모양을 바꿔서 원래 소리대로 맞춰보세요.\n";
                    if (currentGhost == null)
                    {
                        currentGhost = hit.transform.gameObject;
                        var fmodInstance = FMODUnity.RuntimeManager.CreateInstance($"event:/3D/Ghost{currentIndex+1}");
                        ghost = currentGhost.GetComponent<Ghost>();
                        ghost.Init(fmodInstance);
                    }

                    if (!ghost.isPlaying)
                    {
                        ghost.play();
                    }
                    // selection 표시
                    currentGhost.transform.GetChild(1).gameObject.SetActive(true);
                }
            }
        }
    }

    //IEnumerator PlayHint(int ghostIndex)
    //{
    //    ButtonPlayer buttonPlayer = HintsPanel.transform.Find($"AudioHint{currentIndex + 1}").transform.GetComponent<ButtonPlayer>();
    //    buttonPlayer.play();
    //    yield return new WaitForSeconds(3);

    //    currentState = UserState.Playing;
    //    TimerPanel.SetActive(true);
    //    StartGhostRestoring(ghostIndex);
    //}

    void StartGhostRestoring(int ghostIndex)
    {
        ghostPanels[ghostIndex].SetActive(true);
        Ghosts[ghostIndex].SetActive(true);
        GuideText.text = $"{ghostIndex + 1}번째 유령을 탭하여 소리를 들어보세요.\n";
    }

    void DisplayTime(float timeToDisplay, TMPro.TextMeshProUGUI displayText)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        displayText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    

    void ResetForNextRound()
    {
        ghostPanels[currentIndex].SetActive(false);
        currentGhost.transform.GetChild(1).gameObject.SetActive(false);
        currentGhost = null;
        currentIndex++;
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
            currentGhost.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    public void stopPlaying()
    {
        if (currentGhost != null && ghost.isPlaying == true)
        {
            ghost.stop();
            currentGhost.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    public void onPitchScaleChange(int ghostIndex)
    {
        float pitchValue = PitchSliders[ghostIndex].value;
        float sizeRatio = (pitchValue * 1 / 6) + 0.5f;
        ghost.pitchScale = pitchValue;
        Vector3 newSize = new Vector3(ghost.originalScale.x, ghost.originalScale.y * sizeRatio, ghost.changedScale.z);
        ghost.changeScale(newSize);
    }

    public void onFlangerScaleChange(int ghostIndex)
    {
        float flangerValue = FlangerSliders[ghostIndex].value;
        float sizeRatio = (flangerValue * 1 / 8) + 0.5f;
        ghost.flangerScale = flangerValue;
        Vector3 newSize = new Vector3(ghost.originalScale.x, ghost.changedScale.y, ghost.originalScale.z * sizeRatio);
        ghost.changeScale(newSize);
    }

    public void GameStart()
    {
        TimerPanel.SetActive(true);
        currentState = UserState.Playing;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextGame()
    {
        SceneManager.LoadScene("Game 3");
    }
}
