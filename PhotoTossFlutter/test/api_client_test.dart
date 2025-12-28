import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:phototoss_flutter/models/photo_record.dart';
import 'package:phototoss_flutter/models/toss_record.dart';
import 'package:phototoss_flutter/services/api_client.dart';

void main() {
  test('ApiClient builds lineage request with query parameters', () async {
    var callCount = 0;
    final mockClient = MockClient((request) async {
      callCount++;
      if (callCount == 1) {
        expect(
          request.url.toString(),
          'https://phototoss-server-01.appspot.com/api/image/lineage?imageid=12',
        );
        return http.Response(jsonEncode([
          _photoPayload(id: 12),
        ]), 200, headers: {'set-cookie': 'session=abc'});
      }

      final fields = Uri.splitQueryString(request.body);
      expect(request.headers['Cookie'], contains('session=abc'));
      expect(request.url.toString(),
          'https://phototoss-server-01.appspot.com/api/toss');
      expect(fields['image'], '42');
      expect(fields['game'], '1');
      expect(request.method, 'POST');

      return http.Response(jsonEncode(_tossPayload()), 200);
    });

    final client = ApiClient(httpClient: mockClient);
    final lineage = await client.getImageLineage(12);
    expect(lineage, isA<List<PhotoRecord>>());
    expect(lineage.single.id, 12);

    final toss = await client.startToss(42, 1, -122.3, 47.6);
    expect(toss, isA<TossRecord>());
    expect(toss.imageId, 42);
  });
}

Map<String, dynamic> _photoPayload({required int id}) {
  return {
    'id': id,
    'ownername': 'owner',
    'ownerid': 1,
    'caption': '',
    'tags': [],
    'created': '2024-01-01T00:00:00Z',
    'createdlat': 0,
    'createdlong': 0,
    'imageUrl': '',
    'thumbnailurl': '',
    'originid': 0,
    'parentid': 0,
    'catchUrl': '',
    'receivedlong': 0,
    'receivedlat': 0,
    'receivedcaption': '',
    'received': '2024-01-01T00:00:00Z',
    'tosserid': 0,
    'tossername': '',
    'tossid': 0,
    'barcodelocation': null,
    'totalshares': 0,
    'tossCount': 0,
    'lastshared': '2024-01-01T00:00:00Z',
  };
}

Map<String, dynamic> _tossPayload() {
  return {
    'id': 9,
    'ownerId': 2,
    'imageId': 42,
    'gameType': 1,
    'catchCount': 0,
    'shareTime': '2024-01-01T00:00:00Z',
    'shareLong': 0,
    'shareLat': 0,
    'catchList': [],
  };
}
