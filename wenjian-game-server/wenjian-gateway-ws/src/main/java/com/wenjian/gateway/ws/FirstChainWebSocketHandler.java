package com.wenjian.gateway.ws;

import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;
import org.springframework.web.socket.handler.TextWebSocketHandler;

final class FirstChainWebSocketHandler extends TextWebSocketHandler {
  private final FirstChainGatewayService gatewayService;

  FirstChainWebSocketHandler(FirstChainGatewayService gatewayService) {
    this.gatewayService = gatewayService;
  }

  @Override
  protected void handleTextMessage(WebSocketSession session, TextMessage message) throws Exception {
    String payload = message.getPayload().trim();
    if (payload.startsWith("LOGIN ")) {
      session.sendMessage(new TextMessage(formatLogin(handleLogin(payload))));
      return;
    }
    if (payload.startsWith("ENTER_REGION ")) {
      session.sendMessage(new TextMessage(formatRegion(handleEnterRegion(payload))));
      return;
    }
    if (payload.startsWith("SKILL ")) {
      SkillCastResult result = handleSkill(payload);
      session.sendMessage(new TextMessage(formatSkill(result)));
      if (result.damageEvent() != null) {
        session.sendMessage(new TextMessage(formatDamage(result.damageEvent())));
      }
      return;
    }
    if (payload.startsWith("START_ROGUE ")) {
      session.sendMessage(new TextMessage(formatRogueStart(handleStartRogue(payload))));
      return;
    }
    if (payload.startsWith("FINISH_ROGUE ")) {
      session.sendMessage(new TextMessage(formatRogueFinish(handleFinishRogue(payload))));
      return;
    }
    session.sendMessage(new TextMessage("type=ERROR code=BAD_REQUEST"));
  }

  private LoginResult handleLogin(String payload) {
    String[] parts = payload.split("\\s+");
    long serverTimeMs = parts.length >= 3 ? Long.parseLong(parts[2]) : System.currentTimeMillis();
    return gatewayService.login(parts[1], serverTimeMs);
  }

  private EnterRegionResult handleEnterRegion(String payload) {
    String[] parts = payload.split("\\s+");
    return gatewayService.enterRegion(Long.parseLong(parts[1]), Integer.parseInt(parts[2]));
  }

  private SkillCastResult handleSkill(String payload) {
    String[] parts = payload.split("\\s+");
    return gatewayService.castSkill(
        Long.parseLong(parts[1]),
        Integer.parseInt(parts[2]),
        new GridVector(Integer.parseInt(parts[3]), Integer.parseInt(parts[4])));
  }

  private RogueStartResult handleStartRogue(String payload) {
    String[] parts = payload.split("\\s+");
    return gatewayService.startRogue(Long.parseLong(parts[1]), Integer.parseInt(parts[2]));
  }

  private RogueFinishResult handleFinishRogue(String payload) {
    String[] parts = payload.split("\\s+");
    return gatewayService.finishRogue(Long.parseLong(parts[1]), Long.parseLong(parts[2]));
  }

  private String formatLogin(LoginResult result) {
    return "type=LOGIN_OK"
        + " code=" + result.code()
        + " playerId=" + result.playerId()
        + " regionId=" + result.regionId()
        + " x=" + result.position().x()
        + " y=" + result.position().y()
        + " sessionToken=" + result.sessionToken()
        + " serverTimeMs=" + result.serverTimeMs();
  }

  private String formatRegion(EnterRegionResult result) {
    String selfId = result.self() == null ? "none" : Long.toString(result.self().entityId());
    return "type=REGION_SNAPSHOT"
        + " code=" + result.code()
        + " regionId=" + result.regionId()
        + " self=" + selfId
        + " entities=" + result.entities().size()
        + " serverTick=" + result.serverTick();
  }

  private String formatSkill(SkillCastResult result) {
    if (result.skillEvent() == null) {
      return "type=SKILL_EVENT code=" + result.code();
    }

    SkillEvent event = result.skillEvent();
    return "type=SKILL_EVENT"
        + " code=" + result.code()
        + " casterId=" + event.casterId()
        + " skillId=" + event.skillId()
        + " x=" + event.position().x()
        + " y=" + event.position().y()
        + " aimX=" + event.aimDir().x()
        + " aimY=" + event.aimDir().y();
  }

  private String formatDamage(DamageEvent event) {
    return "type=DAMAGE_EVENT"
        + " code=OK"
        + " sourceId=" + event.sourceId()
        + " targetId=" + event.targetId()
        + " skillId=" + event.skillId()
        + " hpDelta=" + event.hpDelta()
        + " dead=" + event.dead();
  }

  private String formatRogueStart(RogueStartResult result) {
    if (result.code() != ResultCode.OK) {
      return "type=ROGUE_START code=" + result.code();
    }

    return "type=ROGUE_START"
        + " code=" + result.code()
        + " playerId=" + result.playerId()
        + " rogueId=" + result.rogueId()
        + " instanceId=" + result.instanceId()
        + " mapId=" + result.mapId()
        + " x=" + result.spawnPosition().x()
        + " y=" + result.spawnPosition().y()
        + " monsterId=" + result.monsterId()
        + " monsters=" + result.monsterCount()
        + " rewardPoolId=" + result.rewardPoolId()
        + " entities=" + result.entities().size();
  }

  private String formatRogueFinish(RogueFinishResult result) {
    if (result.code() != ResultCode.OK) {
      return "type=ROGUE_FINISH code=" + result.code();
    }

    RewardItem reward = result.rewards().getFirst();
    return "type=ROGUE_FINISH"
        + " code=" + result.code()
        + " instanceId=" + result.instanceId()
        + " success=" + result.success()
        + " itemId=" + reward.itemId()
        + " count=" + reward.count();
  }
}
