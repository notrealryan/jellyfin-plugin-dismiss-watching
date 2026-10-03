# Dismiss Watching

Dismiss items from Jellyfin's **Continue Watching** and **Next Up** rows without resetting their playback progress.

Each user's dismissals are private. Starting playback again removes the item from that user's denylist.

## Requirements

- Jellyfin 12.0 or newer
- [Jellyfin JavaScript Injector](https://github.com/n00bcodr/Jellyfin-JavaScript-Injector)

The buttons and client-side filtering work in Jellyfin Web. They may also work in official apps that use Jellyfin Web, but they are not supported in native or third-party clients.

## Installation

1. In Jellyfin, open **Dashboard** → **Plugins** → **Catalog** → **Settings**.
2. Add this repository URL:

   ```text
   https://raw.githubusercontent.com/notrealryan/jellyfin-plugin-dismiss-watching/main/manifest.json
   ```

3. Install **Dismiss Watching** from the catalog.
4. Restart Jellyfin, then enable the plugin under **My Plugins**.
5. Hard refresh Jellyfin Web (`Ctrl` + `Shift` + `R`).

Hover the upper-right corner of a Continue Watching or Next Up card to reveal its × button.

## What it does

- **Continue Watching:** hides the selected item while retaining playback progress.
- **Next Up:** hides all currently listed episodes for the selected show.
- **Per-user lists:** each Jellyfin account has its own dismiss list, managed from the plugin's **User Lists** page.
<img width="795" height="302" alt="image" src="https://github.com/user-attachments/assets/963fd16e-862b-42fb-8601-d25232a8337b" />
<img width="1461" height="319" alt="image" src="https://github.com/user-attachments/assets/bd355913-4383-47b9-8789-f9d201c559c9" />


## Limitations

- The dismiss buttons are injected into the web interface. Native and third-party clients do not receive them.
- Next Up is filtered in the web interface only; its server API is not overridden.
- The **JavaScript Injector** plugin must be installed and enabled before Dismiss Watching starts.

## API

Authenticated clients can use the following endpoints:

```text
GET    /DismissWatching/Items
POST   /DismissWatching/Items/{itemId}
DELETE /DismissWatching/Items/{itemId}
GET    /DismissWatching/Config
```

The original Continue Watching server-side override endpoints remain available for advanced reverse-proxy setups. Most users do not need them.

## Development

```text
make build
```

Releases are built through GitHub Actions. Run **Build and Release Plugin** with a semantic version such as `1.0.1`.

## Credits and license

This project is derived from [jon4hz's Discontinue Watching plugin](https://github.com/jon4hz/jellyfin-plugin-discontinue-watching), with prior work from [Razdnut's fork](https://github.com/Razdnut/jellyfin-plugin-discontinue-watching). It is licensed under [GPL-3.0](LICENSE).
