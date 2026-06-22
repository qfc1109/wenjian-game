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

@SpringBootTest(
    webEnvironment = SpringBootTest.WebEnvironment.RANDOM_PORT,
    properties = "wenjian.config.source-dir=../../config/source")
class FirstChainWebSocketIntegrationTest {
  @LocalServerPort
  private int port;

  @Value("${server.servlet.context-path:}")
  private String contextPath;

  @Test
  void websocketLoginEnterRegionSkillAndRogueReturnEvents() throws Exception {
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
    assertTrue(loginReply.contains("x=3200"));
    assertTrue(loginReply.contains("y=2400"));

    session.sendMessage(new TextMessage("ENTER_REGION 1000001 1001"));
    String regionReply = replies.poll(5, TimeUnit.SECONDS);

    assertTrue(regionReply.contains("type=REGION_SNAPSHOT"));
    assertTrue(regionReply.contains("regionId=1001"));
    assertTrue(regionReply.contains("self=1000001"));
    assertTrue(regionReply.contains("entities=2"));

    session.sendMessage(new TextMessage("SKILL 1000001 2001 1 0"));
    String skillReply = replies.poll(5, TimeUnit.SECONDS);
    String damageReply = replies.poll(5, TimeUnit.SECONDS);

    assertTrue(skillReply.contains("type=SKILL_EVENT"));
    assertTrue(skillReply.contains("casterId=1000001"));
    assertTrue(skillReply.contains("skillId=2001"));
    assertTrue(skillReply.contains("x=3200"));
    assertTrue(skillReply.contains("y=2400"));
    assertTrue(skillReply.contains("aimX=1"));
    assertTrue(skillReply.contains("aimY=0"));
    assertTrue(damageReply.contains("type=DAMAGE_EVENT"));
    assertTrue(damageReply.contains("sourceId=1000001"));
    assertTrue(damageReply.contains("targetId=2000001"));
    assertTrue(damageReply.contains("hpDelta=-12"));

    session.sendMessage(new TextMessage("START_ROGUE 1000001 4001"));
    String rogueStartReply = replies.poll(5, TimeUnit.SECONDS);

    assertTrue(rogueStartReply.contains("type=ROGUE_START"));
    assertTrue(rogueStartReply.contains("playerId=1000001"));
    assertTrue(rogueStartReply.contains("rogueId=4001"));
    assertTrue(rogueStartReply.contains("instanceId=9000001"));
    assertTrue(rogueStartReply.contains("mapId=2001"));
    assertTrue(rogueStartReply.contains("x=1500"));
    assertTrue(rogueStartReply.contains("y=1500"));
    assertTrue(rogueStartReply.contains("monsterId=3001"));
    assertTrue(rogueStartReply.contains("monsters=8"));
    assertTrue(rogueStartReply.contains("rewardPoolId=5001"));
    assertTrue(rogueStartReply.contains("entities=9"));

    session.sendMessage(new TextMessage("FINISH_ROGUE 1000001 9000001"));
    String rogueFinishReply = replies.poll(5, TimeUnit.SECONDS);
    session.close();

    assertTrue(rogueFinishReply.contains("type=ROGUE_FINISH"));
    assertTrue(rogueFinishReply.contains("instanceId=9000001"));
    assertTrue(rogueFinishReply.contains("success=true"));
    assertTrue(rogueFinishReply.contains("itemId=6001"));
    assertTrue(rogueFinishReply.contains("count=3"));
  }
}
