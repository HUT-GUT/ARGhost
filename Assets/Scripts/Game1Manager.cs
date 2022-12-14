using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
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
    private float targetDistance = 0.3f;

    private void Awake()
    {

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
                    GuideText.text = $"?????? {currentIndex + 1}??? ???????????????!\n???????????? ????????? ?????????????????????.\n";
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
                            GuideText.text += $"????????? ???????????? ????????? {targetDistance}m ????????? ???????????????. (?????? ??????: {distance})";
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
        GuideText.text = $"????????? ???????????? ??????????????? ???????????? ????????? {ghostIndex + 1}?????? ????????? ???????????????.\n";
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
        GuideText.text = "???????????????!";
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
        GuideText.text = "'???? ??? ???????????? ???????'";
        yield return new WaitForSeconds(5);

        DisplayFullGuidePanel();
    }

    IEnumerator WaitForPopupClose()
    {
        yield return new WaitForSeconds(4);

        if (PopupPanel.activeSelf)
        {
            PopupPanel.SetActive(false);
            ModelTarget.SetActive(true);
        }
    }

    public void DetectionGuideStart()
    {
        GuideText.text = "'??? ?????? 78??? ??? ?????? ????????? ??????????????? ?'";
        StartCoroutine(WaitForPopupClose());
    }

    public void DisplayFullGuidePanel()
    {
        stopBGM();
        FullScreenGuidePanel.SetActive(true);
    }

    public void GameStart()
    {
        GameScreen.SetActive(true);
        GuideText.text = "??????????????? ???????????? ????????? ???????????????.";
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
