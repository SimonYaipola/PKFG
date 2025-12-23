using UnityEngine;

/// <summary>
/// Représente une instance d'un move en combat.
/// Contient une référence au MoveBase (données statiques)
/// et les PP actuels pour ce move.
/// </summary>
[System.Serializable]
public class Move
{
    /// <summary>
    /// Données de base du move (type, power, accuracy, etc.)
    /// </summary>
    public MoveBase Base { get; private set; }

    /// <summary>
    /// PP actuels de ce move (varient pendant le combat).
    /// </summary>
    public int PP { get; private set; }

    /// <summary>
    /// Indique si le move a encore des PP.
    /// </summary>
    public bool HasPP => PP > 0;

    public Move(MoveBase moveBase)
    {
        Base = moveBase;
        PP = moveBase.PP;
    }

    /// <summary>
    /// Tente de consommer 1 PP.
    /// Retourne true si ça a fonctionné, false si le move n'avait plus de PP.
    /// </summary>
    public bool TryConsumePP()
    {
        if (PP <= 0)
        {
            return false;
        }

        PP--;
        return true;
    }

    /// <summary>
    /// Restaure une certaine quantité de PP (ex: Ether, Elixir).
    /// Le résultat est clampé entre 0 et le PP max du move.
    /// </summary>
    public void RestorePP(int amount)
    {
        PP = Mathf.Clamp(PP + amount, 0, Base.PP);
    }

    /// <summary>
    /// Restaure tous les PP (ex: Centre Pokémon, PP Max, etc.)
    /// </summary>
    public void RestoreAllPP()
    {
        PP = Base.PP;
    }
}
