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
import java.util.List;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

class GameConfigRepositoryTest {

  @TempDir
  Path tempDir;

  // --- Immutability ---

  @Test
  void allAccessors_returnImmutableCollections() {
    GameConfigRepository repo = loadAllFixtures();
    assertThrows(UnsupportedOperationException.class,
        () -> repo.allPlayerTemplates().add(null));
    assertThrows(UnsupportedOperationException.class,
        () -> repo.allMaps().add(null));
    assertThrows(UnsupportedOperationException.class,
        () -> repo.allRegions().add(null));
    assertThrows(UnsupportedOperationException.class,
        () -> repo.allSkills().add(null));
    assertThrows(UnsupportedOperationException.class,
        () -> repo.allMonsters().add(null));
    assertThrows(UnsupportedOperationException.class,
        () -> repo.allRogues().add(null));
    assertThrows(UnsupportedOperationException.class,
        () -> repo.allRewardPools().add(null));
    assertThrows(UnsupportedOperationException.class,
        () -> repo.allItems().add(null));
  }

  // --- Full pipeline: Player Template -> Region -> Map -> Skill -> Monster -> Rogue -> RewardPool -> Item ---

  @Test
  void fullPipeline_playerTemplateToItem_allFieldsMatch() {
    GameConfigRepository repo = loadAllFixtures();

    PlayerTemplateConfig pt = repo.requirePlayerTemplate(1);
    assertEquals(1, pt.templateId());
    assertEquals(1001, pt.initRegionId());
    assertEquals(3200, pt.spawnX());
    assertEquals(2400, pt.spawnY());
    assertEquals(100, pt.maxHp());
    assertEquals(420, pt.moveSpeed());
    assertEquals(2001, pt.defaultSkillId());

    RegionConfig region = repo.requireRegion(pt.initRegionId());
    assertEquals(1001, region.regionId());
    assertEquals(1001, region.mapId());
    assertEquals("TOWN", region.regionType());
    assertFalse(region.pkEnabled());
    assertEquals(3200, region.spawnX());
    assertEquals(2400, region.spawnY());

    MapConfig map = repo.requireMap(region.mapId());
    assertEquals(1001, map.mapId());
    assertEquals(256, map.width());
    assertEquals(256, map.height());
    assertEquals("maps/1001_collision.csv", map.collisionRef());

    SkillConfig skill = repo.requireSkill(pt.defaultSkillId());
    assertEquals(2001, skill.skillId());
    assertEquals(600, skill.cooldownMs());
    assertEquals(180, skill.castMs());
    assertEquals("ARC", skill.hitShape());
    assertEquals(900, skill.range());
    assertEquals(240, skill.radius());
    assertEquals(12, skill.damage());

    RogueConfig rogue = repo.requireRogue(4001);
    assertEquals(4001, rogue.rogueId());
    assertEquals(2001, rogue.mapId());
    assertEquals(1500, rogue.spawnX());
    assertEquals(1500, rogue.spawnY());
    assertEquals(8, rogue.monsterCount());
    assertEquals(5001, rogue.rewardPoolId());

    MapConfig rogueMap = repo.requireMap(rogue.mapId());
    assertEquals(2001, rogueMap.mapId());
    assertEquals(96, rogueMap.width());
    assertEquals(96, rogueMap.height());

    MonsterConfig monster = repo.requireMonster(rogue.monsterId());
    assertEquals(3001, monster.monsterId());
    assertEquals(60, monster.maxHp());
    assertEquals(300, monster.moveSpeed());

    RewardPoolConfig pool = repo.requireRewardPool(rogue.rewardPoolId());
    assertEquals(5001, pool.rewardPoolId());
    assertEquals(6001, pool.itemId());
    assertEquals(3, pool.count());

    ItemConfig item = repo.requireItem(pool.itemId());
    assertEquals(6001, item.itemId());
    assertEquals("试炼铜钱", item.name());
    assertEquals("CURRENCY", item.type());
  }

  // --- Foreign key: player_template -> region ---

  @Test
  void playerTemplate_fkToRegion_valid() {
    GameConfigRepository repo = loadAllFixtures();
    PlayerTemplateConfig pt = repo.requirePlayerTemplate(1);
    RegionConfig region = repo.requireRegion(pt.initRegionId());
    assertEquals(pt.initRegionId(), region.regionId());
  }

