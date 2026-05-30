package com.wenjian.config;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.wenjian.config.model.ItemConfig;
import com.wenjian.config.model.MapConfig;
import com.wenjian.config.model.MonsterConfig;
import com.wenjian.config.model.PlayerTemplateConfig;
import com.wenjian.config.model.RegionConfig;
import com.wenjian.config.model.RewardPoolConfig;
import com.wenjian.config.model.RogueConfig;
import com.wenjian.config.model.SkillConfig;
import java.io.IOException;
import java.io.InputStream;
import java.nio.file.Files;
import java.nio.file.Path;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

class CsvConfigLoaderTest {

  @TempDir
  Path tempDir;

  // --- Real data load tests ---

  @Test
  void loadRealConfigSource_succeeds() {
    GameConfigRepository repo = loadAllFixtures(tempDir);

    assertFalse(repo.allPlayerTemplates().isEmpty());
    assertFalse(repo.allMaps().isEmpty());
    assertFalse(repo.allRegions().isEmpty());
    assertFalse(repo.allSkills().isEmpty());
    assertFalse(repo.allMonsters().isEmpty());
    assertFalse(repo.allRogues().isEmpty());
    assertFalse(repo.allRewardPools().isEmpty());
    assertFalse(repo.allItems().isEmpty());
  }

  @Test
  void loadRealConfig_playerTemplate_fieldsCorrect() {
    GameConfigRepository repo = loadAllFixtures(tempDir);
    PlayerTemplateConfig pt = repo.requirePlayerTemplate(1);

    assertEquals(1, pt.templateId());
    assertEquals(1001, pt.initRegionId());
    assertEquals(3200, pt.spawnX());
    assertEquals(2400, pt.spawnY());
    assertEquals(100, pt.maxHp());
    assertEquals(420, pt.moveSpeed());
    assertEquals(2001, pt.defaultSkillId());
  }

  @Test
  void loadRealConfig_region_fieldsCorrect() {
    GameConfigRepository repo = loadAllFixtures(tempDir);
    RegionConfig region = repo.requireRegion(1001);

    assertEquals(1001, region.regionId());
    assertEquals(1001, region.mapId());
    assertEquals("TOWN", region.regionType());
    assertFalse(region.pkEnabled());
    assertEquals(3200, region.spawnX());
    assertEquals(2400, region.spawnY());
  }

  @Test
  void loadRealConfig_skill_fieldsCorrect() {
    GameConfigRepository repo = loadAllFixtures(tempDir);
    SkillConfig skill = repo.requireSkill(2001);

    assertEquals(2001, skill.skillId());
    assertEquals(600, skill.cooldownMs());
    assertEquals(180, skill.castMs());
    assertEquals("ARC", skill.hitShape());
    assertEquals(900, skill.range());
    assertEquals(240, skill.radius());
    assertEquals(12, skill.damage());
  }

  @Test
  void loadRealConfig_monster_fieldsCorrect() {
    GameConfigRepository repo = loadAllFixtures(tempDir);
    MonsterConfig monster = repo.requireMonster(3001);

    assertEquals(3001, monster.monsterId());
    assertEquals(60, monster.maxHp());
    assertEquals(300, monster.moveSpeed());
  }

  @Test
  void loadRealConfig_rogue_fieldsCorrect() {
    GameConfigRepository repo = loadAllFixtures(tempDir);
    RogueConfig rogue = repo.requireRogue(4001);

    assertEquals(4001, rogue.rogueId());
    assertEquals(2001, rogue.mapId());
    assertEquals(1500, rogue.spawnX());
    assertEquals(1500, rogue.spawnY());
    assertEquals(3001, rogue.monsterId());
    assertEquals(8, rogue.monsterCount());
    assertEquals(5001, rogue.rewardPoolId());
  }

  @Test
  void loadRealConfig_rewardPool_fieldsCorrect() {
    GameConfigRepository repo = loadAllFixtures(tempDir);
    RewardPoolConfig pool = repo.requireRewardPool(5001);

    assertEquals(5001, pool.rewardPoolId());
    assertEquals(6001, pool.itemId());
    assertEquals(3, pool.count());
  }

  @Test
  void loadRealConfig_item_fieldsCorrect() {
    GameConfigRepository repo = loadAllFixtures(tempDir);
    ItemConfig item = repo.requireItem(6001);

    assertEquals(6001, item.itemId());
    assertEquals("试炼铜钱", item.name());
    assertEquals("CURRENCY", item.type());
  }

