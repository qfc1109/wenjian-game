package com.wenjian.gateway.ws;

import java.util.List;

final class FirstChainGatewayService {
  private static final long DEV_PLAYER_ID = 1_000_001L;
  private static final int BAMBOO_REGION_ID = 1001;
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
        2_000_001L,
        EntityKind.MONSTER,
        new GridPosition(18, 12),
        60);

    return new EnterRegionResult(
        ResultCode.OK,
        BAMBOO_REGION_ID,
        self,
        List.of(self, trainingEnemy),
        1L);
  }
}
