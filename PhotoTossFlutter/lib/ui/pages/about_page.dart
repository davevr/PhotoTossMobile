import 'package:flutter/material.dart';

class AboutPage extends StatelessWidget {
  const AboutPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: const [
          Text(
            'About PhotoToss',
            style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
          ),
          SizedBox(height: 12),
          Text('Rebuilt with Flutter and Dart for modern cross-platform support.'),
          SizedBox(height: 8),
          Text('QR tossing, catching, and image lineage features coming soon.'),
        ],
      ),
    );
  }
}
