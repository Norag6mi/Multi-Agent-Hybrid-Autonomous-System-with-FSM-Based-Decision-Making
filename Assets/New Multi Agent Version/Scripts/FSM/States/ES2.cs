using UnityEngine;

/// <summary>
/// ES2 = Neural Engage State
///
/// FSM Layer ONLY
///
/// Handles:
/// - Engage state logic
/// - Target validation
/// - State transitions
/// - Threat broadcasting


public class ES2 : BaseState<AgentState>
{
    private AgentFSM fsm;
    private Transform currentTarget;

    private bool hasBroadcast = false;

    public ES2(AgentFSM fsm) : base(AgentState.Engage)
    {
        this.fsm = fsm;
    }

    public override void EnterState()
    {
        hasBroadcast = false;

        currentTarget =
            fsm.AwarenessModel.currentTarget;

        SetCombatTarget(currentTarget);

        BroadcastThreat();

        Debug.Log(
            $"[STATE] {fsm.gameObject.name} → ES2"
        );
    }

    public override void UpdateState()
    {
        IAgentCombat combat =
            fsm.Identity.Combat;

        if (combat == null)
            return;

        // ======================================
        // DEAD CHECK
        // ======================================

        if (combat.IsDead())
            return;

        // ======================================
        // TARGET UPDATE
        // ======================================

        if (fsm.AwarenessModel.currentTarget != null)
        {
            currentTarget =
                fsm.AwarenessModel.currentTarget;

            SetCombatTarget(currentTarget);
        }

        // ======================================
        // TARGET LOST / DEAD
        // ======================================

        if (currentTarget == null ||
            IsTargetDead())
        {
            combat.StopAttack();

            fsm.Navigation.Stop();

            return;
        }

        // ======================================
        // LOOK AT TARGET
        // ======================================

        fsm.Navigation.LookAt(
            currentTarget,
            10f
        );
    }

    public override void ExitState()
    {
        if (fsm.Identity.Combat != null)
        {
            fsm.Identity.Combat.StopAttack();
        }

        fsm.Navigation.Stop();
    }

    public override AgentState GetNextState()
    {
        if (fsm.Identity.Combat != null &&
            fsm.Identity.Combat.IsDead())
        {
            return AgentState.Dead;
        }

        return
            fsm.AwarenessModel.EvaluateState();
    }

    // =====================================================
    // TARGET DEATH CHECK
    // =====================================================

    private bool IsTargetDead()
    {
        if (currentTarget == null)
            return true;

        AgentIdentity targetIdentity =
            currentTarget.GetComponentInParent<AgentIdentity>();

        if (targetIdentity != null)
        {
            HealthComponent targetHealth =
                currentTarget.GetComponentInParent<HealthComponent>();

            if (targetHealth != null)
                return targetHealth.Model.IsDead;

            return !targetIdentity.IsAlive;
        }

        HealthComponent health =
            currentTarget.GetComponentInParent<HealthComponent>();

        if (health != null)
            return health.Model.IsDead;

        return false;
    }

    // =====================================================
    // COMBAT TARGET
    // =====================================================

    private void SetCombatTarget(Transform target)
    {
        CombatStub stub =
            fsm.GetComponent<CombatStub>();

        if (stub != null)
        {
            stub.currentTarget = target;
        }
    }

    // =====================================================
    // THREAT BROADCAST
    // =====================================================

    private void BroadcastThreat()
    {
        if (hasBroadcast)
            return;

        if (AgentCoordinator.Instance == null)
            return;

        AgentCoordinator.Instance.BroadcastThreat(
            fsm.Identity,
            fsm.AwarenessModel.lastKnownPosition
        );

        hasBroadcast = true;
    }
}