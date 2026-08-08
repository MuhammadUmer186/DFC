import 'dart:convert';
import 'package:http/http.dart' as http;
import '../config/api_config.dart';

class ApiException implements Exception {
  final String message;
  ApiException(this.message);
  @override
  String toString() => message;
}

/// Thin JSON wrapper around http — decodes bodies and turns non-2xx
/// responses into an ApiException carrying the backend's actual error text
/// (PublicController returns BadRequest(ex.Message), a plain JSON string).
class ApiClient {
  static Future<dynamic> get(String path, {Map<String, String>? query}) async {
    final uri = Uri.parse('${ApiConfig.baseUrl}$path').replace(queryParameters: query);
    try {
      final res = await http.get(uri).timeout(const Duration(seconds: 15));
      return _handle(res);
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException('Could not reach the server. Check your connection and try again.');
    }
  }

  static Future<dynamic> post(String path, Map<String, dynamic> body) async {
    final uri = Uri.parse('${ApiConfig.baseUrl}$path');
    try {
      final res = await http
          .post(uri, headers: {'Content-Type': 'application/json'}, body: jsonEncode(body))
          .timeout(const Duration(seconds: 15));
      return _handle(res);
    } on ApiException {
      rethrow;
    } catch (e) {
      throw ApiException('Could not reach the server. Check your connection and try again.');
    }
  }

  static dynamic _handle(http.Response res) {
    if (res.statusCode >= 200 && res.statusCode < 300) {
      if (res.body.isEmpty) return null;
      return jsonDecode(res.body);
    }
    String message = 'Something went wrong (${res.statusCode}).';
    if (res.body.isNotEmpty) {
      try {
        final decoded = jsonDecode(res.body);
        if (decoded is String) {
          message = decoded;
        } else if (decoded is Map && decoded['message'] != null) {
          message = decoded['message'].toString();
        } else if (decoded is Map && decoded['title'] != null) {
          message = decoded['title'].toString();
        }
      } catch (_) {
        message = res.body;
      }
    }
    throw ApiException(message);
  }
}
