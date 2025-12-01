using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{
    public QuestBase Base { get; private set; }
    public QuestStatus Status { get; private set; }

    public Quest(QuestBase pBase)
    {
        Base = pBase;
    }

    public IEnumerator StartQuest()
    {
        Status = QuestStatus.Started;

        yield return DialogManager.i.ShowDialog(Base.StartDialogue);
    }

    public IEnumerator CompleteQuest(Transform player)
    {
        Status = QuestStatus.Complted;

        yield return DialogManager.i.ShowDialog(Base.CompletedDialogue);

        var inventory = Inventory.GetInventory();
        if(Base.RequiredItem != null)
        {
            inventory.RemoveItem(Base.RequiredItem);
        }
        
        if(Base.RewardItem != null)
        {
            inventory.AddItem(Base.RewardItem);

            string playerName = player.GetComponent<PlayerController>().Name;
            yield return DialogManager.i.ShowDialogText($"{playerName}(이)가 {Base.RewardItem.Name}을 받았다!");
        }
    }

    public bool CanBeCompleted()
    {
        var inventory = Inventory.GetInventory();
        if(Base.RequiredItem != null)
        {
            return inventory.HasItem(Base.RequiredItem);
        }

        return false;
    }
}

public enum QuestStatus { None,Started,Complted}
