class TossRecord {
  final String id;
  final String photoId;
  final String catcher;
  final DateTime caughtAt;

  const TossRecord({
    required this.id,
    required this.photoId,
    required this.catcher,
    required this.caughtAt,
  });

  factory TossRecord.fromJson(Map<String, dynamic> json) {
    return TossRecord(
      id: json['id'] as String,
      photoId: json['photoId'] as String? ?? '',
      catcher: json['catcher'] as String? ?? '',
      caughtAt: DateTime.tryParse(json['caughtAt'] as String? ?? '') ??
          DateTime.fromMillisecondsSinceEpoch(0),
    );
  }
}
