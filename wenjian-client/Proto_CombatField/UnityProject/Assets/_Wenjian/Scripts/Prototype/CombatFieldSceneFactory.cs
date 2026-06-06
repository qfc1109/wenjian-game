using UnityEngine;
using UnityEngine.Tilemaps;
using Wenjian.Client.Core;
using Wenjian.Client.Net;
using Wenjian.Client.Presentation;
using Wenjian.Client.UI;

namespace Wenjian.Client.Prototype
{
    public sealed class CombatFieldSceneAssets
    {
        public TileBase GroundTile { get; set; }

        public TileBase BoundaryTile { get; set; }

        public TileBase ObstacleTile { get; set; }

        public Sprite PlayerSprite { get; set; }

        public Sprite TrainingEnemySprite { get; set; }
    }

    public sealed class CombatFieldSceneObjects
    {
        public GameObject Root { get; set; }

        public Grid Grid { get; set; }

        public Tilemap GroundTilemap { get; set; }

        public Tilemap BoundaryTilemap { get; set; }

        public Tilemap ObstacleTilemap { get; set; }

        public Camera MainCamera { get; set; }

        public GameObject Player { get; set; }

        public GameObject TrainingEnemy { get; set; }

        public GameObject Hud { get; set; }
    }

    public static class CombatFieldSceneFactory
    {
        private const string RootName = "Proto_CombatField";
        private const int WidthInTiles = 32;
        private const int HeightInTiles = 24;
        private static readonly Vector2Int PlayerSpawn = new(3200, 2400);
        private static readonly Vector2Int TrainingEnemySpawn = new(3600, 2400);

        public static CombatFieldSceneObjects BuildDefault(CombatFieldSceneAssets assets)
        {
            assets ??= new CombatFieldSceneAssets();
            DestroyExistingRoot();

            var mapper = WorldCoordinateMapper.CreateDefaultCombatField();
            var root = new GameObject(RootName);
            var grid = CreateGrid(root.transform);
            var groundTilemap = CreateTilemap(grid.transform, "GroundTilemap", sortingOrder: 0);
            var boundaryTilemap = CreateTilemap(grid.transform, "BoundaryTilemap", sortingOrder: 1);
            var obstacleTilemap = CreateTilemap(grid.transform, "ObstacleTilemap", sortingOrder: 2);

            FillGround(groundTilemap, assets.GroundTile);
            FillBoundaries(boundaryTilemap, assets.BoundaryTile);
            FillObstacles(obstacleTilemap, assets.ObstacleTile);

            boundaryTilemap.gameObject.AddComponent<TilemapCollider2D>();
            obstacleTilemap.gameObject.AddComponent<TilemapCollider2D>();

            var camera = CreateMainCamera(root.transform);
            var player = CreateActor(
                root.transform,
                "Player_1000001",
                assets.PlayerSprite,
                new Color(0.22f, 0.66f, 0.95f),
                mapper.ServerToUnity(PlayerSpawn),
                sortingOrder: 10,
                colliderRadius: 0.38f);
            var enemy = CreateActor(
                root.transform,
                "TrainingEnemy_2000001",
                assets.TrainingEnemySprite,
                new Color(0.84f, 0.25f, 0.22f),
                mapper.ServerToUnity(TrainingEnemySpawn),
                sortingOrder: 9,
                colliderRadius: 0.42f);

            player.AddComponent<LocalPlayerMotor>().MoveSpeedWorldUnitsPerSecond = 4f;
            var cameraFollow = camera.gameObject.AddComponent<CameraFollow2D>();
            cameraFollow.Target = player.transform;
            cameraFollow.SnapToTarget();
            var hud = CreateHud(camera.transform);
            CreateFirstChainClient(root.transform, hud.GetComponent<PrototypeHudPresenter>());

            return new CombatFieldSceneObjects
            {
                Root = root,
                Grid = grid,
                GroundTilemap = groundTilemap,
                BoundaryTilemap = boundaryTilemap,
                ObstacleTilemap = obstacleTilemap,
                MainCamera = camera,
                Player = player,
                TrainingEnemy = enemy,
                Hud = hud
            };
        }

