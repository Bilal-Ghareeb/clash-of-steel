using UnityEngine;


public class Combatant
{
    public string Id;
    public string DisplayName;
    public WeaponInstanceBase InstanceData;
    public int CurrentHP;

    public int Level => InstanceData.Level;
    public string ClassType => InstanceData.CatalogData.@class;
    public WeaponTimelineSet Timelines => InstanceData?.Asset?.Timelines;


    public int AttackPoints { get; private set; } = 0;
    public int DefendPoints { get; private set; } = 0;
    public int BankedPoints { get; private set; } = 0;

    public Transform ModelRoot { get; set; }

    public Animator CombatantAnimator { get; set; }

    public bool HasSwitchedThisTurn { get; private set; }
    public bool CanSwitchWeapon => !HasSwitchedThisTurn && BankedPoints > 0;

    public int MaxHP => InstanceData.GetHealth();
    public int BaseAttack => InstanceData.GetDamage();

    public bool IsAlive => CurrentHP > 0;

    public Combatant(WeaponInstanceBase instance, string id = null, string displayName = null)
    {
        InstanceData = instance;
        Id = id;
        DisplayName = string.IsNullOrEmpty(displayName) ? instance.CatalogData.GetLocalizedName() : displayName;
        CurrentHP = MaxHP;
    }

    public void StartTurnWithAvailablePoints(int incomingPoints, int bankCap)
    {
        BankedPoints = Mathf.Min(BankedPoints + incomingPoints, bankCap);
    }

    public bool TryAllocate(int attack, int defend, int reserve, out string error)
    {
        error = null;
        if (attack < 0 || defend < 0 || reserve < 0)
        {
            error = "Invalid negative allocation.";
            return false;
        }

        int total = attack + defend + reserve;
        if (total > BankedPoints)
        {
            error = $"Not enough points: requested {total} but have {BankedPoints}.";
            return false;
        }

        AttackPoints = attack;
        DefendPoints = defend;
        BankedPoints = Mathf.Min(BankedPoints - total + reserve, 8);
        return true;
    }

    public void Switch()
    {
        HasSwitchedThisTurn = true;
    }

    public void ResetRoundAllocations()
    {
        AttackPoints = 0;
        DefendPoints = 0;
        HasSwitchedThisTurn = false;
    }
}
