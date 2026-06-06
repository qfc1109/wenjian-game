using System;
using UnityEngine;

namespace Wenjian.Client.Core
{
    public sealed class WorldCoordinateMapper
    {
        public const int DefaultServerUnitsPerTile = 100;
        public const float DefaultTileWorldSize = 1f;
        public static readonly Vector2Int DefaultCombatServerOrigin = new(3200, 2400);

        public WorldCoordinateMapper(
            int serverUnitsPerTile,
            float tileWorldSize,
            Vector2Int serverOrigin,
            Vector2 unityOrigin)
        {
            if (serverUnitsPerTile <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(serverUnitsPerTile), "Server units per tile must be positive.");
            }

            if (tileWorldSize <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(tileWorldSize), "Tile world size must be positive.");
            }

            ServerUnitsPerTile = serverUnitsPerTile;
            TileWorldSize = tileWorldSize;
            ServerOrigin = serverOrigin;
            UnityOrigin = unityOrigin;
        }

        public int ServerUnitsPerTile { get; }

        public float TileWorldSize { get; }

        public Vector2Int ServerOrigin { get; }

        public Vector2 UnityOrigin { get; }

        public static WorldCoordinateMapper CreateDefaultCombatField()
        {
            return new WorldCoordinateMapper(
                DefaultServerUnitsPerTile,
                DefaultTileWorldSize,
                DefaultCombatServerOrigin,
                Vector2.zero);
        }

        public Vector3 ServerToUnity(Vector2Int serverPosition)
        {
            float scale = TileWorldSize / ServerUnitsPerTile;
            float x = UnityOrigin.x + (serverPosition.x - ServerOrigin.x) * scale;
            float y = UnityOrigin.y + (serverPosition.y - ServerOrigin.y) * scale;
            return new Vector3(x, y, 0f);
        }

        public Vector2Int UnityToServer(Vector3 unityPosition)
        {
            float serverUnitsPerWorldUnit = ServerUnitsPerTile / TileWorldSize;
            int x = Mathf.RoundToInt((unityPosition.x - UnityOrigin.x) * serverUnitsPerWorldUnit + ServerOrigin.x);
            int y = Mathf.RoundToInt((unityPosition.y - UnityOrigin.y) * serverUnitsPerWorldUnit + ServerOrigin.y);
            return new Vector2Int(x, y);
        }
    }
}
