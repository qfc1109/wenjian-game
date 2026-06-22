using System;
using System.Collections.Generic;
using UnityEngine;
using Wenjian.Client.Presentation;
using Wenjian.Client.UI;

namespace Wenjian.Client.Net
{
    public sealed class FirstChainHudController : MonoBehaviour
    {
        private readonly Queue<string> outgoingCommands = new();
        private int prototypeWaveEnemyCount = 1;
        private int prototypeWaveDefeatedCount;
        private int prototypeEnemyMaxHp = 100;

        [SerializeField]
        private PrototypeHudPresenter hudPresenter;

        [SerializeField]
        private PrototypeCombatFeedbackPresenter feedbackPresenter;

        [SerializeField]
        private PlayerSpriteAnimator playerVisualAnimator;

        [SerializeField]
        private RogueRoomScenePresenter rogueRoomScenePresenter;

        [SerializeField]
        private int defaultRegionId = 1001;

        public PrototypeHudPresenter HudPresenter
        {
            get => hudPresenter;
            set => hudPresenter = value;
        }

        public PrototypeCombatFeedbackPresenter FeedbackPresenter
        {
            get => feedbackPresenter;
            set => feedbackPresenter = value;
        }

        public PlayerSpriteAnimator PlayerVisualAnimator
        {
            get => playerVisualAnimator;
            set => playerVisualAnimator = value;
        }

        public RogueRoomScenePresenter RogueRoomScenePresenter
        {
            get => rogueRoomScenePresenter;
            set => rogueRoomScenePresenter = value;
        }

        public int DefaultRegionId
        {
            get => defaultRegionId;
            set => defaultRegionId = value;
        }

        public CombatHudSnapshot CurrentSnapshot { get; private set; }

        public bool LastSnapshotMissingEntityDetails { get; private set; }

        public int PrototypeWaveEnemyCount => prototypeWaveEnemyCount;

        public int PrototypeWaveDefeatedCount => prototypeWaveDefeatedCount;

        public event Action<string> OutgoingCommandQueued;

        public event Action<int, int> PrototypeEnemyDefeated;

        public event Action PrototypeRoomCleared;

        public void ApplyOfflineSnapshot()
        {
            CurrentSnapshot = new CombatHudSnapshot
            {
                IsConnected = false,
                PlayerId = 0,
                RegionId = defaultRegionId,
                RogueState = "Field",
                ServerTick = 0,
                ServerX = 3200,
                ServerY = 2400,
                PlayerHp = 100,
                PlayerMaxHp = 100,
                EnemyHp = 100,
                EnemyMaxHp = 100,
                EntityCount = 0,
                RecentEvent = "Client Ready"
            };
            ConfigurePrototypeEnemyWave(1, 100, applySnapshot: false);
            LastSnapshotMissingEntityDetails = false;
            hudPresenter?.ApplySnapshot(CurrentSnapshot);
            rogueRoomScenePresenter?.ApplyFieldState();
        }

        public bool ApplyServerPayload(string payload)
        {
            if (!FirstChainMessage.TryParse(payload, out var message))
            {
                return false;
            }

            switch (message.Type)
            {
                case "LOGIN_OK":
                    ApplyLoginOk(message);
                    return true;
                case "REGION_SNAPSHOT":
                    ApplyRegionSnapshot(message);
                    return true;
                case "SKILL_EVENT":
                    ApplySkillEvent(message);
                    return true;
                case "DAMAGE_EVENT":
                    ApplyDamageEvent(message);
                    return true;
                case "ROGUE_START":
                    ApplyRogueStart(message);
                    return true;
                case "ROGUE_FINISH":
                    ApplyRogueFinish(message);
                    return true;
                default:
                    ApplyEventOnly(message.Type);
                    return true;
            }
        }

        public bool TryDequeueOutgoingCommand(out string command)
        {
            if (outgoingCommands.Count == 0)
            {
                command = null;
                return false;
            }

            command = outgoingCommands.Dequeue();
            return true;
        }

