using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] DestinationIdentifer detinationPortal;
    [SerializeField] Transform spawnPoint;

    public Transform SpawnPoint => spawnPoint;
    PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        this.player = player;
        Debug.Log("Player Triggerable");
        StartCoroutine(SwitchScene());
    }

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);

        GameController.i.PausedGame(true);
        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.detinationPortal == this.detinationPortal);
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);

        GameController.i.PausedGame(false);

        Destroy(gameObject);
    }
}

public enum DestinationIdentifer
{
    A,B,C,D,E
}