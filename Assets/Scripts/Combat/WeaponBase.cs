// =============================================================================
// NeonCity — Combat / WeaponBase.cs
// -----------------------------------------------------------------------------
// Abstract weapon all firearms derive from. Defines the contract for fire,
// reload, aim, animations, and network sync. Supports mods (scope/silencer/
// extended mag) and recoil patterns.
//
// Hitscan vs projectile: most shooter weapons are hitscan (instant ray).
// Throwables/rockets inherit from ProjectileWeapon instead.
//
// Attach to: weapon prefab root (must have a NetworkObject + WeaponBase).
// =============================================================================

using UnityEngine;
using UnityEngine.VFX;
using NeonCity.Networking;
using NeonCity.Player;

namespace NeonCity.Combat
{
    public enum FireMode { Semi, Auto, Burst }

    public abstract class WeaponBase : NetworkBehaviour
    {
        [Header("Stats")]
        public string weaponId   = "pistol_01";
        public float  damage     = 25f;
        public float  range      = 80f;
        public float  fireRate   = 8f;          // rounds per second
        public int    magazineSize = 12;
        public float  reloadTime = 1.5f;
        public FireMode fireMode = FireMode.Semi;

        [Header("Recoil")]
        public float recoilKick   = 1.2f;
        public float recoilSnappiness = 12f;
        public float recoilReturnSpeed = 8f;

        [Header("FX")]
        public GameObject muzzleFlashPrefab;
        public AudioClip   fireClip;
        public AudioClip   reloadClip;
        public VisualEffect muzzleVfx;

        protected PlayerController _owner;
        protected int    _ammo;
        protected float  _fireCooldown;
        protected bool   _reloading;

        public int CurrentAmmo => _ammo;
        public bool IsReloading => _reloading;
        public float Damage => damage;

        // -------------------------------------------------------------------
        public virtual void Initialize(PlayerController owner)
        {
            _owner = owner;
            _ammo  = magazineSize;
        }

        // -------------------------------------------------------------------
        public virtual void OnFireButton(bool pressed)
        {
            if (!pressed) return;
            switch (fireMode)
            {
                case FireMode.Semi:  FireOnce(); break;
                case FireMode.Auto:  TryAutoFire(); break;
                case FireMode.Burst: TryBurstFire(); break;
            }
        }

        protected abstract void FireOnce();

        // -------------------------------------------------------------------
        private float _autoAccumulator;
        private void TryAutoFire()
        {
            _autoAccumulator += Time.deltaTime;
            if (_autoAccumulator >= 1f / fireRate)
            {
                _autoAccumulator = 0;
                FireOnce();
            }
        }

        private int _burstRemaining;
        private float _burstTimer;
        private void TryBurstFire()
        {
            if (_burstRemaining > 0)
            {
                _burstTimer -= Time.deltaTime;
                if (_burstTimer <= 0)
                {
                    FireOnce();
                    _burstRemaining--;
                    _burstTimer = 1f / fireRate;
                }
            }
            else
            {
                _burstRemaining = 3;
                _burstTimer = 0;
                FireOnce();
                _burstRemaining--;
                _burstTimer = 1f / fireRate;
            }
        }

        // -------------------------------------------------------------------
        public virtual void Reload()
        {
            if (_reloading || _ammo == magazineSize) return;
            StartCoroutine(ReloadRoutine());
        }

        private System.Collections.IEnumerator ReloadRoutine()
        {
            _reloading = true;
            yield return new WaitForSeconds(reloadTime);
            _ammo = magazineSize;
            _reloading = false;
        }

        // -------------------------------------------------------------------
        public virtual bool CanFire() =>
            _ammo > 0 && !_reloading && Time.time >= _fireCooldown && _owner != null;

        public virtual void ConsumeAmmo()
        {
            _ammo = Mathf.Max(0, _ammo - 1);
            _fireCooldown = Time.time + 1f / fireRate;
        }

        // -------------------------------------------------------------------
        // Recoil: simple kickback on the weapon and camera-shake hook.
        public virtual void ApplyRecoil()
        {
            var kick = Random.insideUnitSphere * recoilKick;
            transform.localRotation = Quaternion.Euler(kick);
            // Camera shake would call CinemachineImpulseSource.GenerateImpulse
        }

        // -------------------------------------------------------------------
        // Hit registration. Used by hitscan weapons.
        protected bool RaycastFromOwner(out RaycastHit hit)
        {
            var cam = _owner != null ? _owner.GetAimPoint() : transform;
            var ray = new Ray(cam.position, cam.forward);
            return Physics.Raycast(ray, out hit, range, ~0, QueryTriggerInteraction.Ignore);
        }

        public void TryApplyDamage(RaycastHit hit, float dmg)
        {
            var dmgReceiver = hit.collider.GetComponentInParent<IDamageable>();
            if (dmgReceiver != null) dmgReceiver.ApplyDamage(dmg, hit.point, hit.normal);
        }

        protected void SpawnMuzzleFx()
        {
            if (muzzleFlashPrefab == null) return;
            var fx = Instantiate(muzzleFlashPrefab, transform.position, transform.rotation);
            Destroy(fx, 0.05f);
            if (muzzleVfx != null) muzzleVfx.Play();
        }

        protected void PlayFireSound()
        {
            if (fireClip == null) return;
            var src = GetComponent<AudioSource>();
            if (src == null) { src = gameObject.AddComponent<AudioSource>(); src.spatialBlend = 0; }
            src.PlayOneShot(fireClip);
        }
    }

    /// <summary>Anything that can take damage implements this.</summary>
    public interface IDamageable
    {
        void ApplyDamage(float amount, Vector3 point, Vector3 normal);
    }
}