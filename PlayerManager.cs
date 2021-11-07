using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public bool AcceptPlayerInput { get; private set; } = true;

    public List<Vector3> Coords { get; private set; }

    public Transform PlayerCoords { get; private set; }

    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this.gameObject);
        } else {
            Instance = this;
        }

        Coords = new List<Vector3>();
        PlayerCoords = transform;
    }

    public void SetPlayerInputInactive(){
        AcceptPlayerInput = false;
    }

    public void SetPlayerInputActive(){
        AcceptPlayerInput = true;
    }

    public void QueueCoord(Vector3 coord){
        Coords.Add(coord);
    }

    public void PopCoord(){
        Coords.RemoveAt(0);
    }

    public Vector3 GetNextCoord()
    {
        return Coords.Count != 0 ? Coords[0] : new Vector3(0,0,0);
    }
}
