import 'package:flutter/material.dart';

import 'ui/pages/about_page.dart';
import 'ui/pages/browse_page.dart';
import 'ui/pages/home_page.dart';
import 'ui/pages/profile_page.dart';

class PhotoTossApp extends StatelessWidget {
  const PhotoTossApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'PhotoToss',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.teal),
        useMaterial3: true,
      ),
      home: const _RootScaffold(),
    );
  }
}

class _RootScaffold extends StatefulWidget {
  const _RootScaffold();

  @override
  State<_RootScaffold> createState() => _RootScaffoldState();
}

class _RootScaffoldState extends State<_RootScaffold> {
  int _selectedIndex = 0;

  final _pages = const [
    HomePage(),
    BrowsePage(),
    ProfilePage(),
    AboutPage(),
  ];

  final _titles = const [
    'Home',
    'Browse',
    'Profile',
    'About',
  ];

  void _onSelect(int index) {
    setState(() => _selectedIndex = index);
    Navigator.pop(context);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(_titles[_selectedIndex])),
      drawer: Drawer(
        child: ListView(
          padding: EdgeInsets.zero,
          children: [
            const DrawerHeader(
              decoration: BoxDecoration(color: Colors.teal),
              child: Align(
                alignment: Alignment.bottomLeft,
                child: Text(
                  'PhotoToss',
                  style: TextStyle(color: Colors.white, fontSize: 24),
                ),
              ),
            ),
            for (var i = 0; i < _titles.length; i++)
              ListTile(
                leading: Icon(_iconForIndex(i)),
                title: Text(_titles[i]),
                selected: _selectedIndex == i,
                onTap: () => _onSelect(i),
              ),
          ],
        ),
      ),
      body: _pages[_selectedIndex],
    );
  }

  IconData _iconForIndex(int index) {
    switch (index) {
      case 0:
        return Icons.home;
      case 1:
        return Icons.leaderboard;
      case 2:
        return Icons.person;
      case 3:
        return Icons.info_outline;
      default:
        return Icons.home;
    }
  }
}
