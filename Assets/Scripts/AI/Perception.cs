// =============================================================================
// NeonCity — AI / Perception.cs
// -----------------------------------------------------------------------------
// Sensory system for NPCs: vision cone, hearing radius, line-of-sight check.
// Updates the shared Blackboard each tick. Listening for footstep/gunfire
// events (raised by AudioService) makes stealth-aware AI possible.
// =============================================================================

using UnityEngine;
using UnityEngine.AI;

namespace NeonCity.AI
{
    public class Perception : MonoBehaviour
    {
        [Header("Vision")]
        public float viewRange   = 30f;
        [Range(0, 180)] public float viewAngle = 110f;
        public LayerMask lineOfSightMask = ~0;

        [Header("Hearing")]
        public float hearingRange = 18f;

        public Transform CurrentTarget { get; private set; }

        public void Tick(Blackboard bb, Transform self)
        {
            // 1) Look for any visible player.
            var players = GameObject.FindGameObjectsWithTag("Player");
            float closestSqr = float.PositiveInfinity;
            Transform closest = null;

            foreach (var p in players)
            {
                var t = p.transform;
                var to = t.position - self.position;
                var distSqr = to.sqrMagnitude;
                if (distSqr > viewRange * viewRange) continue;

                // Angle check (cone)
                var flat = new Vector3(to.x, 0, to.z);
                if (flat.sqrMagnitude < 0.001f) continue;
                var angle = Vector3.Angle(self.forward, flat.normalized);
                if (angle > viewAngle * 0.5f) continue;

                // LOS check
                if (Physics.Linecast(self.position + Vector3.up, t.position + Vector3.up,
                                     lineOfSightMask, QueryTriggerInteraction.Ignore))
                    continue;

                if (distSqr < closestSqr) { closestSqr = distSqr; closest = t; }
            }

            CurrentTarget = closest;
            if (closest != null)
            {
                bb.LastKnownPosition = closest.position;
                bb.HasLastKnownPosition = true;
            }
        }

        /// <summary>Called when something loud happens (gunshot, footstep).</summary>
        public void OnNoise(Vector3 pos, float radius)
        {
            var d = Vector3.Distance(transform.position, pos);
            if (d <= radius) CurrentTarget = FindClosestPlayer(pos);
        }

        private Transform FindClosestPlayer(Vector3 pos)
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            Transform best = null; float bestDist = float.PositiveInfinity;
            foreach (var p in players)
            {
                var d = Vector3.Distance(pos, p.transform.position);
                if (d < bestDist) { bestDist = d; best = p.transform; }
            }
            return best;
        }
    }
}