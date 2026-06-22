package com.wenjian.gateway.ws;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.wenjian.protobuf.account.LoginResp;
import com.wenjian.protobuf.common.EntityState;
import com.wenjian.protobuf.common.EntityType;
import com.wenjian.protobuf.common.ErrorCode;
import com.wenjian.protobuf.common.Vec2i;
import com.wenjian.protobuf.rogue.FinishRoguePush;
import com.wenjian.protobuf.rogue.StartRogueResp;
import com.wenjian.protobuf.world.EnterRegionResp;
import java.util.List;
import org.junit.jupiter.api.Test;

class FirstChainProtocolMapperTest {

  @Test
  void toProtobufErrorCode_mapsOk() {
    ErrorCode result = FirstChainProtocolMapper.toProtobufErrorCode(ResultCode.OK);
    assertEquals(ErrorCode.ERROR_CODE_OK, result);
  }

  @Test
  void toProtobufErrorCode_mapsNotFound() {
    ErrorCode result = FirstChainProtocolMapper.toProtobufErrorCode(ResultCode.NOT_FOUND);
    assertEquals(ErrorCode.ERROR_CODE_NOT_FOUND, result);
  }

  // --- Vec2i mapping tests ---

  @Test
  void toVec2i_fromGridPosition() {
    Vec2i result = FirstChainProtocolMapper.toVec2i(new GridPosition(10, 12));
    assertEquals(10, result.getX());
    assertEquals(12, result.getY());
  }

  @Test
  void toVec2i_fromGridVector() {
    Vec2i result = FirstChainProtocolMapper.toVec2i(new GridVector(1, 0));
    assertEquals(1, result.getX());
    assertEquals(0, result.getY());
  }

  // --- EntityType mapping tests ---

  @Test
  void toProtobufEntityType_mapsPlayer() {
    EntityType result = FirstChainProtocolMapper.toProtobufEntityType(EntityKind.PLAYER);
    assertEquals(EntityType.ENTITY_TYPE_PLAYER, result);
  }

  @Test
  void toProtobufEntityType_mapsMonster() {
    EntityType result = FirstChainProtocolMapper.toProtobufEntityType(EntityKind.MONSTER);
    assertEquals(EntityType.ENTITY_TYPE_MONSTER, result);
  }

  // --- LoginResp mapping tests ---

  @Test
  void toLoginResp_mapsSuccessFields() {
    LoginResult internal = new LoginResult(
        ResultCode.OK,
        1_000_001L,
        "session-1000001-1700000000000",
        1001,
        new GridPosition(10, 12),
        1_700_000_000_000L);

    LoginResp proto = FirstChainProtocolMapper.toLoginResp(internal);

    assertEquals(ErrorCode.ERROR_CODE_OK, proto.getCode());
    assertEquals(1_000_001L, proto.getPlayerId());
    assertEquals("session-1000001-1700000000000", proto.getSessionToken());
    assertEquals(1001, proto.getRegionId());
    assertEquals(10, proto.getPosition().getX());
    assertEquals(12, proto.getPosition().getY());
    assertEquals(1_700_000_000_000L, proto.getServerTimeMs());
  }

  @Test
  void toLoginResp_mapsNotFoundCode() {
    LoginResult internal = new LoginResult(
        ResultCode.NOT_FOUND,
        0L,
        "",
        0,
        null,
        0L);

    LoginResp proto = FirstChainProtocolMapper.toLoginResp(internal);

    assertEquals(ErrorCode.ERROR_CODE_NOT_FOUND, proto.getCode());
    assertEquals(0L, proto.getPlayerId());
  }

  // --- EnterRegionResp mapping tests ---

  @Test
  void toEnterRegionResp_mapsFullSnapshot() {
    EntitySnapshot self = new EntitySnapshot(
        1_000_001L, EntityKind.PLAYER,
        new GridPosition(10, 12), 100);
    EntitySnapshot monster = new EntitySnapshot(
        2_000_001L, EntityKind.MONSTER,
        new GridPosition(18, 12), 60);

    EnterRegionResult internal = new EnterRegionResult(
        ResultCode.OK,
        1001,
        self,
        List.of(self, monster),
        1L);

    EnterRegionResp proto = FirstChainProtocolMapper.toEnterRegionResp(internal);

    assertEquals(ErrorCode.ERROR_CODE_OK, proto.getCode());
    assertEquals(1001, proto.getRegionId());
    assertEquals(1L, proto.getServerTick());
    assertNotNull(proto.getSelf());
    assertEquals(1_000_001L, proto.getSelf().getEntityId());
    assertEquals(EntityType.ENTITY_TYPE_PLAYER, proto.getSelf().getEntityType());
    assertEquals(10, proto.getSelf().getPosition().getX());
    assertEquals(12, proto.getSelf().getPosition().getY());
    assertEquals(100, proto.getSelf().getHp());
    assertEquals(2, proto.getEntitiesList().size());
    assertEquals(2_000_001L, proto.getEntitiesList().get(1).getEntityId());
    assertEquals(EntityType.ENTITY_TYPE_MONSTER, proto.getEntitiesList().get(1).getEntityType());
    assertEquals(18, proto.getEntitiesList().get(1).getPosition().getX());
    assertEquals(12, proto.getEntitiesList().get(1).getPosition().getY());
    assertEquals(60, proto.getEntitiesList().get(1).getHp());
  }

