using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class Game1Manager : MonoBehaviour
{
    // public within editor
    public TMPro.TextMeshProUGUI GuideText, RemainingTimeDisplay;
    public GameObject TimerPanel, GameScreen, GameOverScreen, GameSuccessScreen;
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
    private float timeRemaining = 60.0f;
    private float targetDistance = 0.2f;

    private void Awake()
    {

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
                    GuideText.text = $"{hit.transform.name}을 찾았습니다!\n유물함에 유령을 잡아넣어보세요.\n";
                    if (Input.touches[0].phase == TouchPhase.Began)
                    {
                        currentGhost.GetComponent<Ghost>().stop();
                        currentGhost.GetComponent<Target>().enabled = false;

                        // make it visible!
                        Renderer[] renderers = hit.transform.GetComponentsInChildren<Renderer>();
                        foreach (Renderer renderer in renderers)
                        {
                            foreach (Material material in renderer.materials)
                            {
                                Color color = material.color;
                                material.color = new Color(color.r, color.g, color.b, 1f);
                            }
                        }
                    }
                    else  // Drag and Drop!
                    {
                        currentGhost.transform.position = hit.transform.position;
                        Vector3 originalPosition = GameObject.Find(ghostNames[currentIndex]).transform.position;
                        float distance = Vector3.Distance(currentGhost.transform.position, originalPosition);
                        if (distance <= targetDistance)
                        {
                            currentGhost.transform.position = Vector3.Lerp(hit.transform.position, originalPosition, 0.1f);
                            isFoundGhosts[currentIndex] = true;
                            ResetForNextRound();
                        }
                        else
                        {
                            GuideText.text += $"hit: {hit}, {hit.transform.position} 유령과 유물함의 거리를 {targetDistance}m 이내로 좁혀보세요. (현재 거리: {distance})";
                        }
                    }
                }
            }
        }
    }

    void DisplayTime(float timeToDisplay, TMPro.TextMeshProUGUI displayText)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        displayText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    void StartGhostFinding(int ghostIndex)
    {
        GuideText.text = $"노랫소리를 따라가서 {ghostIndex + 1}번째 유령을 찾아보세요.\n";
        GameObject modelTarget = GameObject.Find("ModelTarget");
        currentGhost = Instantiate(
            GhostPrefabs[ghostIndex],
            modelTarget.transform.position + GhostPositions[ghostIndex],
            Quaternion.Euler(0, 0, 0),
            modelTarget.transform
        );
        Ghost ghost = currentGhost.GetComponent<Ghost>();
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
        if (currentState == UserState.Ready)
        {
            StartCoroutine(waitForHiding());
        }
    }

    IEnumerator waitForHiding()
    {
        currentState = UserState.Hiding;
        GuideText.text = "앗, 유령이 도망친다!";
        yield return new WaitForSeconds(3);

        GameStart();
    }

    public void GameStart()
    {
        GuideText.text = "노랫소리를 따라가서 유령을 찾아보세요.";
        TimerPanel.SetActive(true);
        currentState = UserState.Playing;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextGame()
    {
        SceneManager.LoadScene("Game 2");
    }
}
