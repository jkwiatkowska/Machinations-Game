using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI StageText;
    [SerializeField] TextMeshProUGUI GoldEarnedText;
    [SerializeField] TextMeshProUGUI MaxComboText;
    [SerializeField] TextMeshProUGUI DamageDealtText;
    [SerializeField] TextMeshProUGUI AttacksReturnedText;
    [SerializeField] TextMeshProUGUI PerfectReturnsText;
    [SerializeField] TextMeshProUGUI CreaturesDodgedText;

    [SerializeField] GameObject SkillsUnlockedSection;
    [SerializeField] List<TextMeshProUGUI> SkillsUnlockedText;

    public void Setup(int lastStage, int newStage, int goldEarned, int maxCombo, int damageDealt, int attacksReturned, 
                      int perfectReturns, int creaturesDodged, List<string> skillsUnlockedText)
    {
        if (lastStage < newStage)
        {
            StageText.text = $"{lastStage} <color=\"black\">-></color> {newStage}";
        }
        else
        {
            StageText.text = $"{newStage}";
        }

        GoldEarnedText.text = $"{Utility.FormatNumber(goldEarned)}";
        MaxComboText.text = $"{maxCombo}";
        DamageDealtText.text = $"{Utility.FormatNumber(damageDealt)}";
        AttacksReturnedText.text = $"{attacksReturned}";
        PerfectReturnsText.text = $"{perfectReturns}";
        CreaturesDodgedText.text = $"{creaturesDodged}";

        var showSkillsUnlocked = skillsUnlockedText != null && skillsUnlockedText.Count > 0;
        SkillsUnlockedSection.SetActive(showSkillsUnlocked);
        for (int i = 0; i < SkillsUnlockedText.Count; i++)
        {
            var show = skillsUnlockedText.Count > i;
            SkillsUnlockedText[i].gameObject.SetActive(show);
            if (show)
            {
                SkillsUnlockedText[i].text = skillsUnlockedText[i];
            }
        }
    }
}
