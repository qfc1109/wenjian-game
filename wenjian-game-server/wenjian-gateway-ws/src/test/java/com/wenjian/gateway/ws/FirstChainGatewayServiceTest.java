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
    assertEquals(new GridPosition(3200, 2400), result.position());
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
    assertEquals(new GridPosition(3200, 2400), result.self().position());
    assertFalse(result.entities().isEmpty());
    assertTrue(result.entities().stream().anyMatch(entity -> entity.entityId() == login.playerId()));
    assertTrue(result.serverTick() > 0);
  }

  @Test
  void castDefaultSkillReturnsSkillEventAndDamageEvent() {
    FirstChainGatewayService gateway = FirstChainGatewayService.createDefault();
    LoginResult login = gateway.login("local-dev-key", 1_700_000_000_000L);
    gateway.enterRegion(login.playerId(), 1001);

    SkillCastResult result = gateway.castSkill(login.playerId(), 2001, new GridVector(1, 0));

    assertEquals(ResultCode.OK, result.code());
    assertEquals(login.playerId(), result.skillEvent().casterId());
    assertEquals(2001, result.skillEvent().skillId());
    assertEquals(new GridPosition(3200, 2400), result.skillEvent().position());
    assertEquals(new GridVector(1, 0), result.skillEvent().aimDir());
    assertNotNull(result.damageEvent());
    assertEquals(login.playerId(), result.damageEvent().sourceId());
    assertEquals(2_000_001L, result.damageEvent().targetId());
    assertEquals(2001, result.damageEvent().skillId());
    assertEquals(-12, result.damageEvent().hpDelta());
    assertFalse(result.damageEvent().dead());
  }

  @Test
  void startRogueReturnsDeterministicConfigDrivenInstance() {
    FirstChainGatewayService gateway = FirstChainGatewayService.createDefault();
    LoginResult login = gateway.login("local-dev-key", 1_700_000_000_000L);

    RogueStartResult result = gateway.startRogue(login.playerId(), 4001);

    assertEquals(ResultCode.OK, result.code());
    assertEquals(login.playerId(), result.playerId());
    assertEquals(4001, result.rogueId());
    assertEquals(9_000_001L, result.instanceId());
    assertEquals(2001, result.mapId());
    assertEquals(new GridPosition(1500, 1500), result.spawnPosition());
    assertEquals(3001, result.monsterId());
    assertEquals(8, result.monsterCount());
    assertEquals(5001, result.rewardPoolId());
    assertEquals(9, result.entities().size());
  }

  @Test
  void finishRogueReturnsDeterministicReward() {
    FirstChainGatewayService gateway = FirstChainGatewayService.createDefault();
    LoginResult login = gateway.login("local-dev-key", 1_700_000_000_000L);
    RogueStartResult start = gateway.startRogue(login.playerId(), 4001);

    RogueFinishResult result = gateway.finishRogue(login.playerId(), start.instanceId());

    assertEquals(ResultCode.OK, result.code());
    assertEquals(start.instanceId(), result.instanceId());
    assertTrue(result.success());
    assertEquals(1, result.rewards().size());
    assertEquals(6001, result.rewards().getFirst().itemId());
    assertEquals(3, result.rewards().getFirst().count());
  }

  @Test
  void finishRogueWithoutStartedInstanceReturnsNotFound() {
    FirstChainGatewayService gateway = FirstChainGatewayService.createDefault();
    LoginResult login = gateway.login("local-dev-key", 1_700_000_000_000L);

    RogueFinishResult result = gateway.finishRogue(login.playerId(), 9_000_001L);

    assertEquals(ResultCode.NOT_FOUND, result.code());
    assertFalse(result.success());
    assertTrue(result.rewards().isEmpty());
  }
}
