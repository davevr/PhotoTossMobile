class PhotoRecord {
  final String id;
  final String owner;
  final String imageUrl;
  final DateTime createdAt;
  final int catchCount;

  const PhotoRecord({
    required this.id,
    required this.owner,
    required this.imageUrl,
    required this.createdAt,
    required this.catchCount,
  });

  factory PhotoRecord.fromJson(Map<String, dynamic> json) {
    return PhotoRecord(
      id: json['id'] as String,
      owner: json['owner'] as String? ?? '',
      imageUrl: json['imageUrl'] as String? ?? '',
      createdAt: DateTime.tryParse(json['createdAt'] as String? ?? '') ??
          DateTime.fromMillisecondsSinceEpoch(0),
      catchCount: json['catchCount'] as int? ?? 0,
    );
  }
}
