import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import '../models/menu_item.dart';
import '../providers/cart_provider.dart';
import '../theme/app_theme.dart';
import '../widgets/dfc_logo.dart';

class ProductDetailScreen extends StatefulWidget {
  final MenuItem item;
  const ProductDetailScreen({super.key, required this.item});

  @override
  State<ProductDetailScreen> createState() => _ProductDetailScreenState();
}

class _ProductDetailScreenState extends State<ProductDetailScreen> {
  int _qty = 1;

  num get _total => widget.item.price * _qty;

  @override
  Widget build(BuildContext context) {
    final item = widget.item;
    return Scaffold(
      backgroundColor: Colors.white,
      body: Column(
        children: [
          // ── Hero image ──
          Stack(
            children: [
              FoodImage(
                imageUrl: item.networkImageUrl,
                width: double.infinity,
                height: 320,
                radius: BorderRadius.zero,
              ),
              Positioned(
                top: 0,
                left: 0,
                right: 0,
                child: SafeArea(
                  child: Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 6),
                    child: _RoundBtn(icon: Icons.arrow_back, onTap: () => Navigator.pop(context)),
                  ),
                ),
              ),
            ],
          ),
          // ── Details sheet ──
          Expanded(
            child: SingleChildScrollView(
              padding: const EdgeInsets.fromLTRB(20, 18, 20, 10),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(item.name,
                      style: GoogleFonts.inter(fontSize: 26, fontWeight: FontWeight.w800)),
                  const SizedBox(height: 4),
                  Text(item.categoryName,
                      style: const TextStyle(color: AppColors.textGrey, fontSize: 13)),
                  const SizedBox(height: 10),
                  if (item.description != null && item.description!.isNotEmpty)
                    Text(item.description!,
                        style: const TextStyle(color: AppColors.textGrey, fontSize: 14.5)),
                  const SizedBox(height: 18),
                  Text('Rs ${item.price}',
                      style: GoogleFonts.inter(
                          fontSize: 22, fontWeight: FontWeight.w800, color: AppColors.orange)),
                  const SizedBox(height: 20),
                  Row(
                    children: [
                      const _SectionTitle('Quantity'),
                      const Spacer(),
                      _QtyStepper(qty: _qty, onChanged: (q) => setState(() => _qty = q)),
                    ],
                  ),
                  const SizedBox(height: 8),
                ],
              ),
            ),
          ),
          // ── Add to cart bar ──
          SafeArea(
            top: false,
            child: Padding(
              padding: const EdgeInsets.fromLTRB(20, 6, 20, 12),
              child: ElevatedButton(
                onPressed: () {
                  context.read<CartProvider>().addItem(item, qty: _qty);
                  Navigator.pop(context);
                  ScaffoldMessenger.of(context).showSnackBar(SnackBar(
                    content: Text('${item.name} added to cart'),
                    duration: const Duration(milliseconds: 1000),
                  ));
                },
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Icon(Icons.shopping_cart, size: 20),
                    const SizedBox(width: 10),
                    Text('Add to Cart  •  Rs $_total'),
                  ],
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _RoundBtn extends StatelessWidget {
  final IconData icon;
  final VoidCallback onTap;
  const _RoundBtn({required this.icon, required this.onTap});
  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 42,
        height: 42,
        decoration: const BoxDecoration(color: Colors.white, shape: BoxShape.circle),
        child: Icon(icon, color: AppColors.textDark, size: 22),
      ),
    );
  }
}

class _SectionTitle extends StatelessWidget {
  final String text;
  const _SectionTitle(this.text);
  @override
  Widget build(BuildContext context) =>
      Text(text, style: GoogleFonts.inter(fontSize: 16.5, fontWeight: FontWeight.w800));
}

class _QtyStepper extends StatelessWidget {
  final int qty;
  final ValueChanged<int> onChanged;
  const _QtyStepper({required this.qty, required this.onChanged});
  @override
  Widget build(BuildContext context) {
    Widget btn(IconData i, VoidCallback onTap, {bool primary = false}) => GestureDetector(
          onTap: onTap,
          child: Container(
            width: 34,
            height: 34,
            decoration: BoxDecoration(
              color: primary ? AppColors.yellow : Colors.white,
              shape: BoxShape.circle,
              border: primary ? null : Border.all(color: AppColors.border),
            ),
            child: Icon(i, size: 18, color: AppColors.textDark),
          ),
        );
    return Row(children: [
      btn(Icons.remove, () => onChanged(qty > 1 ? qty - 1 : 1)),
      Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16),
        child: Text('$qty', style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w800)),
      ),
      btn(Icons.add, () => onChanged(qty + 1), primary: true),
    ]);
  }
}
