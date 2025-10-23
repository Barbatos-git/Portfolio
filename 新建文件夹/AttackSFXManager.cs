using UnityEngine;

public class AttackSFXManager : MonoBehaviour
{
    public static AttackSFXManager Instance;

    public AudioClip warriorAttack;
    public AudioClip frostMageAttack;
    public AudioClip archerAttack;
    public AudioClip assassinAttack;
    public AudioClip fireMageAttack;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayAttackSound(string charaID)
    {
        AudioClip clip = null;

        switch (charaID)
        {
            case "Warrior_Prefab": clip = warriorAttack; break;
            case "FrostMage_Prefab": clip = frostMageAttack; break;
            case "Archer_Prefab": clip = archerAttack; break;
            case "Assassin_Prefab": clip = assassinAttack; break;
            case "FireMage_Prefab": clip = fireMageAttack; break;
        }

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"未找到 {charaID} 的攻击音效！");
        }
    }
}