        public bool QueueSkillCommand(int skillId, Vector2Int aimDirection)
        {
            if (CurrentSnapshot.PlayerId <= 0)
            {
                return false;
            }

            int aimX = Mathf.Clamp(aimDirection.x, -1, 1);
            int aimY = Mathf.Clamp(aimDirection.y, -1, 1);
            EnqueueOutgoingCommand(
                FirstChainCommandBuilder.BuildSkill(CurrentSnapshot.PlayerId, skillId, aimX, aimY),
                notify: true);
            return true;
        }

        public bool QueueStartRogueCommand(int rogueId)
        {
            if (CurrentSnapshot.PlayerId <= 0)
            {
                return false;
            }

            EnqueueOutgoingCommand(
                FirstChainCommandBuilder.BuildStartRogue(CurrentSnapshot.PlayerId, rogueId),
                notify: true);
            return true;
        }

        public bool QueueFinishRogueCommand()
        {
            if (CurrentSnapshot.PlayerId <= 0 || CurrentSnapshot.RogueInstanceId <= 0)
            {
                return false;
            }

            EnqueueOutgoingCommand(
                FirstChainCommandBuilder.BuildFinishRogue(CurrentSnapshot.PlayerId, CurrentSnapshot.RogueInstanceId),
                notify: true);
            return true;
        }

        public bool ApplyPrototypeEnemyAttack(int damage)
        {
            if (damage <= 0)
            {
                return false;
            }

            var snapshot = CurrentSnapshot;
            int maxPlayerHp = snapshot.PlayerMaxHp > 0 ? snapshot.PlayerMaxHp : 100;
            int currentPlayerHp = snapshot.PlayerHp > 0 ? snapshot.PlayerHp : maxPlayerHp;
            int hpDelta = -damage;
            snapshot.PlayerMaxHp = maxPlayerHp;
            snapshot.PlayerHp = Mathf.Clamp(currentPlayerHp + hpDelta, 0, maxPlayerHp);
            snapshot.RecentEvent = $"ENEMY_ATTACK hpDelta={hpDelta}";
            CurrentSnapshot = snapshot;
            playerVisualAnimator?.PlayHit();
            hudPresenter?.ApplySnapshot(CurrentSnapshot);
            return true;
        }

        public void ConfigurePrototypeEnemyWave(int enemyCount, int enemyMaxHp)
        {
            ConfigurePrototypeEnemyWave(enemyCount, enemyMaxHp, applySnapshot: true);
        }

        public void ApplyPrototypeRewardChoice(int rewardItemId, string rewardSummary)
        {
            var snapshot = CurrentSnapshot;
            snapshot.RewardItemId = Mathf.Max(0, rewardItemId);
            snapshot.RewardCount = rewardItemId > 0 ? 1 : 0;
            snapshot.PrototypeRewardSummary = string.IsNullOrWhiteSpace(rewardSummary) ? string.Empty : rewardSummary;
            snapshot.RecentEvent = string.IsNullOrWhiteSpace(snapshot.PrototypeRewardSummary)
                ? "REWARD_CHOSEN"
                : $"REWARD_CHOSEN {snapshot.PrototypeRewardSummary}";
            CurrentSnapshot = snapshot;
            hudPresenter?.ApplySnapshot(CurrentSnapshot);
        }

        public bool ApplyPrototypeSkillHit(int skillId, Vector2Int aimDirection, int damage)
        {
            int safeSkillId = Mathf.Max(1, skillId);
            int safeDamage = Mathf.Max(1, damage);
            int aimX = Mathf.Clamp(aimDirection.x, -1, 1);
            int aimY = Mathf.Clamp(aimDirection.y, -1, 1);
            if (aimX == 0 && aimY == 0)
            {
                aimY = -1;
            }

            long playerId = CurrentSnapshot.PlayerId > 0 ? CurrentSnapshot.PlayerId : 1000001L;
            long targetId = 2000001L + Mathf.Max(0, prototypeWaveDefeatedCount);
            bool skillApplied = ApplyServerPayload(
                $"type=SKILL_EVENT code=LOCAL casterId={playerId} skillId={safeSkillId} x={CurrentSnapshot.ServerX} y={CurrentSnapshot.ServerY} aimX={aimX} aimY={aimY}");
            bool damageApplied = ApplyServerPayload(
                $"type=DAMAGE_EVENT code=LOCAL sourceId={playerId} targetId={targetId} hpDelta=-{safeDamage}");
            return skillApplied && damageApplied;
        }

