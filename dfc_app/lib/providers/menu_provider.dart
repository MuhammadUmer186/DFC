import 'package:flutter/foundation.dart';
import '../models/deal.dart';
import '../models/menu_item.dart';
import '../services/api_client.dart';
import '../services/menu_service.dart';

enum MenuLoadState { loading, loaded, error }

/// Loads the live menu from GET /api/Public/menu once and shares it across
/// Home/Menu screens, replacing the old hardcoded kMenu/kCategories.
class MenuProvider extends ChangeNotifier {
  MenuLoadState state = MenuLoadState.loading;
  String? errorMessage;
  List<MenuCategory> categories = [];
  List<Deal> deals = [];

  MenuProvider() {
    load();
  }

  List<MenuItem> get allItems => categories.expand((c) => c.items).toList();

  MenuItem? itemById(int id) {
    for (final item in allItems) {
      if (item.id == id) return item;
    }
    return null;
  }

  Future<void> load() async {
    state = MenuLoadState.loading;
    errorMessage = null;
    notifyListeners();
    try {
      final menu = await MenuService.getMenu();
      categories = menu.categories;
      deals = menu.deals;
      state = MenuLoadState.loaded;
    } on ApiException catch (e) {
      errorMessage = e.message;
      state = MenuLoadState.error;
    } catch (_) {
      errorMessage = 'Could not load the menu. Pull down to try again.';
      state = MenuLoadState.error;
    }
    notifyListeners();
  }
}
