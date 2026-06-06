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
    }
}
