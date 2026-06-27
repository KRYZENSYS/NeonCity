// =============================================================================
// NeonCity — AI / Blackboard.cs
// -----------------------------------------------------------------------------
// Per-NPC scratchpad: shared between Behavior Tree leaves, FSM states,
// Perception, and Utility AI scorer. Plain data + a few helpers.
// =============================================================================

using UnityEngine;

namespace NeonCity.AI
{
    public class Blackboard
    {
        public Transform[] PatrolPoints;
        public bool   Sprint;
        public float  WalkSpeed  = 2f;
        public float  SprintSpeed = 5.5f;

        // Memory of last sighting
        public Vector3 LastKnownPosition;
        public bool    HasLastKnownPosition;

        // Threat tracking
        public float ThreatLevel;     // 0..1
        public float TimeSinceLastSawTarget = 999f;

        // Cover
        public Vector3 CoverPoint;
        public bool    HasCoverPoint;
    }
}