  // --- Foreign key: region -> map ---

  @Test
  void region_fkToMap_valid() {
    GameConfigRepository repo = loadAllFixtures();
    RegionConfig region = repo.requireRegion(1001);
    MapConfig map = repo.requireMap(region.mapId());
    assertEquals(region.mapId(), map.mapId());
  }

  // --- Foreign key: rogue -> map ---

  @Test
  void rogue_fkToMap_valid() {
    GameConfigRepository repo = loadAllFixtures();
    RogueConfig rogue = repo.requireRogue(4001);
    MapConfig map = repo.requireMap(rogue.mapId());
    assertEquals(rogue.mapId(), map.mapId());
  }

  // --- Foreign key: rogue -> monster ---

  @Test
  void rogue_fkToMonster_valid() {
    GameConfigRepository repo = loadAllFixtures();
    RogueConfig rogue = repo.requireRogue(4001);
    MonsterConfig monster = repo.requireMonster(rogue.monsterId());
    assertEquals(rogue.monsterId(), monster.monsterId());
  }

  // --- Foreign key: rogue -> reward_pool ---

  @Test
  void rogue_fkToRewardPool_valid() {
    GameConfigRepository repo = loadAllFixtures();
    RogueConfig rogue = repo.requireRogue(4001);
    RewardPoolConfig pool = repo.requireRewardPool(rogue.rewardPoolId());
    assertEquals(rogue.rewardPoolId(), pool.rewardPoolId());
  }

  // --- Foreign key: reward_pool -> item ---

  @Test
  void rewardPool_fkToItem_valid() {
    GameConfigRepository repo = loadAllFixtures();
    RewardPoolConfig pool = repo.requireRewardPool(5001);
    ItemConfig item = repo.requireItem(pool.itemId());
    assertEquals(pool.itemId(), item.itemId());
  }

  // --- Snapshot counts ---

  @Test
  void allXxx_returnCorrectCounts() {
    GameConfigRepository repo = loadAllFixtures();
    assertEquals(1, repo.allPlayerTemplates().size());
    assertEquals(2, repo.allMaps().size());
    assertEquals(2, repo.allRegions().size());
    assertEquals(3, repo.allSkills().size());
    assertEquals(1, repo.allMonsters().size());
    assertEquals(1, repo.allRogues().size());
    assertEquals(1, repo.allRewardPools().size());
    assertEquals(1, repo.allItems().size());
  }

  // --- ConfigException metadata ---

  @Test
  void requireXxx_throwsConfigException_withCorrectMetadata() {
    GameConfigRepository repo = loadAllFixtures();

    ConfigException ex = assertThrows(ConfigException.class,
        () -> repo.requirePlayerTemplate(9999));
    assertEquals("player_template.csv", ex.fileName());
    assertEquals("template_id", ex.fieldName());
    assertEquals("9999", ex.rawValue());
    assertTrue(ex.getMessage().contains("Player template not found"));

    ConfigException ex2 = assertThrows(ConfigException.class,
        () -> repo.requireRegion(9999));
    assertEquals("region.csv", ex2.fileName());
    assertEquals("region_id", ex2.fieldName());
    assertEquals("9999", ex2.rawValue());
  }

  // --- Helper ---

  private GameConfigRepository loadAllFixtures() {
    copyFixture("player_template.csv");
    copyFixture("map.csv");
    copyFixture("region.csv");
    copyFixture("skill.csv");
    copyFixture("monster.csv");
    copyFixture("rogue.csv");
    copyFixture("reward_pool.csv");
    copyFixture("item.csv");
    return CsvConfigLoader.load(tempDir).build();
  }

  private void copyFixture(String fixtureName) {
    try {
      String resourcePath = "config-fixtures/" + fixtureName;
      InputStream is = getClass().getClassLoader().getResourceAsStream(resourcePath);
      if (is == null) {
        throw new RuntimeException("Fixture not found on classpath: " + resourcePath);
      }
      Files.copy(is, tempDir.resolve(fixtureName));
      is.close();
    } catch (IOException e) {
      throw new RuntimeException("Failed to copy fixture: " + fixtureName, e);
    }
  }
}
