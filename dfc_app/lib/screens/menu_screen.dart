import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import '../models/deal.dart';
import '../models/menu_item.dart';
import '../providers/cart_provider.dart';
import '../providers/menu_provider.dart';
import '../theme/app_theme.dart';
import '../widgets/cart_icon.dart';
import '../widgets/dfc_logo.dart';
import 'product_detail_screen.dart';

const _dealsTab = 'Deals';

class MenuScreen extends StatefulWidget {
  const MenuScreen({super.key});
  @override
  State<MenuScreen> createState() => _MenuScreenState();
}

class _MenuScreenState extends State<MenuScreen> {
  String? _selected;
  String _query = '';
  final Set<int> _favs = {};

  @override
  Widget build(BuildContext context) {
    final menu = context.watch<MenuProvider>();
    final tabs = [
      if (menu.deals.isNotEmpty) _dealsTab,
      ...menu.categories.map((c) => c.name),
    ];
    final selected = (_selected != null && tabs.contains(_selected)) ? _selected : tabs.firstOrNull;

    return Scaffold(
      backgroundColor: AppColors.bg,
      body: SafeArea(
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.fromLTRB(18, 10, 18, 0),
              child: Row(
                children: [
                  const SizedBox(width: 28),
                  Expanded(
                    child: Text('Explore Menu',
                        textAlign: TextAlign.center,
                        style: GoogleFonts.inter(fontSize: 20, fontWeight: FontWeight.w800)),
                  ),
                  const CartIcon(color: AppColors.textDark),
                ],
              ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(18, 14, 18, 12),
              child: TextField(
                onChanged: (v) => setState(() => _query = v),
                decoration: const InputDecoration(
                  hintText: 'Search menu items...',
                  prefixIcon: Icon(Icons.search, color: AppColors.textGrey),
                ),
              ),
            ),
            if (tabs.isNotEmpty)
              SizedBox(
                height: 44,
                child: ListView.separated(
                  scrollDirection: Axis.horizontal,
                  padding: const EdgeInsets.symmetric(horizontal: 18),
                  itemCount: tabs.length,
                  separatorBuilder: (_, __) => const SizedBox(width: 10),
                  itemBuilder: (_, i) {
                    final label = tabs[i];
                    final sel = label == selected;
                    return GestureDetector(
                      onTap: () => setState(() => _selected = label),
                      child: Container(
                        padding: const EdgeInsets.symmetric(horizontal: 18),
                        decoration: BoxDecoration(
                          color: sel ? AppColors.yellow : Colors.white,
                          borderRadius: BorderRadius.circular(22),
                          border: Border.all(color: sel ? AppColors.yellow : AppColors.border),
                        ),
                        alignment: Alignment.center,
                        child: Text(label,
                            style: TextStyle(
                                fontSize: 13.5,
                                fontWeight: sel ? FontWeight.w800 : FontWeight.w600,
                                color: AppColors.textDark)),
                      ),
                    );
                  },
                ),
              ),
            const SizedBox(height: 8),
            Expanded(child: _Body(menu: menu, selected: selected, query: _query, favs: _favs, onFavChanged: () => setState(() {}))),
          ],
        ),
      ),
    );
  }
}

extension _FirstOrNull<T> on List<T> {
  T? get firstOrNull => isEmpty ? null : first;
}

class _Body extends StatelessWidget {
  final MenuProvider menu;
  final String? selected;
  final String query;
  final Set<int> favs;
  final VoidCallback onFavChanged;
  const _Body({
    required this.menu,
    required this.selected,
    required this.query,
    required this.favs,
    required this.onFavChanged,
  });

  @override
  Widget build(BuildContext context) {
    if (menu.state == MenuLoadState.loading && menu.categories.isEmpty) {
      return const Center(child: CircularProgressIndicator(color: AppColors.yellow));
    }
    if (menu.state == MenuLoadState.error && menu.categories.isEmpty) {
      return RefreshIndicator(
        onRefresh: menu.load,
        child: ListView(children: [
          const SizedBox(height: 100),
          Center(
            child: Text(menu.errorMessage ?? 'Could not load the menu.',
                textAlign: TextAlign.center, style: const TextStyle(color: AppColors.textGrey)),
          ),
          const SizedBox(height: 12),
          Center(child: OutlinedButton(onPressed: menu.load, child: const Text('Retry'))),
        ]),
      );
    }

    if (selected == _dealsTab) {
      final deals = menu.deals.where((d) => query.isEmpty || d.dealName.toLowerCase().contains(query.toLowerCase())).toList();
      return RefreshIndicator(
        onRefresh: menu.load,
        child: GridView.builder(
          padding: const EdgeInsets.fromLTRB(18, 8, 18, 16),
          gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
            crossAxisCount: 2,
            mainAxisSpacing: 14,
            crossAxisSpacing: 14,
            childAspectRatio: 0.82,
          ),
          itemCount: deals.length,
          itemBuilder: (_, i) => _DealCard(deal: deals[i]),
        ),
      );
    }

