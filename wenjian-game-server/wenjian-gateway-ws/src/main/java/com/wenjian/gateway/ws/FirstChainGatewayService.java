package com.wenjian.gateway.ws;

import com.wenjian.config.CsvConfigLoader;
import com.wenjian.config.GameConfigRepository;
import com.wenjian.core.firstchain.FirstChainCoreDtos;
import com.wenjian.core.firstchain.FirstChainCoreService;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.List;
import java.util.Objects;

final class FirstChainGatewayService {
  private final FirstChainCoreService coreService;

  private FirstChainGatewayService(FirstChainCoreService coreService) {
    this.coreService = Objects.requireNonNull(coreService, "coreService");
  }

  static FirstChainGatewayService createDefault() {
    return createFromConfigDir(resolveDefaultConfigSourceDir());
  }

  static FirstChainGatewayService createFromConfigDir(Path configSourceDir) {
    GameConfigRepository configs = CsvConfigLoader.load(configSourceDir).build();
    return new FirstChainGatewayService(new FirstChainCoreService(configs));
  }

  LoginResult login(String loginKey, long serverTimeMs) {
    return toGateway(coreService.login(loginKey, serverTimeMs));
  }

  EnterRegionResult enterRegion(long playerId, int regionId) {
    return toGateway(coreService.enterRegion(playerId, regionId));
  }

  SkillCastResult castSkill(long playerId, int skillId, GridVector aimDir) {
    FirstChainCoreDtos.GridVector coreAimDir =
        new FirstChainCoreDtos.GridVector(aimDir.x(), aimDir.y());
    return toGateway(coreService.castSkill(playerId, skillId, coreAimDir));
  }

  RogueStartResult startRogue(long playerId, int rogueId) {
    return toGateway(coreService.startRogue(playerId, rogueId));
  }

  RogueFinishResult finishRogue(long playerId, long instanceId) {
    return toGateway(coreService.finishRogue(playerId, instanceId));
  }

  private static LoginResult toGateway(FirstChainCoreDtos.LoginResult result) {
    return new LoginResult(
        toGateway(result.code()),
        result.playerId(),
        result.sessionToken(),
        result.regionId(),
        toGateway(result.position()),
        result.serverTimeMs());
  }

  private static EnterRegionResult toGateway(FirstChainCoreDtos.EnterRegionResult result) {
    return new EnterRegionResult(
        toGateway(result.code()),
        result.regionId(),
        toGateway(result.self()),
        result.entities().stream().map(FirstChainGatewayService::toGateway).toList(),
        result.serverTick());
  }

  private static SkillCastResult toGateway(FirstChainCoreDtos.SkillCastResult result) {
    return new SkillCastResult(
        toGateway(result.code()),
        toGateway(result.skillEvent()),
        toGateway(result.damageEvent()));
  }

  private static RogueStartResult toGateway(FirstChainCoreDtos.RogueStartResult result) {
    return new RogueStartResult(
        toGateway(result.code()),
        result.playerId(),
        result.rogueId(),
        result.instanceId(),
        result.mapId(),
        toGateway(result.spawnPosition()),
        result.monsterId(),
        result.monsterCount(),
        result.rewardPoolId(),
        result.entities().stream().map(FirstChainGatewayService::toGateway).toList());
  }

  private static RogueFinishResult toGateway(FirstChainCoreDtos.RogueFinishResult result) {
    return new RogueFinishResult(
        toGateway(result.code()),
        result.instanceId(),
        result.success(),
        result.rewards().stream().map(FirstChainGatewayService::toGateway).toList());
  }

  private static ResultCode toGateway(FirstChainCoreDtos.ResultCode code) {
    return switch (code) {
      case OK -> ResultCode.OK;
      case NOT_FOUND -> ResultCode.NOT_FOUND;
    };
  }

  private static EntityKind toGateway(FirstChainCoreDtos.EntityKind kind) {
    return switch (kind) {
      case PLAYER -> EntityKind.PLAYER;
      case MONSTER -> EntityKind.MONSTER;
    };
  }

  private static GridPosition toGateway(FirstChainCoreDtos.GridPosition position) {
    if (position == null) {
      return null;
    }
    return new GridPosition(position.x(), position.y());
  }

  private static EntitySnapshot toGateway(FirstChainCoreDtos.EntitySnapshot snapshot) {
    if (snapshot == null) {
      return null;
    }
    return new EntitySnapshot(
        snapshot.entityId(),
        toGateway(snapshot.kind()),
        toGateway(snapshot.position()),
        snapshot.hp());
  }

  private static SkillEvent toGateway(FirstChainCoreDtos.SkillEvent event) {
    if (event == null) {
      return null;
    }
    return new SkillEvent(
        event.casterId(),
        event.skillId(),
        toGateway(event.position()),
        new GridVector(event.aimDir().x(), event.aimDir().y()));
  }

  private static DamageEvent toGateway(FirstChainCoreDtos.DamageEvent event) {
    if (event == null) {
      return null;
    }
    return new DamageEvent(
        event.sourceId(),
        event.targetId(),
        event.skillId(),
        event.hpDelta(),
        event.dead());
  }

  private static RewardItem toGateway(FirstChainCoreDtos.RewardItem reward) {
    return new RewardItem(reward.itemId(), reward.count());
  }

  private static Path resolveDefaultConfigSourceDir() {
    List<Path> candidates = List.of(
        Path.of("config", "source"),
        Path.of("..", "config", "source"),
        Path.of("..", "..", "config", "source"));
    for (Path candidate : candidates) {
      if (Files.exists(candidate.resolve("player_template.csv"))) {
        return candidate;
      }
    }
    return Path.of("..", "config", "source");
  }
}
