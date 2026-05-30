package com.wenjian.config;

import com.wenjian.config.model.ItemConfig;
import com.wenjian.config.model.MapConfig;
import com.wenjian.config.model.MonsterConfig;
import com.wenjian.config.model.PlayerTemplateConfig;
import com.wenjian.config.model.RegionConfig;
import com.wenjian.config.model.RewardPoolConfig;
import com.wenjian.config.model.RogueConfig;
import com.wenjian.config.model.SkillConfig;
import java.util.List;
import java.util.Map;

/**
 * Immutable repository of all game configuration data, loaded from CSV files.
 *
 * <p>Provides typed accessors that throw {@link ConfigException} if a requested
 * configuration entry is not found. Also validates foreign key relationships
 * and positive-number constraints during construction.
 *
 * <p>Use {@link CsvConfigLoader#load(Path)} to create instances.
 */
public final class GameConfigRepository {

  private final Map<Integer, PlayerTemplateConfig> playerTemplates;
  private final Map<Integer, MapConfig> maps;
  private final Map<Integer, RegionConfig> regions;
  private final Map<Integer, SkillConfig> skills;
  private final Map<Integer, MonsterConfig> monsters;
  private final Map<Integer, RogueConfig> rogues;
  private final Map<Integer, RewardPoolConfig> rewardPools;
  private final Map<Integer, ItemConfig> items;

  private GameConfigRepository(Builder builder) {
    this.playerTemplates = Map.copyOf(builder.playerTemplates);
    this.maps = Map.copyOf(builder.maps);
    this.regions = Map.copyOf(builder.regions);
    this.skills = Map.copyOf(builder.skills);
    this.monsters = Map.copyOf(builder.monsters);
    this.rogues = Map.copyOf(builder.rogues);
    this.rewardPools = Map.copyOf(builder.rewardPools);
    this.items = Map.copyOf(builder.items);

    validateForeignKeys();
    validatePositiveNumbers();
  }

  // --- Validation: Foreign Keys ---

  private void validateForeignKeys() {
    for (PlayerTemplateConfig pt : playerTemplates.values()) {
      if (!regions.containsKey(pt.initRegionId())) {
        throw new ConfigException(
            "player_template.init_region_id references non-existent region",
            "player_template.csv", "init_region_id", String.valueOf(pt.initRegionId()));
      }
      if (!skills.containsKey(pt.defaultSkillId())) {
        throw new ConfigException(
            "player_template.default_skill_id references non-existent skill",
            "player_template.csv", "default_skill_id", String.valueOf(pt.defaultSkillId()));
      }
    }

    for (RegionConfig region : regions.values()) {
      if (!maps.containsKey(region.mapId())) {
        throw new ConfigException(
            "region.map_id references non-existent map",
            "region.csv", "map_id", String.valueOf(region.mapId()));
      }
    }

    for (RogueConfig rogue : rogues.values()) {
      if (!maps.containsKey(rogue.mapId())) {
        throw new ConfigException(
            "rogue.map_id references non-existent map",
            "rogue.csv", "map_id", String.valueOf(rogue.mapId()));
      }
      if (!monsters.containsKey(rogue.monsterId())) {
        throw new ConfigException(
            "rogue.monster_id references non-existent monster",
            "rogue.csv", "monster_id", String.valueOf(rogue.monsterId()));
      }
      if (!rewardPools.containsKey(rogue.rewardPoolId())) {
        throw new ConfigException(
            "rogue.reward_pool_id references non-existent reward_pool",
            "rogue.csv", "reward_pool_id", String.valueOf(rogue.rewardPoolId()));
      }
    }

    for (RewardPoolConfig pool : rewardPools.values()) {
      if (!items.containsKey(pool.itemId())) {
        throw new ConfigException(
            "reward_pool.item_id references non-existent item",
            "reward_pool.csv", "item_id", String.valueOf(pool.itemId()));
      }
    }
  }

  // --- Validation: Positive numbers ---

  private void validatePositiveNumbers() {
    for (PlayerTemplateConfig pt : playerTemplates.values()) {
      if (pt.spawnX() <= 0) throw nonPositive("player_template.csv", "spawn_x", pt.templateId());
      if (pt.spawnY() <= 0) throw nonPositive("player_template.csv", "spawn_y", pt.templateId());
      if (pt.maxHp() <= 0) throw nonPositive("player_template.csv", "max_hp", pt.templateId());
      if (pt.moveSpeed() <= 0) throw nonPositive("player_template.csv", "move_speed", pt.templateId());
    }

    for (MapConfig map : maps.values()) {
      if (map.width() <= 0) throw nonPositive("map.csv", "width", map.mapId());
      if (map.height() <= 0) throw nonPositive("map.csv", "height", map.mapId());
    }

    for (RegionConfig region : regions.values()) {
      if (region.spawnX() <= 0) throw nonPositive("region.csv", "spawn_x", region.regionId());
      if (region.spawnY() <= 0) throw nonPositive("region.csv", "spawn_y", region.regionId());
    }

    for (SkillConfig skill : skills.values()) {
      if (skill.cooldownMs() <= 0) throw nonPositive("skill.csv", "cooldown_ms", skill.skillId());
      if (skill.castMs() <= 0) throw nonPositive("skill.csv", "cast_ms", skill.skillId());
      if (skill.damage() <= 0) throw nonPositive("skill.csv", "damage", skill.skillId());
    }

    for (MonsterConfig monster : monsters.values()) {
      if (monster.maxHp() <= 0) throw nonPositive("monster.csv", "max_hp", monster.monsterId());
      if (monster.moveSpeed() <= 0) throw nonPositive("monster.csv", "move_speed", monster.monsterId());
    }

    for (RogueConfig rogue : rogues.values()) {
      if (rogue.spawnX() <= 0) throw nonPositive("rogue.csv", "spawn_x", rogue.rogueId());
      if (rogue.spawnY() <= 0) throw nonPositive("rogue.csv", "spawn_y", rogue.rogueId());
      if (rogue.monsterCount() <= 0) throw nonPositive("rogue.csv", "monster_count", rogue.rogueId());
    }

    for (RewardPoolConfig pool : rewardPools.values()) {
      if (pool.count() <= 0) throw nonPositive("reward_pool.csv", "count", pool.rewardPoolId());
    }
  }

