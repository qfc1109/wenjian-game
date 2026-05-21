package com.wenjian.gateway.ws;

import java.util.List;

final class FirstChainGatewayService {
  private static final long DEV_PLAYER_ID = 1_000_001L;
  private static final long TRAINING_ENEMY_ID = 2_000_001L;
  private static final int BAMBOO_REGION_ID = 1001;
  private static final int DEFAULT_SKILL_ID = 2001;
  private static final int DEFAULT_SKILL_DAMAGE = 12;
  private static final int TRAINING_ENEMY_HP = 60;
  private static final GridPosition SPAWN_POSITION = new GridPosition(10, 12);

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
}
