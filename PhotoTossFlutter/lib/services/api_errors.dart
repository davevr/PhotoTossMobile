import 'dart:convert';

import 'package:http/http.dart' as http;

class ApiException implements Exception {
  ApiException(this.message);
  final String message;

  @override
  String toString() => message;
}

class HttpRequestException extends ApiException {
  HttpRequestException(this.statusCode, this.body)
      : super('Request failed with status $statusCode');

  final int statusCode;
  final String body;
}

class DeserializationException extends ApiException {
  DeserializationException(String message) : super(message);
}

class ApiErrors {
  static void throwOnFailure(http.Response response) {
    if (response.statusCode >= 400) {
      throw HttpRequestException(response.statusCode, response.body);
    }
  }

  static T decodeObject<T>(String payload, T Function(Map<String, dynamic>) map) {
    try {
      final jsonBody = jsonDecode(payload) as Map<String, dynamic>;
      return map(jsonBody);
    } catch (error) {
      throw DeserializationException('Unable to parse response: $error');
    }
  }

  static List<T> decodeList<T>(
    String payload,
    T Function(Map<String, dynamic>) map,
  ) {
    try {
      final jsonBody = jsonDecode(payload) as List<dynamic>;
      return jsonBody
          .map((item) => map(item as Map<String, dynamic>))
          .toList();
    } catch (error) {
      throw DeserializationException('Unable to parse response: $error');
    }
  }
}
