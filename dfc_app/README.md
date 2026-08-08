# 🍟 DFC — Data Finger Chips (Flutter App)

**Crispy. Fresh. Delivered.**

Flutter customer ordering app, wired up to the real RMS-DFC backend (`../Backend`) — same guest-checkout API (`api/Public/...`) that `Frontend/CustomerOrderingWeb` uses. No login: customers just give their name and phone at checkout, and track any order later with its order number + phone.

## Screens
| Screen | File |
|---|---|
| Splash (dark navy, logo, tagline) | `lib/screens/splash_screen.dart` |
| Home (categories, popular items, live deal banner) | `lib/screens/home_screen.dart` |
| Explore Menu (search, category/deal tabs, grid) | `lib/screens/menu_screen.dart` |
| Product Detail (real price, quantity, Add to Cart) | `lib/screens/product_detail_screen.dart` |
| Cart (qty steppers, live fee breakdown, totals) | `lib/screens/cart_screen.dart` |
| Checkout (DineIn/Takeaway/Delivery, address + GPS, Cash/Online transfer) | `lib/screens/checkout_screen.dart` |
| Track Order (real status, order number + phone) | `lib/screens/track_order_screen.dart` |
| Orders + Profile (locally-remembered order history) | `lib/screens/orders_screen.dart`, `profile_screen.dart` |

## 1. Point it at your backend

Edit `lib/config/api_config.dart`:
```dart
static const String apiHub = 'http://192.168.100.250:7122'; // <- your backend's LAN IP
```
- **Physical phone on the same WiFi as your PC** (default here): use the PC's LAN IP (`ipconfig` / `Get-NetIPAddress`), e.g. `192.168.100.250`.
- **Android emulator**: use `10.0.2.2` — the emulator's alias for the host machine's `localhost`.
- **Production**: point it at the deployed backend's public URL.

The backend must be running (`cd ../Backend && dotnet run --launch-profile https`, listens on `http://0.0.0.0:7122`) and reachable from your phone/emulator.

## 2. Run it

```bash
flutter create . --platforms=android,ios   # first time only — generates android/ ios/
flutter pub get
flutter run
```

### Android setup (required — one-time)
Android blocks plaintext HTTP by default, which would silently break every API call:
- In `android/app/src/main/AndroidManifest.xml`, add `android:usesCleartextTraffic="true"` to the `<application>` tag (or scope it to your backend's IP via a `network_security_config.xml`).
- Add location permissions for the checkout screen's "Use my location" button:
  ```xml
  <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION"/>
  <uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION"/>
  ```

### iOS setup (required — one-time)
- `ios/Runner/Info.plist`: add `NSLocationWhenInUseUsageDescription` (for "Use my location") and an `NSAppTransportSecurity` cleartext exception for your backend's IP.

## 3. Replace placeholder images

Menu item / deal photos come from the backend (`MenuItem.ImageUrl` / `Deal.ImageUrl`, set via the RMS admin panel) — upload them there, not in this app. `assets/images/` is only used for the app's own branding (`logo.png`).

## 4. Where things live
- Backend base URL → `lib/config/api_config.dart`
- Live menu/deals (fetched from `GET /api/Public/menu`) → `lib/providers/menu_provider.dart`, `lib/services/menu_service.dart`
- Cart (persisted locally) → `lib/providers/cart_provider.dart`
- Placing/tracking orders (`POST /api/Public/order`, `GET /api/Public/order/{id}/status`) → `lib/services/order_service.dart`
- Delivery/packaging fee rules → `lib/pricing.dart`
- Brand colors/theme → `lib/theme/app_theme.dart`

## 5. Golden path to test end-to-end
1. Run the Backend and confirm it's listening on `0.0.0.0:7122`.
2. `flutter run` on a phone on the same WiFi.
3. Browse the menu (real items from the DB) → add to cart → checkout with a name/phone/address → Place Order.
4. In RMS, open **Online Orders** — the order should appear as *Pending Approval*.
5. Approve it in RMS, then pull-to-refresh the app's Track Order screen — the status should update.
