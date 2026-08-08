import '../providers/order_type_provider.dart';
import 'api_client.dart';

class ServiceTimeSetting {
  final String serviceType;
  final int minMinutes;
  final int maxMinutes;
  const ServiceTimeSetting({
    required this.serviceType,
    required this.minMinutes,
    required this.maxMinutes,
  });

  factory ServiceTimeSetting.fromJson(Map<String, dynamic> json) => ServiceTimeSetting(
        serviceType: json['serviceType'] as String? ?? '',
        minMinutes: json['minMinutes'] as int? ?? 0,
        maxMinutes: json['maxMinutes'] as int? ?? 0,
      );

  String get label => '$minMinutes–$maxMinutes min';
}

class ServiceTimeService {
  static Future<List<ServiceTimeSetting>> getServiceTimes() async {
    final json = await ApiClient.get('/Public/service-times') as List<dynamic>;
    return json.map((e) => ServiceTimeSetting.fromJson(e as Map<String, dynamic>)).toList();
  }

  static ServiceTimeSetting? forType(List<ServiceTimeSetting> settings, ServiceType type) {
    for (final s in settings) {
      if (s.serviceType == type.apiValue) return s;
    }
    return null;
  }
}
