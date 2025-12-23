using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public PokemonBase Base
    {
        get
        {
            return _base;
        }
    }
    public int Level
    {
        get
        {
            return level;
        }
    }
    public int HP {get; set;}
    public List<Move> Moves {get; set; }
    public Move CurrentMove {get; set; }
    public Dictionary<Stat,int> Stats {get; private set;}
    public Dictionary<Stat, int> StatBoosts { get; private set; } 
    public Queue<string> StatusChanges {get; private set;} = new Queue<string>();
    public bool HpChanged {get; set; }
    public Condition Status {get; private set; }
    public int StatusTime {get; set; }
    public Condition VolatileStatus {get; set; }
    public int VolatileStatusTime {get; set; }
    public event System.Action OnStatusChanged;
    public void Init()
    {
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
            Moves.Add(new Move(move.Base));

            if (Moves.Count >= 4)
            break;
        }
        CalculateStats();

        HP= MaxHP;

        ResetStatBoosts();
        Status = null;
        VolatileStatus = null;

    }
    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoosts();
    }
    void ResetStatBoosts()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
            {Stat.Accuracy, 0},
            {Stat.Evasion, 0}

        };
    }
    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHP = Mathf.FloorToInt((Base.Maxhp * Level) / 100f) + 10 + Level;
        
    }

    int GetStat(Stat stat)
    {
        int statVal= Stats[stat];

        // TODO : APPly stat boosts
        int boosts = StatBoosts[stat];
        var boostValues = new float[]{1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f};
        if (boosts >= 0)
            statVal = Mathf.FloorToInt(statVal * boostValues[boosts]);
        else
            statVal = Mathf.FloorToInt(statVal / boostValues[-boosts]);
        return statVal;
    }
    public void ApplyBoosts(List<StatBoosts> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp( StatBoosts[stat] + boost, -6,6);

            if (boost> 0)
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            else
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            Debug.Log($"{stat} has been boosted by {boost}. Current boost: {StatBoosts[stat]}");

        }
    }

public int Attack
{
    get { return GetStat(Stat.Attack); }
}

public int Defense
{
    get { return GetStat(Stat.Defense); }
}

public int SpAttack
{
    get { return GetStat(Stat.SpAttack); }
}

public int SpDefense
{
    get { return GetStat(Stat.SpDefense); }
}

public int Speed
{
    get { return GetStat(Stat.Speed); }
}

public int MaxHP
{
    get ; private set;
}

public DamageDetails TakeDamage(Move move, Pokemon attacker)
{
    float critical = 1f;
    if (Random.value * 100f < 6.25f)
        critical = 2f;

    // Type (double type supporté)
    float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) 
               * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

    var damageDetails = new DamageDetails()
    {
        TypeEffectiveness = type,
        Critical = critical,
        Fainted = false
    };

    // Si c'est un move de statut → aucun dégât direct
    if (move.Base.Category == MoveCategory.Status)
    {
        // Ici, plus tard : appliquer l'effet (buff, debuff, statut, etc.)
        return damageDetails;
    }

    // Choix des stats selon la catégorie
    int attackStat;
    int defenseStat;

    if (move.Base.Category == MoveCategory.Physical)
    {
        attackStat = attacker.Attack;
        defenseStat = Defense;
    }
    else // MoveCategory.Special
    {
        attackStat = attacker.SpAttack;
        defenseStat = SpDefense;
    }

    float modifiers = Random.Range(0.85f, 1f) * type * critical;
    float a = (2 * attacker.Level + 10) / 250f;
    float d = a * move.Base.Power * ((float)attackStat / defenseStat) + 2;
    int damage = Mathf.FloorToInt(d * modifiers);

    UpdateHP(damage);

    return damageDetails;
}
public void OnAfterTurn()
    {
        Status?.OnAFterTurn?.Invoke(this);
        VolatileStatus?.OnAFterTurn?.Invoke(this);
    }
public void SetStatus(ConditionID conditionId)
    {
        if (Status != null) return;

        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();

    }
public void SetVolatileStatus(ConditionID conditionId)
    {
        if (VolatileStatus != null) return;

        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");

    }

public Move GetRandomMove()
    {
        var moveWithPP = Moves.Where(m => m.PP > 0).ToList();

        int r = Random.Range(0,moveWithPP.Count);
        return moveWithPP[r];
    }
public void UpdateHP(int damage)
{
    HP= Mathf.Clamp(HP - damage, 0, MaxHP);
    HpChanged = true;
}
public bool OnBeforeMove()
{
    bool canPerforMove = true;
    if (Status?.OnBeforeMove != null)
    {
        if(Status.OnBeforeMove(this))
            canPerforMove = false;
    }
        if (VolatileStatus?.OnBeforeMove != null)
    {
        if(VolatileStatus.OnBeforeMove(this))
            canPerforMove = false;
    }
    return canPerforMove;
}
public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }
public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

public void Heal()
{
    // Restore HP
    HP = MaxHP;

    // Clear status
    Status = null;
    StatusTime = 0;
    VolatileStatus = null;

    // Clear stat boosts
    ResetStatBoosts();

    // Restore PP on all moves
    foreach (var move in Moves)
    {
        move.RestoreAllPP();
    }
}



//END OF CLASS
}


public class DamageDetails
{
    public bool Fainted {get; set;}
    public float Critical {get; set;}
    public float TypeEffectiveness {get; set;}
}
