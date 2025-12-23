using System;
using UnityEngine;

public class Condition 
{
    public string Name {get; set;}
    public string Description {get; set; }
    public string StartMessage {get; set; }

    public Action<Pokemon> OnAFterTurn {get; set; }
    public Func<Pokemon, bool> OnBeforeMove {get; set; }
    public Action<Pokemon> OnStart {get; set; }
    public ConditionID ID {get; set; }
}
