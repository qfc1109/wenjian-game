package com.wenjian.gateway.ws;

import java.util.List;

enum ResultCode {
  OK,
  NOT_FOUND
}

enum EntityKind {
  PLAYER,
  MONSTER
}

record GridPosition(int x, int y) {
}

record GridVector(int x, int y) {
}

record LoginResult(
    ResultCode code,
    long playerId,
    String sessionToken,
    int regionId,
    GridPosition position,
    long serverTimeMs) {
}

record EntitySnapshot(
    long entityId,
    EntityKind kind,
    GridPosition position,
    int hp) {
}

record EnterRegionResult(
    ResultCode code,
    int regionId,
    EntitySnapshot self,
    List<EntitySnapshot> entities,
    long serverTick) {
}

record SkillEvent(
    long casterId,
    int skillId,
    GridPosition position,
    GridVector aimDir) {
}

record DamageEvent(
    long sourceId,
    long targetId,
    int skillId,
    int hpDelta,
    boolean dead) {
}

record SkillCastResult(
    ResultCode code,
    SkillEvent skillEvent,
    DamageEvent damageEvent) {
}

record RogueStartResult(
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

record RewardItem(
    int itemId,
    int count) {
}

record RogueFinishResult(
    ResultCode code,
    long instanceId,
    boolean success,
    List<RewardItem> rewards) {
}
