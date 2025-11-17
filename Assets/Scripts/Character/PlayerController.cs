using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public LayerMask solidObjectsLayer;
    public LayerMask longgrassLayer;
    public LayerMask interactableLayer;

    public float moveSpeed;
    bool isMoving;

    public event Action onEncounter;

    Vector2 input;

    Animator animtor;

    private void Awake()
    {
        animtor = GetComponent<Animator>();
    }

    public void HandleUpdate()
    {
        if(!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0) input.y = 0;

            if(input != Vector2.zero)
            {
                animtor.SetFloat("moveX", input.x);
                animtor.SetFloat("moveY", input.y);

                Vector3 targetPos = transform.position;

                targetPos.x += input.x;
                targetPos.y += input.y;

                if(IsWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }

        animtor.SetBool("isMoving", isMoving);

        if (Input.GetKeyDown(KeyCode.Z))
            Interact();
    }

    void Interact()
    {
        var faceDir = new Vector3(animtor.GetFloat("moveX"), animtor.GetFloat("moveY"));
        var interactPos = faceDir + transform.position;

        //Debug.DrawLine(transform.position, interactPos, Color.red, 0.5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.2f, interactableLayer);
        if(collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact();
        }
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        while((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        isMoving = false;

        CheckForEncounter();
    }

    bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer | interactableLayer) != null)
            return false;

        return true;
    }

    void CheckForEncounter()
    {
        if(Physics2D.OverlapCircle(transform.position,0.2f,longgrassLayer) != null)
        {
            if(UnityEngine.Random.Range(1,101) <= 10)
            {
                //Debug.Log("배틀시작!");
                animtor.SetBool("isMoving", false);
                onEncounter?.Invoke();
            }
        }
    }
}
