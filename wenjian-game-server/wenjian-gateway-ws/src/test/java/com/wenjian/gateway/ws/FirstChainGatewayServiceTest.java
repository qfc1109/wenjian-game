package com.wenjian.gateway.ws;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;

import org.junit.jupiter.api.Test;

class FirstChainGatewayServiceTest {

  @Test
  void loginReturnsPlayerIdentityAndInitialRegion() {
    FirstChainGatewayService gateway = FirstChainGatewayService.createDefault();

    LoginResult result = gateway.login("local-dev-key", 1_700_000_000_000L);

    assertEquals(ResultCode.OK, result.code());
    assertTrue(result.playerId() > 0);
    assertFalse(result.sessionToken().isBlank());
    assertEquals(1001, result.regionId());
    assertEquals(new GridPosition(10, 12), result.position());
    assertEquals(1_700_000_000_000L, result.serverTimeMs());
  }

  @Test
  void enterRegionReturnsSelfAndSnapshotEntities() {
    FirstChainGatewayService gateway = FirstChainGatewayService.createDefault();
    LoginResult login = gateway.login("local-dev-key", 1_700_000_000_000L);

    EnterRegionResult result = gateway.enterRegion(login.playerId(), 1001);

    assertEquals(ResultCode.OK, result.code());
    assertEquals(1001, result.regionId());
    assertNotNull(result.self());
    assertEquals(login.playerId(), result.self().entityId());
    assertEquals(new GridPosition(10, 12), result.self().position());
    assertFalse(result.entities().isEmpty());
    assertTrue(result.entities().stream().anyMatch(entity -> entity.entityId() == login.playerId()));
    assertTrue(result.serverTick() > 0);
  }
}
