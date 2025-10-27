using System.Collections.Generic;

public class BattleSession
{
    public class CombatantDTO
    {
        public string friendlyId;
        public int level;         
        public bool isPlayerOwned;
        public string instanceId;
    }

    public List<CombatantDTO> playerTeam = new List<CombatantDTO>(); 
    public List<CombatantDTO> enemyTeam = new List<CombatantDTO>();

    public int stageId;
    public string stageName;
}
