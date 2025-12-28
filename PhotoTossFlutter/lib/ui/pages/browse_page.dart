import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../services/api_client.dart';
import '../../state/app_state.dart';

class BrowsePage extends StatefulWidget {
  const BrowsePage({super.key});

  @override
  State<BrowsePage> createState() => _BrowsePageState();
}

class _BrowsePageState extends State<BrowsePage> {
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
      if (state.recentPhotos.isNotEmpty) {
        final tosses = await _api.fetchTossesForPhoto(state.recentPhotos.first.id);
        state.setTossFeed(tosses);
      }
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
          for (final toss in state.tossFeed)
            ListTile(
              leading: const Icon(Icons.near_me_outlined),
              title: Text('Catch by ${toss.catcher}'),
              subtitle: Text(toss.caughtAt.toIso8601String()),
            ),
          if (!state.isLoading && state.tossFeed.isEmpty)
            const Center(
              child: Text('No toss activity yet. Pull to refresh.'),
            ),
        ],
      ),
    );
  }
}
