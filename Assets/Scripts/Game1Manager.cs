using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game1Manager : MonoBehaviour
{
    // public within editor
    public TMPro.TextMeshProUGUI GuideText;
    public Slider RemainingTimeSlider;
    public GameObject GameScreen, GameOverScreen, GameSuccessScreen;
    public GameObject TimerPanel, FullScreenGuidePanel, PopupPanel, ModelTarget;
    public List<GameObject> GhostPrefabs;
    public List<Vector3> GhostPositions;

    // public within codes
    public enum UserState
    {
        Ready,
        Hiding,
        Playing,
        Success,
        GameOver
    }
    public UserState currentState = UserState.Ready;
    public float targetDistance = 0.5f;

    // private variables
    private List<bool> isFoundGhosts = new List<bool>
    {
        false, false
    };
    private List<string> ghostNames = new List<string>
    {
        "Ghost1", "Ghost2"
    };
    private int currentIndex = 0;
    private GameObject currentGhost = null;
    private Ghost ghost = null;
    private float timeRemaining = 60.0f;
    private SceneController sceneController;

    private void Awake()
    {
        sceneController = FindObjectOfType<SceneController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        currentState = UserState.Ready;
        DetectionGuideStart();
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
            DisplayTime(timeRemaining, RemainingTimeSlider);

            // Check Mission Clear
            if (isFoundGhosts.TrueForAll(x => x == true))
            {
                MissionClear();
            }
            // Check Reset 
            else if (currentGhost == null && currentIndex < GhostPrefabs.Count)
            {
                StartGhostFinding(currentIndex);
            }
            else if (currentGhost != null && Input.touchCount > 0)
            {
                // Touch processing
                Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit) && hit.transform.gameObject == currentGhost)
                {
                    GuideText.text = $"유령 {currentIndex + 1}을 찾았습니다!\n이제 유물함에 유령을 다시 잡아넣어보세요.\n";
                    if (Input.touches[0].phase == TouchPhase.Began)
                    {
                        ghost.select();
                        ghost.stop();
                        currentGhost.GetComponent<Target>().enabled = false;
                    }
                    else  // Drag and Drop!
                    {
                        Vector3 originalPosition = GameObject.Find(ghostNames[currentIndex]).transform.position;
                        float distance = Vector3.Distance(currentGhost.transform.position, originalPosition);
                        if (distance <= targetDistance)
                        {
                            Destroy(currentGhost);
                            GameObject modelTarget = GameObject.Find("ModelTarget");
                            Instantiate(
                                GhostPrefabs[currentIndex],
                                originalPosition,
                                modelTarget.transform.rotation,
                                modelTarget.transform
                            );
                            isFoundGhosts[currentIndex] = true;
                            ResetForNextRound();
                        }
                        else
                        {
                            GuideText.text += $"유령과 유물함의 거리를 {targetDistance}m 이내로 좁혀보세요. (현재 거리: {distance.ToString("0.00")})";
                        }
                    }
                }
            }
        }
    }

    void DisplayTime(float timeToDisplay, Slider RemainingTimeSlider)
    {
        float seconds = Mathf.Floor(timeToDisplay);
        RemainingTimeSlider.value = seconds;
    }

    void StartGhostFinding(int ghostIndex)
    {
        GuideText.text = $"주위를 둘러보며 노랫소리를 따라가서 도망간 {ghostIndex + 1}번째 유령을 찾아보세요.\n";
        GameObject modelTarget = GameObject.Find("ModelTarget");
        currentGhost = Instantiate(
            GhostPrefabs[ghostIndex],
            modelTarget.transform.position + GhostPositions[ghostIndex],
            modelTarget.transform.rotation,
            modelTarget.transform
        );
        currentGhost.GetComponent<Target>().enabled = true;
        ghost = currentGhost.GetComponent<Ghost>();
        ghost.deselect();
        var fmodInstance = FMODUnity.RuntimeManager.CreateInstance($"event:/3D/{ghostNames[ghostIndex]}");
        ghost.Init(fmodInstance);
        ghost.play();
    }

    void ResetForNextRound()
    {
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

    public void TargetDetected()
    {
        playDetectionSound();
        if (currentState == UserState.Ready)
        {
            StartCoroutine(waitForHiding());
        }
    }

    IEnumerator waitForHiding()
    {
        currentState = UserState.Hiding;
        GuideText.text = "'아니, 이 유령들은 뭐지?'";
        yield return new WaitForSeconds(5);

        DisplayFullGuidePanel();
    }

    IEnumerator WaitForPopupClose()
    {
        yield return new WaitForSeconds(2);

        if (PopupPanel.activeSelf)
        {
            PopupPanel.SetActive(false);
            ModelTarget.SetActive(true);
        }
    }

    public void DetectionGuideStart()
    {
        GuideText.text = "'이 안에 약 80년 전 소리 유물이 들어있다고 ?'";
        StartCoroutine(WaitForPopupClose());
    }

    public void DisplayFullGuidePanel()
    {
        stopBGM();
        sceneController.JumpToScene("Game 2");
        // FullScreenGuidePanel.SetActive(true);
    }

    public void GameStart()
    {
        GameScreen.SetActive(true);
        GuideText.text = "노랫소리를 따라가서 유령을 찾아보세요.";
        TimerPanel.SetActive(true);
        currentState = UserState.Playing;
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
