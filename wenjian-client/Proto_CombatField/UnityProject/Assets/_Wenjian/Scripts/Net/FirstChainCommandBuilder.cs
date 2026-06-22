namespace Wenjian.Client.Net
{
    public static class FirstChainCommandBuilder
    {
        public static string BuildLogin(string key, long clientTimeMs)
        {
            return $"LOGIN {key} {clientTimeMs}";
        }

        public static string BuildEnterRegion(long playerId, int regionId)
        {
            return $"ENTER_REGION {playerId} {regionId}";
        }

        public static string BuildSkill(long playerId, int skillId, int aimX, int aimY)
        {
            return $"SKILL {playerId} {skillId} {aimX} {aimY}";
        }

        public static string BuildStartRogue(long playerId, int rogueId)
        {
            return $"START_ROGUE {playerId} {rogueId}";
        }

        public static string BuildFinishRogue(long playerId, long instanceId)
        {
            return $"FINISH_ROGUE {playerId} {instanceId}";
        }
    }
}
