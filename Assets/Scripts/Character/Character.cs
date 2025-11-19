using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Character : MonoBehaviour
{
    [SerializeField] float moveSpeed;

    CharacterAnimator animator;

    public bool IsMoving { get; set; }

    public CharacterAnimator Animator => animator;

    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
    }

    public IEnumerator Move(Vector2 moveVec, Action onMoveOver = null)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1, 1);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1, 1);

        Vector3 targetPos = transform.position;

        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if (!IsWalkable(targetPos))
            yield break;
        IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        IsMoving = false;

        onMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer) != null)
            return false;

        return true;
    }
}
