package com.wenjian.gateway.ws;

import static org.junit.jupiter.api.Assertions.assertTrue;

import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.TimeUnit;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.web.server.LocalServerPort;
import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;
import org.springframework.web.socket.client.standard.StandardWebSocketClient;
import org.springframework.web.socket.handler.TextWebSocketHandler;

@SpringBootTest(webEnvironment = SpringBootTest.WebEnvironment.RANDOM_PORT)
class FirstChainWebSocketIntegrationTest {
  @LocalServerPort
  private int port;

  @Value("${server.servlet.context-path:}")
  private String contextPath;

  @Test
  void websocketLoginEnterRegionAndSkillReturnEvents() throws Exception {
    BlockingQueue<String> replies = new LinkedBlockingQueue<>();
    StandardWebSocketClient client = new StandardWebSocketClient();
    String uri = "ws://localhost:" + port + contextPath + "/ws/first-chain";

    WebSocketSession session = client.execute(new TextWebSocketHandler() {
      @Override
      protected void handleTextMessage(WebSocketSession session, TextMessage message) {
        replies.add(message.getPayload());
      }
    }, uri).get(5, TimeUnit.SECONDS);

    session.sendMessage(new TextMessage("LOGIN local-dev-key 1700000000000"));
    String loginReply = replies.poll(5, TimeUnit.SECONDS);

    assertTrue(loginReply.contains("type=LOGIN_OK"));
    assertTrue(loginReply.contains("playerId=1000001"));
    assertTrue(loginReply.contains("regionId=1001"));

    session.sendMessage(new TextMessage("ENTER_REGION 1000001 1001"));
    String regionReply = replies.poll(5, TimeUnit.SECONDS);

    assertTrue(regionReply.contains("type=REGION_SNAPSHOT"));
    assertTrue(regionReply.contains("regionId=1001"));
    assertTrue(regionReply.contains("self=1000001"));
    assertTrue(regionReply.contains("entities=2"));

    session.sendMessage(new TextMessage("SKILL 1000001 2001 1 0"));
    String skillReply = replies.poll(5, TimeUnit.SECONDS);
    String damageReply = replies.poll(5, TimeUnit.SECONDS);
    session.close();

    assertTrue(skillReply.contains("type=SKILL_EVENT"));
    assertTrue(skillReply.contains("casterId=1000001"));
    assertTrue(skillReply.contains("skillId=2001"));
    assertTrue(skillReply.contains("aimX=1"));
    assertTrue(skillReply.contains("aimY=0"));
    assertTrue(damageReply.contains("type=DAMAGE_EVENT"));
    assertTrue(damageReply.contains("sourceId=1000001"));
    assertTrue(damageReply.contains("targetId=2000001"));
    assertTrue(damageReply.contains("hpDelta=-12"));
  }
}
