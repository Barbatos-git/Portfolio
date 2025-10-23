using UnityEngine;

public enum CharaState
{
    Idle,
    Run,
    LightAttack,
    HeavyAttack,//archerはなし
    TakeDamage,
    Die
}

public class CharaStateMachine : MonoBehaviour
{
    private CharaState currentState = CharaState.Idle;
    private Animator animator;
    public int direction;
    private UnitActionHandler currentUnit;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentUnit = GetComponent<UnitActionHandler>();
        if (currentUnit.owner == UnitOwner.Player1)
            direction = 1;
        else
            direction = 5;
        animator.SetFloat("Direction", direction); // 右
        ChangeState(CharaState.Idle, direction);   // Idleを再生
    }

    public void ChangeState(CharaState newState, int direction = -1)
    {
        if (currentState == newState) return;

        currentState = newState;

        if (direction >= 0)
            animator.SetFloat("Direction", direction);

        animator.Play(newState.ToString());
        Debug.Log($"{gameObject.name} 状態変更: {newState}（方向: {direction}）");
    }

    public void SetDirectionOnly(float direction)
    {
        if (animator == null) animator = GetComponent<Animator>();
        animator.SetFloat("Direction", direction);
    }

    public CharaState GetCurrentState() => currentState;
}