import 'dart:convert';
import 'package:shared_preferences/shared_preferences.dart';

/// Backend has no "list my orders" endpoint for guests (only id+phone
/// lookup, by design). This locally remembers every order this device has
/// placed so the Orders tab has something to show without an account.
class OrderHistoryEntry {
  final int orderId;
  final String phone;
  final DateTime placedAt;
  const OrderHistoryEntry({required this.orderId, required this.phone, required this.placedAt});

  Map<String, dynamic> toJson() => {
        'orderId': orderId,
        'phone': phone,
        'placedAt': placedAt.toIso8601String(),
      };

  factory OrderHistoryEntry.fromJson(Map<String, dynamic> json) => OrderHistoryEntry(
        orderId: json['orderId'] as int,
        phone: json['phone'] as String,
        placedAt: DateTime.tryParse(json['placedAt'] as String? ?? '') ?? DateTime.now(),
      );
}

class OrderHistoryService {
  static const _key = 'dfc_order_history';

  static Future<List<OrderHistoryEntry>> getAll() async {
    final prefs = await SharedPreferences.getInstance();
    final raw = prefs.getString(_key);
    if (raw == null || raw.isEmpty) return [];
    try {
      final list = jsonDecode(raw) as List<dynamic>;
      final entries = list.map((e) => OrderHistoryEntry.fromJson(e as Map<String, dynamic>)).toList();
      entries.sort((a, b) => b.placedAt.compareTo(a.placedAt));
      return entries;
    } catch (_) {
      return [];
    }
  }

  static Future<void> add(int orderId, String phone) async {
    final prefs = await SharedPreferences.getInstance();
    final existing = await getAll();
    existing.removeWhere((e) => e.orderId == orderId);
    existing.insert(0, OrderHistoryEntry(orderId: orderId, phone: phone, placedAt: DateTime.now()));
    await prefs.setString(_key, jsonEncode(existing.map((e) => e.toJson()).toList()));
  }

  /// Last-used phone number, so checkout/track screens can pre-fill it.
  static Future<String?> lastPhone() async {
    final entries = await getAll();
    return entries.isEmpty ? null : entries.first.phone;
  }
}
