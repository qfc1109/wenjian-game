package com.wenjian.gateway.ws;

import java.util.ArrayList;
import java.util.List;

final class FirstChainGatewayService {
  private static final long DEV_PLAYER_ID = 1_000_001L;
  private static final long TRAINING_ENEMY_ID = 2_000_001L;
  private static final int BAMBOO_REGION_ID = 1001;
  private static final int DEFAULT_SKILL_ID = 2001;
  private static final int DEFAULT_SKILL_DAMAGE = 12;
  private static final int TRAINING_ENEMY_HP = 60;
  private static final GridPosition SPAWN_POSITION = new GridPosition(10, 12);
  private static final int DEFAULT_ROGUE_ID = 4001;
  private static final long DEFAULT_ROGUE_INSTANCE_ID = 9_000_001L;
  private static final int DEFAULT_ROGUE_MAP_ID = 2001;
  private static final GridPosition DEFAULT_ROGUE_SPAWN = new GridPosition(1500, 1500);
  private static final int DEFAULT_ROGUE_MONSTER_ID = 3001;
  private static final int DEFAULT_ROGUE_MONSTER_COUNT = 8;
  private static final int DEFAULT_ROGUE_REWARD_POOL_ID = 5001;
  private static final int DEFAULT_REWARD_ITEM_ID = 6001;
  private static final int DEFAULT_REWARD_COUNT = 3;
  private static final long ROGUE_MONSTER_ENTITY_ID_BASE = 3_000_000L;

  private Long activeRogueInstanceId;

  static FirstChainGatewayService createDefault() {
    return new FirstChainGatewayService();
  }

  LoginResult login(String loginKey, long serverTimeMs) {
    return new LoginResult(
        ResultCode.OK,
        DEV_PLAYER_ID,
        "session-" + DEV_PLAYER_ID + "-" + serverTimeMs,
        BAMBOO_REGION_ID,
        SPAWN_POSITION,
        serverTimeMs);
  }

  EnterRegionResult enterRegion(long playerId, int regionId) {
    if (playerId != DEV_PLAYER_ID || regionId != BAMBOO_REGION_ID) {
      return new EnterRegionResult(
          ResultCode.NOT_FOUND,
          regionId,
          null,
          List.of(),
          0L);
    }

    EntitySnapshot self = new EntitySnapshot(
        playerId,
        EntityKind.PLAYER,
        SPAWN_POSITION,
        100);
    EntitySnapshot trainingEnemy = new EntitySnapshot(
        TRAINING_ENEMY_ID,
        EntityKind.MONSTER,
        new GridPosition(18, 12),
        TRAINING_ENEMY_HP);

    return new EnterRegionResult(
        ResultCode.OK,
        BAMBOO_REGION_ID,
        self,
        List.of(self, trainingEnemy),
        1L);
  }

  SkillCastResult castSkill(long playerId, int skillId, GridVector aimDir) {
    if (playerId != DEV_PLAYER_ID || skillId != DEFAULT_SKILL_ID) {
      return new SkillCastResult(ResultCode.NOT_FOUND, null, null);
    }

    SkillEvent skillEvent = new SkillEvent(playerId, skillId, SPAWN_POSITION, aimDir);
    DamageEvent damageEvent = new DamageEvent(
        playerId,
        TRAINING_ENEMY_ID,
        skillId,
        -DEFAULT_SKILL_DAMAGE,
        false);

    return new SkillCastResult(ResultCode.OK, skillEvent, damageEvent);
  }

  RogueStartResult startRogue(long playerId, int rogueId) {
    if (playerId != DEV_PLAYER_ID || rogueId != DEFAULT_ROGUE_ID) {
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

    EntitySnapshot self = new EntitySnapshot(
        playerId,
        EntityKind.PLAYER,
        DEFAULT_ROGUE_SPAWN,
        100);
    List<EntitySnapshot> entities = new ArrayList<>();
    entities.add(self);
    for (int index = 0; index < DEFAULT_ROGUE_MONSTER_COUNT; index++) {
      entities.add(new EntitySnapshot(
          ROGUE_MONSTER_ENTITY_ID_BASE + index + 1,
          EntityKind.MONSTER,
          new GridPosition(DEFAULT_ROGUE_SPAWN.x() + 2 + index, DEFAULT_ROGUE_SPAWN.y()),
          TRAINING_ENEMY_HP));
    }

    activeRogueInstanceId = DEFAULT_ROGUE_INSTANCE_ID;
    return new RogueStartResult(
        ResultCode.OK,
        playerId,
        DEFAULT_ROGUE_ID,
        DEFAULT_ROGUE_INSTANCE_ID,
        DEFAULT_ROGUE_MAP_ID,
        DEFAULT_ROGUE_SPAWN,
        DEFAULT_ROGUE_MONSTER_ID,
        DEFAULT_ROGUE_MONSTER_COUNT,
        DEFAULT_ROGUE_REWARD_POOL_ID,
        List.copyOf(entities));
  }

  RogueFinishResult finishRogue(long playerId, long instanceId) {
    if (playerId != DEV_PLAYER_ID
        || activeRogueInstanceId == null
        || activeRogueInstanceId.longValue() != instanceId) {
      return new RogueFinishResult(ResultCode.NOT_FOUND, instanceId, false, List.of());
    }

    activeRogueInstanceId = null;
    return new RogueFinishResult(
        ResultCode.OK,
        DEFAULT_ROGUE_INSTANCE_ID,
        true,
        List.of(new RewardItem(DEFAULT_REWARD_ITEM_ID, DEFAULT_REWARD_COUNT)));
  }
}
