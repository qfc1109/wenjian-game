package com.wenjian.config.model;

public record RegionConfig(
    int regionId,
    int mapId,
    String regionType,
    boolean pkEnabled,
    int spawnX,
    int spawnY) {
}