        private static Grid CreateGrid(Transform parent)
        {
            var gridObject = new GameObject("Grid");
            gridObject.transform.SetParent(parent);
            var grid = gridObject.AddComponent<Grid>();
            grid.cellSize = Vector3.one;
            return grid;
        }

        private static Tilemap CreateTilemap(Transform parent, string name, int sortingOrder)
        {
            var tilemapObject = new GameObject(name);
            tilemapObject.transform.SetParent(parent);
            var tilemap = tilemapObject.AddComponent<Tilemap>();
            tilemapObject.AddComponent<TilemapRenderer>().sortingOrder = sortingOrder;
            return tilemap;
        }

        private static void FillGround(Tilemap tilemap, TileBase tile)
        {
            for (int x = -WidthInTiles / 2; x < WidthInTiles / 2; x++)
            {
                for (int y = -HeightInTiles / 2; y < HeightInTiles / 2; y++)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }

        private static void FillBoundaries(Tilemap tilemap, TileBase tile)
        {
            int minX = -WidthInTiles / 2;
            int maxX = WidthInTiles / 2 - 1;
            int minY = -HeightInTiles / 2;
            int maxY = HeightInTiles / 2 - 1;

            for (int x = minX; x <= maxX; x++)
            {
                tilemap.SetTile(new Vector3Int(x, minY, 0), tile);
                tilemap.SetTile(new Vector3Int(x, maxY, 0), tile);
            }

            for (int y = minY; y <= maxY; y++)
            {
                tilemap.SetTile(new Vector3Int(minX, y, 0), tile);
                tilemap.SetTile(new Vector3Int(maxX, y, 0), tile);
            }
        }

        private static void FillObstacles(Tilemap tilemap, TileBase tile)
        {
            SetObstacleCluster(tilemap, tile, new Vector2Int(-6, 4));
            SetObstacleCluster(tilemap, tile, new Vector2Int(5, -3));
            SetObstacleCluster(tilemap, tile, new Vector2Int(8, 5));
        }

        private static void SetObstacleCluster(Tilemap tilemap, TileBase tile, Vector2Int center)
        {
            tilemap.SetTile(new Vector3Int(center.x, center.y, 0), tile);
            tilemap.SetTile(new Vector3Int(center.x + 1, center.y, 0), tile);
            tilemap.SetTile(new Vector3Int(center.x, center.y + 1, 0), tile);
        }

        private static Camera CreateMainCamera(Transform parent)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.transform.SetParent(parent);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.tag = "MainCamera";

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.12f, 0.10f);
            return camera;
        }

        private static GameObject CreateHud(Transform cameraTransform)
        {
            var hud = new GameObject("HUD");
            hud.transform.SetParent(cameraTransform);
            hud.transform.localPosition = new Vector3(0f, 0f, 10f);
            hud.transform.localRotation = Quaternion.identity;
            hud.transform.localScale = Vector3.one;

            var presenter = hud.AddComponent<PrototypeHudPresenter>();
            var statusPanel = CreateHudNode(hud.transform, "StatusPanel", new Vector3(-8.3f, 5.8f, 0f));
            presenter.ConnectionText = CreateHudText(statusPanel.transform, "ConnectionStatusText", Vector3.zero, 0.13f);
            presenter.PlayerText = CreateHudText(statusPanel.transform, "PlayerStatusText", new Vector3(0f, -0.42f, 0f), 0.11f);
            presenter.RegionText = CreateHudText(statusPanel.transform, "RegionStatusText", new Vector3(0f, -0.75f, 0f), 0.10f);
            presenter.PlayerHealthFill = CreateHudBar(statusPanel.transform, "PlayerHealthBar", new Vector3(0.72f, -0.55f, 0f), new Color(0.34f, 0.86f, 0.48f));
            presenter.EnemyHealthFill = CreateHudBar(statusPanel.transform, "EnemyHealthBar", new Vector3(0.72f, -1.08f, 0f), new Color(0.92f, 0.28f, 0.22f));

            var debugPanel = CreateHudNode(hud.transform, "DebugPanel", new Vector3(-8.3f, -5.45f, 0f));
            presenter.DebugText = CreateHudText(debugPanel.transform, "DebugText", Vector3.zero, 0.10f);
            presenter.RecentEventText = CreateHudText(debugPanel.transform, "RecentEventText", new Vector3(0f, -0.33f, 0f), 0.10f);

            presenter.ApplySnapshot(new CombatHudSnapshot
            {
                IsConnected = false,
                PlayerId = 1000001,
                RegionId = 1001,
                RogueState = "Field",
                ServerTick = 0,
                ServerX = 3200,
                ServerY = 2400,
                PlayerHp = 100,
                PlayerMaxHp = 100,
                EnemyHp = 100,
                EnemyMaxHp = 100,
                RecentEvent = "Prototype Ready"
            });

            return hud;
        }

