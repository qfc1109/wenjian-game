package com.wenjian.protobuf;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertInstanceOf;
import static org.junit.jupiter.api.Assertions.assertNotNull;

import com.google.protobuf.Message;
import com.wenjian.protobuf.account.LoginResp;
import com.wenjian.protobuf.battle.DamageEvent;
import com.wenjian.protobuf.battle.SkillEvent;
import com.wenjian.protobuf.rogue.FinishRoguePush;
import com.wenjian.protobuf.rogue.StartRogueResp;
import com.wenjian.protobuf.world.EnterRegionResp;
import org.junit.jupiter.api.Test;

class ProtobufGeneratedTypesTest {

  @Test
  void loginRespCanBeConstructed() {
    LoginResp resp = LoginResp.newBuilder()
        .setCode(com.wenjian.protobuf.common.ErrorCode.ERROR_CODE_OK)
        .setPlayerId(1_000_001L)
        .setSessionToken("session-token-test")
        .setRegionId(1001)
        .setPosition(com.wenjian.protobuf.common.Vec2i.newBuilder().setX(10).setY(12))
        .setServerTimeMs(1_700_000_000_000L)
        .build();

    assertNotNull(resp);
    assertEquals(com.wenjian.protobuf.common.ErrorCode.ERROR_CODE_OK, resp.getCode());
    assertEquals(1_000_001L, resp.getPlayerId());
    assertEquals("session-token-test", resp.getSessionToken());
    assertEquals(1001, resp.getRegionId());
    assertEquals(10, resp.getPosition().getX());
    assertEquals(12, resp.getPosition().getY());
    assertEquals(1_700_000_000_000L, resp.getServerTimeMs());
  }

  @Test
  void enterRegionRespCanBeConstructed() {
    EnterRegionResp resp = EnterRegionResp.newBuilder()
        .setCode(com.wenjian.protobuf.common.ErrorCode.ERROR_CODE_OK)
        .setRegionId(1001)
        .setSelf(com.wenjian.protobuf.common.EntityState.newBuilder()
            .setEntityId(1_000_001L)
            .setEntityType(com.wenjian.protobuf.common.EntityType.ENTITY_TYPE_PLAYER)
            .setPosition(com.wenjian.protobuf.common.Vec2i.newBuilder().setX(10).setY(12))
            .setHp(100)
            .setMaxHp(100))
        .setServerTick(1L)
        .build();

    assertNotNull(resp);
    assertEquals(com.wenjian.protobuf.common.ErrorCode.ERROR_CODE_OK, resp.getCode());
    assertEquals(1001, resp.getRegionId());
    assertEquals(1_000_001L, resp.getSelf().getEntityId());
    assertEquals(com.wenjian.protobuf.common.EntityType.ENTITY_TYPE_PLAYER, resp.getSelf().getEntityType());
    assertEquals(10, resp.getSelf().getPosition().getX());
    assertEquals(12, resp.getSelf().getPosition().getY());
  }

  @Test
  void skillEventCanBeConstructed() {
    SkillEvent event = SkillEvent.newBuilder()
        .setCasterId(1_000_001L)
        .setSkillId(2001)
        .setPosition(com.wenjian.protobuf.common.Vec2i.newBuilder().setX(10).setY(12))
        .setAimDir(com.wenjian.protobuf.common.Vec2i.newBuilder().setX(1).setY(0))
        .build();

    assertNotNull(event);
    assertEquals(1_000_001L, event.getCasterId());
    assertEquals(2001, event.getSkillId());
    assertEquals(10, event.getPosition().getX());
    assertEquals(12, event.getPosition().getY());
    assertEquals(1, event.getAimDir().getX());
    assertEquals(0, event.getAimDir().getY());
  }

  @Test
  void damageEventCanBeConstructed() {
    DamageEvent event = DamageEvent.newBuilder()
        .setSourceId(1_000_001L)
        .setTargetId(2_000_001L)
        .setSkillId(2001)
        .setHpDelta(-12)
        .setDead(false)
        .build();

    assertNotNull(event);
    assertEquals(1_000_001L, event.getSourceId());
    assertEquals(2_000_001L, event.getTargetId());
    assertEquals(2001, event.getSkillId());
    assertEquals(-12, event.getHpDelta());
    assertEquals(false, event.getDead());
  }

  @Test
  void startRogueRespCanBeConstructed() {
    StartRogueResp resp = StartRogueResp.newBuilder()
        .setCode(com.wenjian.protobuf.common.ErrorCode.ERROR_CODE_OK)
        .setInstanceId(9_000_001L)
        .setMapId(2001)
        .setSelf(com.wenjian.protobuf.common.EntityState.newBuilder()
            .setEntityId(1_000_001L)
            .setEntityType(com.wenjian.protobuf.common.EntityType.ENTITY_TYPE_PLAYER)
            .setPosition(com.wenjian.protobuf.common.Vec2i.newBuilder().setX(1500).setY(1500))
            .setHp(100)
            .setMaxHp(100))
        .build();

    assertNotNull(resp);
    assertEquals(com.wenjian.protobuf.common.ErrorCode.ERROR_CODE_OK, resp.getCode());
    assertEquals(9_000_001L, resp.getInstanceId());
    assertEquals(2001, resp.getMapId());
    assertEquals(1_000_001L, resp.getSelf().getEntityId());
    assertEquals(1500, resp.getSelf().getPosition().getX());
    assertEquals(1500, resp.getSelf().getPosition().getY());
  }

  @Test
  void finishRoguePushCanBeConstructed() {
    FinishRoguePush push = FinishRoguePush.newBuilder()
        .setInstanceId(9_000_001L)
        .setSuccess(true)
        .addRewards(com.wenjian.protobuf.rogue.RewardItem.newBuilder()
            .setItemId(6001)
            .setCount(3))
        .build();

    assertNotNull(push);
    assertEquals(9_000_001L, push.getInstanceId());
    assertEquals(true, push.getSuccess());
    assertEquals(1, push.getRewardsCount());
    assertEquals(6001, push.getRewards(0).getItemId());
    assertEquals(3, push.getRewards(0).getCount());
  }

  @Test
  void allMessageTypesExtendGeneratedMessage() {
    assertInstanceOf(Message.class, LoginResp.getDefaultInstance());
    assertInstanceOf(Message.class, EnterRegionResp.getDefaultInstance());
    assertInstanceOf(Message.class, SkillEvent.getDefaultInstance());
    assertInstanceOf(Message.class, DamageEvent.getDefaultInstance());
    assertInstanceOf(Message.class, StartRogueResp.getDefaultInstance());
    assertInstanceOf(Message.class, FinishRoguePush.getDefaultInstance());
  }
}
