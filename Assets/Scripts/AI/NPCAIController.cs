// =============================================================================
// NeonCity — AI / NPCAIController.cs
// -----------------------------------------------------------------------------
// The orchestrator that ties Perception + BehaviorTree + FSM together.
//
// TICK FLOW (every FixedUpdate):
//   1. Perception updates Blackboard (vision/hearing).
//   2. Behavior Tree produces high-level intent (Chase / Attack / TakeCover).
//   3. FSM drives animations and movement on the body.
//   4. NavMeshAgent moves toward the chosen destination.
// =============================================================================

using UnityEngine;
using UnityEngine.AI;
using NeonCity.AI.BehaviorTree;
using NeonCity.Combat;

namespace NeonCity.AI
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Perception))]
    public class NPCAIController : MonoBehaviour, IDamageable
    {
        public float maxHealth = 100f;
        public float currentHealth;

        public WeaponBase weapon;
        public Animator  animator;
        public Transform[] patrolPoints;

        // BT root — built once on Awake.
        private BTNode _root;
        private BTContext _ctx;
        private Perception _perception;
        private NavMeshAgent _agent;
        private Blackboard _blackboard = new Blackboard();

        // -------------------------------------------------------------------
        private void Awake()
        {
            currentHealth = maxHealth;
            _perception = GetComponent<Perception>();
            _agent      = GetComponent<NavMeshAgent>();

            _blackboard.PatrolPoints = patrolPoints;

            _ctx = new BTContext
            {
                Self         = transform,
                Blackboard   = _blackboard,
                Perception   = _perception,
                Nav          = new NavController(_agent),
                Weapon       = new WeaponAPI(weapon),
                DeltaTime    = Time.fixedDeltaTime,
            };

            BuildBehaviorTree();
        }

        private void BuildBehaviorTree()
        {
            // High-level: react to low-health first (take cover), otherwise combat, otherwise patrol.
            _root = new BTSelector("Root",
                new BTSequence("LowHealthTakeCover",
                    new HasLowHealth(0.3f),
                    new TakeCover()),
                new BTSequence("Combat",
                    new HasTarget(),
                    new BTSelector("CombatAction",
                        new BTSequence("Engage",
                            new AimAtTarget(),
                            new FireAtTarget()),
                        new MoveToTarget())),
                new Patrol()
            );
        }

        // -------------------------------------------------------------------
        private void FixedUpdate()
        {
            _perception.Tick(_blackboard, transform);
            _ctx.DeltaTime    = Time.fixedDeltaTime;
            _ctx.Target       = _perception.CurrentTarget;
            _ctx.HasTarget    = _ctx.Target != null;
            _ctx.TargetDistance = _ctx.HasTarget
                ? Vector3.Distance(transform.position, _ctx.Target.position)
                : 0;
            _ctx.HealthPct    = currentHealth / maxHealth;

            if (_ctx.HasTarget) _blackboard.LastKnownPosition = _ctx.Target.position;

            _root.Tick(_ctx);

            UpdateAnimator();
        }

        private void UpdateAnimator()
        {
            if (animator == null) return;
            var vel = _agent.velocity.magnitude;
            animator.SetFloat("Speed", vel);
            animator.SetBool("Combat", _ctx.HasTarget);
            animator.SetBool("Aim",    _ctx.HasTarget && _ctx.TargetDistance < 20f);
        }

        // -------------------------------------------------------------------
        public void ApplyDamage(float dmg, Vector3 point, Vector3 normal)
        {
            currentHealth -= dmg;
            // Hit reaction animation
            animator?.SetTrigger("Hit");
            if (currentHealth <= 0) Die();
        }

        private void Die()
        {
            animator?.SetTrigger("Die");
            _agent.isStopped = true;
            GetComponent<Collider>().enabled = false;
            Destroy(gameObject, 4f);
        }
    }

    /// <summary>Tiny wrapper so the BT can fire weapons without knowing concrete type.</summary>
    public class WeaponAPI
    {
        private readonly WeaponBase _w;
        public WeaponAPI(WeaponBase w) => _w = w;
        public void Fire() => _w?.OnFireButton(true);
    }
}