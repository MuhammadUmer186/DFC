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

class HomeScreen extends StatelessWidget {
  final VoidCallback onSeeMenu;
  const HomeScreen({super.key, required this.onSeeMenu});

  String get _greeting {
    final h = DateTime.now().hour;
    if (h < 12) return 'Good morning!';
    if (h < 17) return 'Good afternoon!';
    return 'Good evening!';
  }

  @override
  Widget build(BuildContext context) {
    final menu = context.watch<MenuProvider>();
    return Scaffold(
      backgroundColor: AppColors.bg,
      body: Column(
        children: [
          // ── Dark header ──
          Container(
            color: AppColors.navy,
            padding: const EdgeInsets.fromLTRB(18, 0, 18, 16),
            child: SafeArea(
              bottom: false,
              child: Column(
                children: [
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      const DfcLogo(size: 46),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(_greeting,
                                style: const TextStyle(color: Colors.white70, fontSize: 13)),
                            const Text('Data Finger Chips',
                                style: TextStyle(
                                    color: Colors.white,
                                    fontSize: 16,
                                    fontWeight: FontWeight.w700)),
                          ],
                        ),
                      ),
                      const CartIcon(),
                    ],
                  ),
                  const SizedBox(height: 16),
                  GestureDetector(
                    onTap: onSeeMenu,
                    child: Container(
                      height: 48,
                      padding: const EdgeInsets.symmetric(horizontal: 14),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(14),
                      ),
                      child: Row(children: const [
                        Icon(Icons.search, color: AppColors.textGrey),
                        SizedBox(width: 8),
                        Text('Search burgers, fries & more',
                            style: TextStyle(color: AppColors.textGrey, fontSize: 15)),
                      ]),
                    ),
                  ),
                ],
              ),
            ),
          ),
          // ── Body ──
          Expanded(
            child: RefreshIndicator(
              onRefresh: menu.load,
              child: _HomeBody(menu: menu, onSeeMenu: onSeeMenu),
            ),
          ),
        ],
      ),
    );
  }
}

class _HomeBody extends StatelessWidget {
  final MenuProvider menu;
  final VoidCallback onSeeMenu;
  const _HomeBody({required this.menu, required this.onSeeMenu});

  @override
  Widget build(BuildContext context) {
    if (menu.state == MenuLoadState.loading && menu.categories.isEmpty) {
      return const Center(child: CircularProgressIndicator(color: AppColors.yellow));
    }
    if (menu.state == MenuLoadState.error && menu.categories.isEmpty) {
      return ListView(children: [
        const SizedBox(height: 80),
        Icon(Icons.wifi_off, size: 48, color: AppColors.textGrey.withOpacity(0.6)),
        const SizedBox(height: 12),
        Center(
          child: Text(menu.errorMessage ?? 'Could not load the menu.',
              textAlign: TextAlign.center,
              style: const TextStyle(color: AppColors.textGrey)),
        ),
        const SizedBox(height: 12),
        Center(
          child: OutlinedButton(onPressed: menu.load, child: const Text('Retry')),
        ),
      ]);
    }

    final popular = menu.allItems.take(6).toList();
    final deal = menu.deals.isNotEmpty ? menu.deals.first : null;

    return ListView(
      padding: const EdgeInsets.fromLTRB(18, 16, 18, 16),
      children: [
        _DealBanner(deal: deal, onTap: onSeeMenu),
        const SizedBox(height: 16),
        SizedBox(
          height: 78,
          child: ListView.separated(
            scrollDirection: Axis.horizontal,
            itemCount: menu.categories.length,
            separatorBuilder: (_, __) => const SizedBox(width: 12),
            itemBuilder: (_, i) => _CategoryChip(label: menu.categories[i].name, onTap: onSeeMenu),
          ),
        ),
        const SizedBox(height: 18),
        Row(
          children: [
            Text('Popular items',
                style: GoogleFonts.inter(fontSize: 18, fontWeight: FontWeight.w800)),
            const Spacer(),
            GestureDetector(
              onTap: onSeeMenu,
              child: const Text('View all',
                  style: TextStyle(color: AppColors.orange, fontWeight: FontWeight.w600)),
            ),
          ],
        ),
        const SizedBox(height: 12),
        if (popular.isEmpty)
          const Padding(
            padding: EdgeInsets.symmetric(vertical: 24),
            child: Text('No menu items available right now.',
                style: TextStyle(color: AppColors.textGrey)),
          )
        else
          SizedBox(
            height: 210,
            child: ListView.separated(
              scrollDirection: Axis.horizontal,
              itemCount: popular.length,
              separatorBuilder: (_, __) => const SizedBox(width: 14),
              itemBuilder: (_, i) => _PopularCard(item: popular[i]),
            ),
          ),
      ],
    );
  }
}

