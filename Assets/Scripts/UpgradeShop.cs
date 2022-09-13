using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeShop : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI GoldText;
    [SerializeField] TextMeshProUGUI GoldSpentText;

    [SerializeField] TextMeshProUGUI AttackLevelText;
    [SerializeField] TextMeshProUGUI DamageText;
    [SerializeField] TextMeshProUGUI AttackLevelCostText;
    [SerializeField] Button UpgradeAttackButton;

    [SerializeField] TextMeshProUGUI ComboBonusLevelText;
    [SerializeField] TextMeshProUGUI ComboBonusText;
    [SerializeField] TextMeshProUGUI ComboBonusCostText;
    [SerializeField] Button UpgradeComboButton;

    [SerializeField] List<TextMeshProUGUI> SkillTitleText;
    [SerializeField] List<TextMeshProUGUI> SkillDescText;
    [SerializeField] List<TextMeshProUGUI> SkillUnlockText;
    public void SetText(int gold, int goldSpent, int attackLevel, int comboLevel, int stage, List<Skill> skills)
    {
        // Gold
        GoldText.text = $"{Utility.FormatNumber(gold)}";
        GoldSpentText.text = $"{Utility.FormatNumber(goldSpent)}";

        // Shop
        AttackLevelText.text = $"Attack Level {attackLevel}";
        ComboBonusLevelText.text = $"Combo Bonus Level {comboLevel}";

        DamageText.text = $"Damage\n{Game.PlayerDamageForLevel(attackLevel)} -> {Game.PlayerDamageForLevel(attackLevel + 1)}";
        ComboBonusText.text = $"Bonus gold per combo point\n{Utility.FormatNumberSmall(Game.ComboBonusForLevel(comboLevel))} -> {Utility.FormatNumberSmall(Game.ComboBonusForLevel(comboLevel + 1))}";

        var upgradeAttackCost = Game.UpgradeAttackCost(attackLevel);
        AttackLevelCostText.text = $"{Utility.FormatNumber(upgradeAttackCost)}g";
        UpgradeAttackButton.interactable = upgradeAttackCost <= gold;

        var upgradeComboCost = Game.UpgradeComboBonusCost(comboLevel);
        ComboBonusCostText.text = $"{Utility.FormatNumber(upgradeComboCost)}g";
        UpgradeComboButton.interactable = upgradeComboCost <= gold;

        // Skills
        for (int i = 0; i < skills.Count; i++)
        {
            var skill = skills[i];

            var skillLevel = skill.GetLevel(stage);
            SkillTitleText[i].text = $"{skill.SkillName} Lv. {skillLevel}";
            string unlockText;
            int nextLevel = skill.StageForNextLevel(skillLevel);
            if (skillLevel == 0)
            {
                SkillDescText[i].text = $"Skill locked";
                unlockText = "Unlocks at: ";
            }
            else
            {
                switch (skill.SkillType)
                {
                    case eSkillType.Shield:
                    {
                        SkillDescText[i].text = $"Max shields: {skillLevel}";
                        break;
                    }
                    case eSkillType.SpecialAttack:
                    {
                        SkillDescText[i].text = $"Damage dealt: {Game.SpecialAttackDamage(attackLevel, skillLevel)}";
                        break;
                    }
                    case eSkillType.DoubleDamage:
                    {
                        SkillDescText[i].text = $"Hits doubled: {1 + skillLevel}";
                        break;
                    }
                    case eSkillType.MakeGold:
                    {
                        SkillDescText[i].text = $"Gold produced: {Game.GoldMakerGold(skillLevel, goldSpent)}";
                        break;
                    }
                }

                unlockText = nextLevel >= 0 ? "Next level: " : "Max level reached";
            }

            if (nextLevel >= 0)
            {
                unlockText += $"Stage {nextLevel}";
            }

            SkillUnlockText[i].text = unlockText;
        }
    }
}