        public void ApplyPrototypeNextRoom(int roomIndex, int monsterCount, int enemyMaxHp = 100)
        {
            int safeRoomIndex = Mathf.Max(1, roomIndex);
            int safeMonsterCount = Mathf.Max(1, monsterCount);
            ConfigurePrototypeEnemyWave(safeMonsterCount, enemyMaxHp, applySnapshot: false);
            feedbackPresenter?.ResetEnemyState();

            var snapshot = CurrentSnapshot;
            snapshot.RogueId = snapshot.RogueId > 0 ? snapshot.RogueId : 4001;
            snapshot.RogueInstanceId = snapshot.RogueInstanceId > 0 ? snapshot.RogueInstanceId : 9000001L;
            snapshot.RogueMapId = 2000 + safeRoomIndex;
            snapshot.RogueRewardPoolId = snapshot.RogueRewardPoolId > 0 ? snapshot.RogueRewardPoolId : 5001;
            snapshot.RogueState = $"Room {safeRoomIndex}";
            snapshot.RogueMonsterCount = safeMonsterCount;
            snapshot.EnemyMaxHp = prototypeEnemyMaxHp;
            snapshot.EnemyHp = prototypeEnemyMaxHp;
            snapshot.RecentEvent = $"NEXT_ROOM room={safeRoomIndex} monsters={safeMonsterCount}";
            CurrentSnapshot = snapshot;
            hudPresenter?.ApplySnapshot(CurrentSnapshot);
        }

        public void ApplyConnectionFailure(string reason)
        {
            var snapshot = CurrentSnapshot;
            snapshot.IsConnected = false;
            snapshot.RecentEvent = string.IsNullOrWhiteSpace(reason) ? "Disconnected" : reason;
            CurrentSnapshot = snapshot;
            hudPresenter?.ApplySnapshot(CurrentSnapshot);
        }

        private void ApplyLoginOk(FirstChainMessage message)
        {
            long playerId = message.GetLong("playerId");
            int regionId = message.Has("regionId") ? message.GetInt("regionId") : defaultRegionId;

            CurrentSnapshot = new CombatHudSnapshot
            {
                IsConnected = true,
                PlayerId = playerId,
                RegionId = regionId,
                RogueState = "Field",
                ServerTick = CurrentSnapshot.ServerTick,
                ServerX = message.Has("x") ? message.GetInt("x") : CurrentSnapshot.ServerX,
                ServerY = message.Has("y") ? message.GetInt("y") : CurrentSnapshot.ServerY,
                PlayerHp = CurrentSnapshot.PlayerMaxHp > 0 ? CurrentSnapshot.PlayerHp : 100,
                PlayerMaxHp = CurrentSnapshot.PlayerMaxHp > 0 ? CurrentSnapshot.PlayerMaxHp : 100,
                EnemyHp = CurrentSnapshot.EnemyMaxHp > 0 ? CurrentSnapshot.EnemyHp : 100,
                EnemyMaxHp = CurrentSnapshot.EnemyMaxHp > 0 ? CurrentSnapshot.EnemyMaxHp : 100,
                EntityCount = CurrentSnapshot.EntityCount,
                RecentEvent = "LOGIN_OK"
            };
            LastSnapshotMissingEntityDetails = false;
            hudPresenter?.ApplySnapshot(CurrentSnapshot);
            EnqueueOutgoingCommand(FirstChainCommandBuilder.BuildEnterRegion(playerId, regionId), notify: false);
        }

        private void ApplyRegionSnapshot(FirstChainMessage message)
        {
            int entityCount = message.Has("entities") ? message.GetInt("entities") : 0;
            var snapshot = CurrentSnapshot;
            snapshot.IsConnected = true;
            snapshot.RegionId = message.Has("regionId") ? message.GetInt("regionId") : snapshot.RegionId;
            snapshot.PlayerId = message.Has("self") ? message.GetLong("self") : snapshot.PlayerId;
            snapshot.ServerTick = message.Has("serverTick") ? message.GetLong("serverTick") : snapshot.ServerTick;
            snapshot.EntityCount = entityCount;
            snapshot.RecentEvent = $"REGION_SNAPSHOT entities={entityCount}";
            CurrentSnapshot = snapshot;
            LastSnapshotMissingEntityDetails = !HasEntityDetailFields(message);
            hudPresenter?.ApplySnapshot(CurrentSnapshot);
        }

