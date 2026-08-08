import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../theme/app_theme.dart';

/// Circular DFC logo (yellow ring, fries icon, DFC text).
/// Uses assets/images/logo.png automatically if you add one.
class DfcLogo extends StatelessWidget {
  final double size;
  final bool onDark;
  const DfcLogo({super.key, this.size = 96, this.onDark = true});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: size,
      height: size,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        color: onDark ? Colors.black : Colors.white,
        border: Border.all(color: AppColors.yellow, width: size * 0.045),
      ),
      child: ClipOval(
        child: Image.asset(
          'assets/images/logo.png',
          fit: BoxFit.cover,
          errorBuilder: (_, __, ___) => Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text('🍟', style: TextStyle(fontSize: size * 0.26)),
              Text(
                'DFC',
                style: GoogleFonts.playfairDisplay(
                  color: AppColors.yellow,
                  fontSize: size * 0.28,
                  fontWeight: FontWeight.w800,
                  height: 1,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

/// Product image loaded from the backend (MenuItem/Deal.ImageUrl), with a
/// graceful icon fallback when no image has been uploaded for that item.
class FoodImage extends StatelessWidget {
  final String? imageUrl;
  final double? width, height;
  final BoxFit fit;
  final BorderRadius? radius;
  const FoodImage({
    super.key,
    required this.imageUrl,
    this.width,
    this.height,
    this.fit = BoxFit.cover,
    this.radius,
  });

  Widget _fallback() => Container(
        width: width,
        height: height,
        color: const Color(0xFFF1EDE4),
        alignment: Alignment.center,
        child: Icon(Icons.restaurant, size: (height ?? 80) * 0.35, color: AppColors.textGrey),
      );

  @override
  Widget build(BuildContext context) {
    return ClipRRect(
      borderRadius: radius ?? BorderRadius.circular(14),
      child: (imageUrl == null || imageUrl!.isEmpty)
          ? _fallback()
          : Image.network(
              imageUrl!,
              width: width,
              height: height,
              fit: fit,
              loadingBuilder: (_, child, progress) =>
                  progress == null ? child : _fallback(),
              errorBuilder: (_, __, ___) => _fallback(),
            ),
    );
  }
}
