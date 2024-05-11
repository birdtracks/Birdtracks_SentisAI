using System;
using System.Collections;
using System.Collections.Generic;
using BirdTracks.Game.ONS;
using UnityEngine;

public class VADActions : MonoBehaviour
{
    [SerializeField] private List<GameObject> shapesList = new List<GameObject>();
    [SerializeField] private ONS_VoiceActvityDetection vad;
    
    private int counter = 0;

    void Start()
    {
        
        foreach (var s in shapesList)
        {
            s.SetActive(false);
        }

        vad.OnVoiceDetected += ShowShape;
    }

    private void ShowShape(bool value)
    {
        if (!value || counter >= shapesList.Count) return;
        shapesList[counter].SetActive(true);
        counter++;
    }

    private void OnDisable()
    {
        vad.OnVoiceDetected -= ShowShape;
    }
}
