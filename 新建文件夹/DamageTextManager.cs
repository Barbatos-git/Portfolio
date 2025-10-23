using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance;
    public GameObject damageTextPrefab;
    public Canvas targetCanvas;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        //Damage test
        //if (Input.GetKeyDown(KeyCode.T))
        //    SpawnDamage(Vector3.zero, Random.Range(1, 999));
    }

    public void SpawnDamage(Vector3 worldPos, int damage)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos + Vector3.up * 1.5f);
        var obj = Instantiate(damageTextPrefab, targetCanvas.transform);
        obj.transform.position = screenPos;

        var dt = obj.GetComponent<DamageText>();
        dt.Show(damage);
    }
}
