using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eSkillType
{
    Shield,
    SpecialAttack,
    DoubleDamage,
    MakeGold
}

public class Skill
{
    public GameObject SkillUI;
    public GameObject SkillReadyUI;
    public GameObject SkillActiveUI;

    public List<int> LevelRequirements;
    public int Cost;
    public int Uses;
    System.Action OnUse;

    bool UsesNeeded;

    public bool Unlocked;
    public bool Active;
    public bool Ready;

    public eSkillType SkillType;
    public string SkillName;

    public Skill(eSkillType skillType, string skillName, List<GameObject> ui, int cost, List<int> levelRequirement, System.Action onUse, bool usesNeeded = false)
    {
        SkillType = skillType;
        SkillName = skillName;

        if (ui != null && ui.Count > 0)
        {
            SkillUI = ui[0];
            if (ui.Count > 1)
            {
                SkillReadyUI = ui[1];
                if (ui.Count > 2)
                {
                    SkillActiveUI = ui[2];
                }
            }
        }

        Cost = cost;
        LevelRequirements = levelRequirement;
        OnUse = onUse;

        UsesNeeded = usesNeeded;
    }

    public void UpdateStates(int maxStage, int energy)
    {
        Unlocked = LevelRequirements == null || LevelRequirements.Count == 0 || maxStage > LevelRequirements[0];

        SkillUI?.SetActive(Unlocked);

        Active = Unlocked && Uses > 0;

        SkillActiveUI?.SetActive(Active);

        Ready = Unlocked && energy >= Cost;
        if (UsesNeeded && Uses <= 0)
        {
            Ready = false;
        }

        SkillReadyUI?.SetActive(Ready);
    }

    public int GetLevel(int maxStage)
    {
        int level = 0;

        if (LevelRequirements != null)
        {
            foreach (var l in LevelRequirements)
            {
                if (l >= maxStage)
                {
                    return level;
                }

                level++;
            }
        }

        return level;
    }

    public int StageForNextLevel(int currentLevel)
    {
        if (currentLevel >= LevelRequirements.Count)
        {
            return -1;
        }
        else
        {
            return LevelRequirements[currentLevel];
        }
    }

    public void Use()
    {
        OnUse?.Invoke();
    }
}