        private void ApplySkillEvent(FirstChainMessage message)
        {
            feedbackPresenter?.ApplySkillEvent(message);
            ApplyEventOnly("SKILL_EVENT");
        }

        private void ApplyDamageEvent(FirstChainMessage message)
        {
            int hpDelta = message.Has("hpDelta") ? message.GetInt("hpDelta") : 0;
            var snapshot = CurrentSnapshot;
            long targetId = message.Has("targetId") ? message.GetLong("targetId") : 0L;
            bool hitsPlayer = targetId > 0L && snapshot.PlayerId > 0L && targetId == snapshot.PlayerId;
            if (hitsPlayer)
            {
                int maxPlayerHp = snapshot.PlayerMaxHp > 0 ? snapshot.PlayerMaxHp : 100;
                int currentPlayerHp = snapshot.PlayerHp > 0 ? snapshot.PlayerHp : maxPlayerHp;
                snapshot.PlayerMaxHp = maxPlayerHp;
                snapshot.PlayerHp = Mathf.Clamp(currentPlayerHp + hpDelta, 0, maxPlayerHp);
                playerVisualAnimator?.PlayHit();
            }
            else
            {
                int maxHp = snapshot.EnemyMaxHp > 0 ? snapshot.EnemyMaxHp : 100;
                int currentHp = snapshot.EnemyHp > 0 ? snapshot.EnemyHp : maxHp;
                snapshot.EnemyMaxHp = maxHp;
                snapshot.EnemyHp = Mathf.Clamp(currentHp + hpDelta, 0, maxHp);
                feedbackPresenter?.ApplyDamageEvent(message);
            }

            snapshot.RecentEvent = $"DAMAGE_EVENT hpDelta={hpDelta}";
            if (!hitsPlayer && snapshot.EnemyHp <= 0)
            {
                HandlePrototypeEnemyDefeated(ref snapshot);
                return;
            }

            CurrentSnapshot = snapshot;
            hudPresenter?.ApplySnapshot(CurrentSnapshot);
        }

        private void ApplyRogueStart(FirstChainMessage message)
        {
            var snapshot = CurrentSnapshot;
            int rogueId = message.Has("rogueId") ? message.GetInt("rogueId") : 0;
            long instanceId = message.Has("instanceId") ? message.GetLong("instanceId") : 0L;
            int monsters = message.Has("monsters") ? message.GetInt("monsters") : 0;
            ConfigurePrototypeEnemyWave(monsters > 0 ? monsters : 1, snapshot.EnemyMaxHp > 0 ? snapshot.EnemyMaxHp : 100, applySnapshot: false);
            snapshot.IsConnected = true;
            snapshot.RogueState = rogueId > 0 ? $"Rogue {rogueId}" : "Rogue";
            snapshot.RogueId = rogueId;
            snapshot.RogueInstanceId = instanceId;
            snapshot.RogueMapId = message.Has("mapId") ? message.GetInt("mapId") : 0;
            snapshot.RogueMonsterCount = monsters;
            snapshot.EnemyMaxHp = prototypeEnemyMaxHp;
            snapshot.EnemyHp = prototypeEnemyMaxHp;
            snapshot.RogueRewardPoolId = message.Has("rewardPoolId") ? message.GetInt("rewardPoolId") : 0;
            snapshot.ServerX = message.Has("x") ? message.GetInt("x") : snapshot.ServerX;
            snapshot.ServerY = message.Has("y") ? message.GetInt("y") : snapshot.ServerY;
            snapshot.EntityCount = message.Has("entities") ? message.GetInt("entities") : snapshot.EntityCount;
            snapshot.RewardItemId = 0;
            snapshot.RewardCount = 0;
            snapshot.PrototypeRewardSummary = string.Empty;
            snapshot.RecentEvent = $"ROGUE_START instance={instanceId} monsters={monsters}";
            CurrentSnapshot = snapshot;
            hudPresenter?.ApplySnapshot(CurrentSnapshot);
            rogueRoomScenePresenter?.ApplyRogueStart(message);
        }

