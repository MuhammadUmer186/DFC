enum CartLineKind { item, deal }

/// A single cart row: either a real MenuItem or a Deal, referenced by its
/// real backend id (menuItemId or dealId) — mirrors CustomerOrderingWeb's
/// CartLine model so the checkout payload maps 1:1 onto PublicOrderRequest.
class CartLine {
  final CartLineKind kind;
  final int id;
  final String name;
  final num price;
  final String? imageUrl;
  int quantity;

  CartLine({
    required this.kind,
    required this.id,
    required this.name,
    required this.price,
    this.imageUrl,
    this.quantity = 1,
  });

  num get total => price * quantity;

  Map<String, dynamic> toJson() => {
        'kind': kind.name,
        'id': id,
        'name': name,
        'price': price,
        'imageUrl': imageUrl,
        'quantity': quantity,
      };

  factory CartLine.fromJson(Map<String, dynamic> json) => CartLine(
        kind: json['kind'] == 'deal' ? CartLineKind.deal : CartLineKind.item,
        id: json['id'] as int,
        name: json['name'] as String? ?? '',
        price: json['price'] as num? ?? 0,
        imageUrl: json['imageUrl'] as String?,
        quantity: json['quantity'] as int? ?? 1,
      );
}
