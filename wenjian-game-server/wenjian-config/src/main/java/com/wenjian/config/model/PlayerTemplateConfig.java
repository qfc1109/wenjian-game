package com.wenjian.config.model;

public record PlayerTemplateConfig(
    int templateId,
    int initRegionId,
    int spawnX,
    int spawnY,
    int maxHp,
    int moveSpeed,
    int defaultSkillId) {
}
