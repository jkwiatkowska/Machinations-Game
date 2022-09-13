using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    #region SerializeFields
    // Debug
    [SerializeField] bool AutoPlay;
    [SerializeField] float PerfectReturnChance;
    [SerializeField] float TimeScale = 1.0f;

    // Canvases and UI
    [SerializeField] GameObject StartCanvas;
    [SerializeField] ResultScreen ResultScreen;
    [SerializeField] UpgradeShop UpgradeShop;
    [SerializeField] List<GameObject> SetActiveOnStart;
    [SerializeField] PopupTextPool PopupTextPool;
    [SerializeField] Color ColourCorrect;
    [SerializeField] Color ColourWrong;
    [SerializeField] Color ColourSpecial;
    [SerializeField] Image Danger;

    // Player
    [SerializeField] Transform Player;
    [SerializeField] float PlayerRadius;
    [SerializeField] float HitRadius;
    [SerializeField] float PerfectRadius;

    // Monsters
    [SerializeField] Movement Monster;

    [SerializeField] Movement Boss;

    [SerializeField] GameObject MonsterHPDisplay;
    [SerializeField] Image MonsterHPBar;

    [SerializeField] Vector2 MonsterDeathOffset;

    // Attacks
    [SerializeField] List<IncomingObjectPool> AttackPools;
    [SerializeField] List<IncomingObjectPool> NotAttackPools;
    [SerializeField] Transform AttackSpawnPosition;

    // Timing
    [SerializeField] Vector2 TickTime;

    // Skills
    [SerializeField] List<GameObject> ShieldSkillUI = new List<GameObject>();
    [SerializeField] List<GameObject> SpecialAttackSkillUI = new List<GameObject>();
    [SerializeField] List<GameObject> DoubleDamageSkillUI = new List<GameObject>();
    [SerializeField] List<GameObject> MakeGoldSkillUI = new List<GameObject>();
    [SerializeField] GameObject ShieldReady;

    // Text
    [SerializeField] TextMeshProUGUI StageText;
    [SerializeField] TextMeshProUGUI GoldText;
    [SerializeField] TextMeshProUGUI ComboText;
    [SerializeField] TextMeshProUGUI ComboBonusText;
    [SerializeField] TextMeshProUGUI EnergyText;
    [SerializeField] TextMeshProUGUI ShieldText;
    #endregion

    enum eGameState
    {
        Start,
        Active,
        Result,
        Upgrade,
    }

    // Game state
    eGameState GameState = eGameState.Start;

    int Stage = 1;
    int Checkpoint = 1;

    int FirstStage = 1;

    int Combo = 0;
    int Gold = 0;
    int GoldSpent = 0;
    int Energy = 0;

    int GoldEarned = 0;
    int MaxCombo = 0;
    int DamageDealt = 0;
    int AttacksReturned = 0;
    int PerfectReturns = 0;
    int CreaturesDodged = 0;

    int MonsterHP = 0;
    int MaxMonsterHP = 0;

    const int TickLimit = 30;
    int MonsterTimer = TickLimit;

    Movement CurrentEnemy;

    // Upgrades
    int AttackLevel = 1;
    int ComboBonusLevel = 1;

    // Attacks
    float TickTimer = 0.0f;
    float AttackChance = 0.5f;

    Queue<IncomingObject> IncomingObjects = new Queue<IncomingObject>();

    // Skills
    Dictionary<eSkillType, Skill> Skills = new Dictionary<eSkillType, Skill>();

    // Monsters
    Vector3 BossPosition;
    Vector3 MonsterPosition;

    #region Start and End Game
    void Awake()
    {
        // Skill setup
        Skills[eSkillType.Shield] = new Skill(eSkillType.Shield, "Shield", ShieldSkillUI, cost: 30, levelRequirement: new List<int>() { 5, 35, 60 }, onUse: UseShield, usesNeeded: true);
        Skills[eSkillType.SpecialAttack] = new Skill(eSkillType.SpecialAttack, "Special Attack", SpecialAttackSkillUI, cost: 50, levelRequirement: new List<int>() { 15, 30, 70, 100, 250 }, onUse: UseSpecialAttack);
        Skills[eSkillType.DoubleDamage] = new Skill(eSkillType.DoubleDamage, "Double Damage", DoubleDamageSkillUI, cost: 40, levelRequirement: new List<int>() { 20, 45, 120, 200, 300 }, onUse: UseDoubleDamageSkill);
        Skills[eSkillType.MakeGold] = new Skill(eSkillType.MakeGold, "Gold Maker", MakeGoldSkillUI, cost: 100, levelRequirement: new List<int>() { 35, 50, 90, 150, 500 }, onUse: UseGoldMaker);

        // Keep track of starting monster positions
        BossPosition = Boss.transform.position;
        MonsterPosition = Monster.transform.position;

        // Set any hidden objects active
        foreach (var item in SetActiveOnStart)
        {
            item.SetActive(true);
        }
    }

    public void StartGame()
    {
        StartCanvas.SetActive(false);
        ResultScreen.gameObject.SetActive(false);
        UpgradeShop.gameObject.SetActive(false);

        Combo = -1;
        Energy = 0;

        FirstStage = Checkpoint;

        GoldEarned = 0;
        MaxCombo = 0;
        DamageDealt = 0;
        AttacksReturned = 0;
        CreaturesDodged = 0;

        UpdateEnergy(0);
        UpdateGold(0);
        ComboUp();
        UpdateSkills();

        TickTimer = Random.Range(TickTime.x, TickTime.y);

        Skills[eSkillType.Shield].Uses = Skills[eSkillType.Shield].GetLevel(Checkpoint);
        ShieldText.text = $"{Skills[eSkillType.Shield].Uses}";
        StartStage(Checkpoint);

        GameState = eGameState.Active;
    }

    void GameOver(Movement monster = null)
    {
        GameState = eGameState.Result;

        // Show result screen
        OpenResultScreen();

        // Hide incoming objects
        foreach (var thing in IncomingObjects)
        {
            thing.ReturnToPool();
        }

        IncomingObjects.Clear();
    }
    #endregion

    #region Result screen and upgrade shop
    void OpenResultScreen()
    {
        var unlockedSkills = new List<string>();

        if (Stage > FirstStage)
        {
            foreach (var skill in Skills)
            {
                var levelBefore = skill.Value.GetLevel(FirstStage);
                var levelNow = skill.Value.GetLevel(Stage);

                if (levelNow > levelBefore)
                {
                    unlockedSkills.Add($"{skill.Value.SkillName} Lv. {levelNow}");
                }
            }
        }

        ResultScreen.Setup(FirstStage, Stage, GoldEarned, MaxCombo, DamageDealt, AttacksReturned, PerfectReturns, CreaturesDodged, unlockedSkills);
        ResultScreen.gameObject.SetActive(true);
    }

    public void OpenUpgradeShop()
    {
        GameState = eGameState.Upgrade;
        UpdateUpgradeShop();
        UpgradeShop.gameObject.SetActive(true);
    }
    #endregion

    #region Stages and Update
    void StartStage(int stageNumber)
    {
        Stage = stageNumber;
        StageText.text = $"{stageNumber}";

        SpawnMonster(stageNumber);
        MonsterTimer = TickLimit;
    }

    void NewStage(Movement enemyDefeated = null)
    {
        if (Stage % 5 == 0)
        {
            Checkpoint = Stage + 1;
        }

        StartStage(Stage + 1);
    }

    void Tick()
    {
        // Spawn object
        var isAttack = Random.value < AttackChance;
        if (isAttack)
        {
            AttackChance -= 0.1f;
            SpawnAttack();
        }
        else
        {
            AttackChance += 0.1f;
            SpawnNotAttack();
        }

        // Update and check timer
        MonsterTimer--;
        if (MonsterTimer <= 0)
        {
            //Monster attacks player
            CurrentEnemy.SetStraightMovement(Player.transform.position, PlayerRadius, GameOver, null);
        }
    }

    void SpawnAttack()
    {
#if UNITY_EDITOR
        if (AutoPlay)
        {
            if (Random.value < PerfectReturnChance)
            {
                PerfectReturn(null);
            }
            else
            {
                NormalReturn(null);
            }
            return;
        }
#endif

        var incomingObject = AttackPools[Random.Range(0, AttackPools.Count)].GetFromPool();
        incomingObject.transform.position = AttackSpawnPosition.position;

        incomingObject.Activate();

        incomingObject.SetArchMovement(Utility.Vec3Dto2D(Player.position), PlayerRadius, HitPlayer, ReturnAttack);

        IncomingObjects.Enqueue(incomingObject);
    }

    void SpawnNotAttack()
    {
#if UNITY_EDITOR
        if (AutoPlay)
        {
            PassBy(null);
            return;
        }
#endif

        var incomingObject = NotAttackPools[Random.Range(0, AttackPools.Count)].GetFromPool();
        incomingObject.transform.position = AttackSpawnPosition.position;

        incomingObject.Activate();

        incomingObject.SetArchMovement(Player.position, PlayerRadius, PassBy, ReturnNotAttack);

        IncomingObjects.Enqueue(incomingObject);
    }

    void Update()
    {
        switch(GameState)
        {
            case eGameState.Start:
            {
                // Start game
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0))
                {
                    StartGame();
                }
                break;
            }
            case eGameState.Active:
            {
                // Timer
                TickTimer -= Time.deltaTime;
                if (TickTimer < 0.0f)
                {
                    TickTimer += Random.Range(TickTime.x, TickTime.y);
                    Tick();
                }

                // Player input - returning attacks
                if (IncomingObjects.Count > 0 && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0)))
                {
                    var incomingAttack = IncomingObjects?.Peek();
                    incomingAttack?.InvokeOnHit();
                }

                // Player input - skills
                if (Skills[eSkillType.SpecialAttack].Ready && Input.GetKeyDown(KeyCode.Z))
                {
                    Skills[eSkillType.SpecialAttack].Use();
                }
                else if (Skills[eSkillType.DoubleDamage].Ready && Input.GetKeyDown(KeyCode.X))
                {
                    Skills[eSkillType.DoubleDamage].Use();
                }
                else if (Skills[eSkillType.MakeGold].Ready && Input.GetKeyDown(KeyCode.C))
                {
                    Skills[eSkillType.MakeGold].Use();
                }

                // Slow time if an attack is about to hit player
                var danger = false;
                var timeScale = 1.0f;
                if (IncomingObjects.Count > 0 && IncomingObjects.Peek().ObjectType == IncomingObject.eObjectType.Attack)
                {
                    var dist = Vector2.Distance(Utility.Vec3Dto2D(Player.position), Utility.Vec3Dto2D(IncomingObjects.Peek().transform.position));
                    danger = dist < PerfectRadius;

                    if (danger)
                    {
                        var colour = Danger.color;
                        var dangerRatio = dist / PerfectRadius;
                        colour.a = 1.0f - dangerRatio;
                        Danger.color = colour;
                        timeScale = 1.0f - dangerRatio * 0.9f;
                    }
                }

                Danger.gameObject.SetActive(danger);
                Time.timeScale = timeScale * TimeScale;

                // Update skills
                UpdateSkills();
                break;
            }
            case eGameState.Result:
            {
                // Open upgrade shop
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    OpenUpgradeShop();
                }
                break;
            }
            case eGameState.Upgrade:
            {
                break;
            }
        }
    }

    void UpdateSkills()
    {
        foreach (var skill in Skills)
        {
            skill.Value.UpdateStates(Stage, Energy);
        }

        ShieldReady.SetActive(Skills[eSkillType.Shield].Active && Skills[eSkillType.Shield].Ready);
    }
    #endregion

    #region Monsters
    void SpawnMonster(int stageNumber)
    {
        CurrentEnemy?.StopMovement();

        MaxMonsterHP = GetMonsterHPForStage(stageNumber);
        MonsterHP = MaxMonsterHP;

        MonsterHPBar.fillAmount = 1.0f;
        MonsterHPDisplay.SetActive(true);

        var isBoss = stageNumber % 5 == 0;

        if (isBoss)
        {
            Boss.transform.position = BossPosition;
            CurrentEnemy = Boss;
        }
        else
        {
            Monster.transform.position = MonsterPosition;
            CurrentEnemy = Monster;
        }

        Monster.gameObject.SetActive(!isBoss);
        Boss.gameObject.SetActive(isBoss);
    }

    void DamageMonster(int damage, int exclamationMarks, bool special)
    {
        if (MonsterHP <= 0)
        {
            return;
        }

        if (Skills[eSkillType.DoubleDamage].Uses > 0)
        {
            damage *= 2;
            Skills[eSkillType.DoubleDamage].Uses--;
        }

        MonsterHP -= damage;
        DamageDealt += damage;

        MonsterHPBar.fillAmount = (float)MonsterHP / MaxMonsterHP;

        var text = Utility.FormatNumber(damage);
        for (int i = 0; i < exclamationMarks; i++)
        {
            text += "!";
        }
        ShowText(CurrentEnemy.transform.position, text, special ? ColourSpecial : ColourCorrect);

        if (MonsterHP <= 0)
        {
            var gold = GoldForStage(Stage);
            UpdateGold(gold);
            CurrentEnemy.SetStraightMovement(Utility.Vec3Dto2D(CurrentEnemy.transform.position) - MonsterDeathOffset, HitRadius, NewStage, null);
            MonsterHPDisplay.SetActive(false);
        }
    }

    void CritHit(Movement attack)
    {
        var special = Skills[eSkillType.DoubleDamage].Uses > 0;
        DamageMonster(GetPlayerDamage(crit: true), exclamationMarks: 1, special);
        (attack as IncomingObject)?.ReturnToPool();
    }

    void Hit(Movement attack)
    {
        var special = Skills[eSkillType.DoubleDamage].Uses > 0;
        DamageMonster(GetPlayerDamage(crit: false), exclamationMarks: 0, special);
        (attack as IncomingObject)?.ReturnToPool();
    }

    void HealMonster(Movement thing)
    {
        if (MonsterHP <= 0)
        {
            return;
        }

        var heal = (int)(MaxMonsterHP * 0.2f);
        MonsterHP = Mathf.Min(MaxMonsterHP, MonsterHP + heal);
        MonsterHPBar.fillAmount = (float)MonsterHP / MaxMonsterHP;

        var food = thing as IncomingObject;
        food?.ReturnToPool();

        ShowText(CurrentEnemy.transform.position, $"+{heal}HP", ColourWrong);
    }
    #endregion

    #region Incoming Objects
    Vector2 AttackTarget()
    {
        if (MonsterHP > 0)
        {
            return CurrentEnemy.transform.position;
        }

        return AttackSpawnPosition.position;
    }

    void ReturnAttack(Movement attack)
    {
        var dist = Vector2.Distance(Utility.Vec3Dto2D(Player.position), Utility.Vec3Dto2D(attack.transform.position));

        if (dist <= HitRadius)
        {
            if (dist <= PerfectRadius)
            {
                PerfectReturn(attack);
            }
            else
            {
                NormalReturn(attack);
            }
        }
    }

    void PerfectReturn(Movement attack)
    {
        AttacksReturned++;
        PerfectReturns++;
        ComboUp();
        ShowText(Player.position, "Perfect!", ColourCorrect);

#if UNITY_EDITOR
        if (AutoPlay)
        {
            CritHit(null);
            return;
        }
#endif
        attack.SetStraightMovement(AttackTarget(), PerfectRadius, CritHit, onHit: null);
        IncomingObjects.Dequeue();
    }    

    void NormalReturn(Movement attack)
    {
        AttacksReturned++;
        ComboUp();
        ShowText(Player.position, "Nice!", ColourCorrect);

#if UNITY_EDITOR
        if (AutoPlay)
        {
            Hit(null);
            return;
        }
#endif

        attack.SetStraightMovement(AttackTarget(), PerfectRadius, Hit, onHit: null);
        IncomingObjects.Dequeue();
    }

    void ReturnNotAttack(Movement notAttack)
    {
        var dist = Vector2.Distance(Utility.Vec3Dto2D(Player.position), Utility.Vec3Dto2D(notAttack.transform.position));

        if (dist <= HitRadius)
        {
            notAttack.SetStraightMovement(AttackTarget(), HitRadius, HealMonster, onHit: null);
            IncomingObjects.Dequeue();
            ShowText(Player.position, "Wrong!", ColourWrong);
            ResetCombo();
        }
    }

    void HitPlayer(Movement thing)
    {
        ResetCombo();

        if (IncomingObjects.Count == 0)
        {
            return;
        }

        var shield = Skills[eSkillType.Shield];
        if (shield.Uses > 0 && shield.Cost <= Energy)
        {
            ShowText(Player.position, "-1 SHIELD", ColourWrong);
            shield.Use();
        }
        else
        {
            ShowText(Player.position, "GAME OVER", ColourWrong);
            GameOver();
            return;
        }
        
        var attack = IncomingObjects.Dequeue();
        attack.ReturnToPool();
    }

    void PassBy(Movement thing)
    {
        CreaturesDodged++;
        ComboUp();
#if UNITY_EDITOR
        if (AutoPlay)
        {
            return;
        }
#endif

        // This assumes that the objects collide with the player in the same order they spawn
        if (IncomingObjects.Count == 0)
        {
            return;
        }

        IncomingObjects.Dequeue();
    }
    #endregion

    #region Skills
    void UseShield()
    {
        var shield = Skills[eSkillType.Shield];

        shield.Uses--;
        ShieldText.text = $"{Skills[eSkillType.Shield].Uses}";
    }

    public void UseSpecialAttack()
    {
        var specialAttack = Skills[eSkillType.SpecialAttack];

        var cost = specialAttack.Cost;
        if (Energy < cost)
        {
            return;
        }

        var exclamationMarks = 2;
        if (Skills[eSkillType.DoubleDamage].Uses > 0)
        {
            exclamationMarks++;
        }

        var damage = SpecialAttackDamage(AttackLevel, Skills[eSkillType.SpecialAttack].GetLevel(Stage));

        DamageMonster(damage, exclamationMarks, special: true);
        UpdateEnergy(-cost);
    }

    public void UseDoubleDamageSkill()
    {
        var skill = Skills[eSkillType.DoubleDamage];

        var cost = skill.Cost;
        if (Energy < cost)
        {
            return;
        }

        skill.Uses += skill.GetLevel(Stage);
        UpdateEnergy(-cost);
    }

    public void UseGoldMaker()
    {
        var skill = Skills[eSkillType.MakeGold];

        var cost = skill.Cost;
        if (Energy < cost)
        {
            return;
        }

        UpdateEnergy(-Energy);

        var gold = (int)(GoldMakerGold(skill.GetLevel(Stage), GoldSpent) * (1.0f + ComboBonus()));
        UpdateGold(gold);
    }
    #endregion

    #region Values
    public static int GetMonsterHPForStage(int stageNumber)
    {
        float hp = 14.0f * stageNumber * (0.5f + (stageNumber % 5) * 0.125f);

        if (stageNumber % 5 == 0)
        {
            hp *= 3.7f;
        }

        return Mathf.FloorToInt(hp);
    }

    int GetPlayerDamage(bool crit)
    {
        var damage = PlayerDamageForLevel(AttackLevel);

        if (crit)
        {
            damage = (int)(damage * 1.5f);
        }

        return damage;
    }

    public static int PlayerDamageForLevel(int level)
    {
        return 5 * level + 10;
    }

    float ComboBonus()
    {
        var bonus = ComboBonusForLevel(ComboBonusLevel) * Combo;

        return bonus;
    }

    public static float ComboBonusForLevel(int level)
    {
        return 0.001f * level;
    }

    int GoldForStage(int stageNumber)
    {
        var gold = GetMonsterHPForStage(stageNumber) * 0.6f * (1.0f + ComboBonus());

        return (int)gold;
    }

    public static int SpecialAttackDamage(int attackLevel, int skillLevel)
    {
        var mult = 0.0f;
        for (int i = 0; i < skillLevel; i++)
        {
            mult += 1.7f - i * 0.35f;
        }
        var damage = 70.0f + PlayerDamageForLevel(attackLevel) * mult;

        return (int)damage;
    }

    public static int GoldMakerGold(int skillLevel, int goldSpent)
    {
        return skillLevel * ((int)(goldSpent * 0.0015f) + 2500);
    }

    public static int UpgradeAttackCost(int level)
    {
        return 800 * level - 700;
    }

    public static int UpgradeComboBonusCost(int level)
    {
        return 2000 * level * level - 1000;
    }

    #endregion

    #region Update Values
    void ComboUp()
    {
        Combo++;
        ComboText.text = $"{Combo}";
        ComboBonusText.text = $"{Utility.FormatNumber(ComboBonus() * 100.0f)}%";

        UpdateEnergy(1);

        if (Combo > MaxCombo)
        {
            MaxCombo = Combo;
        }
    }

    void ResetCombo()
    {
        Combo = 0;
        ComboText.text = $"{Combo}";
        ComboBonusText.text = $"0%";
    }

    void UpdateGold(int change)
    {
        Gold += change;
        GoldEarned += change;
        GoldText.text = $"{Utility.FormatNumber(Gold)}";
    }

    void UpdateEnergy(int change)
    {
        Energy = Mathf.Clamp(Energy + change, 0, 100);
        EnergyText.text = $"{Energy}";
    }

    bool TryPurchase(int cost)
    {
        if (cost > Gold)
        {
            return false;
        }

        Gold -= cost;
        GoldSpent += cost;

        return true;
    }

    public void UpgradeAttack()
    {
        var cost = UpgradeAttackCost(AttackLevel);

        if (!TryPurchase(cost))
        {
            return;
        }

        AttackLevel++;

        UpdateUpgradeShop();
    }

    public void UpgradeComboBonus()
    {
        var cost = UpgradeComboBonusCost(ComboBonusLevel);

        if (!TryPurchase(cost))
        {
            return;
        }

        ComboBonusLevel++;

        UpdateUpgradeShop();
    }

    void UpdateUpgradeShop()
    {
        UpgradeShop.SetText(Gold, GoldSpent, AttackLevel, ComboBonusLevel, Checkpoint, Skills.Values.ToList());
    }
    #endregion

    #region Text
    void ShowText(Vector3 position, string message, Color colour)
    {
        var text = PopupTextPool.GetFromPool();
        text.Setup(position, message, colour);
        text.gameObject.SetActive(true);
    }
    #endregion
}
