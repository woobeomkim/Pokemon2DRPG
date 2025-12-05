using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Character : MonoBehaviour
{
    [SerializeField] float moveSpeed;

    CharacterAnimator animator;

    public float OffsetY { get; private set; } = 0.3f;
    public bool IsMoving { get; set; }

    public CharacterAnimator Animator => animator;

    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
        SetPositionAndSnapToTile(transform.position);
    }

    public void SetPositionAndSnapToTile(Vector2 pos)
    {
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.5f + OffsetY;

        transform.position = pos;
    }

    public IEnumerator Move(Vector2 moveVec, Action onMoveOver = null)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1, 1);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1, 1);

        Vector3 targetPos = transform.position;

        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        var ledge = ChechForLedge(targetPos);

        if (ledge != null)
        {
            if (ledge.TryToJump(this, moveVec))
            {
                transform.position = targetPos;
                yield break;
            }
        }

        if (!IsPathClear(targetPos))
            yield break;

        if (animator.IsSurfing && Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.i.WaterLayer) == null)
        {
            animator.IsSurfing = false;
            animator.IsJumping = true;
            yield return transform.DOJump(targetPos, 0.35f, 1, 0.5f).WaitForCompletion();
            animator.IsJumping = false;
            transform.position = targetPos;
            //yield break;
        }
        IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        IsMoving = false;

        onMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized;

        var collisionLayer = GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer | GameLayers.i.PlayerLayer;

        if (!animator.IsSurfing)
            collisionLayer |= GameLayers.i.WaterLayer;

        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, collisionLayer) == true)
            return false;
        return true;
    }

    bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer) != null)
            return false;

        return true;
    }

    Ledge ChechForLedge(Vector3 targetPos)
    {
        var collider = Physics2D.OverlapCircle(targetPos - new Vector3(0, OffsetY), 0.15f, GameLayers.i.LegesLayer);
       return collider?.GetComponent<Ledge>();
    }


    public void LookTowards(Vector3 targetPos)
    {
        var xdiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

        if(xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1, 1);
            animator.MoveY = Mathf.Clamp(ydiff, -1, 1);
        }
        else
        {
            Debug.LogError("Diagonal error");

        }
    }
}
