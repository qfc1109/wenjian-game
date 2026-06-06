using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Wenjian.Client.Prototype;

namespace Wenjian.Client.Editor.Prototype
{
    public static class ProtoCombatFieldSceneGenerator
    {
        private const string ScenePath = "Assets/_Wenjian/Scenes/Proto_CombatField.unity";
        private const string PlaceholderSpriteDir = "Assets/_Wenjian/Art/Placeholders";
        private const string TileAssetDir = "Assets/_Wenjian/Art/Tiles";

        [MenuItem("Wenjian/Prototype/Rebuild Proto Combat Field Scene")]
        public static void BuildDefaultScene()
        {
            EnsureAssetDirectories();

            var assets = new CombatFieldSceneAssets
            {
                GroundTile = LoadOrCreateTile("BambooGround", new Color32(67, 116, 66, 255)),
                BoundaryTile = LoadOrCreateTile("BambooBoundary", new Color32(35, 60, 38, 255)),
                ObstacleTile = LoadOrCreateTile("BambooObstacle", new Color32(49, 101, 55, 255)),
                PlayerSprite = LoadOrCreateSprite("PlayerPlaceholder", new Color32(62, 166, 232, 255)),
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
            ConfigureSpriteImporter(assetPath);
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static void ConfigureSpriteImporter(string assetPath)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            bool changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (!Mathf.Approximately(importer.spritePixelsPerUnit, 32f))
            {
                importer.spritePixelsPerUnit = 32f;
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
