import '../config/api_config.dart';

class DealItem {
  final int menuItemId;
  final String name;
  final int quantity;

  const DealItem({required this.menuItemId, required this.name, required this.quantity});

  factory DealItem.fromJson(Map<String, dynamic> json) => DealItem(
        menuItemId: json['menuItemId'] as int,
        name: json['name'] as String? ?? '',
        quantity: json['quantity'] as int? ?? 1,
      );
}

/// Mirrors the backend's PublicDealDto.
class Deal {
  final int id;
  final String dealName;
  final num price;
  final String? imageUrl;
  final List<DealItem> items;

  const Deal({
    required this.id,
    required this.dealName,
    required this.price,
    this.imageUrl,
    this.items = const [],
  });

  factory Deal.fromJson(Map<String, dynamic> json) => Deal(
        id: json['id'] as int,
        dealName: json['dealName'] as String? ?? '',
        price: json['price'] as num? ?? 0,
        imageUrl: json['imageUrl'] as String?,
        items: (json['items'] as List<dynamic>? ?? [])
            .map((e) => DealItem.fromJson(e as Map<String, dynamic>))
            .toList(),
      );

  String? get networkImageUrl => ApiConfig.imageUrl(imageUrl);

  String get itemsSummary => items.map((i) => '${i.quantity}x ${i.name}').join(', ');
}