        private void ApplyRogueFinish(FirstChainMessage message)
        {
            int itemId = message.Has("itemId") ? message.GetInt("itemId") : 0;
            int count = message.Has("count") ? message.GetInt("count") : 0;
            bool success = !message.Has("success") || message.GetString("success") == "true";

            var snapshot = CurrentSnapshot;
            snapshot.IsConnected = true;
            snapshot.RogueState = success ? "Finished" : "Failed";
            snapshot.RogueInstanceId = message.Has("instanceId") ? message.GetLong("instanceId") : snapshot.RogueInstanceId;
            snapshot.RewardItemId = itemId;
            snapshot.RewardCount = count;
            snapshot.PrototypeRewardSummary = string.Empty;
            snapshot.RecentEvent = itemId > 0 && count > 0
                ? $"ROGUE_FINISH item={itemId} x{count}"
                : "ROGUE_FINISH";
            CurrentSnapshot = snapshot;
            hudPresenter?.ApplySnapshot(CurrentSnapshot);
            rogueRoomScenePresenter?.ApplyRogueFinish(message);
        }

        private void ApplyEventOnly(string eventType)
        {
            var snapshot = CurrentSnapshot;
            snapshot.RecentEvent = eventType;
            CurrentSnapshot = snapshot;
            hudPresenter?.ApplySnapshot(CurrentSnapshot);
        }

        private void EnqueueOutgoingCommand(string command, bool notify)
        {
            outgoingCommands.Enqueue(command);
            if (notify)
            {
                OutgoingCommandQueued?.Invoke(command);
            }
        }

        private void ConfigurePrototypeEnemyWave(int enemyCount, int enemyMaxHp, bool applySnapshot)
        {
            prototypeWaveEnemyCount = Mathf.Max(1, enemyCount);
            prototypeWaveDefeatedCount = 0;
            prototypeEnemyMaxHp = Mathf.Max(1, enemyMaxHp);
            if (!applySnapshot)
            {
                return;
            }

            var snapshot = CurrentSnapshot;
            snapshot.RogueMonsterCount = prototypeWaveEnemyCount;
            snapshot.EnemyMaxHp = prototypeEnemyMaxHp;
            snapshot.EnemyHp = prototypeEnemyMaxHp;
            CurrentSnapshot = snapshot;
            hudPresenter?.ApplySnapshot(CurrentSnapshot);
        }

        private void HandlePrototypeEnemyDefeated(ref CombatHudSnapshot snapshot)
        {
            int defeatedIndex = prototypeWaveDefeatedCount;
            prototypeWaveDefeatedCount = Mathf.Min(prototypeWaveDefeatedCount + 1, prototypeWaveEnemyCount);
            int remaining = Mathf.Max(0, prototypeWaveEnemyCount - prototypeWaveDefeatedCount);

            feedbackPresenter?.ApplyEnemyDefeated();
            PrototypeEnemyDefeated?.Invoke(defeatedIndex, remaining);

            if (remaining > 0)
            {
                snapshot.RogueMonsterCount = remaining;
                snapshot.EnemyMaxHp = prototypeEnemyMaxHp;
                snapshot.EnemyHp = prototypeEnemyMaxHp;
                snapshot.RecentEvent = $"ENEMY_DEFEATED remaining={remaining}";
                CurrentSnapshot = snapshot;
                hudPresenter?.ApplySnapshot(CurrentSnapshot);
                return;
            }

            snapshot.RogueMonsterCount = 0;
            snapshot.EnemyHp = 0;
            snapshot.RogueState = "Cleared";
            snapshot.RecentEvent = "ROOM_CLEARED";
            CurrentSnapshot = snapshot;
            rogueRoomScenePresenter?.ApplyRoomCleared();
            hudPresenter?.ApplySnapshot(CurrentSnapshot);
            PrototypeRoomCleared?.Invoke();
        }

        private static bool HasEntityDetailFields(FirstChainMessage message)
        {
            return message.Has("entityType")
                || message.Has("visualId")
                || message.Has("hp")
                || message.Has("maxHp")
                || message.Has("state")
                || message.Has("facing");
        }
    }
}
