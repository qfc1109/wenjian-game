package com.wenjian.gateway.ws;

import com.wenjian.protobuf.account.LoginResp;
import com.wenjian.protobuf.common.EntityState;
import com.wenjian.protobuf.common.EntityType;
import com.wenjian.protobuf.common.ErrorCode;
import com.wenjian.protobuf.common.Vec2i;
import com.wenjian.protobuf.rogue.FinishRoguePush;
import com.wenjian.protobuf.rogue.StartRogueResp;
import com.wenjian.protobuf.world.EnterRegionResp;

/**
 * Maps internal gateway record types to protobuf-generated message types.
 * This mapper establishes the type mapping from the current text-based WebSocket
 * debug protocol to the canonical protobuf schema defined in protobuf/.
 *
 * <p>Key responsibilities:
 * <ul>
 *   <li>Convert internal result codes to {@link ErrorCode}</li>
 *   <li>Convert internal grid coordinates to {@link Vec2i}</li>
 *   <li>Convert internal entity kinds to {@link EntityType}</li>
 *   <li>Convert internal entity snapshots to {@link EntityState}</li>
 *   <li>Build protobuf responses from internal results</li>
 * </ul>
 *
 * <p>This mapper does NOT modify existing text formatting. It provides the
 * type-safe protobuf path for future binary transport while keeping the
 * current text WebSocket integration tests passing.
 */
public final class FirstChainProtocolMapper {

  private FirstChainProtocolMapper() {
  }

  // --- Error code mapping ---

  public static ErrorCode toProtobufErrorCode(ResultCode code) {
    return switch (code) {
      case OK -> ErrorCode.ERROR_CODE_OK;
      case NOT_FOUND -> ErrorCode.ERROR_CODE_NOT_FOUND;
    };
  }

  // --- Vec2i mapping ---

  public static Vec2i toVec2i(GridPosition pos) {
    return Vec2i.newBuilder()
        .setX(pos.x())
        .setY(pos.y())
        .build();
  }

  public static Vec2i toVec2i(GridVector vec) {
    return Vec2i.newBuilder()
        .setX(vec.x())
        .setY(vec.y())
        .build();
  }

  // --- EntityType mapping ---

  public static EntityType toProtobufEntityType(EntityKind kind) {
    return switch (kind) {
      case PLAYER -> EntityType.ENTITY_TYPE_PLAYER;
      case MONSTER -> EntityType.ENTITY_TYPE_MONSTER;
    };
  }

  // --- EntityState mapping ---

  public static EntityState toEntityState(EntitySnapshot snapshot, int maxHp) {
    return EntityState.newBuilder()
        .setEntityId(snapshot.entityId())
        .setEntityType(toProtobufEntityType(snapshot.kind()))
        .setPosition(toVec2i(snapshot.position()))
        .setHp(snapshot.hp())
        .setMaxHp(maxHp)
        .build();
  }

  public static EntityState toEntityStateWithDefaultMaxHp(EntitySnapshot snapshot) {
    return toEntityState(snapshot, snapshot.hp());
  }

  // --- Response builders ---

  public static LoginResp toLoginResp(LoginResult result) {
    LoginResp.Builder builder = LoginResp.newBuilder()
        .setCode(toProtobufErrorCode(result.code()))
        .setPlayerId(result.playerId())
        .setSessionToken(result.sessionToken())
        .setRegionId(result.regionId())
        .setServerTimeMs(result.serverTimeMs());

    if (result.position() != null) {
      builder.setPosition(toVec2i(result.position()));
    }

    return builder.build();
  }

  public static EnterRegionResp toEnterRegionResp(EnterRegionResult result) {
    EnterRegionResp.Builder builder = EnterRegionResp.newBuilder()
        .setCode(toProtobufErrorCode(result.code()))
        .setRegionId(result.regionId())
        .setServerTick(result.serverTick());

    if (result.self() != null) {
      builder.setSelf(toEntityStateWithDefaultMaxHp(result.self()));
    }

    for (EntitySnapshot entity : result.entities()) {
      builder.addEntities(toEntityStateWithDefaultMaxHp(entity));
    }

    return builder.build();
  }

  public static com.wenjian.protobuf.battle.SkillEvent toProtobufSkillEvent(
      SkillEvent event) {
    return com.wenjian.protobuf.battle.SkillEvent.newBuilder()
        .setCasterId(event.casterId())
        .setSkillId(event.skillId())
        .setPosition(toVec2i(event.position()))
        .setAimDir(toVec2i(event.aimDir()))
        .build();
  }

  public static com.wenjian.protobuf.battle.DamageEvent toProtobufDamageEvent(
      DamageEvent event) {
    return com.wenjian.protobuf.battle.DamageEvent.newBuilder()
        .setSourceId(event.sourceId())
        .setTargetId(event.targetId())
        .setSkillId(event.skillId())
        .setHpDelta(event.hpDelta())
        .setDead(event.dead())
        .build();
  }

  public static StartRogueResp toStartRogueResp(RogueStartResult result) {
    StartRogueResp.Builder builder = StartRogueResp.newBuilder()
        .setCode(toProtobufErrorCode(result.code()))
        .setInstanceId(result.instanceId())
        .setMapId(result.mapId());

    if (result.spawnPosition() != null) {
      builder.setSelf(EntityState.newBuilder()
          .setEntityId(result.playerId())
          .setEntityType(EntityType.ENTITY_TYPE_PLAYER)
          .setPosition(toVec2i(result.spawnPosition()))
          .setHp(100)
          .setMaxHp(100));
    }

    for (EntitySnapshot entity : result.entities()) {
      builder.addEntities(toEntityStateWithDefaultMaxHp(entity));
    }

    return builder.build();
  }

  public static FinishRoguePush toFinishRoguePush(RogueFinishResult result) {
    FinishRoguePush.Builder builder = FinishRoguePush.newBuilder()
        .setInstanceId(result.instanceId())
        .setSuccess(result.success());

    for (RewardItem reward : result.rewards()) {
      builder.addRewards(com.wenjian.protobuf.rogue.RewardItem.newBuilder()
          .setItemId(reward.itemId())
          .setCount(reward.count()));
    }

    return builder.build();
  }
}
