/// Points at the RMS-DFC backend (Backend/Properties/launchSettings.json
/// binds http://0.0.0.0:7122, so it's reachable on the LAN at the host
/// machine's IP). Update this when the backend's address changes:
///   - Android emulator talking to a backend on the same PC: 10.0.2.2
///   - Physical device on the same WiFi as the backend PC: the PC's LAN IP
///   - Production: the deployed backend's public URL
class ApiConfig {
  static const String apiHub = 'http://192.168.100.250:7122';
  static const String baseUrl = '$apiHub/api';

  /// Backend image paths (e.g. MenuItem.ImageUrl) are relative to apiHub.
  static String? imageUrl(String? path) {
    if (path == null || path.isEmpty) return null;
    return '$apiHub$path';
  }
}
