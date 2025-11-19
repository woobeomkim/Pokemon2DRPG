using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;

    public event Action onEncounter;

    Vector2 input;

    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        if(!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0) input.y = 0;

            if(input != Vector2.zero)
            {
                StartCoroutine(character.Move(input,CheckForEncounter));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
            Interact();
    }

    void Interact()
    {
        var faceDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = faceDir + transform.position;

        //Debug.DrawLine(transform.position, interactPos, Color.red, 0.5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.2f, GameLayers.i.InteractableLayer);
        if(collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact();
        }
    }

    void CheckForEncounter()
    {
        if(Physics2D.OverlapCircle(transform.position,0.2f,GameLayers.i.GrassLayer) != null)
        {
            if(UnityEngine.Random.Range(1,101) <= 10)
            {
                //Debug.Log("배틀시작!");
                character.Animator.IsMoving = false;
                onEncounter?.Invoke();
            }
        }
    }
}
