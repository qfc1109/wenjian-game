package com.wenjian.config.model;

public record SkillConfig(
    int skillId,
    int cooldownMs,
    int castMs,
    String hitShape,
    int range,
    int radius,
    int damage) {
}
