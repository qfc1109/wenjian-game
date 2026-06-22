using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Wenjian.Client.Presentation;
using Wenjian.Client.Prototype;

namespace Wenjian.Client.Editor.Prototype
{
    public static class ProtoCombatFieldSceneGenerator
    {
        private const string ScenePath = "Assets/_Wenjian/Scenes/Proto_CombatField.unity";
        private const string PlaceholderSpriteDir = "Assets/_Wenjian/Art/Placeholders";
        private const string TileAssetDir = "Assets/_Wenjian/Art/Tiles";
        private const string CharacterSpriteDir = "Assets/_Wenjian/Art/Characters";
        private const string PlayerLightSwordSpritePath = CharacterSpriteDir + "/PlayerLightSword_Idle_Front.png";
        private const float PlayerSpritePixelsPerUnit = 48f;

        private enum PlayerSpriteDirection
        {
            Down,
            Up,
            Left,
            Right
        }

        private enum PlayerSpritePose
        {
            Idle,
            WalkA,
            WalkB,
            Attack,
            Hit
        }

        [MenuItem("Wenjian/Prototype/Rebuild Proto Combat Field Scene")]
        public static void BuildDefaultScene()
        {
            EnsureAssetDirectories();

            var playerAnimationSet = LoadOrCreatePlayerLightSwordAnimationSet();
            var assets = new CombatFieldSceneAssets
            {
                GroundTile = LoadOrCreateTile("BambooGround", new Color32(67, 116, 66, 255)),
                BoundaryTile = LoadOrCreateTile("BambooBoundary", new Color32(35, 60, 38, 255)),
                ObstacleTile = LoadOrCreateTile("BambooObstacle", new Color32(49, 101, 55, 255)),
                PlayerSprite = playerAnimationSet.IdleDown,
                PlayerAnimationSet = playerAnimationSet,
                TrainingEnemySprite = LoadOrCreateSprite("TrainingEnemyPlaceholder", new Color32(210, 66, 58, 255))
            };

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CombatFieldSceneFactory.BuildDefault(assets);

            EnsureDirectoryForAsset(ScenePath);
            var activeScene = SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(activeScene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Generated proto combat field scene at {ScenePath}");
        }

        private static void EnsureAssetDirectories()
        {
            EnsureDirectoryForAsset($"{PlaceholderSpriteDir}/.keep");
            EnsureDirectoryForAsset($"{TileAssetDir}/.keep");
            EnsureDirectoryForAsset($"{CharacterSpriteDir}/.keep");
            EnsureDirectoryForAsset(ScenePath);
        }

        private static Tile LoadOrCreateTile(string name, Color32 color)
        {
            string spritePath = $"{PlaceholderSpriteDir}/{name}.png";
            string tilePath = $"{TileAssetDir}/{name}.asset";
            Sprite sprite = LoadOrCreateSpriteAtPath(spritePath, color);
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                EnsureDirectoryForAsset(tilePath);
                AssetDatabase.CreateAsset(tile, tilePath);
            }

            tile.sprite = sprite;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static Sprite LoadOrCreateSprite(string name, Color32 color)
        {
            return LoadOrCreateSpriteAtPath($"{PlaceholderSpriteDir}/{name}.png", color);
        }

        private static Sprite LoadOrCreatePlayerLightSwordSprite()
        {
            string fullPath = ToFullPath(PlayerLightSwordSpritePath);
            if (!File.Exists(fullPath))
            {
                CreatePlayerLightSwordSpritePng(PlayerLightSwordSpritePath);
            }

            AssetDatabase.ImportAsset(PlayerLightSwordSpritePath);
            ConfigureSpriteImporter(PlayerLightSwordSpritePath, pixelsPerUnit: PlayerSpritePixelsPerUnit);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlayerLightSwordSpritePath);
            if (sprite != null)
            {
                sprite.name = "PlayerLightSword_Idle_Front";
            }

            return sprite;
        }

