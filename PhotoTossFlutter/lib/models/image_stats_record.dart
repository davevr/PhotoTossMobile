class ImageStatsRecord {
  const ImageStatsRecord({
    required this.imageId,
    required this.copyCount,
    required this.tossCount,
    required this.parentCount,
    required this.childCount,
  });

  factory ImageStatsRecord.fromJson(Map<String, dynamic> json) {
    return ImageStatsRecord(
      imageId: _asInt(json['imageid']),
      copyCount: _asInt(json['numcopies']),
      tossCount: _asInt(json['numtosses']),
      parentCount: _asInt(json['numparents']),
      childCount: _asInt(json['numchildren']),
    );
  }

  final int imageId;
  final int copyCount;
  final int tossCount;
  final int parentCount;
  final int childCount;
}

int _asInt(dynamic value) {
  if (value == null) return 0;
  if (value is int) return value;
  if (value is double) return value.toInt();
  return int.tryParse(value.toString()) ?? 0;
}
