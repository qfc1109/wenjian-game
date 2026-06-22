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
            Assert.That(created.TrainingEnemies, Has.Length.GreaterThanOrEqualTo(3));
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
            Assert.That(created.GroundTilemap.color.a, Is.LessThanOrEqualTo(0.82f));
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
        public void BuildDefaultConstrainsPlayerMovementInsideCombatField()
        {
            var created = CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());
            var playerMotor = created.Player.GetComponent<LocalPlayerMotor>();

            playerMotor.MoveSpeedWorldUnitsPerSecond = 100f;
            playerMotor.ApplyMovement(Vector2.right + Vector2.up, 1f);

            Assert.That(playerMotor.ConstrainToWorldBounds, Is.True);
            Assert.That(created.Player.transform.position.x, Is.LessThanOrEqualTo(playerMotor.WorldBoundsMax.x));
            Assert.That(created.Player.transform.position.y, Is.LessThanOrEqualTo(playerMotor.WorldBoundsMax.y));
            Assert.That(created.Player.transform.position.x, Is.EqualTo(13.5f).Within(0.001f));
            Assert.That(created.Player.transform.position.y, Is.EqualTo(9.5f).Within(0.001f));
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
        public void BuildDefaultCreatesCompactHudPanelsWithBackplates()
        {
            var created = CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            var statusPanel = GameObject.Find("Proto_CombatField/Main Camera/HUD/StatusPanel");
            var debugPanel = GameObject.Find("Proto_CombatField/Main Camera/HUD/DebugPanel");
            var statusBackplate = GameObject.Find("Proto_CombatField/Main Camera/HUD/StatusPanel/Backplate");
            var debugBackplate = GameObject.Find("Proto_CombatField/Main Camera/HUD/DebugPanel/Backplate");
            var presenter = created.Hud.GetComponent<PrototypeHudPresenter>();

            Assert.That(statusPanel.transform.localPosition.y, Is.LessThanOrEqualTo(5.45f));
            Assert.That(debugPanel.transform.localPosition.y, Is.GreaterThanOrEqualTo(-5.2f));
            Assert.That(statusBackplate, Is.Not.Null);
            Assert.That(debugBackplate, Is.Not.Null);
            Assert.That(presenter.ConnectionText.characterSize, Is.LessThanOrEqualTo(0.095f));
            Assert.That(presenter.PlayerText.characterSize, Is.LessThanOrEqualTo(0.085f));
            Assert.That(presenter.RegionText.transform.localPosition.y, Is.LessThan(presenter.PlayerText.transform.localPosition.y - 0.25f));
            Assert.That(presenter.PlayerHealthFill.parent.localPosition.x, Is.GreaterThanOrEqualTo(2.65f));
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

        [Test]
        public void BuildDefaultWiresSkillInputAndCombatFeedback()
        {
            var created = CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            var clientObject = GameObject.Find("Proto_CombatField/FirstChainClient");
            var hudController = clientObject.GetComponent<FirstChainHudController>();
            var skillIntent = clientObject.GetComponent<SkillIntentController>();
            var feedback = GameObject.Find("Proto_CombatField/CombatFeedback")
                .GetComponent<PrototypeCombatFeedbackPresenter>();

            Assert.That(skillIntent, Is.Not.Null);
            Assert.That(skillIntent.HudController, Is.EqualTo(hudController));
            Assert.That(skillIntent.PlayerMotor, Is.EqualTo(created.Player.GetComponent<LocalPlayerMotor>()));
            Assert.That(hudController.FeedbackPresenter, Is.EqualTo(feedback));
            Assert.That(feedback.SwordQi, Is.Not.Null);
            Assert.That(feedback.HitRangePreview, Is.Not.Null);
            Assert.That(feedback.DamageText, Is.Not.Null);
        }

        [Test]
        public void BuildDefaultWiresTrainingEnemyAIAndSkillCooldownHud()
        {
            var created = CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            var clientObject = GameObject.Find("Proto_CombatField/FirstChainClient");
            var enemyMotor = created.TrainingEnemy.GetComponent<TrainingEnemyMotor>();
            var skillIntent = clientObject.GetComponent<SkillIntentController>();
            var loopController = clientObject.GetComponent<PrototypeCombatLoopController>();
            var cooldownText = GameObject.Find("Proto_CombatField/Main Camera/HUD/DebugPanel/SkillCooldownText");
            var attackPreview = GameObject.Find("Proto_CombatField/TrainingEnemy_2000001/EnemyAttackRangePreview");

            Assert.That(enemyMotor, Is.Not.Null);
            Assert.That(enemyMotor.Target, Is.EqualTo(created.Player.transform));
            Assert.That(enemyMotor.ObstacleRectCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(enemyMotor.ConstrainToWorldBounds, Is.True);
            Assert.That(enemyMotor.AttackRangeWorldUnits, Is.GreaterThan(0f));
            Assert.That(enemyMotor.AttackRangePreview, Is.EqualTo(attackPreview.transform));
            Assert.That(loopController, Is.Not.Null);
            Assert.That(loopController.EnemyMotor, Is.EqualTo(enemyMotor));
            Assert.That(loopController.HudController, Is.EqualTo(clientObject.GetComponent<FirstChainHudController>()));
            Assert.That(skillIntent.CooldownSeconds, Is.GreaterThan(0f));
            Assert.That(skillIntent.CooldownText, Is.EqualTo(cooldownText.GetComponent<TextMesh>()));
            Assert.That(cooldownText.GetComponent<TextMesh>().text, Is.EqualTo("Skill Ready"));
        }

        [Test]
        public void BuildDefaultTargetsCombatFeedbackAtEnemyVisualRootSoAICanMoveRoot()
        {
            var created = CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            var feedback = GameObject.Find("Proto_CombatField/CombatFeedback")
                .GetComponent<PrototypeCombatFeedbackPresenter>();
            var visualRoot = created.TrainingEnemy.transform.Find("VisualRoot");

            Assert.That(visualRoot, Is.Not.Null);
            Assert.That(feedback.TargetTransform, Is.EqualTo(visualRoot));
        }

        [Test]
        public void BuildDefaultCreatesRogueRoomSceneFeedbackAndWiresHudController()
        {
            CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            var hudController = GameObject.Find("Proto_CombatField/FirstChainClient").GetComponent<FirstChainHudController>();
            var roomObject = GameObject.Find("Proto_CombatField/RogueRoomScene");
            var roomPresenter = roomObject.GetComponent<RogueRoomScenePresenter>();

            Assert.That(roomPresenter, Is.Not.Null);
            Assert.That(hudController.RogueRoomScenePresenter, Is.EqualTo(roomPresenter));
            Assert.That(roomObject.transform.Find("RoomActiveTint"), Is.Not.Null);
            Assert.That(roomObject.transform.Find("ExitSeal"), Is.Not.Null);
            Assert.That(roomObject.transform.Find("ExitPortal"), Is.Not.Null);
            Assert.That(roomObject.transform.Find("ExitPromptText"), Is.Not.Null);
            Assert.That(roomPresenter.MonsterMarkerRoot.Find("MonsterMarker_00"), Is.Not.Null);
            Assert.That(roomPresenter.LastRoomState, Is.EqualTo("Room 1"));
        }

        [Test]
        public void BuildDefaultCreatesRewardControllerPanelAndMultiEnemyWave()
        {
            var created = CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            var clientObject = GameObject.Find("Proto_CombatField/FirstChainClient");
            var rewardController = clientObject.GetComponent<PrototypeRogueRewardController>();
            var loopController = clientObject.GetComponent<PrototypeCombatLoopController>();
            var rewardPanel = created.Hud.transform.Find("RewardChoicePanel");
            var choice0 = rewardPanel.Find("RewardChoice_0");
            var enemyMotors = Resources.FindObjectsOfTypeAll<TrainingEnemyMotor>();

            Assert.That(rewardController, Is.Not.Null);
            Assert.That(rewardController.HudController, Is.EqualTo(clientObject.GetComponent<FirstChainHudController>()));
            Assert.That(rewardController.RoomScenePresenter, Is.EqualTo(GameObject.Find("Proto_CombatField/RogueRoomScene").GetComponent<RogueRoomScenePresenter>()));
            Assert.That(rewardController.Player, Is.EqualTo(created.Player.transform));
            Assert.That(rewardController.RewardPanel, Is.EqualTo(rewardPanel));
            Assert.That(rewardPanel.gameObject.activeSelf, Is.False);
            Assert.That(choice0.GetComponent<TextMesh>().text, Does.Contain("1."));
            Assert.That(enemyMotors, Has.Length.GreaterThanOrEqualTo(3));
            Assert.That(loopController.EnemyMotors, Has.Length.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void BuildDefaultWiresRogueDebugControllerAndHudTexts()
        {
            var created = CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            var clientObject = GameObject.Find("Proto_CombatField/FirstChainClient");
            var hudController = clientObject.GetComponent<FirstChainHudController>();
            var rogueDebug = clientObject.GetComponent<RogueDebugController>();
            var presenter = created.Hud.GetComponent<PrototypeHudPresenter>();

            Assert.That(rogueDebug, Is.Not.Null);
            Assert.That(rogueDebug.HudController, Is.EqualTo(hudController));
            Assert.That(rogueDebug.RogueId, Is.EqualTo(4001));
            Assert.That(presenter.RogueDetailText, Is.Not.Null);
            Assert.That(presenter.RewardText, Is.Not.Null);
        }

        [Test]
        public void BuildDefaultCreatesLayeredEnvironmentDressing()
        {
            CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            var dressing = GameObject.Find("Proto_CombatField/EnvironmentDressing");
            var focusRing = GameObject.Find("Proto_CombatField/EnvironmentDressing/CombatFocusRing");
            var backMist = GameObject.Find("Proto_CombatField/EnvironmentDressing/MistBand_Back");
            var frontMist = GameObject.Find("Proto_CombatField/EnvironmentDressing/MistBand_Front");
            var bambooClusters = GameObject.FindObjectsOfType<Transform>();
            int clusterCount = 0;
            foreach (var transform in bambooClusters)
            {
                if (transform.name.StartsWith("BambooCluster_"))
                {
                    clusterCount++;
                }
            }

            Assert.That(dressing, Is.Not.Null);
            Assert.That(focusRing, Is.Not.Null);
            Assert.That(backMist, Is.Not.Null);
            Assert.That(frontMist, Is.Not.Null);
            Assert.That(clusterCount, Is.GreaterThanOrEqualTo(10));
            Assert.That(backMist.GetComponent<MeshRenderer>().sortingOrder, Is.LessThan(0));
            Assert.That(frontMist.GetComponent<MeshRenderer>().sortingOrder, Is.GreaterThan(20));
        }

        [Test]
        public void BuildDefaultAddsGroundDetailAndReadableStaging()
        {
            CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            Assert.That(GameObject.Find("Proto_CombatField/EnvironmentDressing/GroundDetailLayer"), Is.Not.Null);
            var softWash = GameObject.Find("Proto_CombatField/EnvironmentDressing/GroundDetailLayer/GroundSoftWash");

            Assert.That(softWash, Is.Not.Null);
            Assert.That(softWash.GetComponent<MeshRenderer>().sharedMaterial.color.a, Is.GreaterThanOrEqualTo(0.25f));
            Assert.That(GameObject.Find("Proto_CombatField/EnvironmentDressing/GroundDetailLayer/TrainingLane"), Is.Not.Null);
            Assert.That(GameObject.Find("Proto_CombatField/EnvironmentDressing/GroundDetailLayer/FocusEdge_North"), Is.Not.Null);
            Assert.That(CountTransformsStartingWith("StonePatch_"), Is.GreaterThanOrEqualTo(4));
            Assert.That(CountTransformsStartingWith("GrassTuft_"), Is.GreaterThanOrEqualTo(8));
        }

        [Test]
        public void BuildDefaultCreatesReadableActorLabelsAndWorldHealth()
        {
            var created = CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            var enemyName = GameObject.Find("Proto_CombatField/TrainingEnemy_2000001/ActorNameText");
            var enemyBarFill = GameObject.Find("Proto_CombatField/TrainingEnemy_2000001/WorldHealthBar/Fill");
            var enemyShadow = GameObject.Find("Proto_CombatField/TrainingEnemy_2000001/DropShadow");
            var presenter = created.Hud.GetComponent<PrototypeHudPresenter>();

            Assert.That(GameObject.Find("Proto_CombatField/Player_1000001/ActorNameText"), Is.Not.Null);
            Assert.That(enemyName, Is.Not.Null);
            Assert.That(enemyName.GetComponent<TextMesh>().text, Is.EqualTo("Training Dummy"));
            Assert.That(enemyBarFill, Is.Not.Null);
            Assert.That(enemyShadow, Is.Not.Null);
            Assert.That(presenter.EnemyWorldHealthFill, Is.EqualTo(enemyBarFill.transform));
        }

        [Test]
        public void BuildDefaultCreatesLayeredActorSilhouettesInsteadOfFlatBlocks()
        {
            CombatFieldSceneFactory.BuildDefault(CreateInMemoryAssets());

            Assert.That(GameObject.Find("Proto_CombatField/Player_1000001/VisualRoot/CharacterSprite"), Is.Not.Null);
            Assert.That(GameObject.Find("Proto_CombatField/TrainingEnemy_2000001/VisualRoot/Core"), Is.Not.Null);
            Assert.That(GameObject.Find("Proto_CombatField/TrainingEnemy_2000001/VisualRoot/GuardRim"), Is.Not.Null);
        }

        [Test]
        public void BuildDefaultUsesProvidedPlayerSpriteAsCharacterArt()
        {
            var assets = CreateInMemoryAssets();
            assets.PlayerSprite.name = "PlayerLightSword_Idle_Front";

            CombatFieldSceneFactory.BuildDefault(assets);

            var characterSprite = GameObject.Find("Proto_CombatField/Player_1000001/VisualRoot/CharacterSprite");

            Assert.That(characterSprite, Is.Not.Null);
            var renderer = characterSprite.GetComponent<SpriteRenderer>();
            Assert.That(renderer.sprite, Is.EqualTo(assets.PlayerSprite));
            Assert.That(renderer.color, Is.EqualTo(Color.white));
            Assert.That(characterSprite.transform.localScale.x, Is.GreaterThanOrEqualTo(1.2f));
        }

        [Test]
        public void BuildDefaultWiresPlayerSpriteAnimatorWithAnimationSet()
        {
            var assets = CreateInMemoryAssets();
            assets.PlayerAnimationSet = CreateAnimationSet();

            var created = CombatFieldSceneFactory.BuildDefault(assets);

            var visualAnimator = created.Player.GetComponent<LocalPlayerMotor>().VisualAnimator;
            var hudController = GameObject.Find("Proto_CombatField/FirstChainClient").GetComponent<FirstChainHudController>();

            Assert.That(visualAnimator, Is.Not.Null);
            Assert.That(visualAnimator.AnimationSet.WalkRightA.name, Is.EqualTo("WalkRightA"));
            Assert.That(visualAnimator.AnimationSet.AttackUp.name, Is.EqualTo("AttackUp"));
            Assert.That(visualAnimator.AnimationSet.HitDown.name, Is.EqualTo("HitDown"));
            Assert.That(hudController.PlayerVisualAnimator, Is.EqualTo(visualAnimator));
        }

        private static int CountTransformsStartingWith(string prefix)
        {
            int count = 0;
            var transforms = GameObject.FindObjectsOfType<Transform>();
            foreach (var transform in transforms)
            {
                if (transform.name.StartsWith(prefix))
                {
                    count++;
                }
            }

            return count;
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

        private static PlayerSpriteAnimationSet CreateAnimationSet()
        {
            return new PlayerSpriteAnimationSet
            {
                IdleDown = CreateNamedSprite("IdleDown"),
                IdleUp = CreateNamedSprite("IdleUp"),
                IdleLeft = CreateNamedSprite("IdleLeft"),
                IdleRight = CreateNamedSprite("IdleRight"),
                WalkDownA = CreateNamedSprite("WalkDownA"),
                WalkDownB = CreateNamedSprite("WalkDownB"),
                WalkUpA = CreateNamedSprite("WalkUpA"),
                WalkUpB = CreateNamedSprite("WalkUpB"),
                WalkLeftA = CreateNamedSprite("WalkLeftA"),
                WalkLeftB = CreateNamedSprite("WalkLeftB"),
                WalkRightA = CreateNamedSprite("WalkRightA"),
                WalkRightB = CreateNamedSprite("WalkRightB"),
                AttackDown = CreateNamedSprite("AttackDown"),
                AttackUp = CreateNamedSprite("AttackUp"),
                AttackLeft = CreateNamedSprite("AttackLeft"),
                AttackRight = CreateNamedSprite("AttackRight"),
                HitDown = CreateNamedSprite("HitDown"),
                HitUp = CreateNamedSprite("HitUp"),
                HitLeft = CreateNamedSprite("HitLeft"),
                HitRight = CreateNamedSprite("HitRight")
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

        private static Sprite CreateNamedSprite(string name)
        {
            var sprite = CreateSprite(Color.white);
            sprite.name = name;
            return sprite;
        }
    }
}
