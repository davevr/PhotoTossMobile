import 'photo_record.dart';

class TossRecord {
  const TossRecord({
    required this.id,
    required this.ownerId,
    required this.imageId,
    required this.gameType,
    required this.catchCount,
    required this.shareTime,
    required this.shareLongitude,
    required this.shareLatitude,
    required this.catchList,
  });

  final int id;
  final int ownerId;
  final int imageId;
  final int gameType;
  final int catchCount;
  final DateTime shareTime;
  final double shareLongitude;
  final double shareLatitude;
  final List<PhotoRecord> catchList;

  factory TossRecord.fromJson(Map<String, dynamic> json) {
    return TossRecord(
      id: _asInt(json['id']),
      ownerId: _asInt(json['ownerId']),
      imageId: _asInt(json['imageId']),
      gameType: _asInt(json['gameType']),
      catchCount: _asInt(json['catchCount']),
      shareTime: _asDateTime(json['shareTime']),
      shareLongitude: _asDouble(json['shareLong']),
      shareLatitude: _asDouble(json['shareLat']),
      catchList: (json['catchList'] as List<dynamic>? ?? const [])
          .map((value) =>
              PhotoRecord.fromJson(value as Map<String, dynamic>))
          .toList(),
    );
  }
}

int _asInt(dynamic value) {
  if (value == null) return 0;
  if (value is int) return value;
  if (value is double) return value.toInt();
  return int.tryParse(value.toString()) ?? 0;
}

double _asDouble(dynamic value) {
  if (value == null) return 0;
  if (value is double) return value;
  if (value is int) return value.toDouble();
  return double.tryParse(value.toString()) ?? 0;
}

DateTime _asDateTime(dynamic value) {
  if (value == null) {
    return DateTime.fromMillisecondsSinceEpoch(0);
  }
  if (value is DateTime) return value;
  return DateTime.tryParse(value.toString()) ??
      DateTime.fromMillisecondsSinceEpoch(0);
}
