import 'package:flutter/material.dart';

import '../models/photo_record.dart';
import '../models/toss_record.dart';

class AppState extends ChangeNotifier {
  List<PhotoRecord> recentPhotos = const [];
  List<TossRecord> tossFeed = const [];
  bool isLoading = false;
  String? errorMessage;

  void setLoading(bool value) {
    isLoading = value;
    notifyListeners();
  }

  void setError(String? message) {
    errorMessage = message;
    notifyListeners();
  }

  void setRecentPhotos(List<PhotoRecord> photos) {
    recentPhotos = photos;
    notifyListeners();
  }

  void setTossFeed(List<TossRecord> tosses) {
    tossFeed = tosses;
    notifyListeners();
  }
}
