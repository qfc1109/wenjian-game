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
