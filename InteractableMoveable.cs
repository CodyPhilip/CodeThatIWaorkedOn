using System;
using JetBrains.Annotations;
using Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class InteractableMoveable : Interactable
{
    public bool Held { get; private set; }
    
    [CanBeNull] private Rigidbody _rb;

    public override void Interact()
    {
        Debug.Log("MOVE");
        
        if (Held)
        {
            Drop();
            return;
        }
        
        Held = true;
        if (_rb) _rb.constraints = RigidbodyConstraints.FreezeAll;
        this.enabled = true;
        transform.parent = PlayerManager.Instance.PlayerCoords;
    }

    public void Drop()
    {
        if (!Held) return;

        if (_rb)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        Held = false;
        transform.parent = null;
        if (_rb) _rb.constraints = RigidbodyConstraints.None;
        this.enabled = false;
    }

    private void Start()
    {
        Held = false;
        this.enabled = false;
        _rb = GetComponent<Rigidbody>();
    }
}