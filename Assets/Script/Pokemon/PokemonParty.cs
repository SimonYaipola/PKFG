using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Collections;
public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public List<Pokemon> Pokemons { get { return pokemons; } }

    void Start()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }
    public Pokemon GetHealthyPokemon()
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }
    public void HealAll()
    {
    foreach (var pokemon in Pokemons)
    {
        pokemon.Heal();
    }
    }


}
