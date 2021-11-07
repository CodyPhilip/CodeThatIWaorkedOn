using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteracableClimbZone : Interactable
{

    public Transform endpoint;

    public override void Interact(){
        //Debug.Log("CLIMB");
        PlayerManager.Instance.SetPlayerInputInactive();
        PlayerManager.Instance.QueueCoord(endpoint.position - transform.position);
    }
}
