using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils.GenericSelectionUI;

public class MainMenuController : SelectionUI<TextSlot>
{
    private void Start()
    {
        var textSlots = GetComponentsInChildren<TextSlot>().ToList();

        if (SavingSystem.i.CheckIfSaveExist("saveSlot1"))
        {
            SetItems(textSlots);
        }
        else
        {
            SetItems(textSlots.TakeLast(2).ToList());
            textSlots.First().GetComponent<Text>().color = Color.gray;
        }
        onSelected += OnItemSelected;
    }

    private void Update()
    {
        HandleUpdate();
    }

    void OnItemSelected(int selection)
    {
        if (!SavingSystem.i.CheckIfSaveExist("saveSlot1"))
            ++selection;

        if(selection == 0)
        {
            // Continue
            DontDestroyOnLoad(gameObject);

            GameController.i.StateMachine.ChangeState(FreeRoamState.i);
            SceneManager.LoadScene(1);
            SavingSystem.i.Load("saveSlot1");

            Destroy(gameObject);
        }
        else if(selection == 1)
        {
            // New Game

            GameController.i.StateMachine.ChangeState(FreeRoamState.i);
            SavingSystem.i.Delete("saveSlot1");
            SceneManager.LoadScene(1);
        }
        else if(selection == 2)
        {
            // Credits
        }
    }
}
