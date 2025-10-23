using UnityEngine;

public class CharaBase : MonoBehaviour
{
    [SerializeField] protected string charaID;
    protected CharaInfo baseData;
    public UnitOwner owner;
    public CharaType charaType;
    protected QTEType qteType;
    protected int maxHP;
    protected int currentHP;
    protected int maxMP;
    protected int currentMP;
    protected int maxSTA;
    protected int currentSTA;
    protected int atk;
    protected int atkRange;
    protected AttackPattern attackPattern; 
    protected int def;
    protected int ats;
    protected int adf;
    protected int mov;

    public void Initialize(string id, int playerNum)
    {
        this.charaID = id;
        this.owner = (playerNum == 1) ? UnitOwner.Player1 : UnitOwner.Player2;
        baseData = CharaData.Get(charaID);
        if (baseData != null)
        {
            charaType = baseData.charaType;
            qteType = baseData.qteType;
            maxHP = baseData.maxHP;
            currentHP = baseData.maxHP;
            maxMP = baseData.maxMP;
            currentMP = baseData.maxMP;
            maxSTA = baseData.maxSTA;
            currentSTA = baseData.maxSTA;
            atk = baseData.atk;
            atkRange = baseData.atkRange;
            attackPattern = baseData.attackPattern;
            def = baseData.def;
            ats = baseData.ats;
            adf = baseData.adf;
            mov = baseData.mov;

            Debug.Log($"【初期化成功】" +
              $"\n● 名前: {gameObject.name}" +
              $"\n● ID: {charaID}" +
              $"\n● タイプ: {charaType}" +
              $"\n● HP: {currentHP}/{maxHP}" +
              $"\n● MP: {currentMP}/{maxMP}" +
              $"\n● STA: {currentSTA}/{maxSTA}" +
              $"\n● 攻撃力: {atk}" +
              $"\n● 攻撃範囲: {atkRange}" +
              $"\n● 範囲攻撃: {attackPattern}" +
              $"\n● 物理防御力: {def}" +
              $"\n● 魔法攻撃力: {ats}" +
              $"\n● 魔法防御力: {adf}" +
              $"\n● 移動範囲: {mov}");
        }
        else
        {
            Debug.LogError($"[CharaBase] baseData 取得に失敗しました！ID = {charaID}");
            return;
        }

        // 方向を初期化する
        int dir = playerNum == 1 ? 1 : 5;
        GetComponent<CharaAnimator>()?.InitializeDirection(dir);
    }

    public void TakeDamage(int baseAtk, DamageType type, CharaType atkType)
    {
        float multiplier = type switch
        {
            DamageType.Low => 0.8f,
            DamageType.Normal => 1.0f,
            DamageType.Crit => 1.2f,
            _ => 1.0f
        };

        float variance = Random.Range(0.95f, 1.05f);
        float rawDamage;

        if (atkType == CharaType.Physical)
            rawDamage = (baseAtk - def) * multiplier * variance;
        else
            rawDamage = (baseAtk - adf) * multiplier * variance;

        int finalDmg = Mathf.Max(1, Mathf.FloorToInt(rawDamage));

        currentHP = Mathf.Max(0, currentHP - finalDmg);

        DamageTextManager.Instance.SpawnDamage(transform.position, finalDmg);

        CharaDataUI ui = FindObjectOfType<CharaDataUI>();
        if (ui != null)
            ui.ShowPreview(GetCurrentCharaInfo(), charaType);

        string logType = atkType == CharaType.Physical ? "物理" : "魔法";
        Debug.Log($"{gameObject.name} は {finalDmg} の{logType}ダメージを受けた（残HP: {currentHP}）");
    }

    public bool TryConsumeSTA(int amount)
    {
        if (currentSTA < amount)
        {
            Debug.LogWarning($"{name} の STA 不足！必要: {amount}, 現在: {currentSTA}");
            return false;
        }

        currentSTA -= amount;
        Debug.Log($"{name} STA 消耗: -{amount} → 残り {currentSTA}");

        FindObjectOfType<CharaDataUI>()?.ShowPreview(this);
        return true;
    }

    public bool TryConsumeMP(int amount)
    {
        if (currentMP < amount)
        {
            Debug.LogWarning($"{name} の MP 不足！必要: {amount}, 現在: {currentMP}");
            return false;
        }

        currentMP -= amount;
        Debug.Log($"{name} MP 消耗: -{amount} → 残り {currentMP}");

        FindObjectOfType<CharaDataUI>()?.ShowPreview(this);
        return true;
    }

    public void RecoverSTA(int amount)
    {
        int before = currentSTA;
        currentSTA = Mathf.Min(maxSTA, currentSTA + amount);
        Debug.Log($"{name} STA 回復: +{currentSTA - before} → {currentSTA}/{maxSTA}");

        FindObjectOfType<CharaDataUI>()?.ShowPreview(this);
    }

    public void RecoverMP(int amount)
    {
        int before = currentMP;
        currentMP = Mathf.Min(maxMP, currentMP + amount);
        Debug.Log($"{name} MP 回復: +{currentMP - before} → {currentMP}/{maxMP}");

        FindObjectOfType<CharaDataUI>()?.ShowPreview(this);
    }


    public CharaInfo GetCurrentCharaInfo()
    {
        return new CharaInfo
        {
            id = charaID,
            maxHP = maxHP,
            currentHP = currentHP,
            maxMP = maxMP,
            currentMP = currentMP,
            maxSTA = maxSTA,
            currentSTA = currentSTA,
            atk = atk,
            def = def,
            ats = ats,
            adf = adf,
            mov = mov,
            charaType= charaType
        };
    }

    public bool IsDead() => currentHP <= 0;

    public string GetCharaID() => charaID;
    public CharaType GetCharaType() => charaType;
    public QTEType GetQTEType() => qteType;
    public int GetCurrentHP() => currentHP;
    public int GetMaxHP() => maxHP;
    public int GetCurrentMP() => currentMP;
    public int GetCurrentSTA() => currentSTA;
    public int GetAtk() => atk;
    public int GetAtkRange() => atkRange;
    public AttackPattern GetAtkPattern() => attackPattern;
    public int GetDef() => def;
    public int GetAts() => ats;
    public int GetAdf() => adf;
    public int GetSpd() => mov;
}