package com.wenjian.core.firstchain;

import static com.wenjian.core.firstchain.FirstChainCoreDtos.GridPosition;
import static com.wenjian.core.firstchain.FirstChainCoreDtos.GridVector;
import static com.wenjian.core.firstchain.FirstChainCoreDtos.ResultCode;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.wenjian.config.CsvConfigLoader;
import com.wenjian.config.GameConfigRepository;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.List;
import org.junit.jupiter.api.Test;

class FirstChainCoreServiceTest {

  @Test
  void loginReturnsConfigDrivenPlayerIdentityAndInitialRegion() {
    FirstChainCoreService core = createCore();

    FirstChainCoreDtos.LoginResult result =
        core.login("local-dev-key", 1_700_000_000_000L);

    assertEquals(ResultCode.OK, result.code());
    assertEquals(1_000_001L, result.playerId());
    assertFalse(result.sessionToken().isBlank());
    assertEquals(1001, result.regionId());
    assertEquals(new GridPosition(3200, 2400), result.position());
    assertEquals(100, result.maxHp());
    assertEquals(1_700_000_000_000L, result.serverTimeMs());
  }

  @Test
  void enterRegionReturnsConfigDrivenSelfAndTrainingMonster() {
    FirstChainCoreService core = createCore();
    FirstChainCoreDtos.LoginResult login =
        core.login("local-dev-key", 1_700_000_000_000L);

    FirstChainCoreDtos.EnterRegionResult result =
        core.enterRegion(login.playerId(), login.regionId());

    assertEquals(ResultCode.OK, result.code());
    assertEquals(1001, result.regionId());
    assertNotNull(result.self());
    assertEquals(login.playerId(), result.self().entityId());
    assertEquals(new GridPosition(3200, 2400), result.self().position());
    assertEquals(100, result.self().hp());
    assertEquals(2, result.entities().size());
    assertTrue(result.entities().stream().anyMatch(entity -> entity.entityId() == 2_000_001L
        && entity.hp() == 60));
    assertTrue(result.serverTick() > 0);
  }

  @Test
  void enterRegionRejectsUnknownPlayerOrWrongRegion() {
    FirstChainCoreService core = createCore();

    FirstChainCoreDtos.EnterRegionResult wrongPlayer = core.enterRegion(404L, 1001);
    FirstChainCoreDtos.EnterRegionResult wrongRegion = core.enterRegion(1_000_001L, 9999);

    assertEquals(ResultCode.NOT_FOUND, wrongPlayer.code());
    assertTrue(wrongPlayer.entities().isEmpty());
    assertEquals(ResultCode.NOT_FOUND, wrongRegion.code());
    assertTrue(wrongRegion.entities().isEmpty());
  }

  @Test
  void castDefaultSkillUsesConfigDamage() {
    FirstChainCoreService core = createCore();
    FirstChainCoreDtos.LoginResult login =
        core.login("local-dev-key", 1_700_000_000_000L);

    FirstChainCoreDtos.SkillCastResult result =
        core.castSkill(login.playerId(), 2001, new GridVector(1, 0));

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
  void castUnknownSkillReturnsNotFound() {
    FirstChainCoreService core = createCore();

    FirstChainCoreDtos.SkillCastResult result =
        core.castSkill(1_000_001L, 9999, new GridVector(1, 0));

    assertEquals(ResultCode.NOT_FOUND, result.code());
    assertEquals(null, result.skillEvent());
    assertEquals(null, result.damageEvent());
  }

  @Test
  void startRogueReturnsConfigDrivenInstance() {
    FirstChainCoreService core = createCore();
    FirstChainCoreDtos.LoginResult login =
        core.login("local-dev-key", 1_700_000_000_000L);

    FirstChainCoreDtos.RogueStartResult result = core.startRogue(login.playerId(), 4001);

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
    assertTrue(result.entities().stream().filter(entity -> entity.entityId() >= 3_000_001L)
        .allMatch(entity -> entity.hp() == 60));
  }

  @Test
  void startUnknownRogueReturnsNotFound() {
    FirstChainCoreService core = createCore();

    FirstChainCoreDtos.RogueStartResult result = core.startRogue(1_000_001L, 9999);

    assertEquals(ResultCode.NOT_FOUND, result.code());
    assertEquals(9999, result.rogueId());
    assertTrue(result.entities().isEmpty());
  }

  @Test
  void finishRogueReturnsConfigDrivenRewardAfterStart() {
    FirstChainCoreService core = createCore();
    FirstChainCoreDtos.LoginResult login =
        core.login("local-dev-key", 1_700_000_000_000L);
    FirstChainCoreDtos.RogueStartResult start = core.startRogue(login.playerId(), 4001);

    FirstChainCoreDtos.RogueFinishResult result =
        core.finishRogue(login.playerId(), start.instanceId());

    assertEquals(ResultCode.OK, result.code());
    assertEquals(start.instanceId(), result.instanceId());
    assertTrue(result.success());
    assertEquals(List.of(new FirstChainCoreDtos.RewardItem(6001, 3)), result.rewards());
  }

  @Test
  void finishRogueWithoutStartedInstanceReturnsNotFound() {
    FirstChainCoreService core = createCore();

    FirstChainCoreDtos.RogueFinishResult result =
        core.finishRogue(1_000_001L, 9_000_001L);

    assertEquals(ResultCode.NOT_FOUND, result.code());
    assertFalse(result.success());
    assertTrue(result.rewards().isEmpty());
  }

  private FirstChainCoreService createCore() {
    GameConfigRepository configs = CsvConfigLoader.load(configSourceDir()).build();
    return new FirstChainCoreService(configs);
  }

  private Path configSourceDir() {
    List<Path> candidates = List.of(
        Path.of("..", "..", "config", "source"),
        Path.of("..", "config", "source"),
        Path.of("config", "source"));
    for (Path candidate : candidates) {
      if (Files.exists(candidate.resolve("player_template.csv"))) {
        return candidate;
      }
    }
    throw new IllegalStateException("Cannot locate config/source from " + Path.of("").toAbsolutePath());
  }
}
