import '../models/deal.dart';
import '../models/menu_item.dart';
import 'api_client.dart';

class PublicMenu {
  final List<MenuCategory> categories;
  final List<Deal> deals;
  const PublicMenu({required this.categories, required this.deals});

  List<MenuItem> get allItems => categories.expand((c) => c.items).toList();
}

class MenuService {
  static Future<PublicMenu> getMenu() async {
    final json = await ApiClient.get('/Public/menu') as Map<String, dynamic>;
    final categories = (json['categories'] as List<dynamic>? ?? [])
        .map((e) => MenuCategory.fromJson(e as Map<String, dynamic>))
        .toList();
    final deals = (json['deals'] as List<dynamic>? ?? [])
        .map((e) => Deal.fromJson(e as Map<String, dynamic>))
        .toList();
    return PublicMenu(categories: categories, deals: deals);
  }
}
