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
}
