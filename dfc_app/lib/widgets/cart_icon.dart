import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/cart_provider.dart';
import '../screens/cart_screen.dart';
import '../theme/app_theme.dart';

class CartIcon extends StatelessWidget {
  final Color color;
  const CartIcon({super.key, this.color = Colors.white});

  @override
  Widget build(BuildContext context) {
    final count = context.watch<CartProvider>().itemCount;
    return GestureDetector(
      onTap: () => Navigator.of(context)
          .push(MaterialPageRoute(builder: (_) => const CartScreen())),
      child: Stack(
        clipBehavior: Clip.none,
        children: [
          Icon(Icons.shopping_cart_outlined, color: color, size: 28),
          if (count > 0)
            Positioned(
              right: -6,
              top: -6,
              child: Container(
                padding: const EdgeInsets.all(4),
                decoration: const BoxDecoration(
                    color: AppColors.yellow, shape: BoxShape.circle),
                constraints: const BoxConstraints(minWidth: 20, minHeight: 20),
                child: Text('$count',
                    textAlign: TextAlign.center,
                    style: const TextStyle(
                        fontSize: 11,
                        fontWeight: FontWeight.w800,
                        color: AppColors.textDark)),
              ),
            ),
        ],
      ),
    );
  }
}
