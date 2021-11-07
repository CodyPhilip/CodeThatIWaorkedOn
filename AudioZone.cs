using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioZone : MonoBehaviour
{
    private bool _puzzleComplete;

    private AudioZone()
    {
        _puzzleComplete = false;
    }

    public void SetPuzzleStatus(bool status)
    {
        _puzzleComplete = status;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        //Debug.Log("entered into a zone");
        AudioManager.Instance.SwitchBackgroundTrack(_puzzleComplete
            ? AudioManager.ESound.BackgroundTrackLively
            : AudioManager.ESound.BackgroundTrackSlowSomber);
    }
}
