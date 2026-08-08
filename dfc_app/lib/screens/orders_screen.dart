import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../models/order.dart';
import '../services/order_history_service.dart';
import '../services/order_service.dart';
import '../theme/app_theme.dart';
import 'track_order_screen.dart';

class OrdersScreen extends StatefulWidget {
  const OrdersScreen({super.key});

  @override
  State<OrdersScreen> createState() => _OrdersScreenState();
}

class _OrdersScreenState extends State<OrdersScreen> {
  List<_OrderRow> _rows = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    final history = await OrderHistoryService.getAll();
    final rows = <_OrderRow>[];
    for (final entry in history) {
      try {
        final status = await OrderService.getOrderStatus(entry.orderId, entry.phone);
        rows.add(_OrderRow(entry: entry, status: status));
      } catch (_) {
        rows.add(_OrderRow(entry: entry, status: null));
      }
    }
    if (mounted) {
      setState(() {
        _rows = rows;
        _loading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.bg,
      body: SafeArea(
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 14),
              child: Text('My Orders',
                  style: GoogleFonts.inter(fontSize: 20, fontWeight: FontWeight.w800)),
            ),
            Expanded(
              child: RefreshIndicator(
                onRefresh: _load,
                child: _loading
                    ? ListView(children: const [
                        SizedBox(height: 100),
                        Center(child: CircularProgressIndicator(color: AppColors.yellow)),
                      ])
                    : _rows.isEmpty
                        ? ListView(children: const [
                            SizedBox(height: 100),
                            Center(
                              child: Text('No orders yet — place one from the Menu tab!',
                                  textAlign: TextAlign.center,
                                  style: TextStyle(color: AppColors.textGrey)),
                            ),
                          ])
                        : ListView(
                            padding: const EdgeInsets.fromLTRB(18, 0, 18, 16),
                            children: _rows.map((r) => _OrderTile(row: r)).toList(),
                          ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _OrderRow {
  final OrderHistoryEntry entry;
  final OrderStatusResult? status;
  const _OrderRow({required this.entry, required this.status});
}

class _OrderTile extends StatelessWidget {
  final _OrderRow row;
  const _OrderTile({required this.row});

  @override
  Widget build(BuildContext context) {
    final status = row.status;
    final statusColor = status == null
        ? AppColors.textGrey
        : status.stepIndex < 0
            ? AppColors.red
            : status.stepIndex == 3
                ? AppColors.green
                : AppColors.orange;
    final itemsSummary = status == null
        ? 'Order details unavailable'
        : [...status.items, ...status.deals].map((l) => l.name).join(', ');

    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppColors.border),
      ),
      child: Row(children: [
        Container(
          width: 62,
          height: 62,
          decoration: BoxDecoration(
              color: const Color(0xFFF6F0E2), borderRadius: BorderRadius.circular(12)),
          alignment: Alignment.center,
          child: const Icon(Icons.receipt_long, color: AppColors.yellowDark, size: 28),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(children: [
                Text('Order #DFC-${row.entry.orderId}',
                    style: const TextStyle(fontWeight: FontWeight.w800, fontSize: 14.5)),
                const Spacer(),
                Text(status?.statusLabel ?? '—',
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: TextStyle(color: statusColor, fontSize: 11, fontWeight: FontWeight.w700)),
              ]),
              const SizedBox(height: 3),
              Text(itemsSummary,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(color: AppColors.textGrey, fontSize: 13)),
              const SizedBox(height: 4),
              Row(children: [
                Text(status != null ? 'Rs ${status.totalAmount}' : '',
                    style: const TextStyle(
                        color: AppColors.orange, fontWeight: FontWeight.w800, fontSize: 15)),
                const Spacer(),
                TextButton(
                  onPressed: () => Navigator.of(context).push(MaterialPageRoute(
                      builder: (_) => TrackOrderScreen(
                          orderId: row.entry.orderId, phone: row.entry.phone))),
                  child: const Text('Track',
                      style: TextStyle(color: AppColors.orange, fontWeight: FontWeight.w700)),
                ),
              ]),
            ],
          ),
        ),
      ]),
    );
  }
}
