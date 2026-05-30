package com.wenjian.config;

import com.wenjian.config.model.ItemConfig;
import com.wenjian.config.model.MapConfig;
import com.wenjian.config.model.MonsterConfig;
import com.wenjian.config.model.PlayerTemplateConfig;
import com.wenjian.config.model.RegionConfig;
import com.wenjian.config.model.RewardPoolConfig;
import com.wenjian.config.model.RogueConfig;
import com.wenjian.config.model.SkillConfig;
import java.io.IOException;
import java.io.Reader;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;
import org.apache.commons.csv.CSVFormat;
import org.apache.commons.csv.CSVParser;
import org.apache.commons.csv.CSVRecord;

/**
 * Loads and parses game configuration from CSV files under a source directory.
 *
 * <p>Uses Apache Commons CSV for robust parsing. Each table is validated for:
 * <ul>
 *   <li>File existence</li>
 *   <li>Expected headers</li>
 *   <li>Primary key uniqueness</li>
 *   <li>Required field non-emptiness</li>
 *   <li>Strict int/long/boolean parsing</li>
 * </ul>
 *
 * <p>Foreign key validation and positive-number checks are performed by
 * {@link GameConfigRepository} after all tables are loaded.
 */
public final class CsvConfigLoader {

  private CsvConfigLoader() {}

  /**
   * Loads all game configuration tables from the given source directory.
   *
   * @param sourceDir directory containing the CSV files (e.g. config/source/)
   * @return a builder containing loaded data, ready for foreign-key validation
   * @throws ConfigException if a required file is missing or cannot be parsed
   */
  public static GameConfigRepository.Builder load(Path sourceDir) {
    var playerTemplates = loadPlayerTemplates(sourceDir);
    var maps = loadMaps(sourceDir);
    var regions = loadRegions(sourceDir);
    var skills = loadSkills(sourceDir);
    var monsters = loadMonsters(sourceDir);
    var rogueInstances = loadRogues(sourceDir);
    var rewardPools = loadRewardPools(sourceDir);
    var items = loadItems(sourceDir);

    return new GameConfigRepository.Builder()
        .playerTemplates(playerTemplates)
        .maps(maps)
        .regions(regions)
        .skills(skills)
        .monsters(monsters)
        .rogues(rogueInstances)
        .rewardPools(rewardPools)
        .items(items);
  }

  // --- Player Template ---

  private static Map<Integer, PlayerTemplateConfig> loadPlayerTemplates(Path sourceDir) {
    Path file = sourceDir.resolve("player_template.csv");
    requireFileExists(file);
    Map<Integer, PlayerTemplateConfig> result = new HashMap<>();
    Set<Integer> seenKeys = new HashSet<>();
    List<String> expectedHeaders = List.of("template_id", "init_region_id", "spawn_x", "spawn_y",
        "max_hp", "move_speed", "default_skill_id");

    try (Reader reader = Files.newBufferedReader(file);
         CSVParser parser = CSVFormat.DEFAULT
             .builder()
             .setHeader()
             .setSkipHeaderRecord(true)
             .setTrim(true)
             .build()
             .parse(reader)) {
      validateHeaders(parser, file.getFileName().toString(), expectedHeaders);
      for (CSVRecord record : parser) {
        int templateId = parsePositiveInt(record, "template_id", file.getFileName().toString());
        checkDuplicateKey(templateId, seenKeys, file.getFileName().toString(), "template_id");
        int initRegionId = parsePositiveInt(record, "init_region_id", file.getFileName().toString());
        int spawnX = parsePositiveInt(record, "spawn_x", file.getFileName().toString());
        int spawnY = parsePositiveInt(record, "spawn_y", file.getFileName().toString());
        int maxHp = parsePositiveInt(record, "max_hp", file.getFileName().toString());
        int moveSpeed = parsePositiveInt(record, "move_speed", file.getFileName().toString());
        int defaultSkillId = parsePositiveInt(record, "default_skill_id", file.getFileName().toString());

        result.put(templateId, new PlayerTemplateConfig(
            templateId, initRegionId, spawnX, spawnY, maxHp, moveSpeed, defaultSkillId));
        seenKeys.add(templateId);
      }
    } catch (ConfigException e) {
      throw e;
    } catch (IOException e) {
      throw new ConfigException("Failed to read " + file.getFileName(), e);
    }
    return result;
  }

  // --- Map ---

