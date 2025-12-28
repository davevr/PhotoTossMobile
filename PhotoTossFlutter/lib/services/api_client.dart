import 'package:http/http.dart' as http;
import 'package:http_parser/http_parser.dart';

import '../models/image_stats_record.dart';
import '../models/photo_record.dart';
import '../models/toss_record.dart';
import '../models/user_stats_record.dart';
import 'api_errors.dart';

class ApiClient {
  ApiClient({http.Client? httpClient, Uri? baseUri})
      : _http = httpClient ?? http.Client(),
        _baseUri = baseUri ??
            Uri.parse('http://phototoss-server-01.appspot.com/api/');

  static final ApiClient instance = ApiClient();

  final http.Client _http;
  final Uri _baseUri;
  String? _cookieHeader;

  Uri _resolve(String path, [Map<String, String>? query]) {
    final uri = _baseUri.resolve(path);
    return query == null ? uri : uri.replace(queryParameters: query);
  }

  Map<String, String> _buildHeaders([Map<String, String>? extra]) {
    final headers = <String, String>{
      'Accept': 'application/json',
      ...?extra,
    };
    if (_cookieHeader != null) {
      headers['Cookie'] = _cookieHeader!;
    }
    return headers;
  }

  void _updateCookies(http.Response response) {
    final setCookie = response.headers['set-cookie'];
    if (setCookie != null && setCookie.isNotEmpty) {
      _cookieHeader = setCookie;
    }
  }

  Future<List<PhotoRecord>> getUserImages() async {
    final response = await _http.get(_resolve('images'), headers: _buildHeaders());
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
    return ApiErrors.decodeList(response.body, PhotoRecord.fromJson)
      ..sort((a, b) => b.created.compareTo(a.created));
  }

  Future<List<PhotoRecord>> getImageLineage(int imageId) async {
    final response = await _http.get(
      _resolve('image/lineage', {'imageid': '$imageId'}),
      headers: _buildHeaders(),
    );
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
    return ApiErrors.decodeList(response.body, PhotoRecord.fromJson);
  }

  Future<List<TossRecord>> getImageTosses(int imageId) async {
    final response = await _http.get(
      _resolve('image/tosses', {'imageid': '$imageId'}),
      headers: _buildHeaders(),
    );
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
    return ApiErrors.decodeList(response.body, TossRecord.fromJson);
  }

  Future<void> removeImage(int imageId, {bool removeTosses = false}) async {
    final response = await _http.delete(
      _resolve('image', {
        'id': '$imageId',
        'all': '$removeTosses',
      }),
      headers: _buildHeaders(),
    );
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
  }

  Future<List<PhotoRecord>> getTossCatches(int tossId) async {
    final response = await _http.get(
      _resolve('toss/catches', {'tossid': '$tossId'}),
      headers: _buildHeaders(),
    );
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
    return ApiErrors.decodeList(response.body, PhotoRecord.fromJson);
  }

  Future<void> logout() async {
    final response = await _http.post(
      _resolve('user/logout'),
      headers: _buildHeaders(),
    );
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
    _cookieHeader = null;
  }

  Future<String> getUploadUrl() async {
    final response = await _http.get(_resolve('image/upload'), headers: _buildHeaders());
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
    return response.body;
  }

  Future<String> getUserImageUploadUrl() async {
    final response = await _http.get(_resolve('user/image'), headers: _buildHeaders());
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
    return response.body;
  }

  Future<String> getCatchUrl() async {
    final response = await _http.get(_resolve('catch'), headers: _buildHeaders());
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
    return response.body;
  }

  Future<PhotoRecord> getImageById(int imageId) async {
    final response = await _http.get(
      _resolve('image', {'id': '$imageId'}),
      headers: _buildHeaders(),
    );
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
    return ApiErrors.decodeObject(response.body, PhotoRecord.fromJson);
  }

