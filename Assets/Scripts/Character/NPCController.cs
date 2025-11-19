using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    NPCState state;
    Character character;
    int currentPattern = 0;

    float idleTimer;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void Interact()
    {
        if(state == NPCState.Idle)
           StartCoroutine(DialogManager.i.ShowDialog(dialog));
    }

    private void Update()
    {
        if (DialogManager.i.IsShowing) return;

        character.HandleUpdate();
        idleTimer += Time.deltaTime;

        if(idleTimer > timeBetweenPattern)
        {
            idleTimer -= timeBetweenPattern;
            if(movementPattern.Count > 0)
                StartCoroutine(Walk());

        }
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        yield return character.Move(movementPattern[currentPattern]);

        currentPattern = (currentPattern + 1) % movementPattern.Count;
        state = NPCState.Idle;
    }
}

public enum NPCState
{
    Idle,Walking
}