  private static Map<Integer, MapConfig> loadMaps(Path sourceDir) {
    Path file = sourceDir.resolve("map.csv");
    requireFileExists(file);
    Map<Integer, MapConfig> result = new HashMap<>();
    Set<Integer> seenKeys = new HashSet<>();
    List<String> expectedHeaders = List.of("map_id", "width", "height", "collision_ref");

    try (Reader reader = Files.newBufferedReader(file);
         CSVParser parser = CSVFormat.DEFAULT
             .builder()
             .setHeader()
             .setSkipHeaderRecord(true)
             .setTrim(true)
             .build()
             .parse(reader)) {
      validateHeaders(parser, file.getFileName().toString(), expectedHeaders);
      for (CSVRecord record : parser) {
        int mapId = parsePositiveInt(record, "map_id", file.getFileName().toString());
        checkDuplicateKey(mapId, seenKeys, file.getFileName().toString(), "map_id");
        int width = parsePositiveInt(record, "width", file.getFileName().toString());
        int height = parsePositiveInt(record, "height", file.getFileName().toString());
        String collisionRef = parseRequiredString(record, "collision_ref", file.getFileName().toString());

        result.put(mapId, new MapConfig(mapId, width, height, collisionRef));
        seenKeys.add(mapId);
      }
    } catch (ConfigException e) {
      throw e;
    } catch (IOException e) {
      throw new ConfigException("Failed to read " + file.getFileName(), e);
    }
    return result;
  }

  // --- Region ---

  private static Map<Integer, RegionConfig> loadRegions(Path sourceDir) {
    Path file = sourceDir.resolve("region.csv");
    requireFileExists(file);
    Map<Integer, RegionConfig> result = new HashMap<>();
    Set<Integer> seenKeys = new HashSet<>();
    List<String> expectedHeaders = List.of("region_id", "map_id", "region_type", "pk_enabled", "spawn_x", "spawn_y");

    try (Reader reader = Files.newBufferedReader(file);
         CSVParser parser = CSVFormat.DEFAULT
             .builder()
             .setHeader()
             .setSkipHeaderRecord(true)
             .setTrim(true)
             .build()
             .parse(reader)) {
      validateHeaders(parser, file.getFileName().toString(), expectedHeaders);
      for (CSVRecord record : parser) {
        int regionId = parsePositiveInt(record, "region_id", file.getFileName().toString());
        checkDuplicateKey(regionId, seenKeys, file.getFileName().toString(), "region_id");
        int mapId = parsePositiveInt(record, "map_id", file.getFileName().toString());
        String regionType = parseRequiredString(record, "region_type", file.getFileName().toString());
        boolean pkEnabled = parseBoolean(record, "pk_enabled", file.getFileName().toString());
        int spawnX = parsePositiveInt(record, "spawn_x", file.getFileName().toString());
        int spawnY = parsePositiveInt(record, "spawn_y", file.getFileName().toString());

        result.put(regionId, new RegionConfig(regionId, mapId, regionType, pkEnabled, spawnX, spawnY));
        seenKeys.add(regionId);
      }
    } catch (ConfigException e) {
      throw e;
    } catch (IOException e) {
      throw new ConfigException("Failed to read " + file.getFileName(), e);
    }
    return result;
  }

  // --- Skill ---

  private static Map<Integer, SkillConfig> loadSkills(Path sourceDir) {
    Path file = sourceDir.resolve("skill.csv");
    requireFileExists(file);
    Map<Integer, SkillConfig> result = new HashMap<>();
    Set<Integer> seenKeys = new HashSet<>();
    List<String> expectedHeaders = List.of("skill_id", "cooldown_ms", "cast_ms", "hit_shape", "range", "radius", "damage");

    try (Reader reader = Files.newBufferedReader(file);
         CSVParser parser = CSVFormat.DEFAULT
             .builder()
             .setHeader()
             .setSkipHeaderRecord(true)
             .setTrim(true)
             .build()
             .parse(reader)) {
      validateHeaders(parser, file.getFileName().toString(), expectedHeaders);
      for (CSVRecord record : parser) {
        int skillId = parsePositiveInt(record, "skill_id", file.getFileName().toString());
        checkDuplicateKey(skillId, seenKeys, file.getFileName().toString(), "skill_id");
        int cooldownMs = parsePositiveInt(record, "cooldown_ms", file.getFileName().toString());
        int castMs = parsePositiveInt(record, "cast_ms", file.getFileName().toString());
        String hitShape = parseRequiredString(record, "hit_shape", file.getFileName().toString());
        int range = parseNonNegativeInt(record, "range", file.getFileName().toString());
        int radius = parseNonNegativeInt(record, "radius", file.getFileName().toString());
        int damage = parsePositiveInt(record, "damage", file.getFileName().toString());

        result.put(skillId, new SkillConfig(skillId, cooldownMs, castMs, hitShape, range, radius, damage));
        seenKeys.add(skillId);
      }
    } catch (ConfigException e) {
      throw e;
    } catch (IOException e) {
      throw new ConfigException("Failed to read " + file.getFileName(), e);
    }
    return result;
  }

