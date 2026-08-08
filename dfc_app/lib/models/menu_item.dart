import '../config/api_config.dart';

/// Mirrors the backend's PublicMenuItemDto — a real menu item has only an
/// id, name, price, optional image and description. No sizes/add-ons/
/// ratings exist server-side, so this model doesn't fabricate any.
class MenuItem {
  final int id;
  final String name;
  final num price;
  final String? imageUrl;
  final String? description;
  final String categoryName;

  const MenuItem({
    required this.id,
    required this.name,
    required this.price,
    this.imageUrl,
    this.description,
    required this.categoryName,
  });

  factory MenuItem.fromJson(Map<String, dynamic> json, {required String categoryName}) {
    return MenuItem(
      id: json['id'] as int,
      name: json['name'] as String? ?? '',
      price: json['price'] as num? ?? 0,
      imageUrl: json['imageUrl'] as String?,
      description: json['description'] as String?,
      categoryName: categoryName,
    );
  }

  String? get networkImageUrl => ApiConfig.imageUrl(imageUrl);
}

class MenuCategory {
  final int id;
  final String name;
  final List<MenuItem> items;

  const MenuCategory({required this.id, required this.name, required this.items});

  factory MenuCategory.fromJson(Map<String, dynamic> json) {
    final name = json['name'] as String? ?? '';
    final itemsJson = (json['items'] as List<dynamic>? ?? []);
    return MenuCategory(
      id: json['id'] as int,
      name: name,
      items: itemsJson
          .map((e) => MenuItem.fromJson(e as Map<String, dynamic>, categoryName: name))
          .toList(),
    );
  }
}
