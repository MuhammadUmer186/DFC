import 'package:flutter/foundation.dart';

enum ServiceType { dineIn, takeaway, delivery }

extension ServiceTypeApi on ServiceType {
  /// Matches the backend's ServiceType strings exactly (Order.ServiceType).
  String get apiValue => switch (this) {
        ServiceType.dineIn => 'DineIn',
        ServiceType.takeaway => 'Takeaway',
        ServiceType.delivery => 'Delivery',
      };

  String get label => switch (this) {
        ServiceType.dineIn => 'Dine-in',
        ServiceType.takeaway => 'Takeaway',
        ServiceType.delivery => 'Delivery',
      };
}

class OrderTypeProvider extends ChangeNotifier {
  ServiceType _serviceType = ServiceType.delivery;
  ServiceType get serviceType => _serviceType;

  void set(ServiceType type) {
    if (_serviceType == type) return;
    _serviceType = type;
    notifyListeners();
  }
}
