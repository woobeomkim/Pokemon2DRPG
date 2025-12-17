using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour,Interactable,ISavable
{
    [SerializeField] string name;
    [SerializeField] int battleUnitCount = 1;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;

    [SerializeField] AudioClip trainerAppearsMusic;

    bool battleLost = false;

    Character character;

    public string Name => name;
    public Sprite Sprite => sprite;
    public int BattleUnitCount => battleUnitCount;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);
        if (!battleLost)
        {
            AudioManager.i.PlayMusic(trainerAppearsMusic);

            yield return DialogManager.i.ShowDialog(dialog);
            GameController.i.StartTrainerBattle(this);
             
        }
        else
        {
            yield return DialogManager.i.ShowDialog(dialogAfterBattle);
        }
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        GameController.i.StateMachine.Push(CutsceneState.i);
        AudioManager.i.PlayMusic(trainerAppearsMusic);

        exclamation.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.gameObject.SetActive(false);

        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));
        
        yield return character.Move(moveVec);

        yield return DialogManager.i.ShowDialog(dialog);
        
        GameController.i.StateMachine.Pop();
        
        GameController.i.StartTrainerBattle(this);

    }

    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false);
    }
    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0;

        if (dir == FacingDirection.Right)
            angle = 90f;
        else if (dir == FacingDirection.Up)
            angle = 180f;
        else if (dir == FacingDirection.Left)
            angle = 270f;

        fov.transform.eulerAngles = new Vector3(0, 0, angle);
    }

    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state)
    {
        battleLost = (bool)state;

        if (battleLost)
            fov.gameObject.SetActive(false);
    }
}