  // --- Error tests ---

  @Test
  void load_missingFile_throwsConfigException() {
    ConfigException ex = assertThrows(ConfigException.class,
        () -> CsvConfigLoader.load(tempDir).build());
    assertTrue(ex.getMessage().contains("not found"));
  }

  @Test
  void load_duplicatePrimaryKey_throwsConfigException() {
    copyFixture("player_template_dup_key.csv", "player_template.csv");
    copyFixture("map.csv", "map.csv");
    copyFixture("region.csv", "region.csv");
    copyFixture("skill.csv", "skill.csv");
    copyFixture("monster.csv", "monster.csv");
    copyFixture("rogue.csv", "rogue.csv");
    copyFixture("reward_pool.csv", "reward_pool.csv");
    copyFixture("item.csv", "item.csv");

    ConfigException ex = assertThrows(ConfigException.class,
        () -> CsvConfigLoader.load(tempDir).build());
    assertTrue(ex.getMessage().contains("Duplicate primary key"));
  }

  @Test
  void load_missingForeignKey_throwsConfigException() {
    copyFixture("player_template_missing_fk.csv", "player_template.csv");
    copyFixture("map.csv", "map.csv");
    copyFixture("region.csv", "region.csv");
    copyFixture("skill.csv", "skill.csv");
    copyFixture("monster.csv", "monster.csv");
    copyFixture("rogue.csv", "rogue.csv");
    copyFixture("reward_pool.csv", "reward_pool.csv");
    copyFixture("item.csv", "item.csv");

    ConfigException ex = assertThrows(ConfigException.class,
        () -> CsvConfigLoader.load(tempDir).build());
    assertTrue(ex.getMessage().contains("references non-existent region"));
  }

  @Test
  void load_invalidInteger_throwsConfigException() {
    copyFixture("player_template_invalid_int.csv", "player_template.csv");
    copyFixture("map.csv", "map.csv");
    copyFixture("region.csv", "region.csv");
    copyFixture("skill.csv", "skill.csv");
    copyFixture("monster.csv", "monster.csv");
    copyFixture("rogue.csv", "rogue.csv");
    copyFixture("reward_pool.csv", "reward_pool.csv");
    copyFixture("item.csv", "item.csv");

    ConfigException ex = assertThrows(ConfigException.class,
        () -> CsvConfigLoader.load(tempDir).build());
    assertTrue(ex.getMessage().contains("Invalid integer"));
  }

  @Test
  void load_negativeInteger_throwsConfigException() {
    copyFixture("player_template_negative_int.csv", "player_template.csv");
    copyFixture("map.csv", "map.csv");
    copyFixture("region.csv", "region.csv");
    copyFixture("skill.csv", "skill.csv");
    copyFixture("monster.csv", "monster.csv");
    copyFixture("rogue.csv", "rogue.csv");
    copyFixture("reward_pool.csv", "reward_pool.csv");
    copyFixture("item.csv", "item.csv");

    ConfigException ex = assertThrows(ConfigException.class,
        () -> CsvConfigLoader.load(tempDir).build());
    assertTrue(ex.getMessage().contains("must be positive"));
  }

  @Test
  void load_missingRequiredField_throwsConfigException() {
    copyFixture("player_template_missing_field.csv", "player_template.csv");
    copyFixture("map.csv", "map.csv");
    copyFixture("region.csv", "region.csv");
    copyFixture("skill.csv", "skill.csv");
    copyFixture("monster.csv", "monster.csv");
    copyFixture("rogue.csv", "rogue.csv");
    copyFixture("reward_pool.csv", "reward_pool.csv");
    copyFixture("item.csv", "item.csv");

    ConfigException ex = assertThrows(ConfigException.class,
        () -> CsvConfigLoader.load(tempDir).build());
    assertTrue(ex.getMessage().contains("Missing required field"));
  }

  @Test
  void load_invalidForeignKey_throwsConfigException() {
    copyFixture("player_template_invalid_fk.csv", "player_template.csv");
    copyFixture("map.csv", "map.csv");
    copyFixture("region.csv", "region.csv");
    copyFixture("skill.csv", "skill.csv");
    copyFixture("monster.csv", "monster.csv");
    copyFixture("rogue.csv", "rogue.csv");
    copyFixture("reward_pool.csv", "reward_pool.csv");
    copyFixture("item.csv", "item.csv");

    ConfigException ex = assertThrows(ConfigException.class,
        () -> CsvConfigLoader.load(tempDir).build());
    assertTrue(ex.getMessage().contains("Invalid integer"));
  }

