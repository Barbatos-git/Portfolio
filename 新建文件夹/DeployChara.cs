using UnityEngine;

public class DeployChara : MonoBehaviour
{
    public int baseOrder = 1000;

    private SpriteRenderer sr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        // Yが低いほど → sortingOrder が大きくなります（前に近いほど）
        int order = baseOrder - Mathf.RoundToInt(transform.position.y * 100);
        sr.sortingOrder = Mathf.Max(order, 100);
    }
}