  @Test
  void toEnterRegionResp_mapsNotFound() {
    EnterRegionResult internal = new EnterRegionResult(
        ResultCode.NOT_FOUND,
        9999,
        null,
        List.of(),
        0L);

    EnterRegionResp proto = FirstChainProtocolMapper.toEnterRegionResp(internal);

    assertEquals(ErrorCode.ERROR_CODE_NOT_FOUND, proto.getCode());
    assertEquals(9999, proto.getRegionId());
    assertFalse(proto.hasSelf());
    assertTrue(proto.getEntitiesList().isEmpty());
    assertEquals(0L, proto.getServerTick());
  }

  // --- SkillEvent mapping tests ---

  @Test
  void toProtobufSkillEvent_mapsAllFields() {
    SkillEvent internal = new SkillEvent(
        1_000_001L,
        2001,
        new GridPosition(10, 12),
        new GridVector(1, 0));

    com.wenjian.protobuf.battle.SkillEvent proto =
        FirstChainProtocolMapper.toProtobufSkillEvent(internal);

    assertEquals(1_000_001L, proto.getCasterId());
    assertEquals(2001, proto.getSkillId());
    assertEquals(10, proto.getPosition().getX());
    assertEquals(12, proto.getPosition().getY());
    assertEquals(1, proto.getAimDir().getX());
    assertEquals(0, proto.getAimDir().getY());
  }

  // --- DamageEvent mapping tests ---

  @Test
  void toProtobufDamageEvent_mapsAllFields() {
    DamageEvent internal = new DamageEvent(
        1_000_001L,
        2_000_001L,
        2001,
        -12,
        false);

    com.wenjian.protobuf.battle.DamageEvent proto =
        FirstChainProtocolMapper.toProtobufDamageEvent(internal);

    assertEquals(1_000_001L, proto.getSourceId());
    assertEquals(2_000_001L, proto.getTargetId());
    assertEquals(2001, proto.getSkillId());
    assertEquals(-12, proto.getHpDelta());
    assertFalse(proto.getDead());
  }

  @Test
  void toProtobufDamageEvent_mapsDeadTrue() {
    DamageEvent internal = new DamageEvent(
        1_000_001L,
        2_000_001L,
        2001,
        -60,
        true);

    com.wenjian.protobuf.battle.DamageEvent proto =
        FirstChainProtocolMapper.toProtobufDamageEvent(internal);

    assertEquals(-60, proto.getHpDelta());
    assertTrue(proto.getDead());
  }

  // --- StartRogueResp mapping tests ---

  @Test
  void toStartRogueResp_mapsFullInstance() {
    EntitySnapshot self = new EntitySnapshot(
        1_000_001L, EntityKind.PLAYER,
        new GridPosition(1500, 1500), 100);
    EntitySnapshot monster = new EntitySnapshot(
        3_000_001L, EntityKind.MONSTER,
        new GridPosition(1502, 1500), 60);

    RogueStartResult internal = new RogueStartResult(
        ResultCode.OK,
        1_000_001L,
        4001,
        9_000_001L,
        2001,
        new GridPosition(1500, 1500),
        3001,
        8,
        5001,
        List.of(self, monster));

    StartRogueResp proto = FirstChainProtocolMapper.toStartRogueResp(internal);

    assertEquals(ErrorCode.ERROR_CODE_OK, proto.getCode());
    assertEquals(9_000_001L, proto.getInstanceId());
    assertEquals(2001, proto.getMapId());
    assertNotNull(proto.getSelf());
    assertEquals(1_000_001L, proto.getSelf().getEntityId());
    assertEquals(EntityType.ENTITY_TYPE_PLAYER, proto.getSelf().getEntityType());
    assertEquals(1500, proto.getSelf().getPosition().getX());
    assertEquals(1500, proto.getSelf().getPosition().getY());
    assertEquals(100, proto.getSelf().getHp());
    assertEquals(2, proto.getEntitiesList().size());
    assertEquals(3_000_001L, proto.getEntitiesList().get(1).getEntityId());
    assertEquals(EntityType.ENTITY_TYPE_MONSTER, proto.getEntitiesList().get(1).getEntityType());
  }