        private static GameObject CreateFirstChainClient(Transform parent, PrototypeHudPresenter presenter)
        {
            var clientObject = new GameObject("FirstChainClient");
            clientObject.transform.SetParent(parent);

            var hudController = clientObject.AddComponent<FirstChainHudController>();
            hudController.HudPresenter = presenter;
            hudController.DefaultRegionId = 1001;
            hudController.ApplyOfflineSnapshot();

            var wsClient = clientObject.AddComponent<FirstChainWsClient>();
            wsClient.HudController = hudController;
            wsClient.Endpoint = FirstChainWsClient.DefaultEndpoint;
            return clientObject;
        }

        private static Transform CreateHudNode(Transform parent, string name, Vector3 localPosition)
        {
            var node = new GameObject(name);
            node.transform.SetParent(parent);
            node.transform.localPosition = localPosition;
            node.transform.localRotation = Quaternion.identity;
            node.transform.localScale = Vector3.one;
            return node.transform;
        }

        private static TextMesh CreateHudText(Transform parent, string name, Vector3 localPosition, float characterSize)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent);
            textObject.transform.localPosition = localPosition;
            textObject.transform.localRotation = Quaternion.identity;
            textObject.transform.localScale = Vector3.one;

            var text = textObject.AddComponent<TextMesh>();
            text.anchor = TextAnchor.UpperLeft;
            text.alignment = TextAlignment.Left;
            text.characterSize = characterSize;
            text.fontSize = 42;
            text.color = new Color(0.92f, 0.96f, 0.90f);
            text.GetComponent<MeshRenderer>().sortingOrder = 120;
            return text;
        }

        private static Transform CreateHudBar(Transform parent, string name, Vector3 localPosition, Color fillColor)
        {
            var bar = CreateHudNode(parent, name, localPosition);
            CreateHudQuad(bar, "Background", Vector3.zero, new Vector3(1.55f, 0.12f, 1f), new Color(0.04f, 0.05f, 0.05f, 0.90f), 110);
            return CreateHudQuad(bar, "Fill", new Vector3(0f, 0f, -0.01f), new Vector3(1.45f, 0.08f, 1f), fillColor, 111);
        }

        private static Transform CreateHudQuad(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color, int sortingOrder)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent);
            quad.transform.localPosition = localPosition;
            quad.transform.localRotation = Quaternion.identity;
            quad.transform.localScale = localScale;

            var collider = quad.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"))
            {
                color = color
            };
            renderer.sortingOrder = sortingOrder;
            return quad.transform;
        }

        private static GameObject CreateActor(
            Transform parent,
            string name,
            Sprite sprite,
            Color color,
            Vector3 position,
            int sortingOrder,
            float colliderRadius)
        {
            var actor = new GameObject(name);
            actor.transform.SetParent(parent);
            actor.transform.position = position;

            var renderer = actor.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            var collider = actor.AddComponent<CircleCollider2D>();
            collider.radius = colliderRadius;
            return actor;
        }

        private static void DestroyExistingRoot()
        {
            var existing = GameObject.Find(RootName);
            if (existing == null)
            {
                return;
            }

            Object.DestroyImmediate(existing);
        }
    }
}
