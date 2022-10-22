using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPlayer : MonoBehaviour
{
    private FMODUnity.StudioEventEmitter studioEventEmitter;
    private Button button;
    private bool pressed = false;
    private bool isPlaying = false;
    private Game2Manager game2Manager;

    private void Awake()
    {
        studioEventEmitter = GetComponent<FMODUnity.StudioEventEmitter>();
        button = GetComponent<Button>();
        game2Manager = GameObject.Find("GameManager").GetComponent<Game2Manager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        isPlaying = studioEventEmitter.IsPlaying();
        button.interactable = !isPlaying;
        if (pressed && !isPlaying)
        {
            game2Manager.startPlaying();
            pressed = false;
        }
    }

    public void play()
    {
        isPlaying = true;
        pressed = true;
        game2Manager.stopPlaying();
        studioEventEmitter.Play();

    }

    public void stop()
    {
        isPlaying = false;
        studioEventEmitter.Stop();
    }
}
