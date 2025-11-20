using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    public event Action onEncounter;
    public event Action<Collider2D> onEnterTrainersView;

    Vector2 input;

    Character character;

    public string Name => name;
    public Sprite Sprite => sprite;

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
                StartCoroutine(character.Move(input,OnMoveOver));
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
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }
    
    void OnMoveOver()
    {
        CheckForEncounter();
        CheckForInTrainersView();
    }

    void CheckForEncounter()
    {
        if(Physics2D.OverlapCircle(transform.position,0.2f,GameLayers.i.GrassLayer) != null)
        {
            if (UnityEngine.Random.Range(1,101) <= 10)
            {
                //Debug.Log("배틀시작!");
                character.Animator.IsMoving = false;
                onEncounter?.Invoke();
            }
        }
    }

    void CheckForInTrainersView()
    {
        var collider = Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.FovLayer);
        if (collider != null) 
        {
            Debug.Log("In Trainers view");
            character.Animator.IsMoving = false;
            onEnterTrainersView?.Invoke(collider);
        }
    }
}
