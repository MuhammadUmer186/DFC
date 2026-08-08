import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:geolocator/geolocator.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import '../models/order.dart';
import '../pricing.dart';
import '../providers/cart_provider.dart';
import '../providers/order_type_provider.dart';
import '../services/api_client.dart';
import '../services/order_history_service.dart';
import '../services/order_service.dart';
import '../services/service_time_service.dart';
import '../theme/app_theme.dart';
import 'track_order_screen.dart';

class CheckoutScreen extends StatefulWidget {
  const CheckoutScreen({super.key});
  @override
  State<CheckoutScreen> createState() => _CheckoutScreenState();
}

class _CheckoutScreenState extends State<CheckoutScreen> {
  final _nameCtrl = TextEditingController();
  final _phoneCtrl = TextEditingController();
  final _addressCtrl = TextEditingController();
  String _paymentMethod = 'Cash';
  double? _lat, _lng;
  bool _locating = false;
  bool _submitting = false;
  String? _errorMsg;

  List<ServiceTimeSetting> _serviceTimes = [];

  @override
  void initState() {
    super.initState();
    OrderHistoryService.lastPhone().then((phone) {
      if (phone != null && mounted) _phoneCtrl.text = phone;
    });
    ServiceTimeService.getServiceTimes().then((s) {
      if (mounted) setState(() => _serviceTimes = s);
    }).catchError((_) {});
  }

  @override
  void dispose() {
    _nameCtrl.dispose();
    _phoneCtrl.dispose();
    _addressCtrl.dispose();
    super.dispose();
  }

  bool get _requiresAddress =>
      context.read<OrderTypeProvider>().serviceType == ServiceType.delivery;

