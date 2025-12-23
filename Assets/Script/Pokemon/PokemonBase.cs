using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[CreateAssetMenu(fileName =  "Pokemon", menuName =  "Pokemon/Create new pokemon")]

public class PokemonBase : ScriptableObject
{
    [SerializeField] String name;
    [TextArea]
    [SerializeField] String description;
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType type1;
     [SerializeField] PokemonType type2;

     //Base Stat
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;
    // Cri
    [SerializeField] AudioClip cry;

    [SerializeField] List<LearnableMove> learnableMoves;

    public string Name
    {
        get {return name;}
    }
    public string Description
    {
        get {return description;}
    }
    public PokemonType Type1
    {
        get {return type1;}
    }
    public PokemonType Type2
    {
        get {return type2;}
    }
    public int Maxhp
    {
        get {return maxHp;}

    }
        public int Attack
    {
        get {return attack;}
        
    }
        public int Defense
    {
        get {return defense;}
        
    }
        public int SpAttack
    {
        get {return spAttack;}
        
    }
        public int SpDefense
    {
        get {return spDefense;}
        
    }
        public int Speed
    {
        get {return speed;}
        
    }
    public Sprite FrontSprite
    {
        get{return frontSprite;}
    }
    public Sprite BackSprite
    {
        get{return backSprite;}
    }
    public AudioClip Cry
    {
        get{return cry;}
    }
    public List<LearnableMove> LearnableMoves
    {
        get {return learnableMoves;}
    }

}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get {return moveBase;}

    }
    public int Level
    {
        get{return level;}
    }

}
public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Dark,
    Steel,
    Fairy
}
public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,
    // Not actual Stat
    Accuracy,
    Evasion
    
}

public class TypeChart
{
    static float[][] chart =
    {
        /*Normal*/  new float[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0.5f, 0,   1, 1, 0.5f, 1},
        /*Fire*/    new float[] {1, 0.5f,0.5f,1, 2, 2, 1, 1, 1, 1, 1, 2, 0.5f,1, 0.5f,1, 2, 1},
        /*Water*/   new float[] {1, 2, 0.5f,1, 0.5f,1, 1, 1, 2, 1, 1, 1, 2, 1, 0.5f,1, 1, 1},
        /*Electric*/new float[] {1, 1, 2, 0.5f,0.5f,1, 1, 1, 0, 2, 1, 1, 1, 1, 0.5f,1, 1, 1},
        /*Grass*/   new float[] {1, 0.5f,2, 1, 0.5f,1, 1, 0.5f,2, 0.5f,1, 0.5f,2, 1, 0.5f,1, 0.5f,1},
        /*Ice*/     new float[] {1, 0.5f,0.5f,1, 2, 0.5f,1, 1, 2, 2, 1, 1, 1, 1, 2, 1, 0.5f,1},

        /*Fighting*/new float[] {2, 1, 1, 1, 1, 2, 1, 0.5f,1, 0.5f,0.5f,0.5f,2, 0, 1, 2, 2, 0.5f},
        /*Poison*/  new float[] {1, 1, 1, 1, 2, 1, 1, 0.5f,0.5f,1, 1, 1, 0.5f,0.5f,1, 1, 0, 2},
        /*Ground*/  new float[] {1, 2, 1, 2, 0.5f,1, 1, 2, 1, 0, 1, 0.5f,2, 1, 1, 1, 2, 1},
        /*Flying*/  new float[] {1, 1, 1, 0.5f,2, 1, 2, 1, 1, 1, 1, 2, 0.5f,1, 1, 1, 0.5f,1},
        /*Psychic*/ new float[] {1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 0.5f,1, 1, 1, 1, 0, 0.5f,1},
        /*Bug*/     new float[] {1, 0.5f,1, 1, 2, 1, 0.5f,0.5f,1, 0.5f,2, 1, 1, 0.5f,1, 2, 0.5f,0.5f},
        /*Rock*/    new float[] {1, 2, 1, 1, 1, 2, 0.5f,1, 0.5f,2, 1, 2, 1, 1, 1, 1, 0.5f,1},
        /*Ghost*/   new float[] {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 2, 1, 0.5f,1, 1},
        /*Dragon*/  new float[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 0.5f,0},
        /*Dark*/    new float[] {1, 1, 1, 1, 1, 1, 0.5f,1, 1, 1, 2, 1, 1, 2, 1, 0.5f,1, 0.5f},
        /*Steel*/   new float[] {1, 0.5f,0.5f,0.5f,1, 2, 1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 0.5f,2},
        /*Fairy*/   new float[] {1, 1, 1, 1, 1, 1, 2, 0.5f,1, 1, 1, 1, 1, 1, 2, 1, 0.5f,1}
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1;

        int row = (int)attackType -1;
        int col = (int)defenseType -1;
        return chart[row][col];
    }
}


