using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public float pitchScale;  // 1 ~ 7 사이
    public float flangerScale;  // 1 ~ 5 사이
    public Vector3 originalScale;
    public bool isPlaying = false;
    public bool initialized = false;
    private FMOD.Studio.EventInstance instance;

    public void Init(FMOD.Studio.EventInstance fmodInstance)
    {
        instance = fmodInstance;
        initialized = true;
    }

    private void Awake()
    {
        originalScale = transform.localScale;
        //studioEventEmitter = transform.GetComponent<FMODUnity.StudioEventEmitter>();
        //instance = studioEventEmitter.EventInstance;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(this.gameObject));
        instance.setParameterByName("PitchShifter_UpDown", pitchScale);
        instance.setParameterByName("Flanger_LeftRight", flangerScale);

        FMOD.Studio.PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
    }

    public void play()
    {
        instance.start();
        isPlaying = true;
    }

    public void stop()
    {
        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        isPlaying = false;
    }
}
