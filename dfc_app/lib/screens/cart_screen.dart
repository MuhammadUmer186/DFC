import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import '../models/cart_line.dart';
import '../pricing.dart';
import '../providers/cart_provider.dart';
import '../providers/order_type_provider.dart';
import '../theme/app_theme.dart';
import '../widgets/dfc_logo.dart';
import 'checkout_screen.dart';

class CartScreen extends StatelessWidget {
  const CartScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final cart = context.watch<CartProvider>();
    final serviceType = context.watch<OrderTypeProvider>().serviceType;
    final deliveryFee = deliveryFeeFor(serviceType);
    final packagingFee = packagingFeeFor(serviceType);
    final total = cart.subtotal + deliveryFee + packagingFee;

    return Scaffold(
      backgroundColor: Colors.white,
      appBar: AppBar(
        leading: IconButton(
            icon: const Icon(Icons.arrow_back), onPressed: () => Navigator.pop(context)),
        title: Text('Your Cart',
            style: GoogleFonts.inter(fontSize: 19, fontWeight: FontWeight.w800)),
      ),
      body: cart.lines.isEmpty
          ? const Center(
              child: Text('Your cart is empty 🛒',
                  style: TextStyle(fontSize: 17, color: AppColors.textGrey)))
          : Column(
              children: [
                Expanded(
                  child: ListView(
                    padding: const EdgeInsets.fromLTRB(18, 12, 18, 12),
                    children: [
                      ...cart.lines.map((l) => _CartRow(line: l)),
                      const SizedBox(height: 16),
                      _priceRow('Subtotal', 'Rs ${cart.subtotal}'),
                      _priceRow(
                          '${serviceType.label} fee', deliveryFee > 0 ? 'Rs $deliveryFee' : 'Free'),
                      if (packagingFee > 0) _priceRow('Packaging', 'Rs $packagingFee'),
                      const Divider(height: 24),
                      Row(children: [
                        Text('Total',
                            style: GoogleFonts.inter(fontSize: 18, fontWeight: FontWeight.w800)),
                        const Spacer(),
                        Text('Rs $total',
                            style: GoogleFonts.inter(fontSize: 18, fontWeight: FontWeight.w800)),
                      ]),
                    ],
                  ),
                ),
                SafeArea(
                  top: false,
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(18, 6, 18, 12),
                    child: ElevatedButton(
                      onPressed: () => Navigator.of(context)
                          .push(MaterialPageRoute(builder: (_) => const CheckoutScreen())),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: const [
                          Icon(Icons.shopping_cart, size: 20),
                          SizedBox(width: 10),
                          Text('Proceed to Checkout'),
                        ],
                      ),
                    ),
                  ),
                ),
              ],
            ),
    );
  }

  Widget _priceRow(String label, String value, {Color? color}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(children: [
        Text(label,
            style: TextStyle(color: color ?? AppColors.textGrey, fontSize: 15, fontWeight: FontWeight.w500)),
        const Spacer(),
        Text(value,
            style: TextStyle(color: color ?? AppColors.textDark, fontSize: 15, fontWeight: FontWeight.w700)),
      ]),
    );
  }
}

class _CartRow extends StatelessWidget {
  final CartLine line;
  const _CartRow({required this.line});

  @override
  Widget build(BuildContext context) {
    final cart = context.read<CartProvider>();
    return Padding(
      padding: const EdgeInsets.only(bottom: 14),
      child: Row(
        children: [
          FoodImage(imageUrl: line.imageUrl, width: 64, height: 64),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(line.name,
                    style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 15.5)),
                if (line.kind == CartLineKind.deal)
                  const Text('Deal',
                      style: TextStyle(fontSize: 12, color: AppColors.textGrey)),
                const SizedBox(height: 6),
                Row(children: [
                  _stepBtn(Icons.remove, () => cart.changeQty(line, -1)),
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 14),
                    child: Text('${line.quantity}',
                        style: const TextStyle(fontWeight: FontWeight.w800, fontSize: 15)),
                  ),
                  _stepBtn(Icons.add, () => cart.changeQty(line, 1), primary: true),
                ]),
              ],
            ),
          ),
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Text('Rs ${line.total}',
                  style: const TextStyle(fontWeight: FontWeight.w800, fontSize: 15.5)),
              const SizedBox(height: 10),
              GestureDetector(
                onTap: () => cart.remove(line),
                child: const Icon(Icons.delete_outline, color: AppColors.red, size: 22),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _stepBtn(IconData i, VoidCallback onTap, {bool primary = false}) => GestureDetector(
        onTap: onTap,
        child: Container(
          width: 28,
          height: 28,
          decoration: BoxDecoration(
            color: primary ? AppColors.yellow : Colors.white,
            borderRadius: BorderRadius.circular(8),
            border: primary ? null : Border.all(color: AppColors.border),
          ),
          child: Icon(i, size: 16, color: AppColors.textDark),
        ),
      );
}