class _DealBanner extends StatelessWidget {
  final Deal? deal;
  final VoidCallback onTap;
  const _DealBanner({required this.deal, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        height: 170,
        padding: const EdgeInsets.all(18),
        decoration: BoxDecoration(
          color: AppColors.navy,
          borderRadius: BorderRadius.circular(18),
          border: Border.all(color: AppColors.yellow.withOpacity(0.6)),
        ),
        child: deal == null
            ? Row(
                children: [
                  Expanded(
                    child: Text('Explore\nour full menu',
                        style: GoogleFonts.inter(
                            color: Colors.white,
                            height: 1.1,
                            fontSize: 24,
                            fontWeight: FontWeight.w800)),
                  ),
                  const Text('🍔🍟\n🥤', textAlign: TextAlign.center, style: TextStyle(fontSize: 40)),
                ],
              )
            : Row(
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Text(deal!.dealName,
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                            style: GoogleFonts.inter(
                                color: Colors.white,
                                height: 1.1,
                                fontSize: 22,
                                fontWeight: FontWeight.w800)),
                        const SizedBox(height: 6),
                        Text(deal!.itemsSummary,
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                            style: const TextStyle(color: Colors.white70, fontSize: 12.5)),
                        const SizedBox(height: 6),
                        Text('Rs ${deal!.price}',
                            style: GoogleFonts.inter(
                                color: AppColors.yellow,
                                fontSize: 26,
                                fontWeight: FontWeight.w800)),
                      ],
                    ),
                  ),
                  const SizedBox(width: 10),
                  ClipRRect(
                    borderRadius: BorderRadius.circular(14),
                    child: FoodImage(imageUrl: deal!.networkImageUrl, width: 90, height: 130),
                  ),
                ],
              ),
      ),
    );
  }
}

class _CategoryChip extends StatelessWidget {
  final String label;
  final VoidCallback onTap;
  const _CategoryChip({required this.label, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 74,
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: AppColors.border),
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.fastfood, size: 22, color: AppColors.yellowDark),
            const SizedBox(height: 4),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 4),
              child: Text(label,
                  textAlign: TextAlign.center,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(fontSize: 11.5, fontWeight: FontWeight.w600)),
            ),
          ],
        ),
      ),
    );
  }
}

class _PopularCard extends StatelessWidget {
  final MenuItem item;
  const _PopularCard({required this.item});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: () => Navigator.of(context)
          .push(MaterialPageRoute(builder: (_) => ProductDetailScreen(item: item))),
      child: Container(
        width: 150,
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
              imageUrl: item.networkImageUrl,
              width: 150,
              height: 110,
              radius: const BorderRadius.vertical(top: Radius.circular(16)),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(10, 8, 10, 10),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(item.name,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 13.5)),
                  const SizedBox(height: 3),
                  Text(item.categoryName,
                      style: const TextStyle(fontSize: 12, color: AppColors.textGrey)),
                  const SizedBox(height: 5),
                  Row(
                    children: [
                      Text('Rs ${item.price}',
                          style: const TextStyle(fontWeight: FontWeight.w800, fontSize: 14)),
                      const Spacer(),
                      _AddButton(item: item),
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

class _AddButton extends StatelessWidget {
  final MenuItem item;
  const _AddButton({required this.item});
  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: () {
        context.read<CartProvider>().addItem(item);
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(
          content: Text('${item.name} added to cart'),
          duration: const Duration(milliseconds: 900),
        ));
      },
      child: Container(
        width: 28,
        height: 28,
        decoration: const BoxDecoration(color: AppColors.yellow, shape: BoxShape.circle),
        child: const Icon(Icons.add, size: 18, color: AppColors.textDark),
      ),
    );
  }
}
