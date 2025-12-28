import 'package:flutter_test/flutter_test.dart';

import 'package:phototoss_flutter/models/photo_record.dart';
import 'package:phototoss_flutter/models/toss_record.dart';

void main() {
  test('PhotoRecord deserializes full payload', () {
    final record = PhotoRecord.fromJson({
      'id': 42,
      'ownername': 'alice',
      'ownerid': 7,
      'caption': 'Sunset',
      'tags': ['sky', 'travel'],
      'created': '2024-02-01T12:00:00Z',
      'createdlat': 12.34,
      'createdlong': 56.78,
      'imageUrl': 'http://example.com/image.jpg',
      'thumbnailurl': 'http://example.com/thumb.jpg',
      'originid': 1,
      'parentid': 2,
      'catchUrl': 'http://catch.example.com',
      'receivedlong': 90.1,
      'receivedlat': 80.2,
      'receivedcaption': 'caught',
      'received': '2024-02-02T12:00:00Z',
      'tosserid': 99,
      'tossername': 'bob',
      'tossid': 55,
      'barcodelocation': {
        'topleft': {'x': 0.0, 'y': 0.0},
        'topright': {'x': 1.0, 'y': 0.0},
        'bottomleft': {'x': 0.0, 'y': 1.0},
        'bottomright': {'x': 1.0, 'y': 1.0},
      },
      'totalshares': 10,
      'tossCount': 3,
      'lastshared': '2024-02-03T12:00:00Z',
    });

    expect(record.id, 42);
    expect(record.ownerName, 'alice');
    expect(record.ownerId, 7);
    expect(record.tags, ['sky', 'travel']);
    expect(record.created.toUtc().year, 2024);
    expect(record.thumbnailUrl, contains('thumb.jpg'));
    expect(record.barcodeLocation?.topRight.x, 1.0);
    expect(record.tossCount, 3);
    expect(record.totalShares, 10);
    expect(record.lastShared.isAfter(record.created), isTrue);
  });

  test('TossRecord deserializes catch list and coordinates', () {
    final toss = TossRecord.fromJson({
      'id': 5,
      'ownerId': 8,
      'imageId': 42,
      'gameType': 1,
      'catchCount': 2,
      'shareTime': '2024-01-01T00:00:00Z',
      'shareLong': -122.1,
      'shareLat': 47.6,
      'catchList': [
        {
          'id': 99,
          'ownername': 'catcher',
          'ownerid': 77,
          'caption': 'fun',
          'tags': [],
          'created': '2024-01-01T00:00:00Z',
          'createdlat': 0.0,
          'createdlong': 0.0,
          'imageUrl': '',
          'thumbnailurl': '',
          'originid': 0,
          'parentid': 0,
          'catchUrl': '',
          'receivedlong': 0.0,
          'receivedlat': 0.0,
          'receivedcaption': '',
          'received': '2024-01-01T00:00:00Z',
          'tosserid': 0,
          'tossername': '',
          'tossid': 0,
          'barcodelocation': null,
          'totalshares': 0,
          'tossCount': 0,
          'lastshared': '2024-01-01T00:00:00Z',
        }
      ],
    });

    expect(toss.id, 5);
    expect(toss.ownerId, 8);
    expect(toss.imageId, 42);
    expect(toss.catchCount, 2);
    expect(toss.shareLongitude, closeTo(-122.1, 0.001));
    expect(toss.catchList, hasLength(1));
    expect(toss.catchList.first.ownerName, 'catcher');
  });
}
