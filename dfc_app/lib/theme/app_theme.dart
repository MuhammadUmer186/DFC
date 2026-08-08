import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

/// DFC brand colors & theme — matched to the mockups.
class AppColors {
  static const Color yellow = Color(0xFFFFB800);       // primary CTA yellow
  static const Color yellowDark = Color(0xFFF5A700);
  static const Color orange = Color(0xFFFF6B00);       // accent orange
  static const Color navy = Color(0xFF0B1623);         // dark header / splash bg
  static const Color navyLight = Color(0xFF13233A);
  static const Color bg = Color(0xFFF7F7F5);           // app background
  static const Color card = Colors.white;
  static const Color textDark = Color(0xFF17181C);
  static const Color textGrey = Color(0xFF7A7F87);
  static const Color border = Color(0xFFE7E8EA);
  static const Color green = Color(0xFF16A34A);
  static const Color red = Color(0xFFE23B3B);
  static const Color star = Color(0xFFFFB800);
}

class AppTheme {
  static ThemeData get light {
    final base = ThemeData(
      useMaterial3: true,
      scaffoldBackgroundColor: AppColors.bg,
      colorScheme: ColorScheme.fromSeed(
        seedColor: AppColors.yellow,
        primary: AppColors.yellow,
        surface: AppColors.bg,
      ),
    );
    return base.copyWith(
      textTheme: GoogleFonts.interTextTheme(base.textTheme).apply(
        bodyColor: AppColors.textDark,
        displayColor: AppColors.textDark,
      ),
      appBarTheme: const AppBarTheme(
        backgroundColor: Colors.white,
        elevation: 0,
        centerTitle: true,
        foregroundColor: AppColors.textDark,
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: AppColors.yellow,
          foregroundColor: AppColors.textDark,
          elevation: 0,
          minimumSize: const Size.fromHeight(54),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
          textStyle: GoogleFonts.inter(fontSize: 17, fontWeight: FontWeight.w700),
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: Colors.white,
        hintStyle: const TextStyle(color: AppColors.textGrey, fontSize: 15),
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 15),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: const BorderSide(color: AppColors.border),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: const BorderSide(color: AppColors.yellow, width: 1.5),
        ),
      ),
    );
  }
}
