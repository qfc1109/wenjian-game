package com.wenjian.core.firstchain;

import static com.wenjian.core.firstchain.FirstChainCoreDtos.DamageEvent;
import static com.wenjian.core.firstchain.FirstChainCoreDtos.EnterRegionResult;
import static com.wenjian.core.firstchain.FirstChainCoreDtos.EntityKind;
import static com.wenjian.core.firstchain.FirstChainCoreDtos.EntitySnapshot;
import static com.wenjian.core.firstchain.FirstChainCoreDtos.GridPosition;
import static com.wenjian.core.firstchain.FirstChainCoreDtos.GridVector;
import static com.wenjian.core.firstchain.FirstChainCoreDtos.LoginResult;
import static com.wenjian.core.firstchain.FirstChainCoreDtos.ResultCode;
import static com.wenjian.core.firstchain.FirstChainCoreDtos.RewardItem;
import static com.wenjian.core.firstchain.FirstChainCoreDtos.RogueFinishResult;
import static com.wenjian.core.firstchain.FirstChainCoreDtos.RogueStartResult;
import static com.wenjian.core.firstchain.FirstChainCoreDtos.SkillCastResult;
import static com.wenjian.core.firstchain.FirstChainCoreDtos.SkillEvent;

import com.wenjian.config.ConfigException;
import com.wenjian.config.GameConfigRepository;
import com.wenjian.config.model.MonsterConfig;
import com.wenjian.config.model.PlayerTemplateConfig;
import com.wenjian.config.model.RewardPoolConfig;
import com.wenjian.config.model.RogueConfig;
import com.wenjian.config.model.SkillConfig;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

public final class FirstChainCoreService {
  private static final long DEV_PLAYER_ID = 1_000_001L;
  private static final int DEV_PLAYER_TEMPLATE_ID = 1;
  private static final int TRAINING_MONSTER_CONFIG_ID = 3001;
  private static final long TRAINING_ENEMY_ID = 2_000_001L;
  private static final long DEFAULT_ROGUE_INSTANCE_ID = 9_000_001L;
  private static final long ROGUE_MONSTER_ENTITY_ID_BASE = 3_000_000L;
  private static final long FIRST_CHAIN_SERVER_TICK = 1L;

  private final GameConfigRepository configs;
  private Long activeRogueInstanceId;
  private Integer activeRewardPoolId;

  public FirstChainCoreService(GameConfigRepository configs) {
    this.configs = Objects.requireNonNull(configs, "configs");
  }

  public LoginResult login(String loginKey, long serverTimeMs) {
    PlayerTemplateConfig player = playerTemplate();
    return new LoginResult(
        ResultCode.OK,
        DEV_PLAYER_ID,
        "session-" + DEV_PLAYER_ID + "-" + serverTimeMs,
        player.initRegionId(),
        new GridPosition(player.spawnX(), player.spawnY()),
        player.maxHp(),
        serverTimeMs);
  }

  public EnterRegionResult enterRegion(long playerId, int regionId) {
    PlayerTemplateConfig player = playerTemplate();
    if (playerId != DEV_PLAYER_ID || regionId != player.initRegionId()) {
      return new EnterRegionResult(
          ResultCode.NOT_FOUND,
          regionId,
          null,
          List.of(),
          0L);
    }

    configs.requireRegion(regionId);
    MonsterConfig trainingMonster = configs.requireMonster(TRAINING_MONSTER_CONFIG_ID);
    GridPosition playerPosition = new GridPosition(player.spawnX(), player.spawnY());
    EntitySnapshot self = new EntitySnapshot(
        playerId,
        EntityKind.PLAYER,
        playerPosition,
        player.maxHp());
    EntitySnapshot trainingEnemy = new EntitySnapshot(
        TRAINING_ENEMY_ID,
        EntityKind.MONSTER,
        trainingEnemyPosition(playerPosition),
        trainingMonster.maxHp());

    return new EnterRegionResult(
        ResultCode.OK,
        regionId,
        self,
        List.of(self, trainingEnemy),
        FIRST_CHAIN_SERVER_TICK);
  }

