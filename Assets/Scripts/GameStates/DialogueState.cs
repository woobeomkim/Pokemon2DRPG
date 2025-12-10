using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class DialogueState : State<GameController>
{
    public static DialogueState i { get; private set; }

    private void Awake()
    {
        i = this;
    }
}
