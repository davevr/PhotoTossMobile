import 'json_utils.dart';

class PhotoRecord {
  const PhotoRecord({
    this.id,
    required this.ownerName,
    this.ownerId,
    required this.caption,
    required this.tags,
    required this.created,
    required this.createdLat,
    required this.createdLong,
    required this.imageUrl,
    required this.thumbnailUrl,
    this.originId,
    this.parentId,
    required this.catchUrl,
    required this.receivedLong,
    required this.receivedLat,
    required this.receivedCaption,
    required this.received,
    this.tosserId,
    required this.tosserName,
    this.tossId,
    required this.barcodeLocation,
    required this.totalShares,
    required this.tossCount,
    required this.lastShared,
  });

  final int? id;
  final String ownerName;
  final int? ownerId;
  final String caption;
  final List<String> tags;
  final DateTime created;
  final double createdLat;
  final double createdLong;
  final String imageUrl;
  final String thumbnailUrl;
  final int? originId;
  final int? parentId;
  final String catchUrl;
  final double receivedLong;
  final double receivedLat;
  final String receivedCaption;
  final DateTime received;
  final int? tosserId;
  final String tosserName;
  final int? tossId;
  final BarcodeLocation? barcodeLocation;
  final int totalShares;
  final int tossCount;
  final DateTime lastShared;

  factory PhotoRecord.fromJson(Map<String, dynamic> json) {
    return PhotoRecord(
      id: asInt(json['id']),
      ownerName: json['ownername'] as String? ?? '',
      ownerId: asInt(json['ownerid']),
      caption: json['caption'] as String? ?? '',
      tags: (json['tags'] as List<dynamic>? ?? const [])
          .map((value) => value.toString())
          .toList(),
      created: asDateTime(json['created']),
      createdLat: asDouble(json['createdlat']),
      createdLong: asDouble(json['createdlong']),
      imageUrl: json['imageUrl'] as String? ?? '',
      thumbnailUrl: json['thumbnailurl'] as String? ?? '',
      originId: asInt(json['originid']),
      parentId: asInt(json['parentid']),
      catchUrl: json['catchUrl'] as String? ?? '',
      receivedLong: asDouble(json['receivedlong']),
      receivedLat: asDouble(json['receivedlat']),
      receivedCaption: json['receivedcaption'] as String? ?? '',
      received: asDateTime(json['received']),
      tosserId: asInt(json['tosserid']),
      tosserName: json['tossername'] as String? ?? '',
      tossId: asInt(json['tossid']),
      barcodeLocation: (json['barcodelocation'] as Map<String, dynamic>?)
          ?.let(BarcodeLocation.fromJson),
      totalShares: asInt(json['totalshares']) ?? 0,
      tossCount: asInt(json['tossCount']) ?? 0,
      lastShared: asDateTime(json['lastshared']),
    );
  }

  Map<String, dynamic> toJson() => {
        if (id != null) 'id': id,
        'ownername': ownerName,
        if (ownerId != null) 'ownerid': ownerId,
        'caption': caption,
        'tags': tags,
        'created': created.toIso8601String(),
        'createdlat': createdLat,
        'createdlong': createdLong,
        'imageUrl': imageUrl,
        'thumbnailurl': thumbnailUrl,
        if (originId != null) 'originid': originId,
        if (parentId != null) 'parentid': parentId,
        'catchUrl': catchUrl,
        'receivedlong': receivedLong,
        'receivedlat': receivedLat,
        'receivedcaption': receivedCaption,
        'received': received.toIso8601String(),
        if (tosserId != null) 'tosserid': tosserId,
        'tossername': tosserName,
        if (tossId != null) 'tossid': tossId,
        'barcodelocation': barcodeLocation?.toJson(),
        'totalshares': totalShares,
        'tossCount': tossCount,
        'lastshared': lastShared.toIso8601String(),
      };
}

class BarcodeLocation {
  const BarcodeLocation({
    required this.topLeft,
    required this.topRight,
    required this.bottomLeft,
    required this.bottomRight,
  });

  factory BarcodeLocation.fromJson(Map<String, dynamic> json) {
    return BarcodeLocation(
      topLeft: BarcodePoint.fromJson(json['topleft'] as Map<String, dynamic>),
      topRight: BarcodePoint.fromJson(json['topright'] as Map<String, dynamic>),
      bottomLeft:
          BarcodePoint.fromJson(json['bottomleft'] as Map<String, dynamic>),
      bottomRight:
          BarcodePoint.fromJson(json['bottomright'] as Map<String, dynamic>),
    );
  }

  final BarcodePoint topLeft;
  final BarcodePoint topRight;
  final BarcodePoint bottomLeft;
  final BarcodePoint bottomRight;

  Map<String, dynamic> toJson() => {
        'topleft': topLeft.toJson(),
        'topright': topRight.toJson(),
        'bottomleft': bottomLeft.toJson(),
        'bottomright': bottomRight.toJson(),
      };
}

class BarcodePoint {
  const BarcodePoint({required this.x, required this.y});

  factory BarcodePoint.fromJson(Map<String, dynamic> json) => BarcodePoint(
        x: asDouble(json['x']),
        y: asDouble(json['y']),
      );

  final double x;
  final double y;

  Map<String, dynamic> toJson() => {'x': x, 'y': y};
}

extension _NullableMapExtension on Map<String, dynamic> {
  T let<T>(T Function(Map<String, dynamic>) converter) => converter(this);
}
