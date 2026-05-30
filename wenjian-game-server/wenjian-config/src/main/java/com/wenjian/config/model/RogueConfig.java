package com.wenjian.config.model;

public record RogueConfig(
    int rogueId,
    int mapId,
    int spawnX,
    int spawnY,
    int monsterId,
    int monsterCount,
    int rewardPoolId) {
}
