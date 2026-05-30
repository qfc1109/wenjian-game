package com.wenjian.config;

/** Exception thrown when a game configuration is invalid or cannot be loaded. */
public class ConfigException extends RuntimeException {

  private final String fileName;
  private final String fieldName;
  private final String rawValue;

  public ConfigException(String message) {
    super(message);
    this.fileName = null;
    this.fieldName = null;
    this.rawValue = null;
  }

  public ConfigException(String message, Throwable cause) {
    super(message, cause);
    this.fileName = null;
    this.fieldName = null;
    this.rawValue = null;
  }

  public ConfigException(String message, String fileName, String fieldName, String rawValue) {
    super(formatMessage(message, fileName, fieldName, rawValue));
    this.fileName = fileName;
    this.fieldName = fieldName;
    this.rawValue = rawValue;
  }

  private static String formatMessage(String message, String fileName, String fieldName, String rawValue) {
    StringBuilder sb = new StringBuilder();
    sb.append(message);
    if (fileName != null) {
      sb.append(" (file=").append(fileName);
      if (fieldName != null) {
        sb.append(", field=").append(fieldName);
        if (rawValue != null) {
          sb.append(", value='").append(rawValue).append("'");
        }
      }
      sb.append(")");
    }
    return sb.toString();
  }

  public String fileName() {
    return fileName;
  }

  public String fieldName() {
    return fieldName;
  }

  public String rawValue() {
    return rawValue;
  }
}
