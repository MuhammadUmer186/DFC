import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:dfc_app/main.dart';

void main() {
  testWidgets('DfcApp builds without crashing', (WidgetTester tester) async {
    await tester.pumpWidget(const DfcApp());
    expect(find.byType(MaterialApp), findsOneWidget);
  });
}