  Future<PhotoRecord> setImageCaption(int imageId, String caption) async {
    final response = await _http.put(
      _resolve('image', {'id': '$imageId', 'caption': caption}),
      headers: _buildHeaders({'Content-Type': 'application/x-www-form-urlencoded'}),
    );
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
    return ApiErrors.decodeObject(response.body, PhotoRecord.fromJson);
  }

  Future<TossRecord> startToss(
    int imageId,
    int gameType,
    double longitude,
    double latitude,
  ) async {
    final response = await _http.post(
      _resolve('toss'),
      headers: _buildHeaders({'Content-Type': 'application/x-www-form-urlencoded'}),
      body: {
        'image': '$imageId',
        'game': '$gameType',
        'long': '$longitude',
        'lat': '$latitude',
      },
    );
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
    return ApiErrors.decodeObject(response.body, TossRecord.fromJson);
  }

  Future<List<PhotoRecord>> getTossStatus(int tossId) async {
    final response = await _http.get(
      _resolve('toss/status', {'toss': '$tossId'}),
      headers: _buildHeaders(),
    );
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
    return ApiErrors.decodeList(response.body, PhotoRecord.fromJson);
  }

  Future<ImageStatsRecord> getImageStats(int imageId) async {
    final response = await _http.get(
      _resolve('image/stats', {'id': '$imageId'}),
      headers: _buildHeaders(),
    );
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
    return ApiErrors.decodeObject(response.body, ImageStatsRecord.fromJson);
  }

  Future<List<PhotoRecord>> getGlobalStats() async {
    final response = await _http.get(_resolve('stats'), headers: _buildHeaders());
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
    return ApiErrors.decodeList(response.body, PhotoRecord.fromJson);
  }

  Future<UserStatsRecord> getUserStats() async {
    final response = await _http.get(_resolve('user/stats'), headers: _buildHeaders());
    _updateCookies(response);
    ApiErrors.throwOnFailure(response);
    return ApiErrors.decodeObject(response.body, UserStatsRecord.fromJson);
  }

  Future<http.StreamedResponse> uploadImage(
    Uri uploadUrl,
    List<int> fileBytes,
    double longitude,
    double latitude,
  ) async {
    final request = http.MultipartRequest('POST', uploadUrl)
      ..headers.addAll(_buildHeaders())
      ..fields.addAll({
        'long': '$longitude',
        'lat': '$latitude',
      })
      ..files.add(http.MultipartFile.fromBytes(
        'file',
        fileBytes,
        filename: 'file',
        contentType: MediaType.parse('image/jpeg'),
      ));

    return _http.send(request);
  }

  Future<http.StreamedResponse> uploadImageThumbnail(
    Uri uploadUrl,
    List<int> fileBytes,
    int imageId,
  ) async {
    final request = http.MultipartRequest('POST', uploadUrl)
      ..headers.addAll(_buildHeaders())
      ..fields.addAll({
        'thumbnail': 'true',
        'imageid': '$imageId',
      })
      ..files.add(http.MultipartFile.fromBytes(
        'file',
        fileBytes,
        filename: 'file',
        contentType: MediaType.parse('image/jpeg'),
      ));

    return _http.send(request);
  }

  Future<http.StreamedResponse> catchToss(
    Uri catchUrl,
    List<int> fileBytes,
    int tossId,
    double longitude,
    double latitude,
    {
    Map<String, String>? barcodeLocation,
  }) async {
    final request = http.MultipartRequest('POST', catchUrl)
      ..headers.addAll(_buildHeaders({'Accept': '*/*'}))
      ..fields.addAll({
        'toss': '$tossId',
        'long': '$longitude',
        'lat': '$latitude',
        ...?barcodeLocation,
      })
      ..files.add(http.MultipartFile.fromBytes(
        'file',
        fileBytes,
        filename: 'file',
        contentType: MediaType.parse('image/jpeg'),
      ));

    return _http.send(request);
  }
}