    final items = menu.allItems.where((e) {
      final matchCat = selected == null || e.categoryName == selected;
      final matchQ = query.isEmpty || e.name.toLowerCase().contains(query.toLowerCase());
      return matchCat && matchQ;
    }).toList();

    return RefreshIndicator(
      onRefresh: menu.load,
      child: items.isEmpty
          ? ListView(children: const [
              SizedBox(height: 100),
              Center(child: Text('No items found.', style: TextStyle(color: AppColors.textGrey))),
            ])
          : GridView.builder(
              padding: const EdgeInsets.fromLTRB(18, 8, 18, 16),
              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 2,
                mainAxisSpacing: 14,
                crossAxisSpacing: 14,
                childAspectRatio: 0.78,
              ),
              itemCount: items.length,
              itemBuilder: (_, i) => _MenuCard(
                item: items[i],
                isFav: favs.contains(items[i].id),
                onFav: () {
                  favs.contains(items[i].id) ? favs.remove(items[i].id) : favs.add(items[i].id);
                  onFavChanged();
                },
              ),
            ),
    );
  }
}

class _MenuCard extends StatelessWidget {
  final MenuItem item;
  final bool isFav;
  final VoidCallback onFav;
  const _MenuCard({required this.item, required this.isFav, required this.onFav});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: () => Navigator.of(context)
          .push(MaterialPageRoute(builder: (_) => ProductDetailScreen(item: item))),
      child: Container(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
          boxShadow: [
            BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 10, offset: const Offset(0, 4)),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Stack(
              children: [
                FoodImage(
                  imageUrl: item.networkImageUrl,
                  width: double.infinity,
                  height: 120,
                  radius: const BorderRadius.vertical(top: Radius.circular(16)),
                ),
                Positioned(
                  right: 8,
                  top: 8,
                  child: GestureDetector(
                    onTap: onFav,
                    child: Container(
                      padding: const EdgeInsets.all(6),
                      decoration: const BoxDecoration(color: Colors.white, shape: BoxShape.circle),
                      child: Icon(
                        isFav ? Icons.favorite : Icons.favorite_border,
                        size: 18,
                        color: isFav ? AppColors.red : AppColors.textGrey,
                      ),
                    ),
                  ),
                ),
              ],
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(12, 8, 12, 10),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(item.name,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 15)),
                  const SizedBox(height: 4),
                  Text(item.description ?? item.categoryName,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(fontSize: 12, color: AppColors.textGrey)),
                  const SizedBox(height: 6),
                  Row(
                    children: [
                      Text('Rs ${item.price}',
                          style: const TextStyle(fontWeight: FontWeight.w800, fontSize: 15.5)),
                      const Spacer(),
                      GestureDetector(
                        onTap: () {
                          context.read<CartProvider>().addItem(item);
                          ScaffoldMessenger.of(context).showSnackBar(SnackBar(
                            content: Text('${item.name} added to cart'),
                            duration: const Duration(milliseconds: 900),
                          ));
                        },
                        child: Container(
                          width: 30,
                          height: 30,
                          decoration: const BoxDecoration(color: AppColors.yellow, shape: BoxShape.circle),
                          child: const Icon(Icons.add, size: 19, color: AppColors.textDark),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _DealCard extends StatelessWidget {
  final Deal deal;
  const _DealCard({required this.deal});

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 10, offset: const Offset(0, 4)),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          FoodImage(
            imageUrl: deal.networkImageUrl,
            width: double.infinity,
            height: 100,
            radius: const BorderRadius.vertical(top: Radius.circular(16)),
          ),
          Padding(
            padding: const EdgeInsets.fromLTRB(12, 8, 12, 10),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(deal.dealName,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 14.5)),
                const SizedBox(height: 3),
                Text(deal.itemsSummary,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(fontSize: 11.5, color: AppColors.textGrey)),
                const SizedBox(height: 6),
                Row(
                  children: [
                    Text('Rs ${deal.price}',
                        style: const TextStyle(fontWeight: FontWeight.w800, fontSize: 14.5)),
                    const Spacer(),
                    GestureDetector(
                      onTap: () {
                        context.read<CartProvider>().addDeal(deal);
                        ScaffoldMessenger.of(context).showSnackBar(SnackBar(
                          content: Text('${deal.dealName} added to cart'),
                          duration: const Duration(milliseconds: 900),
                        ));
                      },
                      child: Container(
                        width: 30,
                        height: 30,
                        decoration: const BoxDecoration(color: AppColors.yellow, shape: BoxShape.circle),
                        child: const Icon(Icons.add, size: 19, color: AppColors.textDark),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
