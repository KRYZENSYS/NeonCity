// =============================================================================
// NeonCity — Combat / HitscanWeapon.cs
// -----------------------------------------------------------------------------
// The classic FPS weapon model: instant ray, with multi-pellet support for
// shotguns. Damage falloff over distance is applied here.
//
// Network:
//   - Client predicts: ray on its machine, shows tracer/muzzle instantly.
//   - Server validates: actual damage applied to NetworkVariables.
//   - Anti-cheat: server clamps RPM and damage via ServerAuthority component.
// =============================================================================

using UnityEngine;
using NeonCity.Networking;

namespace NeonCity.Combat
{
    public class HitscanWeapon : WeaponBase
    {
        [Header("Hitscan")]
        public int   pellets      = 1;        // 1 = rifle/pistol, 8 = shotgun
        public float spreadDegrees = 0.5f;     // cone per pellet
        public float falloffStart = 30f;
        public float falloffEnd   = 80f;
        public LayerMask hitMask  = ~0;
        public GameObject impactFxPrefab;
        public GameObject tracerPrefab;

        protected override void FireOnce()
        {
            if (!CanFire()) { if (_ammo == 0) Reload(); return; }

            ConsumeAmmo();
            SpawnMuzzleFx();
            PlayFireSound();
            ApplyRecoil();

            for (int p = 0; p < pellets; p++)
            {
                var cam = _owner.GetAimPoint();
                var dir = ApplySpread(cam.forward, spreadDegrees);
                var ray = new Ray(cam.position, dir);

                if (Physics.Raycast(ray, out var hit, range, hitMask, QueryTriggerInteraction.Ignore))
                {
                    var dmg = ComputeDamage(hit.distance);
                    TryApplyDamage(hit, dmg);
                    SpawnImpact(hit);
                }

                SpawnTracer(ray.origin, hit.collider != null ? hit.point : ray.origin + ray.direction * range);
            }

            // Tell the server (via ServerRpc wrapper).
            NotifyServerFireRpc();
        }

        private float ComputeDamage(float dist)
        {
            if (dist <= falloffStart) return damage;
            if (dist >= falloffEnd)   return damage * 0.4f;
            var t = (dist - falloffStart) / (falloffEnd - falloffStart);
            return Mathf.Lerp(damage, damage * 0.4f, t);
        }

        private Vector3 ApplySpread(Vector3 dir, float deg)
        {
            if (deg <= 0f) return dir;
            var spread = deg * Mathf.Deg2Rad;
            var right = Vector3.Cross(dir, Vector3.up).normalized;
            var up    = Vector3.Cross(right, dir).normalized;
            var offset = Random.insideUnitCircle * Mathf.Tan(spread);
            return (dir + right * offset.x + up * offset.y).normalized;
        }

        private void SpawnImpact(RaycastHit hit)
        {
            if (impactFxPrefab == null) return;
            var fx = Instantiate(impactFxPrefab, hit.point + hit.normal * 0.01f,
                                 Quaternion.LookRotation(-hit.normal));
            Destroy(fx, 2f);
        }

        private void SpawnTracer(Vector3 a, Vector3 b)
        {
            if (tracerPrefab == null) return;
            var t = Instantiate(tracerPrefab);
            t.transform.position = (a + b) * 0.5f;
            t.transform.LookAt(b);
            t.transform.localScale = new Vector3(1, 1, Vector3.Distance(a, b));
            Destroy(t, 0.05f);
        }

        // ---------------------------------------------------------------
        // Server RPC stub — in a real project use Netcode for GameObjects
        // [ServerRpc] attribute. We keep it abstract here so the script
        // compiles without NGO references in this preview.
        private void NotifyServerFireRpc()
        {
            // Handled by NetworkPlayer.FireServerRpc(weaponId, dir, origin).
        }
    }
}