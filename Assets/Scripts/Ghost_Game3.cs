using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost_Game3 : MonoBehaviour
{
    public float pitchScale;  // 0 ~ 1 사이
    public float flangerScale;  // 0 ~ 1 사이
    public Vector3 originalScale;
    public Vector3 changedScale;
    public float volumeSize = 0.5f;
    public bool isPlaying = false;
    public bool initialized = false;
    private FMOD.Studio.EventInstance instance;

    public void Init(FMOD.Studio.EventInstance fmodInstance)
    {
        instance = fmodInstance;
        initialized = true;

        FMOD.Studio.EventDescription eventDescription;
        instance.getDescription(out eventDescription);
    }

    private void Awake()
    {
        originalScale = transform.localScale;
        changedScale = originalScale;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (initialized == true)
        {
            //instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(this.gameObject));
            instance.setParameterByName("C_PitchShifter_UpDown", pitchScale);
            instance.setParameterByName("C_Flanger_LeftRight", flangerScale);
            instance.setParameterByName("Size", volumeSize);
        }
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

    public void increaseVolume()
    {
        volumeSize = 1.0f;
    }

    public void decreaseVolume()
    {
        volumeSize = 0.1f;
    }

    public void changeScale(Vector3 newScale)
    {
        transform.localScale = newScale;
        changedScale = newScale;
    }

    public void select()
    {
        foreach (Transform child in transform.GetChild(0))
        {
            Renderer[] renderers = child.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    Color color = material.color;
                    material.color = new Color(color.r, color.g, color.b, 1f);
                }
            }
        }
        increaseVolume();
    }

    public void deselect()
    {
        foreach (Transform child in transform.GetChild(0))
        {
            Renderer[] renderers = child.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    Color color = material.color;
                    material.color = new Color(color.r, color.g, color.b, 0.1f);
                }
            }
        }
    }

    public void disable()
    {
        foreach (Transform child in transform.GetChild(0))
        {
            Renderer[] renderers = child.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    Color color = material.color;
                    material.color = new Color(color.r, color.g, color.b, 0.3f);
                }
            }
        }
        decreaseVolume();
    }
}