        private static PlayerSpriteAnimationSet LoadOrCreatePlayerLightSwordAnimationSet()
        {
            Sprite idleDown = LoadOrCreatePlayerLightSwordSprite();
            var baseTexture = LoadTextureFromAsset(PlayerLightSwordSpritePath);

            try
            {
                return new PlayerSpriteAnimationSet
                {
                    IdleDown = idleDown,
                    IdleUp = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Idle_Up", PlayerSpriteDirection.Up, PlayerSpritePose.Idle),
                    IdleLeft = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Idle_Left", PlayerSpriteDirection.Left, PlayerSpritePose.Idle),
                    IdleRight = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Idle_Right", PlayerSpriteDirection.Right, PlayerSpritePose.Idle),
                    WalkDownA = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Walk_Down_A", PlayerSpriteDirection.Down, PlayerSpritePose.WalkA),
                    WalkDownB = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Walk_Down_B", PlayerSpriteDirection.Down, PlayerSpritePose.WalkB),
                    WalkUpA = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Walk_Up_A", PlayerSpriteDirection.Up, PlayerSpritePose.WalkA),
                    WalkUpB = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Walk_Up_B", PlayerSpriteDirection.Up, PlayerSpritePose.WalkB),
                    WalkLeftA = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Walk_Left_A", PlayerSpriteDirection.Left, PlayerSpritePose.WalkA),
                    WalkLeftB = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Walk_Left_B", PlayerSpriteDirection.Left, PlayerSpritePose.WalkB),
                    WalkRightA = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Walk_Right_A", PlayerSpriteDirection.Right, PlayerSpritePose.WalkA),
                    WalkRightB = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Walk_Right_B", PlayerSpriteDirection.Right, PlayerSpritePose.WalkB),
                    AttackDown = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Attack_Down", PlayerSpriteDirection.Down, PlayerSpritePose.Attack),
                    AttackUp = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Attack_Up", PlayerSpriteDirection.Up, PlayerSpritePose.Attack),
                    AttackLeft = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Attack_Left", PlayerSpriteDirection.Left, PlayerSpritePose.Attack),
                    AttackRight = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Attack_Right", PlayerSpriteDirection.Right, PlayerSpritePose.Attack),
                    HitDown = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Hit_Down", PlayerSpriteDirection.Down, PlayerSpritePose.Hit),
                    HitUp = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Hit_Up", PlayerSpriteDirection.Up, PlayerSpritePose.Hit),
                    HitLeft = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Hit_Left", PlayerSpriteDirection.Left, PlayerSpritePose.Hit),
                    HitRight = LoadOrCreatePlayerLightSwordVariant(baseTexture, "PlayerLightSword_Hit_Right", PlayerSpriteDirection.Right, PlayerSpritePose.Hit)
                };
            }
            finally
            {
                Object.DestroyImmediate(baseTexture);
            }
        }

        private static Sprite LoadOrCreatePlayerLightSwordVariant(Texture2D baseTexture, string name, PlayerSpriteDirection direction, PlayerSpritePose pose)
        {
            string assetPath = $"{CharacterSpriteDir}/{name}.png";
            var output = CreatePlayerLightSwordVariantTexture(baseTexture, direction, pose);
            EnsureDirectoryForAsset(assetPath);
            File.WriteAllBytes(ToFullPath(assetPath), output.EncodeToPNG());
            Object.DestroyImmediate(output);

            AssetDatabase.ImportAsset(assetPath);
            ConfigureSpriteImporter(assetPath, pixelsPerUnit: PlayerSpritePixelsPerUnit);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null)
            {
                sprite.name = name;
            }

            return sprite;
        }

