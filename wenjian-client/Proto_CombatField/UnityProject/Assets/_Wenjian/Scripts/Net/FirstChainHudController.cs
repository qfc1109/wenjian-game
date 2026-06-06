using System.Collections.Generic;
using UnityEngine;
using Wenjian.Client.UI;

namespace Wenjian.Client.Net
{
    public sealed class FirstChainHudController : MonoBehaviour
    {
        private readonly Queue<string> outgoingCommands = new();

        [SerializeField]
        private PrototypeHudPresenter hudPresenter;

        [SerializeField]
        private int defaultRegionId = 1001;

        public PrototypeHudPresenter HudPresenter
        {
            get => hudPresenter;
            set => hudPresenter = value;
        }

        public int DefaultRegionId
        {
            get => defaultRegionId;
            set => defaultRegionId = value;
        }

        public CombatHudSnapshot CurrentSnapshot { get; private set; }

        public bool LastSnapshotMissingEntityDetails { get; private set; }

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
            LastSnapshotMissingEntityDetails = false;
            hudPresenter?.ApplySnapshot(CurrentSnapshot);
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
            outgoingCommands.Enqueue(FirstChainCommandBuilder.BuildEnterRegion(playerId, regionId));
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

        private void ApplyEventOnly(string eventType)
        {
            var snapshot = CurrentSnapshot;
            snapshot.RecentEvent = eventType;
            CurrentSnapshot = snapshot;
            hudPresenter?.ApplySnapshot(CurrentSnapshot);
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
