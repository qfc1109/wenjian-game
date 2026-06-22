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

        public PlayerSpriteAnimationSet PlayerAnimationSet { get; set; }

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

        public GameObject[] TrainingEnemies { get; set; }

        public GameObject Hud { get; set; }
    }

    public static class CombatFieldSceneFactory
    {
        private const string RootName = "Proto_CombatField";
        private const int WidthInTiles = 32;
        private const int HeightInTiles = 24;
        private static readonly Vector2Int PlayerSpawn = new(3200, 2400);
        private static readonly Vector2Int TrainingEnemySpawn = new(3600, 2400);
        private static readonly Vector2Int[] TrainingEnemySpawns =
        {
            TrainingEnemySpawn,
            new(3740, 2510),
            new(3740, 2290)
        };
        private static readonly Vector2 PlayerWorldBoundsMin = new(-14.5f, -10.5f);
        private static readonly Vector2 PlayerWorldBoundsMax = new(13.5f, 9.5f);
        private static readonly Rect[] TrainingEnemyObstacleRects =
        {
            new(-6f, 4f, 2f, 2f),
            new(5f, -3f, 2f, 2f),
            new(8f, 5f, 2f, 2f)
        };

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
            groundTilemap.color = new Color(1f, 1f, 1f, 0.78f);
            FillBoundaries(boundaryTilemap, assets.BoundaryTile);
            FillObstacles(obstacleTilemap, assets.ObstacleTile);

            boundaryTilemap.gameObject.AddComponent<TilemapCollider2D>();
            obstacleTilemap.gameObject.AddComponent<TilemapCollider2D>();
            CreateEnvironmentDressing(root.transform);

            var camera = CreateMainCamera(root.transform);
            var player = CreateActor(
                root.transform,
                "Player_1000001",
                "Player",
                assets.PlayerSprite,
                new Color(0.22f, 0.66f, 0.95f),
                mapper.ServerToUnity(PlayerSpawn),
                sortingOrder: 10,
                colliderRadius: 0.38f,
                enemyVariant: false,
                playerAnimationSet: assets.PlayerAnimationSet);
            var enemy = CreateActor(
                root.transform,
                "TrainingEnemy_2000001",
                "Training Dummy",
                assets.TrainingEnemySprite,
                new Color(0.84f, 0.25f, 0.22f),
                mapper.ServerToUnity(TrainingEnemySpawn),
                sortingOrder: 9,
                colliderRadius: 0.42f,
                enemyVariant: true);
            var enemies = new GameObject[TrainingEnemySpawns.Length];
            enemies[0] = enemy;
            for (int i = 1; i < enemies.Length; i++)
            {
                enemies[i] = CreateActor(
                    root.transform,
                    $"TrainingEnemy_{2000001 + i}",
                    $"Wave Guard {i + 1}",
                    assets.TrainingEnemySprite,
                    new Color(0.78f, 0.32f, 0.24f),
                    mapper.ServerToUnity(TrainingEnemySpawns[i]),
                    sortingOrder: 9,
                    colliderRadius: 0.42f,
                    enemyVariant: true);
            }

            var playerMotor = player.AddComponent<LocalPlayerMotor>();
            playerMotor.MoveSpeedWorldUnitsPerSecond = 4f;
            playerMotor.VisualAnimator = player.GetComponentInChildren<PlayerSpriteAnimator>();
            playerMotor.SetWorldBounds(PlayerWorldBoundsMin, PlayerWorldBoundsMax);
            var enemyMotors = new TrainingEnemyMotor[enemies.Length];
            for (int i = 0; i < enemies.Length; i++)
            {
                enemyMotors[i] = ConfigureTrainingEnemyMotor(enemies[i], player.transform);
            }

            var enemyMotor = enemyMotors[0];
            var cameraFollow = camera.gameObject.AddComponent<CameraFollow2D>();
            cameraFollow.Target = player.transform;
            cameraFollow.SnapToTarget();
            var hud = CreateHud(camera.transform, out var skillCooldownText, out var rewardPanel, out var rewardChoiceTexts);
            var hudPresenter = hud.GetComponent<PrototypeHudPresenter>();
            hudPresenter.EnemyWorldHealthFill = CreateWorldHealthBar(enemy.transform, "WorldHealthBar", new Vector3(0f, 0.72f, -0.03f), new Color(0.94f, 0.26f, 0.20f));
            var feedback = CreateCombatFeedback(root.transform, enemy);
            var roomScene = CreateRogueRoomScene(root.transform);
            CreateFirstChainClient(root.transform, hudPresenter, playerMotor, enemyMotors, feedback, roomScene, skillCooldownText, rewardPanel, rewardChoiceTexts);

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
                TrainingEnemies = enemies,
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

        private static TrainingEnemyMotor ConfigureTrainingEnemyMotor(GameObject enemy, Transform player)
        {
            var enemyMotor = enemy.AddComponent<TrainingEnemyMotor>();
            enemyMotor.Target = player;
            enemyMotor.MoveSpeedWorldUnitsPerSecond = 1.35f;
            enemyMotor.AggroRangeWorldUnits = 6f;
            enemyMotor.StopDistanceWorldUnits = 1.05f;
            enemyMotor.AttackRangeWorldUnits = 1.1f;
            enemyMotor.AttackCooldownSeconds = 0.85f;
            enemyMotor.AttackDamage = 6;
            enemyMotor.AttackRangePreview = CreateEnemyAttackRangePreview(enemy.transform);
            enemyMotor.SetWorldBounds(PlayerWorldBoundsMin, PlayerWorldBoundsMax);
            enemyMotor.SetObstacleRects(TrainingEnemyObstacleRects);
            return enemyMotor;
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

        private static void CreateEnvironmentDressing(Transform parent)
        {
            var dressing = new GameObject("EnvironmentDressing");
            dressing.transform.SetParent(parent);
            dressing.transform.localPosition = Vector3.zero;
            dressing.transform.localRotation = Quaternion.identity;
            dressing.transform.localScale = Vector3.one;

            CreateColoredQuad(dressing.transform, "CombatFocusRing", new Vector3(0f, -0.15f, 0.08f), new Vector3(5.8f, 3.6f, 1f), new Color(0.78f, 0.88f, 0.48f, 0.16f), 3);
            CreateColoredQuad(dressing.transform, "MistBand_Back", new Vector3(-1.5f, 4.8f, 0.2f), new Vector3(18f, 0.58f, 1f), new Color(0.70f, 0.88f, 0.78f, 0.20f), -2);
            CreateColoredQuad(dressing.transform, "MistBand_Front", new Vector3(1.2f, -5.15f, -0.3f), new Vector3(19f, 0.72f, 1f), new Color(0.62f, 0.80f, 0.70f, 0.22f), 24);
            CreateGroundDetailLayer(dressing.transform);

            Vector3[] clusterPositions =
            {
                new(-12.7f, -7.1f, 0f),
                new(-10.2f, 6.2f, 0f),
                new(-7.8f, -6.9f, 0f),
                new(-4.6f, 6.8f, 0f),
                new(-1.2f, -7.4f, 0f),
                new(2.3f, 6.7f, 0f),
                new(5.6f, -7.1f, 0f),
                new(8.8f, 6.4f, 0f),
                new(11.4f, -6.7f, 0f),
                new(13.1f, 3.6f, 0f),
                new(-13.4f, 2.7f, 0f),
                new(12.9f, -2.6f, 0f)
            };

            for (int i = 0; i < clusterPositions.Length; i++)
            {
                CreateBambooCluster(dressing.transform, $"BambooCluster_{i:00}", clusterPositions[i], i % 3);
            }
        }

        private static void CreateGroundDetailLayer(Transform parent)
        {
            var layer = new GameObject("GroundDetailLayer");
            layer.transform.SetParent(parent);
            layer.transform.localPosition = Vector3.zero;
            layer.transform.localRotation = Quaternion.identity;
            layer.transform.localScale = Vector3.one;

            CreateColoredQuad(layer.transform, "GroundSoftWash", new Vector3(0f, 0f, 0.07f), new Vector3(30f, 21f, 1f), new Color(0.47f, 0.66f, 0.40f, 0.28f), 1);
            CreateColoredQuad(layer.transform, "TrainingLane", new Vector3(0.65f, 0f, 0.06f), new Vector3(4.2f, 1.24f, 1f), new Color(0.72f, 0.84f, 0.50f, 0.10f), 2);
            CreateColoredQuad(layer.transform, "FocusEdge_North", new Vector3(0.2f, 1.72f, 0.05f), new Vector3(5.2f, 0.08f, 1f), new Color(0.88f, 0.94f, 0.58f, 0.20f), 4);
            CreateColoredQuad(layer.transform, "FocusEdge_South", new Vector3(0.2f, -2.02f, 0.05f), new Vector3(5.2f, 0.08f, 1f), new Color(0.88f, 0.94f, 0.58f, 0.15f), 4);

            Vector3[] stonePositions =
            {
                new(-3.6f, 2.5f, 0.05f),
                new(-1.8f, -3.2f, 0.05f),
                new(2.5f, 3.1f, 0.05f),
                new(4.2f, -2.5f, 0.05f),
                new(6.7f, 0.8f, 0.05f)
            };

            for (int i = 0; i < stonePositions.Length; i++)
            {
                float size = 0.28f + (i % 2) * 0.08f;
                CreateColoredQuad(layer.transform, $"StonePatch_{i:00}", stonePositions[i], new Vector3(size * 1.45f, size, 1f), new Color(0.32f, 0.42f, 0.30f, 0.34f), 3);
            }

            Vector3[] grassPositions =
            {
                new(-7.2f, 2.7f, 0.04f),
                new(-6.1f, -3.6f, 0.04f),
                new(-3.9f, -1.9f, 0.04f),
                new(-0.8f, 3.0f, 0.04f),
                new(1.6f, -3.5f, 0.04f),
                new(3.9f, 2.2f, 0.04f),
                new(6.3f, -1.8f, 0.04f),
                new(7.5f, 3.7f, 0.04f),
                new(9.1f, -3.1f, 0.04f),
                new(-9.0f, -0.8f, 0.04f)
            };

            for (int i = 0; i < grassPositions.Length; i++)
            {
                CreateGrassTuft(layer.transform, $"GrassTuft_{i:00}", grassPositions[i], i % 3);
            }
        }

        private static void CreateGrassTuft(Transform parent, string name, Vector3 localPosition, int variant)
        {
            var tuft = new GameObject(name);
            tuft.transform.SetParent(parent);
            tuft.transform.localPosition = localPosition;
            tuft.transform.localRotation = Quaternion.identity;
            tuft.transform.localScale = Vector3.one;

            float height = 0.34f + variant * 0.04f;
            CreateColoredQuad(tuft.transform, "Blade_A", new Vector3(-0.06f, 0.04f, 0f), new Vector3(0.045f, height, 1f), new Color(0.19f, 0.47f, 0.22f, 0.55f), 5);
            CreateColoredQuad(tuft.transform, "Blade_B", new Vector3(0.04f, 0.02f, 0f), new Vector3(0.045f, height * 0.82f, 1f), new Color(0.22f, 0.55f, 0.26f, 0.52f), 5);
            var bladeC = CreateColoredQuad(tuft.transform, "Blade_C", new Vector3(0.12f, 0.01f, 0f), new Vector3(0.04f, height * 0.72f, 1f), new Color(0.16f, 0.39f, 0.19f, 0.48f), 5);
            bladeC.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);
        }

        private static void CreateBambooCluster(Transform parent, string name, Vector3 localPosition, int variant)
        {
            var cluster = new GameObject(name);
            cluster.transform.SetParent(parent);
            cluster.transform.localPosition = localPosition;
            cluster.transform.localRotation = Quaternion.identity;
            cluster.transform.localScale = Vector3.one;

            float height = 1.25f + variant * 0.18f;
            CreateColoredQuad(cluster.transform, "Trunk_A", new Vector3(-0.10f, 0.12f, 0f), new Vector3(0.13f, height, 1f), new Color(0.17f, 0.40f, 0.20f), 7);
            CreateColoredQuad(cluster.transform, "Trunk_B", new Vector3(0.10f, 0.05f, 0f), new Vector3(0.11f, height * 0.9f, 1f), new Color(0.20f, 0.48f, 0.23f), 7);
            CreateColoredQuad(cluster.transform, "Leaf_Canopy", new Vector3(0f, 0.72f, -0.01f), new Vector3(0.88f, 0.42f, 1f), new Color(0.24f, 0.58f, 0.29f), 8);
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

        private static GameObject CreateHud(
            Transform cameraTransform,
            out TextMesh skillCooldownText,
            out Transform rewardPanel,
            out TextMesh[] rewardChoiceTexts)
        {
            var hud = new GameObject("HUD");
            hud.transform.SetParent(cameraTransform);
            hud.transform.localPosition = new Vector3(0f, 0f, 10f);
            hud.transform.localRotation = Quaternion.identity;
            hud.transform.localScale = Vector3.one;

            var presenter = hud.AddComponent<PrototypeHudPresenter>();
            var statusPanel = CreateHudNode(hud.transform, "StatusPanel", new Vector3(-8.45f, 5.35f, 0f));
            CreateHudQuad(statusPanel.transform, "Backplate", new Vector3(2.62f, -0.53f, 0.08f), new Vector3(5.75f, 1.16f, 1f), new Color(0.03f, 0.07f, 0.06f, 0.58f), 104);
            presenter.ConnectionText = CreateHudText(statusPanel.transform, "ConnectionStatusText", Vector3.zero, 0.09f);
            presenter.PlayerText = CreateHudText(statusPanel.transform, "PlayerStatusText", new Vector3(0f, -0.34f, 0f), 0.078f);
            presenter.RegionText = CreateHudText(statusPanel.transform, "RegionStatusText", new Vector3(0f, -0.64f, 0f), 0.072f);
            presenter.PlayerHealthFill = CreateHudBar(statusPanel.transform, "PlayerHealthBar", new Vector3(3.12f, -0.39f, 0f), new Color(0.34f, 0.86f, 0.48f));
            presenter.EnemyHealthFill = CreateHudBar(statusPanel.transform, "EnemyHealthBar", new Vector3(3.12f, -0.69f, 0f), new Color(0.92f, 0.28f, 0.22f));

            var debugPanel = CreateHudNode(hud.transform, "DebugPanel", new Vector3(-8.45f, -5.05f, 0f));
            CreateHudQuad(debugPanel.transform, "Backplate", new Vector3(1.95f, -0.56f, 0.08f), new Vector3(4.25f, 1.36f, 1f), new Color(0.03f, 0.07f, 0.06f, 0.50f), 104);
            presenter.DebugText = CreateHudText(debugPanel.transform, "DebugText", Vector3.zero, 0.078f);
            presenter.RecentEventText = CreateHudText(debugPanel.transform, "RecentEventText", new Vector3(0f, -0.28f, 0f), 0.072f);
            presenter.RogueDetailText = CreateHudText(debugPanel.transform, "RogueDetailText", new Vector3(0f, -0.56f, 0f), 0.064f);
            presenter.RewardText = CreateHudText(debugPanel.transform, "RewardText", new Vector3(0f, -0.82f, 0f), 0.064f);
            skillCooldownText = CreateHudText(debugPanel.transform, "SkillCooldownText", new Vector3(0f, -1.08f, 0f), 0.064f);
            skillCooldownText.text = "Skill Ready";
            rewardPanel = CreateRewardChoicePanel(hud.transform, out rewardChoiceTexts);

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

        private static Transform CreateRewardChoicePanel(Transform parent, out TextMesh[] rewardChoiceTexts)
        {
            var panel = CreateHudNode(parent, "RewardChoicePanel", new Vector3(1.65f, 2.35f, 0f));
            CreateHudQuad(panel, "Backplate", new Vector3(1.95f, -0.82f, 0.08f), new Vector3(4.55f, 2.3f, 1f), new Color(0.03f, 0.06f, 0.05f, 0.72f), 124);
            var title = CreateHudText(panel, "RewardTitleText", Vector3.zero, 0.095f);
            title.text = "Choose Reward";
            rewardChoiceTexts = new[]
            {
                CreateHudText(panel, "RewardChoice_0", new Vector3(0f, -0.45f, 0f), 0.078f),
                CreateHudText(panel, "RewardChoice_1", new Vector3(0f, -0.82f, 0f), 0.078f),
                CreateHudText(panel, "RewardChoice_2", new Vector3(0f, -1.19f, 0f), 0.078f)
            };
            rewardChoiceTexts[0].text = "1. Sword Intent +10%";
            rewardChoiceTexts[1].text = "2. Max HP +20";
            rewardChoiceTexts[2].text = "3. Cooldown -10%";
            panel.gameObject.SetActive(false);
            return panel;
        }

        private static PrototypeCombatFeedbackPresenter CreateCombatFeedback(Transform parent, GameObject targetEnemy)
        {
            var feedbackObject = new GameObject("CombatFeedback");
            feedbackObject.transform.SetParent(parent);
            feedbackObject.transform.localPosition = Vector3.zero;
            feedbackObject.transform.localRotation = Quaternion.identity;
            feedbackObject.transform.localScale = Vector3.one;

            var presenter = feedbackObject.AddComponent<PrototypeCombatFeedbackPresenter>();
            var swordQi = GameObject.CreatePrimitive(PrimitiveType.Quad);
            swordQi.name = "SwordQiVfx";
            swordQi.transform.SetParent(feedbackObject.transform);
            swordQi.transform.localPosition = Vector3.zero;
            swordQi.transform.localRotation = Quaternion.identity;
            swordQi.transform.localScale = new Vector3(1.35f, 0.18f, 1f);
            var swordCollider = swordQi.GetComponent<Collider>();
            if (swordCollider != null)
            {
                Object.DestroyImmediate(swordCollider);
            }

            var swordRenderer = swordQi.GetComponent<MeshRenderer>();
            swordRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"))
            {
                color = new Color(0.72f, 0.95f, 1f, 0.88f)
            };
            swordRenderer.sortingOrder = 35;
            swordQi.SetActive(false);
            presenter.SwordQi = swordQi.transform;

            var slashWake = GameObject.CreatePrimitive(PrimitiveType.Quad);
            slashWake.name = "SlashWakeVfx";
            slashWake.transform.SetParent(feedbackObject.transform);
            slashWake.transform.localPosition = Vector3.zero;
            slashWake.transform.localRotation = Quaternion.identity;
            slashWake.transform.localScale = new Vector3(2.15f, 0.34f, 1f);
            var slashCollider = slashWake.GetComponent<Collider>();
            if (slashCollider != null)
            {
                Object.DestroyImmediate(slashCollider);
            }

            var slashRenderer = slashWake.GetComponent<MeshRenderer>();
            slashRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"))
            {
                color = new Color(0.42f, 0.88f, 1f, 0.30f)
            };
            slashRenderer.sortingOrder = 34;
            slashWake.SetActive(false);
            presenter.SlashWake = slashWake.transform;

            var hitRangePreview = CreateColoredQuad(
                feedbackObject.transform,
                "HitRangePreviewVfx",
                Vector3.zero,
                new Vector3(1.45f, 0.82f, 1f),
                new Color(0.60f, 0.96f, 1f, 0.20f),
                33);
            hitRangePreview.SetActive(false);
            presenter.HitRangePreview = hitRangePreview.transform;

            var damageObject = new GameObject("DamageText_2000001");
            damageObject.transform.SetParent(parent);
            damageObject.transform.position = targetEnemy.transform.position + new Vector3(-0.25f, 0.85f, -0.05f);
            var damageText = damageObject.AddComponent<TextMesh>();
            damageText.anchor = TextAnchor.MiddleCenter;
            damageText.alignment = TextAlignment.Center;
            damageText.characterSize = 0.16f;
            damageText.fontSize = 48;
            damageText.color = new Color(1f, 0.92f, 0.48f);
            damageText.GetComponent<MeshRenderer>().sortingOrder = 130;
            damageObject.SetActive(false);
            presenter.DamageText = damageText;

            var hitBurst = GameObject.CreatePrimitive(PrimitiveType.Quad);
            hitBurst.name = "HitBurstVfx";
            hitBurst.transform.SetParent(parent);
            hitBurst.transform.position = targetEnemy.transform.position + new Vector3(0f, 0.18f, -0.06f);
            hitBurst.transform.localRotation = Quaternion.identity;
            hitBurst.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
            var hitCollider = hitBurst.GetComponent<Collider>();
            if (hitCollider != null)
            {
                Object.DestroyImmediate(hitCollider);
            }

            var hitRenderer = hitBurst.GetComponent<MeshRenderer>();
            hitRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"))
            {
                color = new Color(1f, 0.86f, 0.26f, 0.58f)
            };
            hitRenderer.sortingOrder = 45;
            hitBurst.SetActive(false);
            presenter.HitBurst = hitBurst.transform;
            Transform targetVisualRoot = targetEnemy.transform.Find("VisualRoot");
            presenter.TargetTransform = targetVisualRoot == null ? targetEnemy.transform : targetVisualRoot;
            presenter.TargetRenderer = targetEnemy.GetComponentInChildren<SpriteRenderer>();
            presenter.EnemyStateText = CreateEnemyStateText(targetEnemy.transform);
            return presenter;
        }

        private static TextMesh CreateEnemyStateText(Transform parent)
        {
            var textObject = new GameObject("EnemyStateText");
            textObject.transform.SetParent(parent);
            textObject.transform.localPosition = new Vector3(0f, -0.86f, -0.05f);
            textObject.transform.localRotation = Quaternion.identity;
            textObject.transform.localScale = Vector3.one;

            var text = textObject.AddComponent<TextMesh>();
            text.text = "Idle";
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.085f;
            text.fontSize = 34;
            text.color = new Color(0.82f, 0.94f, 0.74f, 0.92f);
            text.GetComponent<MeshRenderer>().sortingOrder = 128;
            return text;
        }

        private static RogueRoomScenePresenter CreateRogueRoomScene(Transform parent)
        {
            var roomObject = new GameObject("RogueRoomScene");
            roomObject.transform.SetParent(parent);
            roomObject.transform.localPosition = Vector3.zero;
            roomObject.transform.localRotation = Quaternion.identity;
            roomObject.transform.localScale = Vector3.one;

            var presenter = roomObject.AddComponent<RogueRoomScenePresenter>();
            presenter.RoomActiveTint = CreateColoredQuad(
                roomObject.transform,
                "RoomActiveTint",
                new Vector3(0.65f, 0f, 0.03f),
                new Vector3(7.15f, 4.45f, 1f),
                new Color(0.42f, 0.72f, 0.52f, 0.24f),
                6).transform;
            presenter.ExitSeal = CreateColoredQuad(
                roomObject.transform,
                "ExitSeal",
                new Vector3(4.05f, 0f, -0.06f),
                new Vector3(0.18f, 1.38f, 1f),
                new Color(0.65f, 0.96f, 1f, 0.58f),
                38).transform;
            presenter.ExitPortal = CreateColoredQuad(
                roomObject.transform,
                "ExitPortal",
                new Vector3(4.05f, 0f, -0.07f),
                new Vector3(0.46f, 1.62f, 1f),
                new Color(0.86f, 0.94f, 0.46f, 0.72f),
                37).transform;
            presenter.ExitPromptText = CreateExitPromptText(roomObject.transform);
            presenter.MonsterMarkerRoot = CreateMonsterMarkers(roomObject.transform);
            presenter.RoomStatusText = CreateRoomStatusText(roomObject.transform);
            presenter.ApplyFieldState();
            return presenter;
        }

        private static Transform CreateMonsterMarkers(Transform parent)
        {
            var root = new GameObject("MonsterMarkerRoot");
            root.transform.SetParent(parent);
            root.transform.localPosition = new Vector3(1.95f, -1.26f, -0.05f);
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            for (int i = 0; i < 8; i++)
            {
                float x = (i % 4) * 0.32f;
                float y = -(i / 4) * 0.28f;
                var marker = CreateColoredQuad(
                    root.transform,
                    $"MonsterMarker_{i:00}",
                    new Vector3(x, y, 0f),
                    new Vector3(0.18f, 0.18f, 1f),
                    new Color(0.92f, 0.30f, 0.22f, 0.78f),
                    39);
                marker.SetActive(false);
            }

            return root.transform;
        }

        private static TextMesh CreateRoomStatusText(Transform parent)
        {
            var textObject = new GameObject("RoomStatusText");
            textObject.transform.SetParent(parent);
            textObject.transform.localPosition = new Vector3(0f, 2.34f, -0.06f);
            textObject.transform.localRotation = Quaternion.identity;
            textObject.transform.localScale = Vector3.one;

            var text = textObject.AddComponent<TextMesh>();
            text.text = "Field";
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.11f;
            text.fontSize = 42;
            text.color = new Color(0.92f, 0.98f, 0.82f, 0.92f);
            text.GetComponent<MeshRenderer>().sortingOrder = 129;
            return text;
        }

        private static TextMesh CreateExitPromptText(Transform parent)
        {
            var textObject = new GameObject("ExitPromptText");
            textObject.transform.SetParent(parent);
            textObject.transform.localPosition = new Vector3(4.05f, 1.12f, -0.08f);
            textObject.transform.localRotation = Quaternion.identity;
            textObject.transform.localScale = Vector3.one;

            var text = textObject.AddComponent<TextMesh>();
            text.text = "Choose Reward";
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.09f;
            text.fontSize = 36;
            text.color = new Color(0.98f, 0.96f, 0.72f, 0.95f);
            text.GetComponent<MeshRenderer>().sortingOrder = 130;
            text.gameObject.SetActive(false);
            return text;
        }

        private static GameObject CreateFirstChainClient(
            Transform parent,
            PrototypeHudPresenter presenter,
            LocalPlayerMotor playerMotor,
            TrainingEnemyMotor[] enemyMotors,
            PrototypeCombatFeedbackPresenter feedbackPresenter,
            RogueRoomScenePresenter roomScenePresenter,
            TextMesh skillCooldownText,
            Transform rewardPanel,
            TextMesh[] rewardChoiceTexts)
        {
            var clientObject = new GameObject("FirstChainClient");
            clientObject.transform.SetParent(parent);

            var hudController = clientObject.AddComponent<FirstChainHudController>();
            hudController.HudPresenter = presenter;
            hudController.FeedbackPresenter = feedbackPresenter;
            hudController.PlayerVisualAnimator = playerMotor == null ? null : playerMotor.VisualAnimator;
            hudController.RogueRoomScenePresenter = roomScenePresenter;
            hudController.DefaultRegionId = 1001;
            hudController.ApplyOfflineSnapshot();

            var wsClient = clientObject.AddComponent<FirstChainWsClient>();
            wsClient.HudController = hudController;
            wsClient.Endpoint = FirstChainWsClient.DefaultEndpoint;

            var skillIntent = clientObject.AddComponent<SkillIntentController>();
            skillIntent.HudController = hudController;
            skillIntent.PlayerMotor = playerMotor;
            skillIntent.SkillId = 2001;
            skillIntent.CooldownSeconds = 0.45f;
            skillIntent.CooldownText = skillCooldownText;

            var loopController = clientObject.AddComponent<PrototypeCombatLoopController>();
            loopController.HudController = hudController;
            loopController.EnemyMotors = enemyMotors;

            var rewardController = clientObject.AddComponent<PrototypeRogueRewardController>();
            rewardController.Player = playerMotor == null ? null : playerMotor.transform;
            rewardController.ExitPoint = roomScenePresenter == null || roomScenePresenter.ExitPortal == null
                ? null
                : roomScenePresenter.ExitPortal;
            rewardController.RoomScenePresenter = roomScenePresenter;
            rewardController.HudController = hudController;
            rewardController.RewardPanel = rewardPanel;
            rewardController.RewardChoiceTexts = rewardChoiceTexts;
            rewardController.ExitInteractDistance = 1.45f;
            rewardController.ConfigureEnemyWave(enemyMotors);
            rewardController.StartRoom(1, Mathf.Min(2, Mathf.Max(1, enemyMotors == null ? 1 : enemyMotors.Length)));

            var rogueDebug = clientObject.AddComponent<RogueDebugController>();
            rogueDebug.HudController = hudController;
            rogueDebug.RogueId = 4001;
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
            text.fontSize = 36;
            text.color = new Color(0.92f, 0.96f, 0.90f);
            text.GetComponent<MeshRenderer>().sortingOrder = 120;
            return text;
        }

        private static Transform CreateWorldHealthBar(Transform parent, string name, Vector3 localPosition, Color fillColor)
        {
            var bar = CreateHudNode(parent, name, localPosition);
            bar.localScale = new Vector3(0.72f, 0.72f, 1f);
            CreateHudQuad(bar, "Background", Vector3.zero, new Vector3(1.18f, 0.13f, 1f), new Color(0.04f, 0.04f, 0.03f, 0.88f), 70);
            return CreateHudQuad(bar, "Fill", new Vector3(0f, 0f, -0.01f), new Vector3(1.06f, 0.075f, 1f), fillColor, 71);
        }

        private static Transform CreateEnemyAttackRangePreview(Transform parent)
        {
            var preview = CreateColoredQuad(
                parent,
                "EnemyAttackRangePreview",
                Vector3.zero,
                new Vector3(1.1f, 0.34f, 1f),
                new Color(1f, 0.36f, 0.22f, 0.30f),
                37);
            preview.SetActive(false);
            return preview.transform;
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
            string displayName,
            Sprite sprite,
            Color color,
            Vector3 position,
            int sortingOrder,
            float colliderRadius,
            bool enemyVariant,
            PlayerSpriteAnimationSet playerAnimationSet = null)
        {
            var actor = new GameObject(name);
            actor.transform.SetParent(parent);
            actor.transform.position = position;

            CreateActorShadow(actor.transform, sortingOrder - 2);
            CreateActorVisual(actor.transform, sprite, color, sortingOrder, enemyVariant, playerAnimationSet);
            CreateActorName(actor.transform, displayName, sortingOrder + 50);

            var collider = actor.AddComponent<CircleCollider2D>();
            collider.radius = colliderRadius;
            return actor;
        }

        private static void CreateActorVisual(
            Transform parent,
            Sprite sprite,
            Color baseColor,
            int sortingOrder,
            bool enemyVariant,
            PlayerSpriteAnimationSet playerAnimationSet)
        {
            var visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(parent);
            visualRoot.transform.localPosition = Vector3.zero;
            visualRoot.transform.localRotation = Quaternion.identity;
            visualRoot.transform.localScale = Vector3.one;

            if (!enemyVariant && sprite != null)
            {
                var renderer = CreateActorSpritePart(
                    visualRoot.transform,
                    "CharacterSprite",
                    sprite,
                    new Vector3(0f, 0.08f, -0.04f),
                    new Vector3(1.25f, 1.25f, 1f),
                    Color.white,
                    sortingOrder + 5);
                var animator = renderer.gameObject.AddComponent<PlayerSpriteAnimator>();
                animator.SpriteRenderer = renderer;
                animator.AnimationSet = BuildPlayerAnimationSet(playerAnimationSet, sprite);
                animator.ApplyMovement(Vector2.down, isMoving: false, deltaTime: 0f);
                return;
            }

            if (enemyVariant)
            {
                CreateColoredQuad(visualRoot.transform, "GuardRim", new Vector3(0f, -0.05f, 0.01f), new Vector3(0.72f, 0.92f, 1f), new Color(0.31f, 0.06f, 0.05f, 0.72f), sortingOrder);
            }

            CreateActorSpritePart(visualRoot.transform, "Core", sprite, new Vector3(0f, -0.05f, -0.01f), new Vector3(0.58f, 0.78f, 1f), baseColor, sortingOrder + 1);
            CreateColoredQuad(visualRoot.transform, "Head", new Vector3(0f, 0.48f, -0.03f), new Vector3(0.34f, 0.30f, 1f), Tint(baseColor, enemyVariant ? 1.08f : 1.22f, 1f), sortingOrder + 3);
            CreateColoredQuad(visualRoot.transform, "Sash", new Vector3(0f, -0.17f, -0.04f), new Vector3(0.62f, 0.08f, 1f), enemyVariant ? new Color(0.98f, 0.78f, 0.28f, 1f) : new Color(0.15f, 0.33f, 0.42f, 1f), sortingOrder + 4);

            var weapon = CreateColoredQuad(
                visualRoot.transform,
                "Weapon",
                enemyVariant ? new Vector3(0.42f, 0.04f, -0.05f) : new Vector3(0.44f, 0.08f, -0.05f),
                enemyVariant ? new Vector3(0.10f, 0.98f, 1f) : new Vector3(0.08f, 1.08f, 1f),
                enemyVariant ? new Color(0.36f, 0.18f, 0.14f, 1f) : new Color(0.78f, 0.95f, 1f, 1f),
                sortingOrder + 5);
            weapon.transform.localRotation = Quaternion.Euler(0f, 0f, enemyVariant ? -8f : -22f);

            if (!enemyVariant)
            {
                CreateColoredQuad(visualRoot.transform, "ShoulderCape", new Vector3(-0.20f, 0.10f, -0.02f), new Vector3(0.24f, 0.42f, 1f), new Color(0.10f, 0.26f, 0.34f, 0.88f), sortingOrder + 2);
            }
        }

        private static SpriteRenderer CreateActorSpritePart(Transform parent, string name, Sprite sprite, Vector3 localPosition, Vector3 localScale, Color color, int sortingOrder)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.identity;
            part.transform.localScale = localScale;

            var renderer = part.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static PlayerSpriteAnimationSet BuildPlayerAnimationSet(PlayerSpriteAnimationSet source, Sprite fallback)
        {
            source ??= new PlayerSpriteAnimationSet();
            return new PlayerSpriteAnimationSet
            {
                IdleDown = source.IdleDown ?? fallback,
                IdleUp = source.IdleUp ?? source.IdleDown ?? fallback,
                IdleLeft = source.IdleLeft ?? source.IdleDown ?? fallback,
                IdleRight = source.IdleRight ?? source.IdleLeft ?? source.IdleDown ?? fallback,
                WalkDownA = source.WalkDownA ?? source.IdleDown ?? fallback,
                WalkDownB = source.WalkDownB ?? source.WalkDownA ?? source.IdleDown ?? fallback,
                WalkUpA = source.WalkUpA ?? source.IdleUp ?? source.IdleDown ?? fallback,
                WalkUpB = source.WalkUpB ?? source.WalkUpA ?? source.IdleUp ?? fallback,
                WalkLeftA = source.WalkLeftA ?? source.IdleLeft ?? source.IdleDown ?? fallback,
                WalkLeftB = source.WalkLeftB ?? source.WalkLeftA ?? source.IdleLeft ?? fallback,
                WalkRightA = source.WalkRightA ?? source.IdleRight ?? source.IdleDown ?? fallback,
                WalkRightB = source.WalkRightB ?? source.WalkRightA ?? source.IdleRight ?? fallback,
                AttackDown = source.AttackDown ?? source.IdleDown ?? fallback,
                AttackUp = source.AttackUp ?? source.IdleUp ?? source.IdleDown ?? fallback,
                AttackLeft = source.AttackLeft ?? source.IdleLeft ?? source.IdleDown ?? fallback,
                AttackRight = source.AttackRight ?? source.IdleRight ?? source.IdleDown ?? fallback,
                HitDown = source.HitDown ?? source.IdleDown ?? fallback,
                HitUp = source.HitUp ?? source.IdleUp ?? source.IdleDown ?? fallback,
                HitLeft = source.HitLeft ?? source.IdleLeft ?? source.IdleDown ?? fallback,
                HitRight = source.HitRight ?? source.IdleRight ?? source.IdleDown ?? fallback
            };
        }

        private static void CreateActorShadow(Transform parent, int sortingOrder)
        {
            var shadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            shadow.name = "DropShadow";
            shadow.transform.SetParent(parent);
            shadow.transform.localPosition = new Vector3(0f, -0.32f, 0.04f);
            shadow.transform.localRotation = Quaternion.identity;
            shadow.transform.localScale = new Vector3(0.82f, 0.28f, 1f);

            var collider = shadow.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            var renderer = shadow.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"))
            {
                color = new Color(0.02f, 0.03f, 0.02f, 0.48f)
            };
            renderer.sortingOrder = sortingOrder;
        }

        private static void CreateActorName(Transform parent, string displayName, int sortingOrder)
        {
            var textObject = new GameObject("ActorNameText");
            textObject.transform.SetParent(parent);
            textObject.transform.localPosition = new Vector3(0f, 1.02f, -0.04f);
            textObject.transform.localRotation = Quaternion.identity;
            textObject.transform.localScale = Vector3.one;

            var text = textObject.AddComponent<TextMesh>();
            text.text = displayName;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.105f;
            text.fontSize = 42;
            text.color = new Color(0.96f, 0.98f, 0.88f);
            text.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        }

        private static GameObject CreateColoredQuad(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color, int sortingOrder)
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
            return quad;
        }

        private static Color Tint(Color color, float multiplier, float alpha)
        {
            return new Color(
                Mathf.Clamp01(color.r * multiplier),
                Mathf.Clamp01(color.g * multiplier),
                Mathf.Clamp01(color.b * multiplier),
                alpha);
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