  @Test
  void toStartRogueResp_mapsNotFound() {
    RogueStartResult internal = new RogueStartResult(
        ResultCode.NOT_FOUND,
        1_000_001L,
        9999,
        0L,
        0,
        null,
        0,
        0,
        0,
        List.of());

    StartRogueResp proto = FirstChainProtocolMapper.toStartRogueResp(internal);

    assertEquals(ErrorCode.ERROR_CODE_NOT_FOUND, proto.getCode());
    assertEquals(0L, proto.getInstanceId());
    assertFalse(proto.hasSelf());
  }

  // --- FinishRoguePush mapping tests ---

  @Test
  void toFinishRoguePush_mapsSuccessWithRewards() {
    RogueFinishResult internal = new RogueFinishResult(
        ResultCode.OK,
        9_000_001L,
        true,
        List.of(
            new RewardItem(6001, 3),
            new RewardItem(6002, 1)));

    FinishRoguePush proto = FirstChainProtocolMapper.toFinishRoguePush(internal);

    assertEquals(9_000_001L, proto.getInstanceId());
    assertTrue(proto.getSuccess());
    assertEquals(2, proto.getRewardsList().size());
    assertEquals(6001, proto.getRewardsList().get(0).getItemId());
    assertEquals(3, proto.getRewardsList().get(0).getCount());
    assertEquals(6002, proto.getRewardsList().get(1).getItemId());
    assertEquals(1, proto.getRewardsList().get(1).getCount());
  }

  @Test
  void toFinishRoguePush_mapsFailedWithoutRewards() {
    RogueFinishResult internal = new RogueFinishResult(
        ResultCode.OK,
        9_000_001L,
        false,
        List.of());

    FinishRoguePush proto = FirstChainProtocolMapper.toFinishRoguePush(internal);

    assertEquals(9_000_001L, proto.getInstanceId());
    assertFalse(proto.getSuccess());
    assertTrue(proto.getRewardsList().isEmpty());
  }

  // --- Full pipeline integration test ---

  @Test
  void loginToEnterRegionToSkill_pipelineProducesConsistentProtobuf() {
    FirstChainGatewayService gateway = FirstChainGatewayService.createDefault();
    long serverTime = 1_700_000_000_000L;

    LoginResult loginResult = gateway.login("local-dev-key", serverTime);
    LoginResp loginProto = FirstChainProtocolMapper.toLoginResp(loginResult);

    EnterRegionResult regionResult =
        gateway.enterRegion(loginResult.playerId(), loginResult.regionId());
    EnterRegionResp regionProto = FirstChainProtocolMapper.toEnterRegionResp(regionResult);

    SkillCastResult skillResult =
        gateway.castSkill(loginResult.playerId(), 2001, new GridVector(1, 0));
    com.wenjian.protobuf.battle.SkillEvent skillProto =
        FirstChainProtocolMapper.toProtobufSkillEvent(skillResult.skillEvent());
    com.wenjian.protobuf.battle.DamageEvent damageProto =
        FirstChainProtocolMapper.toProtobufDamageEvent(skillResult.damageEvent());

    assertEquals(ErrorCode.ERROR_CODE_OK, loginProto.getCode());
    assertEquals(1_000_001L, loginProto.getPlayerId());
    assertEquals(1001, regionProto.getRegionId());
    assertEquals(1_000_001L, regionProto.getSelf().getEntityId());
    assertEquals(3200, regionProto.getSelf().getPosition().getX());
    assertEquals(2400, regionProto.getSelf().getPosition().getY());
    assertEquals(2001, skillProto.getSkillId());
    assertEquals(3200, skillProto.getPosition().getX());
    assertEquals(2400, skillProto.getPosition().getY());
    assertEquals(2_000_001L, damageProto.getTargetId());
    assertEquals(-12, damageProto.getHpDelta());
  }

  @Test
  void roguePipeline_producesConsistentProtobuf() {
    FirstChainGatewayService gateway = FirstChainGatewayService.createDefault();
    LoginResult loginResult = gateway.login("local-dev-key", 1_700_000_000_000L);

    RogueStartResult rogueStart = gateway.startRogue(loginResult.playerId(), 4001);
    StartRogueResp rogueStartProto = FirstChainProtocolMapper.toStartRogueResp(rogueStart);

    RogueFinishResult rogueFinish =
        gateway.finishRogue(loginResult.playerId(), rogueStart.instanceId());
    FinishRoguePush rogueFinishProto = FirstChainProtocolMapper.toFinishRoguePush(rogueFinish);

    assertEquals(ErrorCode.ERROR_CODE_OK, rogueStartProto.getCode());
    assertEquals(9_000_001L, rogueStartProto.getInstanceId());
    assertEquals(2001, rogueStartProto.getMapId());
    assertEquals(9_000_001L, rogueFinishProto.getInstanceId());
    assertTrue(rogueFinishProto.getSuccess());
    assertEquals(1, rogueFinishProto.getRewardsList().size());
    assertEquals(6001, rogueFinishProto.getRewardsList().get(0).getItemId());
    assertEquals(3, rogueFinishProto.getRewardsList().get(0).getCount());
  }
}
