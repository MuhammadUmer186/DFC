import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../models/cart_line.dart';
import '../models/deal.dart';
import '../models/menu_item.dart';

class CartProvider extends ChangeNotifier {
  static const _storageKey = 'dfc_customer_cart';

  final List<CartLine> _lines = [];
  List<CartLine> get lines => List.unmodifiable(_lines);

  int get itemCount => _lines.fold(0, (s, l) => s + l.quantity);
  num get subtotal => _lines.fold<num>(0, (s, l) => s + l.total);

  CartProvider() {
    _load();
  }

  Future<void> _load() async {
    final prefs = await SharedPreferences.getInstance();
    final raw = prefs.getString(_storageKey);
    if (raw == null || raw.isEmpty) return;
    try {
      final list = jsonDecode(raw) as List<dynamic>;
      _lines
        ..clear()
        ..addAll(list.map((e) => CartLine.fromJson(e as Map<String, dynamic>)));
      notifyListeners();
    } catch (_) {
      // Corrupt/old cart data — start fresh rather than crash.
    }
  }

  Future<void> _persist() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_storageKey, jsonEncode(_lines.map((l) => l.toJson()).toList()));
  }

  void addItem(MenuItem item, {int qty = 1}) {
    _addLine(CartLineKind.item, item.id, item.name, item.price, item.imageUrl, qty);
  }

  void addDeal(Deal deal, {int qty = 1}) {
    _addLine(CartLineKind.deal, deal.id, deal.dealName, deal.price, deal.imageUrl, qty);
  }

  void _addLine(CartLineKind kind, int id, String name, num price, String? imageUrl, int qty) {
    final existing = _lines.where((l) => l.kind == kind && l.id == id);
    if (existing.isNotEmpty) {
      existing.first.quantity += qty;
    } else {
      _lines.add(CartLine(kind: kind, id: id, name: name, price: price, imageUrl: imageUrl, quantity: qty));
    }
    notifyListeners();
    _persist();
  }

  void changeQty(CartLine line, int delta) {
    line.quantity += delta;
    if (line.quantity <= 0) {
      _lines.remove(line);
    }
    notifyListeners();
    _persist();
  }

  void remove(CartLine line) {
    _lines.remove(line);
    notifyListeners();
    _persist();
  }

  void clear() {
    _lines.clear();
    notifyListeners();
    _persist();
  }
}
