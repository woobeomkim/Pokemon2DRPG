using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, Interactable,ISavable
{
    [SerializeField] ItemBase item;

    public bool Used { get; set; } = false;

    public IEnumerator Interact(Transform initiator)
    {
        if (!Used)
        {
            initiator.GetComponent<Inventory>().AddItem(item);
            Used = true;

            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;

            string playerName = initiator.GetComponent<PlayerController>().Name;
            AudioManager.i.PlaySfx(AudioID.ItemObtain, pauseMusic: true);

            yield return DialogManager.i.ShowDialogText($"{playerName}(이)가 {item.Name}을 발견했다!");
        }
    }

    public object CaptureState()
    {
        return Used;
    }

    public void RestoreState(object state)
    {
        Used = (bool)state;

        if(Used)
        {

        }
    }
}
