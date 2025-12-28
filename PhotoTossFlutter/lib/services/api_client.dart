import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/photo_record.dart';
import '../models/toss_record.dart';

class ApiClient {
  ApiClient({http.Client? httpClient}) : _http = httpClient ?? http.Client();

  static final ApiClient instance = ApiClient();

  final http.Client _http;
  final String baseUrl = 'https://api.phototoss.example.com';

  Future<List<PhotoRecord>> fetchRecentPhotos() async {
    final response = await _http.get(Uri.parse('$baseUrl/images'));
    _throwOnError(response);
    final body = jsonDecode(response.body) as List<dynamic>;
    return body
        .map((json) => PhotoRecord.fromJson(json as Map<String, dynamic>))
        .toList();
  }

  Future<List<TossRecord>> fetchTossesForPhoto(String photoId) async {
    final response = await _http.get(Uri.parse('$baseUrl/image/$photoId/tosses'));
    _throwOnError(response);
    final body = jsonDecode(response.body) as List<dynamic>;
    return body
        .map((json) => TossRecord.fromJson(json as Map<String, dynamic>))
        .toList();
  }

  Future<void> deletePhoto(String photoId) async {
    final response = await _http.delete(Uri.parse('$baseUrl/image/$photoId'));
    _throwOnError(response);
  }

  void _throwOnError(http.Response response) {
    if (response.statusCode >= 400) {
      throw ApiException('Request failed (${response.statusCode})');
    }
  }
}

class ApiException implements Exception {
  ApiException(this.message);
  final String message;

  @override
  String toString() => 'ApiException: $message';
}
