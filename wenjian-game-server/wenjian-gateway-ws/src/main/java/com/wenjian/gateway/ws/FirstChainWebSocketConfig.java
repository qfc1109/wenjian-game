package com.wenjian.gateway.ws;

import java.nio.file.Path;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Configuration;
import org.springframework.web.socket.config.annotation.EnableWebSocket;
import org.springframework.web.socket.config.annotation.WebSocketConfigurer;
import org.springframework.web.socket.config.annotation.WebSocketHandlerRegistry;

@Configuration
@EnableWebSocket
class FirstChainWebSocketConfig implements WebSocketConfigurer {
  @Value("${wenjian.config.source-dir:../config/source}")
  private String configSourceDir;

  @Override
  public void registerWebSocketHandlers(WebSocketHandlerRegistry registry) {
    registry.addHandler(
        new FirstChainWebSocketHandler(
            FirstChainGatewayService.createFromConfigDir(Path.of(configSourceDir))),
        "/ws/first-chain");
  }
}
