import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../services/order_history_service.dart';
import '../theme/app_theme.dart';
import '../widgets/dfc_logo.dart';
import 'main_shell.dart';
import 'track_order_screen.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  List<OrderHistoryEntry> _recent = [];

  @override
  void initState() {
    super.initState();
    OrderHistoryService.getAll().then((all) {
      if (mounted) setState(() => _recent = all.take(2).toList());
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.bg,
      body: Column(
        children: [
          // ── Dark header ──
          Container(
            color: AppColors.navy,
            padding: const EdgeInsets.fromLTRB(18, 0, 18, 18),
            child: SafeArea(
              bottom: false,
              child: Row(children: [
                const DfcLogo(size: 44),
                const SizedBox(width: 12),
                Text('My DFC',
                    style: GoogleFonts.inter(
                        color: Colors.white, fontSize: 20, fontWeight: FontWeight.w800)),
              ]),
            ),
          ),
          Expanded(
            child: ListView(
              padding: const EdgeInsets.fromLTRB(18, 16, 18, 16),
              children: [
                Container(
                  padding: const EdgeInsets.all(18),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(16),
                    border: Border.all(color: AppColors.border),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('Data Finger Chips',
                          style: GoogleFonts.inter(fontSize: 18, fontWeight: FontWeight.w800)),
                      const SizedBox(height: 6),
                      const Text(
                        'Order as a guest — just your name and phone at checkout. '
                        'Track any order any time with its order number and phone.',
                        style: TextStyle(color: AppColors.textGrey, fontSize: 13.5, height: 1.4),
                      ),
                    ],
                  ),
                ),
                if (_recent.isNotEmpty) ...[
                  const SizedBox(height: 18),
                  Row(children: [
                    Text('Recent Orders',
                        style: GoogleFonts.inter(fontSize: 17, fontWeight: FontWeight.w800)),
                    const Spacer(),
                    GestureDetector(
                      onTap: () => Navigator.of(context).pushAndRemoveUntil(
                          MaterialPageRoute(builder: (_) => const MainShell(initialIndex: 2)),
                          (_) => false),
                      child: const Text('View all',
                          style: TextStyle(color: AppColors.orange, fontWeight: FontWeight.w600)),
                    ),
                  ]),
                  const SizedBox(height: 12),
                  ..._recent.map((e) => _RecentOrderTile(entry: e)),
                ],
                const SizedBox(height: 8),
                _MenuRow(
                  icon: Icons.help_outline,
                  label: 'Help & Support',
                  onTap: () => ScaffoldMessenger.of(context).showSnackBar(const SnackBar(
                      content: Text('For help, contact your nearest DFC branch.'))),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _RecentOrderTile extends StatelessWidget {
  final OrderHistoryEntry entry;
  const _RecentOrderTile({required this.entry});

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: AppColors.border),
      ),
      child: ListTile(
        onTap: () => Navigator.of(context).push(MaterialPageRoute(
            builder: (_) => TrackOrderScreen(orderId: entry.orderId, phone: entry.phone))),
        leading: const CircleAvatar(
            radius: 18,
            backgroundColor: Color(0xFFF4F4F2),
            child: Icon(Icons.receipt_long, size: 18, color: AppColors.yellowDark)),
        title: Text('Order #DFC-${entry.orderId}',
            style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 14.5)),
        subtitle: Text('Placed ${entry.placedAt.day}/${entry.placedAt.month}/${entry.placedAt.year}',
            style: const TextStyle(fontSize: 12)),
        trailing: const Icon(Icons.chevron_right, color: AppColors.textGrey),
      ),
    );
  }
}

class _MenuRow extends StatelessWidget {
  final IconData icon;
  final String label;
  final VoidCallback? onTap;
  const _MenuRow({required this.icon, required this.label, this.onTap});

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: AppColors.border),
      ),
      child: ListTile(
        onTap: onTap ?? () {},
        leading: CircleAvatar(
          radius: 18,
          backgroundColor: const Color(0xFFF4F4F2),
          child: Icon(icon, size: 20, color: AppColors.textDark),
        ),
        title: Text(label,
            style: const TextStyle(
                fontWeight: FontWeight.w700, fontSize: 15, color: AppColors.textDark)),
        trailing: const Icon(Icons.chevron_right, color: AppColors.textGrey),
      ),
    );
  }
}