  private ConfigException nonPositive(String file, String field, int id) {
    return new ConfigException(
        "Field must be positive for entry id=" + id, file, field, null);
  }

  // --- Typed Accessors ---

  public PlayerTemplateConfig requirePlayerTemplate(int templateId) {
    PlayerTemplateConfig config = playerTemplates.get(templateId);
    if (config == null) {
      throw new ConfigException(
          "Player template not found", "player_template.csv", "template_id", String.valueOf(templateId));
    }
    return config;
  }

  public MapConfig requireMap(int mapId) {
    MapConfig config = maps.get(mapId);
    if (config == null) {
      throw new ConfigException(
          "Map not found", "map.csv", "map_id", String.valueOf(mapId));
    }
    return config;
  }

  public RegionConfig requireRegion(int regionId) {
    RegionConfig config = regions.get(regionId);
    if (config == null) {
      throw new ConfigException(
          "Region not found", "region.csv", "region_id", String.valueOf(regionId));
    }
    return config;
  }

  public SkillConfig requireSkill(int skillId) {
    SkillConfig config = skills.get(skillId);
    if (config == null) {
      throw new ConfigException(
          "Skill not found", "skill.csv", "skill_id", String.valueOf(skillId));
    }
    return config;
  }

  public MonsterConfig requireMonster(int monsterId) {
    MonsterConfig config = monsters.get(monsterId);
    if (config == null) {
      throw new ConfigException(
          "Monster not found", "monster.csv", "monster_id", String.valueOf(monsterId));
    }
    return config;
  }

  public RogueConfig requireRogue(int rogueId) {
    RogueConfig config = rogues.get(rogueId);
    if (config == null) {
      throw new ConfigException(
          "Rogue not found", "rogue.csv", "rogue_id", String.valueOf(rogueId));
    }
    return config;
  }

  public RewardPoolConfig requireRewardPool(int rewardPoolId) {
    RewardPoolConfig config = rewardPools.get(rewardPoolId);
    if (config == null) {
      throw new ConfigException(
          "Reward pool not found", "reward_pool.csv", "reward_pool_id", String.valueOf(rewardPoolId));
    }
    return config;
  }

  public ItemConfig requireItem(int itemId) {
    ItemConfig config = items.get(itemId);
    if (config == null) {
      throw new ConfigException(
          "Item not found", "item.csv", "item_id", String.valueOf(itemId));
    }
    return config;
  }

  // --- Snapshot accessors (read-only collections) ---

  public List<PlayerTemplateConfig> allPlayerTemplates() {
    return List.copyOf(playerTemplates.values());
  }

  public List<MapConfig> allMaps() {
    return List.copyOf(maps.values());
  }

  public List<RegionConfig> allRegions() {
    return List.copyOf(regions.values());
  }

  public List<SkillConfig> allSkills() {
    return List.copyOf(skills.values());
  }

  public List<MonsterConfig> allMonsters() {
    return List.copyOf(monsters.values());
  }

  public List<RogueConfig> allRogues() {
    return List.copyOf(rogues.values());
  }

  public List<RewardPoolConfig> allRewardPools() {
    return List.copyOf(rewardPools.values());
  }

  public List<ItemConfig> allItems() {
    return List.copyOf(items.values());
  }

  // --- Builder ---

  public static class Builder {
    private Map<Integer, PlayerTemplateConfig> playerTemplates = Map.of();
    private Map<Integer, MapConfig> maps = Map.of();
    private Map<Integer, RegionConfig> regions = Map.of();
    private Map<Integer, SkillConfig> skills = Map.of();
    private Map<Integer, MonsterConfig> monsters = Map.of();
    private Map<Integer, RogueConfig> rogues = Map.of();
    private Map<Integer, RewardPoolConfig> rewardPools = Map.of();
    private Map<Integer, ItemConfig> items = Map.of();

    public Builder playerTemplates(Map<Integer, PlayerTemplateConfig> v) {
      this.playerTemplates = v;
      return this;
    }

    public Builder maps(Map<Integer, MapConfig> v) {
      this.maps = v;
      return this;
    }

    public Builder regions(Map<Integer, RegionConfig> v) {
      this.regions = v;
      return this;
    }

    public Builder skills(Map<Integer, SkillConfig> v) {
      this.skills = v;
      return this;
    }

    public Builder monsters(Map<Integer, MonsterConfig> v) {
      this.monsters = v;
      return this;
    }

    public Builder rogues(Map<Integer, RogueConfig> v) {
      this.rogues = v;
      return this;
    }

    public Builder rewardPools(Map<Integer, RewardPoolConfig> v) {
      this.rewardPools = v;
      return this;
    }

    public Builder items(Map<Integer, ItemConfig> v) {
      this.items = v;
      return this;
    }

    public GameConfigRepository build() {
      return new GameConfigRepository(this);
    }
  }
}
