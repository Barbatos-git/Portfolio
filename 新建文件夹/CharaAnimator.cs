using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CharaAnimator : MonoBehaviour
{
    private CharaStateMachine stateMachine;
    private int currentDirection = 0;

    void Awake()
    {
        stateMachine = GetComponent<CharaStateMachine>();
    }

    void Start()
    {
        //currentDirection = stateMachine.direction;
        //stateMachine.ChangeState(CharaState.Idle, currentDirection);
    }

    public void InitializeDirection(int dir)
    {
        currentDirection = dir;
        stateMachine.SetDirectionOnly(dir);
        stateMachine.ChangeState(CharaState.Idle, dir);
    }


    //public void PlayIdle(int direction)
    //{
    //    currentDirection = direction;
    //    stateMachine.SetDirectionOnly(direction);
    //    stateMachine.ChangeState(CharaState.Idle, currentDirection);
    //}

    //public void PlayMove(Vector3Int from, Vector3Int to, Tilemap tilemap)
    //{
    //    this.currentDirection = direction;
    //    currentDirection = GetDirection(tilemap,from, to);
    //    stateMachine.ChangeState(CharaState.Run, currentDirection);
    //}

    public void PlayLightAttack(Vector3Int from, Vector3Int to, Tilemap tilemap)
    {
        currentDirection = DirectionUtils.GetDirection8(tilemap,from, to);
        UpdateFacingByDirection(currentDirection);
        PlayAttackSound();
        stateMachine.ChangeState(CharaState.LightAttack, currentDirection);
    }

    public void PlayHeavyAttack(Vector3Int from, Vector3Int to, Tilemap tilemap)
    {
        currentDirection = DirectionUtils.GetDirection8(tilemap,from, to);
        UpdateFacingByDirection(currentDirection);
        PlayAttackSound();
        stateMachine.ChangeState(CharaState.HeavyAttack, currentDirection);
    }

    public void PlayHit()
    {
        stateMachine.ChangeState(CharaState.TakeDamage, currentDirection);
    }

    //攻撃と移動した後
    public void OnAnimEnd()
    {
        stateMachine.ChangeState(CharaState.Idle, currentDirection);
    }

    //攻撃を受けた後
    public void OnAnimEndHit()
    {
        var currentUnit = GetComponent<UnitActionHandler>();
        if (currentUnit.IsDead())
        {
            stateMachine.ChangeState(CharaState.Die, currentDirection);
        }
        else
        {
            stateMachine.ChangeState(CharaState.Idle, currentDirection);
        }
    }

    //死亡した後
    public void OnAnimEndDie()
    {
        var handler = GetComponent<UnitActionHandler>();
        if (handler != null)
        {
            handler.OnDeath();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MoveAlongPath(List<Vector3Int> path, Tilemap tilemap)
    {
        StartCoroutine(MoveWithAnimation(path, tilemap));
    }

    private IEnumerator MoveWithAnimation(List<Vector3Int> path, Tilemap tilemap)
    {
        for (int i = 1; i < path.Count; i++)
        {
            Vector3Int from = path[i - 1];
            Vector3Int to = path[i];

            currentDirection = DirectionUtils.GetDirection8(tilemap,from, to);

            stateMachine.SetDirectionOnly(currentDirection);
            stateMachine.ChangeState(CharaState.Run, currentDirection);

            Vector3 startPos = tilemap.GetCellCenterWorld(from);
            Vector3 targetPos = tilemap.GetCellCenterWorld(to);

            float t = 0f;
            float duration = 0.2f;

            while (t < duration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, targetPos, t / duration);
                yield return null;
            }

            transform.position = targetPos;
        }

        stateMachine.ChangeState(CharaState.Idle, currentDirection);
    }

    private int UpdateFacingByDirection(int direction)
    {
        currentDirection = AttackDirectionHelper.GetFlippedScale(direction);
        return currentDirection;
    }

    //private static readonly int[] isometricFlipMap = {
    //0, // →
    //7, // ↗
    //2, // ↓
    //5, // ↖
    //4, // ←
    //3, // ↙
    //6, // ↑
    //1  // ↘
    //};

    //private int GetDirection(Tilemap tilemap, Vector3Int fromCell, Vector3Int toCell)
    //{
    //    Vector3 from = tilemap.GetCellCenterWorld(fromCell);
    //    Vector3 to = tilemap.GetCellCenterWorld(toCell);
    //    Vector2 dir = to - from;

    //    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    //    // 0～360度の角度マッピング
    //    angle = (angle + 360f) % 360f;

    //    // 360°を8方向に分割し、各方向を45°ずつにする
    //    // 0 = → (0°)、1 = ↘ (45°)、2 = ↓ (90°)...
    //    int direction = Mathf.RoundToInt(angle / 45f) % 8;
    //    return isometricFlipMap[direction];
    //}
    private void PlayAttackSound()
    {
        string id = "";

        var charaBase = GetComponent<CharaBase>();
        if (charaBase != null)
        {
            id = charaBase.GetCharaID();
        }

        AttackSFXManager.Instance?.PlayAttackSound(id);
    }
    public int GetCurrentDirection() => currentDirection;
}