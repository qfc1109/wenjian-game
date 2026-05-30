package com.wenjian.config.model;

public record MapConfig(
    int mapId,
    int width,
    int height,
    String collisionRef) {
}
