// =============================================================================
// NeonCity — AI / BehaviorTree / BTLeaf.cs
// -----------------------------------------------------------------------------
// Concrete leaf nodes used by the BT. Encapsulate the actual actions: chase,
// attack, take cover, patrol, etc. Each leaf uses Unity NavMesh for movement
// and the NPC's weapon for combat.
// =============================================================================

using UnityEngine;
using UnityEngine.AI;
using NeonCity.Combat;

namespace NeonCity.AI.BehaviorTree
{
    // ---------- Conditions ----------
    public class HasTarget : BTNode
    {
        public override BTState Tick(BTContext ctx) => ctx.HasTarget ? BTState.Success : BTState.Failure;
    }

    public class HasLowHealth : BTNode
    {
        public float threshold = 0.3f;
        public override BTState Tick(BTContext ctx) => ctx.HealthPct <= threshold ? BTState.Success : BTState.Failure;
    }

    // ---------- Actions ----------
    public class MoveToTarget : BTNode
    {
        public float stopDistance = 2f;
        public override BTState Tick(BTContext ctx)
        {
            if (!ctx.HasTarget) return BTState.Failure;
            ctx.Nav.SetDestination(ctx.Target.position);
            ctx.Nav.SetSpeed(ctx.Blackboard.Sprint ? ctx.Blackboard.SprintSpeed : ctx.Blackboard.WalkSpeed);
            return ctx.TargetDistance <= stopDistance ? BTState.Success : BTState.Running;
        }
    }

    public class AimAtTarget : BTNode
    {
        public override BTState Tick(BTContext ctx)
        {
            if (!ctx.HasTarget) return BTState.Failure;
            var dir = (ctx.Target.position - ctx.Self.position).normalized;
            ctx.Self.rotation = Quaternion.Slerp(ctx.Self.rotation,
                                                 Quaternion.LookRotation(dir),
                                                 10f * ctx.DeltaTime);
            return BTState.Success;
        }
    }

    public class FireAtTarget : BTNode
    {
        public float fireInterval = 0.25f;
        private float _t;
        public override BTState Tick(BTContext ctx)
        {
            if (!ctx.HasTarget) return BTState.Failure;
            _t -= ctx.DeltaTime;
            if (_t > 0) return BTState.Running;
            _t = fireInterval;
            ctx.Weapon?.Fire();
            return BTState.Running;
        }
    }

    public class TakeCover : BTNode
    {
        public override BTState Tick(BTContext ctx)
        {
            if (!ctx.HasTarget) return BTState.Failure;
            // Sample a position behind the nearest cover (NavMesh).
            if (!ctx.Nav.TryFindCover(ctx.Self.position, ctx.Target.position, out var coverPos))
                return BTState.Failure;
            ctx.Nav.SetDestination(coverPos);
            return BTState.Running;
        }
    }

    public class Patrol : BTNode
    {
        private int _i;
        public override BTState Tick(BTContext ctx)
        {
            if (ctx.Blackboard.PatrolPoints == null || ctx.Blackboard.PatrolPoints.Length == 0)
                return BTState.Failure;
            var p = ctx.Blackboard.PatrolPoints[_i];
            ctx.Nav.SetDestination(p.position);
            if (Vector3.Distance(ctx.Self.position, p.position) < 1.5f)
                _i = (_i + 1) % ctx.Blackboard.PatrolPoints.Length;
            return BTState.Running;
        }
    }

    public class InvestigateLastSeen : BTNode
    {
        public override BTState Tick(BTContext ctx)
        {
            if (!ctx.Blackboard.HasLastKnownPosition) return BTState.Failure;
            ctx.Nav.SetDestination(ctx.Blackboard.LastKnownPosition);
            return BTState.Running;
        }
    }
}