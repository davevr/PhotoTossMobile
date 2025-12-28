import 'json_utils.dart';
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
      id: asInt(json['id']) ?? 0,
      ownerId: asInt(json['ownerId']) ?? 0,
      imageId: asInt(json['imageId']) ?? 0,
      gameType: asInt(json['gameType']) ?? 0,
      catchCount: asInt(json['catchCount']) ?? 0,
      shareTime: asDateTime(json['shareTime']),
      shareLongitude: asDouble(json['shareLong']),
      shareLatitude: asDouble(json['shareLat']),
      catchList: (json['catchList'] as List<dynamic>? ?? const [])
          .map((value) =>
              PhotoRecord.fromJson(value as Map<String, dynamic>))
          .toList(),
    );
  }
}