  // --- Monster ---

  private static Map<Integer, MonsterConfig> loadMonsters(Path sourceDir) {
    Path file = sourceDir.resolve("monster.csv");
    requireFileExists(file);
    Map<Integer, MonsterConfig> result = new HashMap<>();
    Set<Integer> seenKeys = new HashSet<>();
    List<String> expectedHeaders = List.of("monster_id", "max_hp", "move_speed");

    try (Reader reader = Files.newBufferedReader(file);
         CSVParser parser = CSVFormat.DEFAULT
             .builder()
             .setHeader()
             .setSkipHeaderRecord(true)
             .setTrim(true)
             .build()
             .parse(reader)) {
      validateHeaders(parser, file.getFileName().toString(), expectedHeaders);
      for (CSVRecord record : parser) {
        int monsterId = parsePositiveInt(record, "monster_id", file.getFileName().toString());
        checkDuplicateKey(monsterId, seenKeys, file.getFileName().toString(), "monster_id");
        int maxHp = parsePositiveInt(record, "max_hp", file.getFileName().toString());
        int moveSpeed = parsePositiveInt(record, "move_speed", file.getFileName().toString());

        result.put(monsterId, new MonsterConfig(monsterId, maxHp, moveSpeed));
        seenKeys.add(monsterId);
      }
    } catch (ConfigException e) {
      throw e;
    } catch (IOException e) {
      throw new ConfigException("Failed to read " + file.getFileName(), e);
    }
    return result;
  }

  // --- Rogue ---

  private static Map<Integer, RogueConfig> loadRogues(Path sourceDir) {
    Path file = sourceDir.resolve("rogue.csv");
    requireFileExists(file);
    Map<Integer, RogueConfig> result = new HashMap<>();
    Set<Integer> seenKeys = new HashSet<>();
    List<String> expectedHeaders = List.of("rogue_id", "map_id", "spawn_x", "spawn_y", "monster_id", "monster_count", "reward_pool_id");

    try (Reader reader = Files.newBufferedReader(file);
         CSVParser parser = CSVFormat.DEFAULT
             .builder()
             .setHeader()
             .setSkipHeaderRecord(true)
             .setTrim(true)
             .build()
             .parse(reader)) {
      validateHeaders(parser, file.getFileName().toString(), expectedHeaders);
      for (CSVRecord record : parser) {
        int rogueId = parsePositiveInt(record, "rogue_id", file.getFileName().toString());
        checkDuplicateKey(rogueId, seenKeys, file.getFileName().toString(), "rogue_id");
        int mapId = parsePositiveInt(record, "map_id", file.getFileName().toString());
        int spawnX = parsePositiveInt(record, "spawn_x", file.getFileName().toString());
        int spawnY = parsePositiveInt(record, "spawn_y", file.getFileName().toString());
        int monsterId = parsePositiveInt(record, "monster_id", file.getFileName().toString());
        int monsterCount = parsePositiveInt(record, "monster_count", file.getFileName().toString());
        int rewardPoolId = parsePositiveInt(record, "reward_pool_id", file.getFileName().toString());

        result.put(rogueId, new RogueConfig(rogueId, mapId, spawnX, spawnY, monsterId, monsterCount, rewardPoolId));
        seenKeys.add(rogueId);
      }
    } catch (ConfigException e) {
      throw e;
    } catch (IOException e) {
      throw new ConfigException("Failed to read " + file.getFileName(), e);
    }
    return result;
  }

  // --- Reward Pool ---

  private static Map<Integer, RewardPoolConfig> loadRewardPools(Path sourceDir) {
    Path file = sourceDir.resolve("reward_pool.csv");
    requireFileExists(file);
    Map<Integer, RewardPoolConfig> result = new HashMap<>();
    Set<Integer> seenKeys = new HashSet<>();
    List<String> expectedHeaders = List.of("reward_pool_id", "item_id", "count");

    try (Reader reader = Files.newBufferedReader(file);
         CSVParser parser = CSVFormat.DEFAULT
             .builder()
             .setHeader()
             .setSkipHeaderRecord(true)
             .setTrim(true)
             .build()
             .parse(reader)) {
      validateHeaders(parser, file.getFileName().toString(), expectedHeaders);
      for (CSVRecord record : parser) {
        int rewardPoolId = parsePositiveInt(record, "reward_pool_id", file.getFileName().toString());
        checkDuplicateKey(rewardPoolId, seenKeys, file.getFileName().toString(), "reward_pool_id");
        int itemId = parsePositiveInt(record, "item_id", file.getFileName().toString());
        int count = parsePositiveInt(record, "count", file.getFileName().toString());

        result.put(rewardPoolId, new RewardPoolConfig(rewardPoolId, itemId, count));
        seenKeys.add(rewardPoolId);
      }
    } catch (ConfigException e) {
      throw e;
    } catch (IOException e) {
      throw new ConfigException("Failed to read " + file.getFileName(), e);
    }
    return result;
  }

