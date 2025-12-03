using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;
    [SerializeField] AudioClip sceneMusic;
    public bool IsLoaded  { get; private set; }

    public AudioClip SceneMusic => sceneMusic;
    List<SavableEntity> savableEntities;
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.tag == "Player")
        {
            Debug.Log($"{gameObject.name}");

            LoadScene();
            GameController.i.SetCurrentScene(this);
            // Load all connected scenes

            if(sceneMusic != null)
                AudioManager.i.PlayMusic(sceneMusic,fade:true);

            foreach(var scene in connectedScenes)
            {
                scene.LoadScene();
            }

            var prevScene = GameController.i.PrevScene;
            if (prevScene != null) 
            {
                var previouslyLoadedScenes = prevScene.connectedScenes;
                foreach ( var scene in previouslyLoadedScenes)
                {
                    if(!connectedScenes.Contains(scene) && scene != this)
                    {
                        scene.UnLoadScene();
                    }

                    if(!connectedScenes.Contains(prevScene))
                        prevScene.UnLoadScene();
                }
            }
        }
    }

    public void LoadScene()
    {
        if (!IsLoaded)
        {
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            IsLoaded = true;

            operation.completed += (AsyncOperation op) =>
            {
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };
        }
    }

    public void UnLoadScene()
    {
        if (IsLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);

            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currScene).ToList();
        return savableEntities;
    }
}
