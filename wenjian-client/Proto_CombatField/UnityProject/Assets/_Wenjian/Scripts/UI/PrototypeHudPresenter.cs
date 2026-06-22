using UnityEngine;

namespace Wenjian.Client.UI
{
    public struct CombatHudSnapshot
    {
        public bool IsConnected;

        public long PlayerId;

        public int RegionId;

        public string RogueState;

        public long ServerTick;

        public int ServerX;

        public int ServerY;

        public int PlayerHp;

        public int PlayerMaxHp;

        public int EnemyHp;

        public int EnemyMaxHp;

        public int EntityCount;

        public int RogueId;

        public long RogueInstanceId;

        public int RogueMapId;

        public int RogueMonsterCount;

        public int RogueRewardPoolId;

        public int RewardItemId;

        public int RewardCount;

        public string PrototypeRewardSummary;

        public string RecentEvent;
    }

    public sealed class PrototypeHudPresenter : MonoBehaviour
    {
        public TextMesh ConnectionText;

        public TextMesh PlayerText;

        public TextMesh RegionText;

        public TextMesh DebugText;

        public TextMesh RecentEventText;

        public TextMesh RogueDetailText;

        public TextMesh RewardText;

        public Transform PlayerHealthFill;

        public Transform EnemyHealthFill;

        public Transform EnemyWorldHealthFill;

        public void ApplySnapshot(CombatHudSnapshot snapshot)
        {
            SetText(ConnectionText, snapshot.IsConnected ? "Conn: Online" : "Conn: Offline");
            SetText(PlayerText, $"Player {FormatId(snapshot.PlayerId)}  HP {snapshot.PlayerHp}/{snapshot.PlayerMaxHp}");
            SetText(RegionText, $"Region {FormatId(snapshot.RegionId)}  Rogue {FormatText(snapshot.RogueState)}");
            string entitySummary = snapshot.EntityCount > 0 ? $"  Ent {snapshot.EntityCount}" : string.Empty;
            SetText(DebugText, $"Tick {snapshot.ServerTick}  Pos {snapshot.ServerX},{snapshot.ServerY}{entitySummary}");
            SetText(RecentEventText, $"Event: {FormatText(snapshot.RecentEvent)}");
            SetText(RogueDetailText, FormatRogueDetail(snapshot));
            SetText(RewardText, FormatReward(snapshot));
            SetFillScale(PlayerHealthFill, ToRatio(snapshot.PlayerHp, snapshot.PlayerMaxHp));
            SetFillScale(EnemyHealthFill, ToRatio(snapshot.EnemyHp, snapshot.EnemyMaxHp));
            SetFillScale(EnemyWorldHealthFill, ToRatio(snapshot.EnemyHp, snapshot.EnemyMaxHp));
        }

        private static void SetText(TextMesh textMesh, string value)
        {
            if (textMesh != null)
            {
                textMesh.text = value;
            }
        }

        private static string FormatId(long value)
        {
            return value > 0 ? value.ToString() : "--";
        }

        private static string FormatText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "--" : value;
        }

        private static string FormatRogueDetail(CombatHudSnapshot snapshot)
        {
            if (snapshot.RogueId <= 0 || snapshot.RogueInstanceId <= 0)
            {
                return "Rogue --";
            }

            return $"Rogue {snapshot.RogueId}  Inst {snapshot.RogueInstanceId}  Map {snapshot.RogueMapId}  Monsters {snapshot.RogueMonsterCount}  Pool {snapshot.RogueRewardPoolId}";
        }

        private static string FormatReward(CombatHudSnapshot snapshot)
        {
            if (!string.IsNullOrWhiteSpace(snapshot.PrototypeRewardSummary))
            {
                return $"Reward {snapshot.PrototypeRewardSummary}";
            }

            if (snapshot.RewardItemId <= 0 || snapshot.RewardCount <= 0)
            {
                return "Reward --";
            }

            return $"Reward item {snapshot.RewardItemId} x{snapshot.RewardCount}";
        }

        private static float ToRatio(int current, int max)
        {
            if (max <= 0)
            {
                return 0f;
            }

            return Mathf.Clamp01((float)current / max);
        }

        private static void SetFillScale(Transform fill, float ratio)
        {
            if (fill == null)
            {
                return;
            }

            Vector3 scale = fill.localScale;
            scale.x = ratio;
            fill.localScale = scale;
        }
    }
}
