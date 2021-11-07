using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractZone : MonoBehaviour
{
    public float radius = 2f;

    private void OnDrawGizmosSelected()
	{
		Gizmos.color =  Color.yellow;
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}
