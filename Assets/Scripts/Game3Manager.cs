using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game3Manager : MonoBehaviour
{
    public List<GameObject> ghostPrefabs;
    public TMPro.TextMeshProUGUI GuideText;
    public GameObject CongratsScreen, GameScreen, SliderPanel;
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
    private bool isPlaying = false;

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
        // Touch processing
        if (Input.touchCount > 0 && currentState == UserState.Playing)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GuideText.text = $"Touched {hit.transform.name}\n";
                currentGhost = hit.transform.gameObject;
                ghost = currentGhost.GetComponent<Ghost>();
                PitchSlider.value = ghost.pitchScale;
                FlangerSlider.value = ghost.flangerScale;

                // make it unvisible!
                foreach (GameObject ghostObject in ghostObjects)
                {
                    if (ghostObject.name == currentGhost.name)
                    {
                        ghostObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                    }
                    else
                    {
                        ghostObject.transform.localScale = new Vector3(1f, 1f, 1f);
                        Renderer[] renderers = hit.transform.GetComponentsInChildren<Renderer>();
                        foreach (Renderer renderer in renderers)
                        {
                            foreach (Material material in renderer.materials)
                            {
                                Color color = material.color;
                                material.color = new Color(color.r, color.g, color.b, 0.3f);
                            }
                        }
                    }
                }
                // selection 표시
                //currentGhost.transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                //GuideText.text = $"Touched outside objects\n";
                //ResetSelection();
            }
        }
    }

    void ResetSelection()
    {
        foreach (GameObject ghostObject in ghostObjects)
        {
            ghostObject.transform.localScale = new Vector3(1f, 1f, 1f);
            Renderer[] renderers = ghostObject.transform.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    Color color = material.color;
                    material.color = new Color(color.r, color.g, color.b, 1f);
                }
            }
        }
        currentGhost = null;
        ghost = null;
    }

    public void MoveOnToGameScreen()
    {
        CongratsScreen.SetActive(false);
        GameScreen.SetActive(true);
    }

    public void PositioningCompleted()
    {
        GuideText.text = $"두 손가락으로 유령들의 방향을 회전시킬 수 있습니다. \n";
        for (int i = 0; i < ghostObjects.Count; i++)
        {
            GameObject ghostObject = ghostObjects[i];
            GuideText.text += $"ghostObject.name: {ghostObject.name}\n";
            var fmodInstance = FMODUnity.RuntimeManager.CreateInstance($"event:/Choir/Ghost{i + 1}");
            GuideText.text += $"fmodInstance: {fmodInstance}\n";
            ghostObject.GetComponent<Ghost>().Init(fmodInstance);
        }
        foreach (Ghost ghost in ghosts)
        {
            ghost.play();
        }
        SliderPanel.SetActive(true);
        isPlaying = true;
        currentState = UserState.Playing;
    }

    public void onPitchScaleChange()
    {
        float pitchValue = PitchSlider.value;
        GuideText.text = $"new pitchValue: {pitchValue}, original scale: {ghost.originalScale}\n";
        float sizeRatio = (pitchValue * 1 / 6) + 0.5f;
        ghost.pitchScale = pitchValue;
        Vector3 newSize = new Vector3(ghost.originalScale.x, ghost.originalScale.y * sizeRatio, ghost.transform.localScale.z);
        currentGhost.transform.localScale = newSize;
    }

    public void onFlangerScaleChange()
    {
        float flangerValue = FlangerSlider.value;
        GuideText.text = $"new flangerValue: {flangerValue}\n";
        float sizeRatio = (flangerValue * 1 / 8) + 0.5f;
        ghost.flangerScale = flangerValue;
        Vector3 newSize = new Vector3(ghost.originalScale.x * sizeRatio, ghost.transform.localScale.y, ghost.originalScale.z);
        currentGhost.transform.localScale = newSize;
    }
}
