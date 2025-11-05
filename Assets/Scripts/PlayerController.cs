using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public LayerMask solidObjectsLayer;
    
    public float moveSpeed;
    bool isMoving;

    Vector2 input;

    Animator animtor;

    private void Awake()
    {
        animtor = GetComponent<Animator>();
    }

    void Update()
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
    }

    bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer) != null)
            return false;

        return true;
    }
}