  // --- Item ---

  private static Map<Integer, ItemConfig> loadItems(Path sourceDir) {
    Path file = sourceDir.resolve("item.csv");
    requireFileExists(file);
    Map<Integer, ItemConfig> result = new HashMap<>();
    Set<Integer> seenKeys = new HashSet<>();
    List<String> expectedHeaders = List.of("item_id", "name", "type");

    try (Reader reader = Files.newBufferedReader(file);
         CSVParser parser = CSVFormat.DEFAULT
             .builder()
             .setHeader()
             .setSkipHeaderRecord(true)
             .setTrim(true)
             .build()
             .parse(reader)) {
      validateHeaders(parser, file.getFileName().toString(), expectedHeaders);
      for (CSVRecord record : parser) {
        int itemId = parsePositiveInt(record, "item_id", file.getFileName().toString());
        checkDuplicateKey(itemId, seenKeys, file.getFileName().toString(), "item_id");
        String name = parseRequiredString(record, "name", file.getFileName().toString());
        String type = parseRequiredString(record, "type", file.getFileName().toString());

        result.put(itemId, new ItemConfig(itemId, name, type));
        seenKeys.add(itemId);
      }
    } catch (ConfigException e) {
      throw e;
    } catch (IOException e) {
      throw new ConfigException("Failed to read " + file.getFileName(), e);
    }
    return result;
  }

  // --- Shared parsing helpers ---

  private static void requireFileExists(Path path) {
    if (!Files.exists(path)) {
      throw new ConfigException("Required config file not found: " + path.getFileName());
    }
  }

  private static void validateHeaders(CSVParser parser, String fileName, List<String> expected) {
    Map<String, Integer> headers = parser.getHeaderMap();
    for (String field : expected) {
      if (!headers.containsKey(field)) {
        throw new ConfigException("Missing required column '" + field + "' in " + fileName
            + ". Expected: " + expected);
      }
    }
  }

  private static void checkDuplicateKey(int key, Set<Integer> seen, String fileName, String fieldName) {
    if (!seen.add(key)) {
      throw new ConfigException("Duplicate primary key", fileName, fieldName, String.valueOf(key));
    }
  }

  static int parsePositiveInt(CSVRecord record, String field, String fileName) {
    String raw = requireNonEmpty(record, field, fileName);
    try {
      int value = Integer.parseInt(raw);
      if (value <= 0) {
        throw new ConfigException("Value must be positive", fileName, field, raw);
      }
      return value;
    } catch (NumberFormatException e) {
      throw new ConfigException("Invalid integer", fileName, field, raw);
    }
  }

  static int parseNonNegativeInt(CSVRecord record, String field, String fileName) {
    String raw = requireNonEmpty(record, field, fileName);
    try {
      int value = Integer.parseInt(raw);
      if (value < 0) {
        throw new ConfigException("Value must be non-negative", fileName, field, raw);
      }
      return value;
    } catch (NumberFormatException e) {
      throw new ConfigException("Invalid integer", fileName, field, raw);
    }
  }

  static boolean parseBoolean(CSVRecord record, String field, String fileName) {
    String raw = requireNonEmpty(record, field, fileName);
    if ("true".equals(raw)) {
      return true;
    }
    if ("false".equals(raw)) {
      return false;
    }
    throw new ConfigException("Invalid boolean value (must be 'true' or 'false')", fileName, field, raw);
  }

  static String parseRequiredString(CSVRecord record, String field, String fileName) {
    String raw = requireNonEmpty(record, field, fileName);
    return raw;
  }

  private static String requireNonEmpty(CSVRecord record, String field, String fileName) {
    if (!record.isMapped(field) || !record.isSet(field)) {
      throw new ConfigException("Missing required field", fileName, field, null);
    }
    String value = record.get(field);
    if (value == null || value.isEmpty()) {
      throw new ConfigException("Field cannot be empty", fileName, field, null);
    }
    return value;
  }
}