        private static Texture2D LoadTextureFromAsset(string assetPath)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(ToFullPath(assetPath))))
            {
                Object.DestroyImmediate(texture);
                throw new IOException($"Failed to load sprite texture: {assetPath}");
            }

            return texture;
        }

        private static Texture2D CreatePlayerLightSwordVariantTexture(Texture2D source, PlayerSpriteDirection direction, PlayerSpritePose pose)
        {
            int width = source.width;
            int height = source.height;
            var output = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Clear(output);

            bool mirror = direction == PlayerSpriteDirection.Right;
            int xOffset = direction switch
            {
                PlayerSpriteDirection.Left => -2,
                PlayerSpriteDirection.Right => 2,
                _ => 0
            };
            int yOffset = pose == PlayerSpritePose.WalkB ? 1 : 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color32 pixel = source.GetPixel(x, y);
                    if (pixel.a == 0)
                    {
                        continue;
                    }

                    int targetX = (mirror ? width - 1 - x : x) + xOffset;
                    int targetY = y + yOffset;
                    if (targetX < 0 || targetX >= width || targetY < 0 || targetY >= height)
                    {
                        continue;
                    }

                    output.SetPixel(targetX, targetY, TransformPlayerPixel(pixel, direction, pose, y, height));
                }
            }

            if (pose == PlayerSpritePose.Attack)
            {
                DrawAttackArc(output, direction);
            }

            output.Apply();
            return output;
        }

        private static Color TransformPlayerPixel(Color32 pixel, PlayerSpriteDirection direction, PlayerSpritePose pose, int y, int height)
        {
            Color color = pixel;
            if (direction == PlayerSpriteDirection.Up)
            {
                float upperBody = y > height * 0.45f ? 0.78f : 0.92f;
                color = new Color(color.r * upperBody, color.g * upperBody, color.b * (upperBody + 0.08f), color.a);
            }

            if (pose == PlayerSpritePose.WalkB)
            {
                color = new Color(
                    Mathf.Clamp01(color.r * 1.03f),
                    Mathf.Clamp01(color.g * 1.03f),
                    Mathf.Clamp01(color.b * 1.05f),
                    color.a);
            }

            if (pose == PlayerSpritePose.Attack)
            {
                color = new Color(
                    Mathf.Clamp01(color.r * 1.10f),
                    Mathf.Clamp01(color.g * 1.12f),
                    Mathf.Clamp01(color.b * 1.18f),
                    color.a);
            }

            if (pose == PlayerSpritePose.Hit)
            {
                color = new Color(
                    Mathf.Clamp01(color.r * 1.35f + 0.18f),
                    Mathf.Clamp01(color.g * 0.66f + 0.08f),
                    Mathf.Clamp01(color.b * 0.66f + 0.08f),
                    color.a);
            }

            return color;
        }

        private static void DrawAttackArc(Texture2D texture, PlayerSpriteDirection direction)
        {
            var slashColor = new Color32(150, 236, 255, 230);
            Vector2Int start = direction switch
            {
                PlayerSpriteDirection.Up => new Vector2Int(30, 34),
                PlayerSpriteDirection.Left => new Vector2Int(27, 31),
                PlayerSpriteDirection.Right => new Vector2Int(37, 31),
                _ => new Vector2Int(33, 28)
            };
            Vector2Int end = direction switch
            {
                PlayerSpriteDirection.Up => new Vector2Int(31, 55),
                PlayerSpriteDirection.Left => new Vector2Int(8, 34),
                PlayerSpriteDirection.Right => new Vector2Int(55, 34),
                _ => new Vector2Int(31, 8)
            };

            DrawLine(texture, start, end, slashColor, radius: 1);
            DrawLine(texture, start + new Vector2Int(2, 0), end + new Vector2Int(2, 0), new Color32(230, 255, 255, 190), radius: 0);
        }

        private static void DrawLine(Texture2D texture, Vector2Int start, Vector2Int end, Color color, int radius)
        {
            int dx = Mathf.Abs(end.x - start.x);
            int dy = Mathf.Abs(end.y - start.y);
            int steps = Mathf.Max(dx, dy);
            if (steps == 0)
            {
                SetPixelSafe(texture, start.x, start.y, color);
                return;
            }

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(start.x, end.x, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(start.y, end.y, t));
                for (int oy = -radius; oy <= radius; oy++)
                {
                    for (int ox = -radius; ox <= radius; ox++)
                    {
                        SetPixelSafe(texture, x + ox, y + oy, color);
                    }
                }
            }
        }

        private static void SetPixelSafe(Texture2D texture, int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
            {
                return;
            }

            texture.SetPixel(x, y, color);
        }

        private static void Clear(Texture2D texture)
        {
            var clear = new Color32(0, 0, 0, 0);
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, clear);
                }
            }
        }

        private static Sprite LoadOrCreateSpriteAtPath(string assetPath, Color32 color)
        {
            string fullPath = ToFullPath(assetPath);
            if (!File.Exists(fullPath))
            {
                EnsureDirectoryForAsset(assetPath);
                var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                for (int y = 0; y < texture.height; y++)
                {
                    for (int x = 0; x < texture.width; x++)
                    {
                        bool edge = x == 0 || y == 0 || x == texture.width - 1 || y == texture.height - 1;
                        texture.SetPixel(x, y, edge ? Darken(color) : color);
                    }
                }

                texture.Apply();
                File.WriteAllBytes(fullPath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(assetPath);
            ConfigureSpriteImporter(assetPath, pixelsPerUnit: 32f);
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static void CreatePlayerLightSwordSpritePng(string assetPath)
        {
            string sourcePath = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(Application.dataPath),
                "..",
                "..",
                "ArtConcepts",
                "round2-visual-standards",
                "images",
                "08-character-spec-standard.png"));
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Character standard image is required to create player sprite.", sourcePath);
            }

            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(source, File.ReadAllBytes(sourcePath)))
            {
                Object.DestroyImmediate(source);
                throw new IOException($"Failed to load character standard image: {sourcePath}");
            }

            var output = ExtractCharacterSprite(source, new RectInt(315, 145, 85, 110), 64, 64);
            EnsureDirectoryForAsset(assetPath);
            File.WriteAllBytes(ToFullPath(assetPath), output.EncodeToPNG());
            Object.DestroyImmediate(output);
            Object.DestroyImmediate(source);
        }

        private static Texture2D ExtractCharacterSprite(Texture2D source, RectInt crop, int outputWidth, int outputHeight)
        {
            var masked = new Color32[crop.width, crop.height];
            int minX = crop.width;
            int minY = crop.height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < crop.height; y++)
            {
                for (int x = 0; x < crop.width; x++)
                {
                    int sourceX = crop.x + x;
                    int sourceY = source.height - 1 - (crop.y + y);
                    Color32 pixel = source.GetPixel(sourceX, sourceY);
                    if (IsReferenceBackground(pixel))
                    {
                        masked[x, y] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    masked[x, y] = new Color32(pixel.r, pixel.g, pixel.b, 255);
                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
            }

            if (maxX < minX || maxY < minY)
            {
                throw new IOException("Character crop did not contain visible pixels.");
            }

            int contentWidth = maxX - minX + 1;
            int contentHeight = maxY - minY + 1;
            float scale = Mathf.Min(56f / contentWidth, 58f / contentHeight);
            int drawWidth = Mathf.Max(1, Mathf.RoundToInt(contentWidth * scale));
            int drawHeight = Mathf.Max(1, Mathf.RoundToInt(contentHeight * scale));
            int offsetX = Mathf.RoundToInt((outputWidth - drawWidth) * 0.5f);
            int offsetY = 3;

            var output = new Texture2D(outputWidth, outputHeight, TextureFormat.RGBA32, false);
            var clear = new Color32(0, 0, 0, 0);
            for (int y = 0; y < outputHeight; y++)
            {
                for (int x = 0; x < outputWidth; x++)
                {
                    output.SetPixel(x, y, clear);
                }
            }

            for (int y = 0; y < drawHeight; y++)
            {
                for (int x = 0; x < drawWidth; x++)
                {
                    int sourceX = minX + Mathf.Min(contentWidth - 1, Mathf.FloorToInt(x / scale));
                    int sourceY = minY + Mathf.Min(contentHeight - 1, Mathf.FloorToInt(y / scale));
                    int targetX = offsetX + x;
                    int targetY = outputHeight - 1 - (offsetY + y);
                    output.SetPixel(targetX, targetY, masked[sourceX, sourceY]);
                }
            }

            output.Apply();
            return output;
        }

        private static bool IsReferenceBackground(Color32 pixel)
        {
            return pixel.r > 172
                && pixel.g > 160
                && pixel.b > 126
                && Mathf.Abs(pixel.r - pixel.g) < 52
                && Mathf.Abs(pixel.g - pixel.b) < 74;
        }

        private static void ConfigureSpriteImporter(string assetPath, float pixelsPerUnit)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            bool changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (!Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit))
            {
                importer.spritePixelsPerUnit = pixelsPerUnit;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }

            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static Color32 Darken(Color32 color)
        {
            return new Color32(
                (byte)(color.r * 0.65f),
                (byte)(color.g * 0.65f),
                (byte)(color.b * 0.65f),
                color.a);
        }

        private static void EnsureDirectoryForAsset(string assetPath)
        {
            string directory = Path.GetDirectoryName(ToFullPath(assetPath));
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static string ToFullPath(string assetPath)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string relativePath = assetPath.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(projectRoot, relativePath);
        }
    }
}
