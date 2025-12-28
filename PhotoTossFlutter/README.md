# PhotoToss Flutter

A Flutter/Dart recreation of the original PhotoToss Xamarin application. The goal is to deliver the same experience with modern tooling, plugins, and cross-platform support.

## Project layout
- `lib/` – app code organized by features (API services, models, UI pages, state management).
- `assets/` – bundled images/fonts as needed.
- `test/` – widget and unit tests.

## Getting started
1. Ensure Flutter 3.3+ is installed (`flutter --version`).
2. From this folder run:
   ```bash
   flutter pub get
   flutter run
   ```
3. Configure Firebase (Messaging/Analytics/Crashlytics) and Facebook credentials via the respective platform setup guides.

## Feature roadmap
- Implement REST client against the existing PhotoToss API.
- Recreate navigation with a drawer for Home, Browse, Profile, and About.
- Add camera/toss/catch flows with QR scanning, geolocation, and uploads.
- Wire push notifications and analytics.

## Suggested build order
If you are continuing from the scaffolded PR, ship these milestones in order:
1. **API client + models** – finish the REST wrapper, data parsing, and error handling so screens can load data.
2. **Drawer/navigation polish** – replace placeholder pages with real UI backed by Provider/Bloc state.
3. **Capture and toss flows** – integrate camera/gallery, QR scanning, geolocation, and upload progress.
4. **Notifications and analytics** – hook up Firebase Messaging + analytics/crash reporting and any Notification Hub bridge.
5. **UX hardening** – theming, fonts, empty/error states, and widget tests.

Each milestone can be a standalone PR to keep review size manageable; the scaffold PR just creates the workspace.

## Notes
This folder is self contained so it can be developed independently while referencing the original Xamarin code for behavior parity.
