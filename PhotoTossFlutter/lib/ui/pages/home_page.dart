import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../services/api_client.dart';
import '../../state/app_state.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  final _api = ApiClient.instance;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final state = context.read<AppState>();
    state.setLoading(true);
    state.setError(null);
    try {
      final photos = await _api.fetchRecentPhotos();
      state.setRecentPhotos(photos);
    } catch (e) {
      state.setError(e.toString());
    } finally {
      state.setLoading(false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final state = context.watch<AppState>();
    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          if (state.errorMessage != null)
            Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Text(
                state.errorMessage!,
                style: const TextStyle(color: Colors.red),
              ),
            ),
          if (state.isLoading)
            const Center(child: CircularProgressIndicator()),
          for (final photo in state.recentPhotos)
            Card(
              clipBehavior: Clip.hardEdge,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  ListTile(
                    title: Text('Owner: ${photo.owner}'),
                    subtitle: Text('Catches: ${photo.catchCount}'),
                  ),
                  AspectRatio(
                    aspectRatio: 4 / 3,
                    child: Image.network(
                      photo.imageUrl,
                      fit: BoxFit.cover,
                      errorBuilder: (_, __, ___) => const ColoredBox(
                        color: Colors.black12,
                        child: Center(child: Icon(Icons.broken_image)),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          if (!state.isLoading && state.recentPhotos.isEmpty)
            const Center(child: Text('No photos yet.')),
        ],
      ),
    );
  }
}
