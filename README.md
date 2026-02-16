DNG is a small, command-line games launcher and monitor. It provides a console UI for browsing and launching games, and it can watch folders you configure and automatically add new games when they appear.

**Features**

- **Auto-monitor**: Watches configured folder paths and automatically adds newly discovered games.
- **Console UI**: Browse your game list, edit entries, and launch games entirely from the terminal.
- **Configurable**: Monitor paths and other settings are stored locally so the launcher remembers your setup.

**Quick Start**

- Run the launcher executable on your system. The launcher presents a simple menu-driven console UI.
- Open the **Paths to Monitor** screen to add one or more folders the launcher should watch. The launcher will scan those folders and add games it finds.

**Usage**

- **Browse games**: Use the Games screen to see installed and discovered games. Select a game to launch it.
- **Add monitor path**: Use the `Paths to Monitor` screen in the UI to add or remove folders to watch.
- **Manual edits**: If you prefer editing by hand, configuration is stored in %appdata% folder. The file is called DNG-config.json

**How games are discovered**

- The launcher will scan monitored folders and present a list of top level directories. If no executable is selected, first launch will ask to select an executable. Subsequent launches will use the previously selected executable.

**Privacy & Data**

- All configuration and logs are kept locally under the application folders. The launcher does not transmit data externally.

**Tips**

- Keep your game folders organized (one game per folder) to improve automatic detection accuracy.
- If you move or rename a game folder, you will have to select a new executable for that game, as the launcher will see this as a new game.

**Need help or want changes?**

- Feel free to open a pull request or log issues.
