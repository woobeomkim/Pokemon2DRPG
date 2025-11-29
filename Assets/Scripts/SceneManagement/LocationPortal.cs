using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Addtive Scene에서 쓰는 Portal
public class LocationPortal : MonoBehaviour,IPlayerTriggerable
{
    [SerializeField] DestinationIdentifer detinationPortal;
    [SerializeField] Transform spawnPoint;

    public Transform SpawnPoint => spawnPoint;

    public bool TriggerRepeatedly => false;

    PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        this.player = player;
        player.Character.Animator.IsMoving = false;
        Debug.Log("Player Triggerable");
        StartCoroutine(Teleport());
    }

    Fader fader;
    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator Teleport()
    {
        GameController.i.PausedGame(true);
        yield return fader.FadeIn();

        var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.detinationPortal == this.detinationPortal);
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);

        yield return fader.FadeOut();
        GameController.i.PausedGame(false);
    }
}
