class UserStatsRecord {
  const UserStatsRecord({
    required this.userId,
    required this.tossCount,
    required this.catchCount,
    required this.imageCount,
    required this.originalCount,
  });

  factory UserStatsRecord.fromJson(Map<String, dynamic> json) {
    return UserStatsRecord(
      userId: _asInt(json['userid']),
      tossCount: _asInt(json['numtosses']),
      catchCount: _asInt(json['numcatches']),
      imageCount: _asInt(json['numimages']),
      originalCount: _asInt(json['numoriginals']),
    );
  }

  final int userId;
  final int tossCount;
  final int catchCount;
  final int imageCount;
  final int originalCount;
}

int _asInt(dynamic value) {
  if (value == null) return 0;
  if (value is int) return value;
  if (value is double) return value.toInt();
  return int.tryParse(value.toString()) ?? 0;
}
