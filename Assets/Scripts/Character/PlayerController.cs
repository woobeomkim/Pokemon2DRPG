using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour,ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    Vector2 input;

    Character character;

    public string Name => name;
    public Sprite Sprite => sprite;

    public Character Character => character;
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
            StartCoroutine(Interact());
    }

    IEnumerator Interact()
    {
        var faceDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = faceDir + transform.position;

        //Debug.DrawLine(transform.position, interactPos, Color.red, 0.5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.2f, GameLayers.i.InteractableLayer);
        if(collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }
    
    void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.TriggerableLayer);
    
        foreach(var collider in colliders)
        {
            var trigerable = collider.GetComponent<IPlayerTriggerable>();
            if (trigerable != null)
            {
                //character.Animator.IsMoving = false;
                trigerable.OnPlayerTriggered(this);
            }
            break;
        }
    }

    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            pokmons = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetSaveData()).ToList(),

        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;

        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);

        GetComponent<PokemonParty>().Pokemons = saveData.pokmons.Select(p => new Pokemon(p)).ToList();
    }
}

[System.Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<PokemonSaveData> pokmons;
}
