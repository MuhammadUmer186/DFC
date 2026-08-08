import '../models/order.dart';
import 'api_client.dart';

class OrderService {
  static Future<PlaceOrderResponse> placeOrder(PlaceOrderRequest request) async {
    final json = await ApiClient.post('/Public/order', request.toJson()) as Map<String, dynamic>;
    return PlaceOrderResponse.fromJson(json);
  }

  static Future<OrderStatusResult> getOrderStatus(int orderId, String phone) async {
    final json = await ApiClient.get('/Public/order/$orderId/status', query: {'phone': phone})
        as Map<String, dynamic>;
    return OrderStatusResult.fromJson(json);
  }
}
