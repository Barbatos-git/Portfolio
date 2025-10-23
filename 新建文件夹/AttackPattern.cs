using UnityEngine;

[CreateAssetMenu(fileName = "AttackPattern", menuName = "Scriptable Objects/AttackPattern")]
public class AttackPattern : ScriptableObject
{
    public string patternName;
    public Vector3Int[] offsets; // 攻撃中心グリッドに対するオフセットのリスト
    public bool useDirectionalOffset;
}
