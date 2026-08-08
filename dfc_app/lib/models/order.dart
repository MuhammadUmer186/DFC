import 'cart_line.dart';
import '../providers/order_type_provider.dart';

/// Matches the backend's PublicOrderRequest exactly (Backend/DTOs/OrderDto.cs).
class PlaceOrderRequest {
  final String customerName;
  final String phoneNumber;
  final String address;
  final double? latitude;
  final double? longitude;
  final String paymentMethod; // 'Cash' or 'Online transfer'
  final ServiceType serviceType;
  final num deliveryFee;
  final num packagingFee;
  final List<CartLine> lines;

  const PlaceOrderRequest({
    required this.customerName,
    required this.phoneNumber,
    required this.address,
    this.latitude,
    this.longitude,
    required this.paymentMethod,
    required this.serviceType,
    required this.deliveryFee,
    required this.packagingFee,
    required this.lines,
  });

  Map<String, dynamic> toJson() => {
        'customerName': customerName,
        'phoneNumber': phoneNumber,
        'address': address,
        'latitude': latitude,
        'longitude': longitude,
        'paymentMethod': paymentMethod,
        'serviceType': serviceType.apiValue,
        'deliveryFee': deliveryFee,
        'packagingFee': packagingFee,
        'items': lines
            .where((l) => l.kind == CartLineKind.item)
            .map((l) => {'menuItemId': l.id, 'quantity': l.quantity})
            .toList(),
        'deals': lines
            .where((l) => l.kind == CartLineKind.deal)
            .map((l) => {'dealId': l.id, 'quantity': l.quantity})
            .toList(),
      };
}

class OrderLineSummary {
  final String name;
  final num unitPrice;
  final int quantity;
  const OrderLineSummary({required this.name, required this.unitPrice, required this.quantity});

  factory OrderLineSummary.item(Map<String, dynamic> json) => OrderLineSummary(
        name: json['menuItemName'] as String? ?? 'Item',
        unitPrice: json['unitPrice'] as num? ?? 0,
        quantity: json['quantity'] as int? ?? 0,
      );

  factory OrderLineSummary.deal(Map<String, dynamic> json) => OrderLineSummary(
        name: json['dealName'] as String? ?? 'Deal',
        unitPrice: json['dealPrice'] as num? ?? 0,
        quantity: json['quantity'] as int? ?? 0,
      );
}

class PlaceOrderResponse {
  final int id;
  final num totalAmount;
  final String? serviceType;
  final List<OrderLineSummary> items;
  final List<OrderLineSummary> deals;

  const PlaceOrderResponse({
    required this.id,
    required this.totalAmount,
    this.serviceType,
    this.items = const [],
    this.deals = const [],
  });

  factory PlaceOrderResponse.fromJson(Map<String, dynamic> json) => PlaceOrderResponse(
        id: json['id'] as int,
        totalAmount: json['totalAmount'] as num? ?? 0,
        serviceType: json['serviceType'] as String?,
        items: (json['items'] as List<dynamic>? ?? [])
            .map((e) => OrderLineSummary.item(e as Map<String, dynamic>))
            .toList(),
        deals: (json['deals'] as List<dynamic>? ?? [])
            .map((e) => OrderLineSummary.deal(e as Map<String, dynamic>))
            .toList(),
      );
}

/// Matches PublicOrderStatusDto — what GET /Public/order/{id}/status returns.
class OrderStatusResult {
  final int id;
  final String statusLabel;
  final DateTime createdAt;
  final num totalAmount;
  final String? serviceType;
  final List<OrderLineSummary> items;
  final List<OrderLineSummary> deals;

  const OrderStatusResult({
    required this.id,
    required this.statusLabel,
    required this.createdAt,
    required this.totalAmount,
    this.serviceType,
    this.items = const [],
    this.deals = const [],
  });

  factory OrderStatusResult.fromJson(Map<String, dynamic> json) => OrderStatusResult(
        id: json['id'] as int,
        statusLabel: json['statusLabel'] as String? ?? '',
        createdAt: DateTime.tryParse(json['createdAt'] as String? ?? '') ?? DateTime.now(),
        totalAmount: json['totalAmount'] as num? ?? 0,
        serviceType: json['serviceType'] as String?,
        items: (json['items'] as List<dynamic>? ?? [])
            .map((e) => OrderLineSummary.item(e as Map<String, dynamic>))
            .toList(),
        deals: (json['deals'] as List<dynamic>? ?? [])
            .map((e) => OrderLineSummary.deal(e as Map<String, dynamic>))
            .toList(),
      );

  int get itemCount =>
      items.fold(0, (s, i) => s + i.quantity) + deals.fold(0, (s, d) => s + d.quantity);

  /// Maps the backend's free-text status label onto the 4-step visual stepper.
  int get stepIndex {
    if (statusLabel.startsWith('Order received')) return 0;
    if (statusLabel.startsWith('Confirmed')) return 1;
    if (statusLabel == 'Completed') return 3;
    if (statusLabel == 'Cancelled') return -1;
    return 1;
  }
}