  @Test
  void load_rogueMissingMapFk_throwsConfigException() {
    copyFixture("player_template.csv", "player_template.csv");
    copyFixture("map.csv", "map.csv");
    copyFixture("region.csv", "region.csv");
    copyFixture("skill.csv", "skill.csv");
    copyFixture("monster.csv", "monster.csv");
    copyFixture("rogue_missing_map_fk.csv", "rogue.csv");
    copyFixture("reward_pool.csv", "reward_pool.csv");
    copyFixture("item.csv", "item.csv");

    ConfigException ex = assertThrows(ConfigException.class,
        () -> CsvConfigLoader.load(tempDir).build());
    assertTrue(ex.getMessage().contains("rogue.map_id references non-existent map"));
  }

  @Test
  void load_rogueMissingMonsterFk_throwsConfigException() {
    copyFixture("player_template.csv", "player_template.csv");
    copyFixture("map.csv", "map.csv");
    copyFixture("region.csv", "region.csv");
    copyFixture("skill.csv", "skill.csv");
    copyFixture("monster.csv", "monster.csv");
    copyFixture("rogue_missing_monster_fk.csv", "rogue.csv");
    copyFixture("reward_pool.csv", "reward_pool.csv");
    copyFixture("item.csv", "item.csv");

    ConfigException ex = assertThrows(ConfigException.class,
        () -> CsvConfigLoader.load(tempDir).build());
    assertTrue(ex.getMessage().contains("rogue.monster_id references non-existent monster"));
  }

  @Test
  void load_rogueMissingRewardPoolFk_throwsConfigException() {
    copyFixture("player_template.csv", "player_template.csv");
    copyFixture("map.csv", "map.csv");
    copyFixture("region.csv", "region.csv");
    copyFixture("skill.csv", "skill.csv");
    copyFixture("monster.csv", "monster.csv");
    copyFixture("rogue_missing_reward_pool_fk.csv", "rogue.csv");
    copyFixture("reward_pool.csv", "reward_pool.csv");
    copyFixture("item.csv", "item.csv");

    ConfigException ex = assertThrows(ConfigException.class,
        () -> CsvConfigLoader.load(tempDir).build());
    assertTrue(ex.getMessage().contains("rogue.reward_pool_id references non-existent reward_pool"));
  }

  @Test
  void load_rewardPoolMissingItemFk_throwsConfigException() {
    copyFixture("player_template.csv", "player_template.csv");
    copyFixture("map.csv", "map.csv");
    copyFixture("region.csv", "region.csv");
    copyFixture("skill.csv", "skill.csv");
    copyFixture("monster.csv", "monster.csv");
    copyFixture("rogue.csv", "rogue.csv");
    copyFixture("reward_pool_missing_item_fk.csv", "reward_pool.csv");
    copyFixture("item.csv", "item.csv");

    ConfigException ex = assertThrows(ConfigException.class,
        () -> CsvConfigLoader.load(tempDir).build());
    assertTrue(ex.getMessage().contains("reward_pool.item_id references non-existent item"));
  }

  @Test
  void load_mapNegativeWidth_throwsConfigException() {
    copyFixture("player_template.csv", "player_template.csv");
    copyFixture("map_negative_width.csv", "map.csv");
    copyFixture("region.csv", "region.csv");
    copyFixture("skill.csv", "skill.csv");
    copyFixture("monster.csv", "monster.csv");
    copyFixture("rogue.csv", "rogue.csv");
    copyFixture("reward_pool.csv", "reward_pool.csv");
    copyFixture("item.csv", "item.csv");

    ConfigException ex = assertThrows(ConfigException.class,
        () -> CsvConfigLoader.load(tempDir).build());
    assertTrue(ex.getMessage().contains("must be positive"));
  }

