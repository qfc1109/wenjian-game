package com.wenjian.core.firstchain;

import java.util.List;

public final class FirstChainCoreDtos {
  private FirstChainCoreDtos() {
  }

  public enum ResultCode {
    OK,
    NOT_FOUND
  }

  public enum EntityKind {
    PLAYER,
    MONSTER
  }

  public record GridPosition(int x, int y) {
  }

  public record GridVector(int x, int y) {
  }

  public record LoginResult(
      ResultCode code,
      long playerId,
      String sessionToken,
      int regionId,
      GridPosition position,
      int maxHp,
      long serverTimeMs) {
  }

  public record EntitySnapshot(
      long entityId,
      EntityKind kind,
      GridPosition position,
      int hp) {
  }

  public record EnterRegionResult(
      ResultCode code,
      int regionId,
      EntitySnapshot self,
      List<EntitySnapshot> entities,
      long serverTick) {
  }

  public record SkillEvent(
      long casterId,
      int skillId,
      GridPosition position,
      GridVector aimDir) {
  }

  public record DamageEvent(
      long sourceId,
      long targetId,
      int skillId,
      int hpDelta,
      boolean dead) {
  }

  public record SkillCastResult(
      ResultCode code,
      SkillEvent skillEvent,
      DamageEvent damageEvent) {
  }

  public record RogueStartResult(
      ResultCode code,
      long playerId,
      int rogueId,
      long instanceId,
      int mapId,
      GridPosition spawnPosition,
      int monsterId,
      int monsterCount,
      int rewardPoolId,
      List<EntitySnapshot> entities) {
  }

  public record RewardItem(
      int itemId,
      int count) {
  }

  public record RogueFinishResult(
      ResultCode code,
      long instanceId,
      boolean success,
      List<RewardItem> rewards) {
  }
}
