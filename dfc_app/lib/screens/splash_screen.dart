import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../theme/app_theme.dart';
import '../widgets/dfc_logo.dart';
import 'main_shell.dart';

class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});
  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen>
    with SingleTickerProviderStateMixin {
  late final AnimationController _c =
      AnimationController(vsync: this, duration: const Duration(milliseconds: 900))
        ..forward();

  @override
  void initState() {
    super.initState();
    Future.delayed(const Duration(milliseconds: 2400), _go);
  }

  void _go() {
    if (!mounted) return;
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(builder: (_) => const MainShell()),
    );
  }

  @override
  void dispose() {
    _c.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.navy,
      body: Stack(
        children: [
          // Golden light sweep (bottom-left) like the mockup.
          Positioned(
            left: -80,
            bottom: -40,
            child: Container(
              width: 380,
              height: 300,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                gradient: RadialGradient(colors: [
                  AppColors.yellow.withOpacity(0.35),
                  Colors.transparent,
                ]),
              ),
            ),
          ),
          Center(
            child: FadeTransition(
              opacity: CurvedAnimation(parent: _c, curve: Curves.easeOut),
              child: ScaleTransition(
                scale: Tween(begin: 0.85, end: 1.0).animate(
                    CurvedAnimation(parent: _c, curve: Curves.easeOutBack)),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const DfcLogo(size: 150),
                    const SizedBox(height: 28),
                    Text('Data Finger Chips',
                        style: GoogleFonts.inter(
                            color: Colors.white,
                            fontSize: 28,
                            fontWeight: FontWeight.w800)),
                    const SizedBox(height: 8),
                    RichText(
                      text: TextSpan(
                        style: GoogleFonts.inter(
                            fontSize: 16, fontWeight: FontWeight.w600),
                        children: const [
                          TextSpan(text: 'Crispy. ',
                              style: TextStyle(color: AppColors.orange)),
                          TextSpan(text: 'Fresh. ',
                              style: TextStyle(color: AppColors.yellow)),
                          TextSpan(text: 'Delivered.',
                              style: TextStyle(color: Colors.white)),
                        ],
                      ),
                    ),
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
