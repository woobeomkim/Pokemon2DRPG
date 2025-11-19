using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectsLayer;
    [SerializeField] LayerMask grassLayer;
    [SerializeField] LayerMask interactableLayer;

    public static GameLayers i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public LayerMask SolidLayer => solidObjectsLayer;
    public LayerMask InteractableLayer => interactableLayer;
    public LayerMask GrassLayer => grassLayer;
}
