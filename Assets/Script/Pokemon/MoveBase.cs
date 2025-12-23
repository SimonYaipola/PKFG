using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [Header("Infos de base")]
    [SerializeField] private string moveName;
    [TextArea]
    [SerializeField] private string description;

    [Header("Caractéristiques")]
    [SerializeField] private PokemonType type;
    [SerializeField] private MoveCategory category;

    [Tooltip("Puissance du move. Mettre 0 pour les moves de statut.")]
    [SerializeField] private int power = 0;

    [Tooltip("Chance de toucher en pourcentage (0-100).")]
    [Range(0, 100)]
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHits;
    [SerializeField] int priority;

    [Tooltip("Nombre de PP maximum.")]
    [Min(1)]
    [SerializeField] private int pp = 10;

    [Header("Cible du move")]
    [SerializeField] private MoveEffect effects;
    [SerializeField] MoveTarget Target;

    [SerializeField] List<SecondaryEffect> secondaries;
    public List<SecondaryEffect> Secondaries => secondaries;
    public MoveTarget TargetType => Target;
    public int Priority => priority;

    // === Propriétés publiques (lecture seule) ===
    public string Name => moveName;
    public string Description => description;

    public PokemonType Type => type;
    public bool AlwaysHits => alwaysHits;

    // IMPORTANT: celles-là étaient sorties de la classe dans ton code -> ça cassait tout
    public MoveCategory Category => category;
    public MoveEffect Effects => effects;
 

    public int Power => power;
    public int Accuracy => accuracy;
    public int PP => pp;
}

[System.Serializable]
public class MoveEffect
{
    [SerializeField] List<StatBoosts> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;
    public List<StatBoosts> Boosts => boosts;
    public ConditionID Status { get { return status; } }
    public ConditionID VolatileStatus { get { return volatileStatus; } }
}
[System.Serializable]
public class SecondaryEffect : MoveEffect
{
[SerializeField] int chance;
[SerializeField] MoveTarget target;
public int Chance { get { return chance; } }
public MoveTarget Target { get { return target; } }

}



[System.Serializable]
public class StatBoosts
{
    public Stat stat;
    public int boost;
}


public enum MoveCategory
{
    Physical,
    Special,
    Status
}

public enum MoveTarget
{
    Foe,
    Self
}
