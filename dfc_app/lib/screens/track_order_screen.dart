import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../models/order.dart';
import '../services/api_client.dart';
import '../services/order_service.dart';
import '../theme/app_theme.dart';
import 'main_shell.dart';

class TrackOrderScreen extends StatefulWidget {
  final int orderId;
  final String phone;
  const TrackOrderScreen({super.key, required this.orderId, required this.phone});

  @override
  State<TrackOrderScreen> createState() => _TrackOrderScreenState();
}

class _TrackOrderScreenState extends State<TrackOrderScreen> {
  OrderStatusResult? _status;
  String? _error;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _refresh();
  }

  Future<void> _refresh() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final result = await OrderService.getOrderStatus(widget.orderId, widget.phone);
      setState(() => _status = result);
    } on ApiException catch (e) {
      setState(() => _error = e.message);
    } catch (_) {
      setState(() => _error = 'Could not load order status.');
    } finally {
      setState(() => _loading = false);
    }
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
            padding: const EdgeInsets.fromLTRB(14, 0, 14, 14),
            child: SafeArea(
              bottom: false,
              child: Row(
                children: [
                  IconButton(
                    icon: const Icon(Icons.arrow_back, color: Colors.white),
                    onPressed: () => Navigator.of(context).pushAndRemoveUntil(
                        MaterialPageRoute(builder: (_) => const MainShell(initialIndex: 2)),
                        (_) => false),
                  ),
                  Expanded(
                    child: Column(children: [
                      Text('Track Order',
                          style: GoogleFonts.inter(
                              color: Colors.white, fontSize: 18, fontWeight: FontWeight.w800)),
                      Text('#DFC-${widget.orderId}',
                          style: const TextStyle(
                              color: AppColors.yellow, fontSize: 13, fontWeight: FontWeight.w700)),
                    ]),
                  ),
                  IconButton(
                    icon: const Icon(Icons.refresh, color: Colors.white),
                    onPressed: _loading ? null : _refresh,
                  ),
                ],
              ),
            ),
          ),
          Expanded(
            child: RefreshIndicator(
              onRefresh: _refresh,
              child: _loading && _status == null
                  ? ListView(children: const [
                      SizedBox(height: 120),
                      Center(child: CircularProgressIndicator(color: AppColors.yellow)),
                    ])
                  : _error != null && _status == null
                      ? ListView(children: [
                          const SizedBox(height: 100),
                          Center(
                            child: Text(_error!,
                                textAlign: TextAlign.center,
                                style: const TextStyle(color: AppColors.textGrey)),
                          ),
                          const SizedBox(height: 12),
                          Center(child: OutlinedButton(onPressed: _refresh, child: const Text('Retry'))),
                        ])
                      : _StatusView(status: _status!),
            ),
          ),
        ],
      ),
    );
  }
}

class _StatusView extends StatelessWidget {
  final OrderStatusResult status;
  const _StatusView({required this.status});

  @override
  Widget build(BuildContext context) {
    final lines = [...status.items, ...status.deals];
    return ListView(
      padding: const EdgeInsets.fromLTRB(20, 20, 20, 24),
      children: [
        if (status.stepIndex < 0)
          Text(status.statusLabel,
              style: GoogleFonts.inter(
                  fontSize: 20, fontWeight: FontWeight.w800, color: AppColors.red))
        else
          Text(status.statusLabel,
              style: GoogleFonts.inter(
                  fontSize: 18, fontWeight: FontWeight.w800, color: AppColors.textDark)),
        const SizedBox(height: 18),
        if (status.stepIndex >= 0) _StatusStepper(currentStep: status.stepIndex),
        const SizedBox(height: 18),
        Container(
          padding: const EdgeInsets.all(14),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(16),
            border: Border.all(color: AppColors.border),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(children: [
                const Icon(Icons.shopping_bag_outlined, size: 20),
                const SizedBox(width: 10),
                Text('${status.itemCount} items  •  Rs ${status.totalAmount}',
                    style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 15)),
              ]),
              if (lines.isNotEmpty) ...[
                const Divider(height: 20),
                ...lines.map((l) => Padding(
                      padding: const EdgeInsets.symmetric(vertical: 3),
                      child: Row(children: [
                        Expanded(
                            child: Text('${l.quantity}x ${l.name}',
                                style: const TextStyle(fontSize: 13.5))),
                        Text('Rs ${l.unitPrice * l.quantity}',
                            style: const TextStyle(fontSize: 13.5, color: AppColors.textGrey)),
                      ]),
                    )),
              ],
              const Divider(height: 20),
              Row(children: [
                if (status.serviceType != null)
                  Text(status.serviceType!,
                      style: const TextStyle(color: AppColors.textGrey, fontSize: 12.5)),
                const Spacer(),
                Text('Placed ${_formatTime(status.createdAt)}',
                    style: const TextStyle(color: AppColors.textGrey, fontSize: 12.5)),
              ]),
            ],
          ),
        ),
      ],
    );
  }

  String _formatTime(DateTime dt) {
    final local = dt.toLocal();
    final h = local.hour % 12 == 0 ? 12 : local.hour % 12;
    final ampm = local.hour >= 12 ? 'PM' : 'AM';
    final m = local.minute.toString().padLeft(2, '0');
    return '${local.day}/${local.month} $h:$m $ampm';
  }
}

class _StatusStepper extends StatelessWidget {
  final int currentStep; // 0..3
  const _StatusStepper({required this.currentStep});

  static const _steps = <(String, IconData)>[
    ('Received', Icons.check),
    ('Confirmed', Icons.check),
    ('Preparing', Icons.local_fire_department),
    ('Completed', Icons.check_circle),
  ];

  @override
  Widget build(BuildContext context) {
    return Row(
      children: List.generate(_steps.length * 2 - 1, (i) {
        if (i.isOdd) {
          final done = (i ~/ 2) < currentStep;
          return Expanded(
            child: Container(
              height: 3,
              margin: const EdgeInsets.only(bottom: 22),
              color: done ? AppColors.yellow : AppColors.border,
            ),
          );
        }
        final idx = i ~/ 2;
        final done = idx < currentStep;
        final active = idx == currentStep;
        return Column(children: [
          Container(
            width: 34,
            height: 34,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: done || active ? AppColors.yellow : AppColors.border,
            ),
            child: Icon(
              done ? Icons.check : _steps[idx].$2,
              size: 18,
              color: done || active ? AppColors.textDark : Colors.white,
            ),
          ),
          const SizedBox(height: 6),
          Text(_steps[idx].$1,
              style: TextStyle(
                  fontSize: 10.5,
                  fontWeight: active ? FontWeight.w800 : FontWeight.w600,
                  color: done || active ? AppColors.textDark : AppColors.textGrey)),
        ]);
      }),
    );
  }
}
