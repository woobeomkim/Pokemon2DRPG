using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;

    public Pokemon Pokemon { get; set; }

    // test¿ë
    public PokemonBase pBase;
    Image image;

    private void Awake()
    {
        image = GetComponent<Image>();   
    }

    public void Setup(Pokemon pokemon)
    {
      // Pokemon = pokemon;

        Pokemon = new Pokemon(pBase, 15);

        if (isPlayerUnit)
            image.sprite = Pokemon.Base.BackSprite;
        else
            image.sprite = Pokemon.Base.FrontSprite;
    }
}
