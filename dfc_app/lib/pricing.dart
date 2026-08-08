import 'providers/order_type_provider.dart';

const int kDeliveryFee = 60;
const int kPackagingFee = 20;

/// Delivery fee only applies to Delivery orders; packaging fee applies to
/// Delivery and Takeaway (food still needs a container), but not Dine-in
/// (served on real plates). Mirrors CustomerOrderingWeb's pricing.ts.
int deliveryFeeFor(ServiceType type) => type == ServiceType.delivery ? kDeliveryFee : 0;

int packagingFeeFor(ServiceType type) => type == ServiceType.dineIn ? 0 : kPackagingFee;
