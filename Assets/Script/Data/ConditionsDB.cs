using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB : MonoBehaviour
{

    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.ID = conditionId;
        }
    }
    public static Dictionary<ConditionID, Condition> Conditions{get; set;} = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                Description = "Toxified by poison. Lose HP every turn.",
                StartMessage = "has been poisoned.",
                OnAFterTurn = (Pokemon Pokemon)=>
                {
                    Pokemon.UpdateHP(Pokemon.MaxHP / 8);
                    Pokemon.StatusChanges.Enqueue($"{Pokemon.Base.Name} is hurt by poison.");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                Description = "Burned by fire. Lose HP every turn.",
                StartMessage = "has been burned.",
                OnAFterTurn = (Pokemon Pokemon)=>
                {
                    Pokemon.UpdateHP(Pokemon.MaxHP / 16);
                    Pokemon.StatusChanges.Enqueue($"{Pokemon.Base.Name} is hurt by its burn.");
                    
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralysis",
                Description = "Paralyzed. May be unable to move.",
                StartMessage = "has been paralyzed.",
                OnBeforeMove = (Pokemon Pokemon)=>
                {
                    if (Random.Range(1,5) == 1)
                    {
                        Pokemon.StatusChanges.Enqueue($"{Pokemon.Base.Name} is paralyzed and can't move.");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                Description = "Asleep. Unable to move.",
                StartMessage = "has fallen asleep.",
                OnStart = (Pokemon Pokemon)=>
                {
                    // Sleep for 1-3 turns
                    Pokemon.StatusTime = Random.Range(1, 4);
                    Debug.Log($"{Pokemon.Base.Name} will be asleep for {Pokemon.StatusTime} turns.");

                },
                OnBeforeMove = (Pokemon Pokemon)=>
                {
                    if (Pokemon.StatusTime <= 0)
                    {
                        Pokemon.CureStatus();
                        Pokemon.StatusChanges.Enqueue($"{Pokemon.Base.Name} woke up !");
                        return true;
                    }
                    else
                    {
                        Pokemon.StatusTime--;
                        Pokemon.StatusChanges.Enqueue($"{Pokemon.Base.Name} is asleep.");
                        return false;
                    }
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                Description = "Frozen solid. Unable to move.",
                StartMessage = "has been frozen solid.",
                OnBeforeMove = (Pokemon Pokemon)=>
                {
                    if (Random.Range(1,5) == 1)
                    {
                         Pokemon.CureStatus();
                        Pokemon.StatusChanges.Enqueue($"{Pokemon.Base.Name} thawed out.");
                        return true;
                    }
                    return false;
                }
            }
        },
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                Description = "Confused. May hurt itself in its confusion.",
                StartMessage = "became confused.",
                OnStart = (Pokemon Pokemon)=>
                {
                    // Confused for 1-4 turns
                    Pokemon.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"{Pokemon.Base.Name} will be confused for {Pokemon.VolatileStatusTime} turns.");

                },
                OnBeforeMove = (Pokemon Pokemon)=>
                {
                    if (Pokemon.VolatileStatusTime <= 0)
                    {
                        Pokemon.CureVolatileStatus();
                        Pokemon.StatusChanges.Enqueue($"{Pokemon.Base.Name} snapped out of its confusion !");
                        return true;
                    }
                    else
                    {
                        Pokemon.VolatileStatusTime--;
                        // 50% chance to hurt itself
                        if (Random.Range(1,3) == 1)
                        {
                            Pokemon.StatusChanges.Enqueue($"{Pokemon.Base.Name} is confused.");
                            return true;
                        }
                        else
                        {
                            // Hurt itself
                            Pokemon.UpdateHP(Pokemon.MaxHP / 8);
                            Pokemon.StatusChanges.Enqueue($"{Pokemon.Base.Name} hurt itself in its confusion.");
                            return false;
                        }
                    }
                }
            }
        }
            
    };


}

public enum ConditionID
    {
        none,
        psn,
        brn,
        par,
        slp,
        frz,
        confusion,

    }
    