  @Test
  void load_regionInvalidBool_throwsConfigException() {
    copyFixture("player_template.csv", "player_template.csv");
    copyFixture("map.csv", "map.csv");
    copyFixture("region_invalid_bool.csv", "region.csv");
    copyFixture("skill.csv", "skill.csv");
    copyFixture("monster.csv", "monster.csv");
    copyFixture("rogue.csv", "rogue.csv");
    copyFixture("reward_pool.csv", "reward_pool.csv");
    copyFixture("item.csv", "item.csv");

    ConfigException ex = assertThrows(ConfigException.class,
        () -> CsvConfigLoader.load(tempDir).build());
    assertTrue(ex.getMessage().contains("Invalid boolean"));
  }

  // --- requireXxx throw tests ---

  @Test
  void requirePlayerTemplate_notFound_throwsConfigException() {
    GameConfigRepository repo = loadAllFixtures(tempDir);
    ConfigException ex = assertThrows(ConfigException.class,
        () -> repo.requirePlayerTemplate(9999));
    assertTrue(ex.getMessage().contains("Player template not found"));
  }

  @Test
  void requireMap_notFound_throwsConfigException() {
    GameConfigRepository repo = loadAllFixtures(tempDir);
    ConfigException ex = assertThrows(ConfigException.class,
        () -> repo.requireMap(9999));
    assertTrue(ex.getMessage().contains("Map not found"));
  }

  @Test
  void requireRegion_notFound_throwsConfigException() {
    GameConfigRepository repo = loadAllFixtures(tempDir);
    ConfigException ex = assertThrows(ConfigException.class,
        () -> repo.requireRegion(9999));
    assertTrue(ex.getMessage().contains("Region not found"));
  }

  @Test
  void requireSkill_notFound_throwsConfigException() {
    GameConfigRepository repo = loadAllFixtures(tempDir);
    ConfigException ex = assertThrows(ConfigException.class,
        () -> repo.requireSkill(9999));
    assertTrue(ex.getMessage().contains("Skill not found"));
  }

  @Test
  void requireMonster_notFound_throwsConfigException() {
    GameConfigRepository repo = loadAllFixtures(tempDir);
    ConfigException ex = assertThrows(ConfigException.class,
        () -> repo.requireMonster(9999));
    assertTrue(ex.getMessage().contains("Monster not found"));
  }

  @Test
  void requireRogue_notFound_throwsConfigException() {
    GameConfigRepository repo = loadAllFixtures(tempDir);
    ConfigException ex = assertThrows(ConfigException.class,
        () -> repo.requireRogue(9999));
    assertTrue(ex.getMessage().contains("Rogue not found"));
  }

  @Test
  void requireRewardPool_notFound_throwsConfigException() {
    GameConfigRepository repo = loadAllFixtures(tempDir);
    ConfigException ex = assertThrows(ConfigException.class,
        () -> repo.requireRewardPool(9999));
    assertTrue(ex.getMessage().contains("Reward pool not found"));
  }

  @Test
  void requireItem_notFound_throwsConfigException() {
    GameConfigRepository repo = loadAllFixtures(tempDir);
    ConfigException ex = assertThrows(ConfigException.class,
        () -> repo.requireItem(9999));
    assertTrue(ex.getMessage().contains("Item not found"));
  }

  // --- Helpers ---

  /**
   * Copies all 8 valid fixture CSV files to the target directory and loads the repository.
   */
  private GameConfigRepository loadAllFixtures(Path targetDir) {
    copyFixture("player_template.csv", "player_template.csv");
    copyFixture("map.csv", "map.csv");
    copyFixture("region.csv", "region.csv");
    copyFixture("skill.csv", "skill.csv");
    copyFixture("monster.csv", "monster.csv");
    copyFixture("rogue.csv", "rogue.csv");
    copyFixture("reward_pool.csv", "reward_pool.csv");
    copyFixture("item.csv", "item.csv");
    return CsvConfigLoader.load(targetDir).build();
  }

  /**
   * Copies a single fixture from classpath resources to the target directory.
   */
  private void copyFixture(String fixtureName, String outputName) {
    try {
      String resourcePath = "config-fixtures/" + fixtureName;
      InputStream is = getClass().getClassLoader().getResourceAsStream(resourcePath);
      if (is == null) {
        throw new RuntimeException("Fixture not found on classpath: " + resourcePath);
      }
      Files.copy(is, tempDir.resolve(outputName));
      is.close();
    } catch (IOException e) {
      throw new RuntimeException("Failed to copy fixture: " + fixtureName, e);
    }
  }
}
