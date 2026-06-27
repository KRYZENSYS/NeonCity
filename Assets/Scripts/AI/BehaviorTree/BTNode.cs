// =============================================================================
// NeonCity — AI / BehaviorTree / BTNode.cs
// -----------------------------------------------------------------------------
// Base classes for a lightweight Behavior Tree. We support the four common
// composite types (Selector, Sequence, Parallel) plus decorators and leaves.
// This is the *high-level planner* each NPC runs every Think() tick.
//
// A Behavior Tree answers: "Given my state, what should I do next?"
// Lower-level actions (move-to, fire, take-cover) are implemented as Leaf
// nodes and call into Perception/Blackboard/Pathfinding.
// =============================================================================

using UnityEngine;

namespace NeonCity.AI.BehaviorTree
{
    public enum BTState { Success, Failure, Running }

    public abstract class BTNode
    {
        public BTState State { get; protected set; } = BTState.Running;
        public string Name { get; set; } = "Node";

        public abstract BTState Tick(BTContext ctx);
    }

    /// <summary>Shared per-NPC state passed through the tree.</summary>
    public class BTContext
    {
        public Blackboard Blackboard;
        public Perception Perception;
        public NavController Nav;
        public WeaponAPI  Weapon;
        public float      DeltaTime;

        // Reactive values
        public Transform Self;
        public Transform Target;
        public float     TargetDistance;
        public bool      HasTarget;
        public float     HealthPct;
    }
}