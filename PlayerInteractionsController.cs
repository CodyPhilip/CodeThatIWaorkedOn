using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionsController : MonoBehaviour
{
	public CharacterController controller;
	public Transform player;
	public float interactDist = 2f; 

    public void HandleInteraction(InputAction.CallbackContext context)
    {
	    // Only on first press
	    if (!context.started) return;
	    
	    Collider[] hitColliders = Physics.OverlapSphere(player.position, interactDist);
	    List<Interactable> inters = new List<Interactable>();

	    Interactable nearest = null;
	    float nearDist = float.PositiveInfinity;

	    foreach (Collider obj in hitColliders){
		    if (obj.GetComponent<Interactable>() != null){
			    inters.Add(obj.GetComponent<Interactable>());
		    }
	    }

	    foreach (Interactable inter in inters){
		    Vector3 offset = player.transform.position - inter.transform.position;
		    float thisDist = offset.sqrMagnitude;
		    if (thisDist < nearDist){
			    nearDist = thisDist;
			    nearest = inter;
		    }
	    }

	    if (nearest != null){
		    // interact!
		    //Interactable inter = nearest as Interactable;
		    nearest.Interact();
	    }
    }
}
