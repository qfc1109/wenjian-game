using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Wenjian.Client.Net;
using Wenjian.Client.Presentation;
using Wenjian.Client.Prototype;
using Wenjian.Client.UI;

namespace Wenjian.Client.Tests.Prototype
{
    public sealed class CombatFieldSceneFactoryTests
    {
        [SetUp]
        public void SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [Test]
        public void BuildDefaultCreatesCombatFieldRootsCameraAndActors()
        {
            var assets = CreateInMemoryAssets();

            var created = CombatFieldSceneFactory.BuildDefault(assets);

            Assert.That(created.Root.name, Is.EqualTo("Proto_CombatField"));
            Assert.That(GameObject.Find("Proto_CombatField/Grid"), Is.Not.Null);
            Assert.That(GameObject.Find("Proto_CombatField/Grid/GroundTilemap"), Is.Not.Null);
            Assert.That(GameObject.Find("Proto_CombatField/Grid/BoundaryTilemap"), Is.Not.Null);
            Assert.That(GameObject.Find("Proto_CombatField/Grid/ObstacleTilemap"), Is.Not.Null);
            Assert.That(Camera.main, Is.Not.Null);
            Assert.That(created.Player.name, Is.EqualTo("Player_1000001"));
            Assert.That(created.TrainingEnemy.name, Is.EqualTo("TrainingEnemy_2000001"));
        }

        [Test]
        public void BuildDefaultMapsConfiguredPlayerSpawnToWorldOrigin()
        {
            var created = CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            Assert.That(created.Player.transform.position.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(created.Player.transform.position.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(created.Player.transform.position.z, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void BuildDefaultPopulatesGroundAndBlockingTilemaps()
        {
            var created = CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            Assert.That(created.GroundTilemap.GetUsedTilesCount(), Is.GreaterThan(0));
            Assert.That(created.BoundaryTilemap.GetUsedTilesCount(), Is.GreaterThan(0));
            Assert.That(created.ObstacleTilemap.GetUsedTilesCount(), Is.GreaterThan(0));
        }

        [Test]
        public void BuildDefaultWiresLocalPlayerMovementAndCameraFollow()
        {
            var created = CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            var playerMotor = created.Player.GetComponent<LocalPlayerMotor>();
            var cameraFollow = created.MainCamera.GetComponent<CameraFollow2D>();

            Assert.That(playerMotor, Is.Not.Null);
            Assert.That(cameraFollow, Is.Not.Null);
            Assert.That(cameraFollow.Target, Is.EqualTo(created.Player.transform));
        }

        [Test]
        public void BuildDefaultCreatesHudAndDebugPanelUnderCamera()
        {
            var created = CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            Assert.That(created.Hud, Is.Not.Null);
            Assert.That(created.Hud.GetComponent<PrototypeHudPresenter>(), Is.Not.Null);
            Assert.That(GameObject.Find("Proto_CombatField/Main Camera/HUD"), Is.Not.Null);
            Assert.That(GameObject.Find("Proto_CombatField/Main Camera/HUD/StatusPanel"), Is.Not.Null);
            Assert.That(GameObject.Find("Proto_CombatField/Main Camera/HUD/DebugPanel"), Is.Not.Null);
        }

        [Test]
        public void BuildDefaultCreatesFirstChainClientAndWiresHud()
        {
            var created = CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            var clientObject = GameObject.Find("Proto_CombatField/FirstChainClient");
            var hudController = clientObject.GetComponent<FirstChainHudController>();
            var wsClient = clientObject.GetComponent<FirstChainWsClient>();

            Assert.That(clientObject, Is.Not.Null);
            Assert.That(hudController.HudPresenter, Is.EqualTo(created.Hud.GetComponent<PrototypeHudPresenter>()));
            Assert.That(wsClient.HudController, Is.EqualTo(hudController));
            Assert.That(wsClient.Endpoint, Is.EqualTo("ws://127.0.0.1:18080/ws/first-chain"));
        }

        private static CombatFieldSceneAssets CreateInMemoryAssets()
        {
            return new CombatFieldSceneAssets
            {
                GroundTile = CreateTile(new Color(0.32f, 0.52f, 0.30f)),
                BoundaryTile = CreateTile(new Color(0.16f, 0.25f, 0.16f)),
                ObstacleTile = CreateTile(new Color(0.20f, 0.42f, 0.22f)),
                PlayerSprite = CreateSprite(new Color(0.22f, 0.66f, 0.95f)),
                TrainingEnemySprite = CreateSprite(new Color(0.84f, 0.25f, 0.22f))
            };
        }

        private static Tile CreateTile(Color color)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = CreateSprite(color);
            return tile;
        }

        private static Sprite CreateSprite(Color color)
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
