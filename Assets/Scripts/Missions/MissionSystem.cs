// =============================================================================
// NeonCity — Missions / MissionSystem.cs
// -----------------------------------------------------------------------------
// Mission/storyline framework. Each mission is a ScriptableObject chain of
// objectives (kill, escort, hack, drive). Progress is tracked per save and
// synced to the backend for daily/weekly missions.
//
// Flow:
//   1. MissionDefinition (designer-authored) lists MissionObjective[].
//   2. MissionSystem.Instance.StartMission(id) -> activates tracker.
//   3. Game events call NotifyEvent("killed", targetId), which the system
//      matches against active objectives and updates progress.
//   4. When all objectives complete -> reward granted, mission ended.
// =============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonCity.Missions
{
    public enum ObjectiveType { Kill, Collect, Reach, Interact, Escort, Survive }

    [Serializable]
    public class MissionObjective
    {
        public string       id;
        public string       description;
        public ObjectiveType type;
        public string       targetTag;     // e.g. "Enemy.Gang_Lowlife"
        public int          requiredCount  = 1;
        public int          currentCount;
        public bool         isComplete => currentCount >= requiredCount;
    }

    [CreateAssetMenu(fileName = "Mission_", menuName = "NeonCity/Mission")]
    public class MissionDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string briefing;
        public MissionObjective[] objectives;
        public int xpReward;
        public int creditReward;
    }

    public class MissionSystem : MonoBehaviour
    {
        public static MissionSystem Instance { get; private set; }

        public MissionDefinition activeMission;
        public int              activeObjectiveIndex;
        public event Action<MissionObjective> OnObjectiveProgress;
        public event Action<MissionDefinition> OnMissionComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // -------------------------------------------------------------------
        public void StartMission(MissionDefinition def)
        {
            activeMission = def;
            activeObjectiveIndex = 0;
            ResetCounters(def);
            Debug.Log($"[Mission] Started: {def.displayName}");
        }

        public void NotifyEvent(string evt, string target = null)
        {
            if (activeMission == null) return;
            if (activeObjectiveIndex >= activeMission.objectives.Length) return;
            var obj = activeMission.objectives[activeObjectiveIndex];

            if (MatchesEvent(obj, evt, target))
            {
                obj.currentCount++;
                OnObjectiveProgress?.Invoke(obj);
                if (obj.isComplete) AdvanceObjective();
            }
        }

        private bool MatchesEvent(MissionObjective obj, string evt, string target)
        {
            // Mapping: evt == "killed" & targetTag == victim.tag
            if (obj.type == ObjectiveType.Kill && evt == "killed")
                return target == obj.targetTag;
            if (obj.type == ObjectiveType.Collect && evt == "collected")
                return target == obj.targetTag;
            if (obj.type == ObjectiveType.Reach && evt == "reached")
                return target == obj.targetTag;
            return false;
        }

        private void AdvanceObjective()
        {
            activeObjectiveIndex++;
            if (activeObjectiveIndex >= activeMission.objectives.Length)
                CompleteMission();
        }

        private void CompleteMission()
        {
            Debug.Log($"[Mission] Complete: {activeMission.displayName}");
            OnMissionComplete?.Invoke(activeMission);
            // Hook into XPService.AddXp, ShopService.Credits += creditReward
            activeMission = null;
        }

        private void ResetCounters(MissionDefinition def)
        {
            foreach (var o in def.objectives) o.currentCount = 0;
        }
    }
}