  public SkillCastResult castSkill(long playerId, int skillId, GridVector aimDir) {
    PlayerTemplateConfig player = playerTemplate();
    if (playerId != DEV_PLAYER_ID || skillId != player.defaultSkillId()) {
      return new SkillCastResult(ResultCode.NOT_FOUND, null, null);
    }

    SkillConfig skill = configs.requireSkill(skillId);
    SkillEvent skillEvent = new SkillEvent(
        playerId,
        skillId,
        new GridPosition(player.spawnX(), player.spawnY()),
        aimDir);
    DamageEvent damageEvent = new DamageEvent(
        playerId,
        TRAINING_ENEMY_ID,
        skillId,
        -skill.damage(),
        false);

    return new SkillCastResult(ResultCode.OK, skillEvent, damageEvent);
  }

  public RogueStartResult startRogue(long playerId, int rogueId) {
    if (playerId != DEV_PLAYER_ID) {
      return rogueStartNotFound(playerId, rogueId);
    }

    RogueConfig rogue;
    try {
      rogue = configs.requireRogue(rogueId);
    } catch (ConfigException e) {
      return rogueStartNotFound(playerId, rogueId);
    }

    PlayerTemplateConfig player = playerTemplate();
    MonsterConfig monster = configs.requireMonster(rogue.monsterId());
    GridPosition spawn = new GridPosition(rogue.spawnX(), rogue.spawnY());
    EntitySnapshot self = new EntitySnapshot(
        playerId,
        EntityKind.PLAYER,
        spawn,
        player.maxHp());
    List<EntitySnapshot> entities = new ArrayList<>();
    entities.add(self);
    for (int index = 0; index < rogue.monsterCount(); index++) {
      entities.add(new EntitySnapshot(
          ROGUE_MONSTER_ENTITY_ID_BASE + index + 1,
          EntityKind.MONSTER,
          new GridPosition(spawn.x() + 2 + index, spawn.y()),
          monster.maxHp()));
    }

    activeRogueInstanceId = DEFAULT_ROGUE_INSTANCE_ID;
    activeRewardPoolId = rogue.rewardPoolId();
    return new RogueStartResult(
        ResultCode.OK,
        playerId,
        rogue.rogueId(),
        DEFAULT_ROGUE_INSTANCE_ID,
        rogue.mapId(),
        spawn,
        rogue.monsterId(),
        rogue.monsterCount(),
        rogue.rewardPoolId(),
        List.copyOf(entities));
  }

  public RogueFinishResult finishRogue(long playerId, long instanceId) {
    if (playerId != DEV_PLAYER_ID
        || activeRogueInstanceId == null
        || activeRogueInstanceId.longValue() != instanceId
        || activeRewardPoolId == null) {
      return new RogueFinishResult(ResultCode.NOT_FOUND, instanceId, false, List.of());
    }

    RewardPoolConfig rewardPool = configs.requireRewardPool(activeRewardPoolId);
    activeRogueInstanceId = null;
    activeRewardPoolId = null;
    return new RogueFinishResult(
        ResultCode.OK,
        instanceId,
        true,
        List.of(new RewardItem(rewardPool.itemId(), rewardPool.count())));
  }

  private PlayerTemplateConfig playerTemplate() {
    return configs.requirePlayerTemplate(DEV_PLAYER_TEMPLATE_ID);
  }

  private GridPosition trainingEnemyPosition(GridPosition playerPosition) {
    return new GridPosition(playerPosition.x() + 800, playerPosition.y());
  }

  private RogueStartResult rogueStartNotFound(long playerId, int rogueId) {
    return new RogueStartResult(
        ResultCode.NOT_FOUND,
        playerId,
        rogueId,
        0L,
        0,
        null,
        0,
        0,
        0,
        List.of());
  }
}