  Future<void> _useMyLocation() async {
    setState(() => _locating = true);
    try {
      final serviceEnabled = await Geolocator.isLocationServiceEnabled();
      if (!serviceEnabled) throw 'Location services are turned off.';

      var permission = await Geolocator.checkPermission();
      if (permission == LocationPermission.denied) {
        permission = await Geolocator.requestPermission();
        if (permission == LocationPermission.denied) throw 'Location permission denied.';
      }
      if (permission == LocationPermission.deniedForever) {
        throw 'Location permission permanently denied. Enable it from Settings.';
      }

      final pos = await Geolocator.getCurrentPosition(
        locationSettings: const LocationSettings(accuracy: LocationAccuracy.high),
      );
      setState(() {
        _lat = pos.latitude;
        _lng = pos.longitude;
      });
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(const SnackBar(content: Text('Location captured ✓')));
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('$e')));
      }
    } finally {
      if (mounted) setState(() => _locating = false);
    }
  }

  bool _canSubmit(CartProvider cart) {
    return _nameCtrl.text.trim().isNotEmpty &&
        _phoneCtrl.text.trim().length >= 7 &&
        (!_requiresAddress || _addressCtrl.text.trim().isNotEmpty) &&
        cart.lines.isNotEmpty &&
        !_submitting;
  }

  Future<void> _placeOrder(CartProvider cart, ServiceType serviceType) async {
    setState(() {
      _submitting = true;
      _errorMsg = null;
    });

    final request = PlaceOrderRequest(
      customerName: _nameCtrl.text.trim(),
      phoneNumber: _phoneCtrl.text.trim(),
      address: _requiresAddress ? _addressCtrl.text.trim() : '',
      latitude: _requiresAddress ? _lat : null,
      longitude: _requiresAddress ? _lng : null,
      paymentMethod: _paymentMethod,
      serviceType: serviceType,
      deliveryFee: deliveryFeeFor(serviceType),
      packagingFee: packagingFeeFor(serviceType),
      lines: cart.lines,
    );

    try {
      final response = await OrderService.placeOrder(request);
      await OrderHistoryService.add(response.id, _phoneCtrl.text.trim());
      cart.clear();
      if (!mounted) return;
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(
          builder: (_) => TrackOrderScreen(orderId: response.id, phone: _phoneCtrl.text.trim()),
        ),
      );
    } on ApiException catch (e) {
      setState(() => _errorMsg = e.message);
    } catch (_) {
      setState(() => _errorMsg = 'Something went wrong placing your order. Please try again.');
    } finally {
      if (mounted) setState(() => _submitting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final cart = context.watch<CartProvider>();
    final orderType = context.watch<OrderTypeProvider>();
    final serviceType = orderType.serviceType;
    final deliveryFee = deliveryFeeFor(serviceType);
    final packagingFee = packagingFeeFor(serviceType);
    final total = cart.subtotal + deliveryFee + packagingFee;
    final timeSetting = ServiceTimeService.forType(_serviceTimes, serviceType);

    return Scaffold(
      backgroundColor: Colors.white,
      appBar: AppBar(
        leading: IconButton(
            icon: const Icon(Icons.arrow_back), onPressed: () => Navigator.pop(context)),
        title: Text('Checkout',
            style: GoogleFonts.inter(fontSize: 19, fontWeight: FontWeight.w800)),
      ),
      body: Column(
        children: [
          Expanded(
            child: ListView(
              padding: const EdgeInsets.fromLTRB(18, 6, 18, 12),
              children: [
                // Service type toggle
                Container(
                  padding: const EdgeInsets.all(5),
                  decoration: BoxDecoration(
                    color: const Color(0xFFF2F2F0),
                    borderRadius: BorderRadius.circular(14),
                  ),
                  child: Row(
                    children: ServiceType.values.map((type) {
                      final sel = type == serviceType;
                      return Expanded(
                        child: GestureDetector(
                          onTap: () => setState(() => orderType.set(type)),
                          child: Container(
                            padding: const EdgeInsets.symmetric(vertical: 11),
                            decoration: BoxDecoration(
                              color: sel ? AppColors.yellow : Colors.transparent,
                              borderRadius: BorderRadius.circular(11),
                            ),
                            alignment: Alignment.center,
                            child: Text(type.label,
                                style: TextStyle(
                                    fontWeight: FontWeight.w700,
                                    color: sel ? AppColors.textDark : AppColors.textGrey)),
                          ),
                        ),
                      );
                    }).toList(),
                  ),
                ),
                const SizedBox(height: 18),
                _title('Your Details'),
                const SizedBox(height: 10),
                TextField(
                  controller: _nameCtrl,
                  decoration: const InputDecoration(
                      hintText: 'Full name', prefixIcon: Icon(Icons.person_outline)),
                  onChanged: (_) => setState(() {}),
                ),
                const SizedBox(height: 10),
                TextField(
                  controller: _phoneCtrl,
                  keyboardType: TextInputType.phone,
                  inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                  decoration: const InputDecoration(
                      hintText: 'Phone number (e.g. 03001234567)',
                      prefixIcon: Icon(Icons.phone_outlined)),
                  onChanged: (_) => setState(() {}),
                ),
                if (_requiresAddress) ...[
                  const SizedBox(height: 18),
                  _title('Delivery Address'),
                  const SizedBox(height: 10),
                  TextField(
                    controller: _addressCtrl,
                    maxLines: 2,
                    decoration: const InputDecoration(
                        hintText: 'House / street / area', prefixIcon: Icon(Icons.home_outlined)),
                    onChanged: (_) => setState(() {}),
                  ),
                  const SizedBox(height: 10),
                  OutlinedButton.icon(
                    onPressed: _locating ? null : _useMyLocation,
                    icon: _locating
                        ? const SizedBox(
                            width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2))
                        : Icon(_lat != null ? Icons.check_circle : Icons.my_location,
                            size: 18, color: _lat != null ? AppColors.green : AppColors.textDark),
                    label: Text(_lat != null ? 'Location captured' : 'Use my location'),
                  ),
                ],
                const SizedBox(height: 18),
                _title('Estimated Time'),
                const SizedBox(height: 10),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 14),
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(14),
                    border: Border.all(color: AppColors.border),
                  ),
                  child: Row(children: [
                    const Icon(Icons.access_time, color: AppColors.textGrey, size: 20),
                    const SizedBox(width: 10),
                    Text(timeSetting != null ? timeSetting.label : 'Calculating…',
                        style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14.5)),
                  ]),
                ),
                const SizedBox(height: 18),
                _title('Payment Method'),
                const SizedBox(height: 10),
                Container(
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(14),
                    border: Border.all(color: AppColors.border),
                  ),
                  child: Column(
                    children: [
                      RadioListTile<String>(
                        value: 'Cash',
                        groupValue: _paymentMethod,
                        onChanged: (v) => setState(() => _paymentMethod = v!),
                        activeColor: AppColors.yellowDark,
                        controlAffinity: ListTileControlAffinity.trailing,
                        title: Row(children: const [
                          Icon(Icons.payments_outlined, size: 22, color: AppColors.textDark),
                          SizedBox(width: 12),
                          Text('Cash',
                              style: TextStyle(fontSize: 15, fontWeight: FontWeight.w600)),
                        ]),
                      ),
                      const Divider(height: 1, color: AppColors.border),
                      RadioListTile<String>(
                        value: 'Online transfer',
                        groupValue: _paymentMethod,
                        onChanged: (v) => setState(() => _paymentMethod = v!),
                        activeColor: AppColors.yellowDark,
                        controlAffinity: ListTileControlAffinity.trailing,
                        title: Row(children: const [
                          Icon(Icons.account_balance_outlined, size: 22, color: AppColors.textDark),
                          SizedBox(width: 12),
                          Text('Online transfer',
                              style: TextStyle(fontSize: 15, fontWeight: FontWeight.w600)),
                        ]),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 20),
                _priceRow('Subtotal', 'Rs ${cart.subtotal}'),
                _priceRow('${serviceType.label} fee', deliveryFee > 0 ? 'Rs $deliveryFee' : 'Free'),
                if (packagingFee > 0) _priceRow('Packaging', 'Rs $packagingFee'),
                const Divider(height: 24),
                Row(children: [
                  Text('Order Total',
                      style: GoogleFonts.inter(fontSize: 17, fontWeight: FontWeight.w800)),
                  const Spacer(),
                  Text('Rs $total',
                      style: GoogleFonts.inter(
                          fontSize: 22, fontWeight: FontWeight.w800, color: AppColors.orange)),
                ]),
                if (_errorMsg != null) ...[
                  const SizedBox(height: 14),
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: const Color(0xFFFDEDED),
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(color: AppColors.red.withOpacity(0.4)),
                    ),
                    child: Text(_errorMsg!,
                        style: const TextStyle(color: AppColors.red, fontSize: 13.5)),
                  ),
                ],
              ],
            ),
          ),
          SafeArea(
            top: false,
            child: Padding(
              padding: const EdgeInsets.fromLTRB(18, 6, 18, 12),
              child: ElevatedButton(
                onPressed: _canSubmit(cart) ? () => _placeOrder(cart, serviceType) : null,
                child: _submitting
                    ? const SizedBox(
                        width: 22,
                        height: 22,
                        child: CircularProgressIndicator(strokeWidth: 2.4, color: Colors.white))
                    : const Text('Place Order'),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _title(String t) =>
      Text(t, style: GoogleFonts.inter(fontSize: 16.5, fontWeight: FontWeight.w800));

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
