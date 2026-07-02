# Easy Minecraft Serverr

A modern WPF desktop application that makes running and managing Minecraft servers easier. Set up servers, manage versions, allocate resources, and monitor performance—all from a single intuitive interface.

## Features

✨ **Easy Setup**
- One-click server creation with guided configuration
- Automatic server JAR downloading for Vanilla, Paper, and Fabric
- Smart default settings that just work

🎮 **Server Management**
- Manage multiple servers from one window
- Start/stop servers with visual feedback
- Real-time console output with command input
- Monitor server status and performance metrics

⚙️ **Configuration**
- Adjust RAM allocation (min/max) for each server
- Choose Minecraft version and server software
- Configure server port with validation
- Per-server profile storage

🔧 **Plugin & Mod Support**
- Install plugins on Paper servers
- Install mods on Fabric servers
- Manage addon files with a simple interface
- Open addon folders directly from the app

📊 **Performance Monitoring**
- Real-time memory and CPU usage tracking
- Visual performance statistics
- Server uptime and status indicators

## System Requirements

- **Windows** 10 or later
- **.NET Framework** 4.7.2 or later
- **Java** 8 or later (JDK or JRE)
- Minimum **2GB RAM** (4GB+ recommended for running servers)

## Installation

1. Download the latest release from [Releases](https://github.com/OblitR8/Easy-Minecraft-Serverr/releases)
2. Extract the ZIP file to a folder
3. Run `Easy Minecraft Serverr.exe`
4. Create your first server and enjoy!

## Usage

### Creating a Server

1. Click **+ Add Server** in the sidebar
2. Select your preferred server software (Vanilla, Paper, or Fabric)
3. Choose a Minecraft version
4. Adjust RAM allocation
5. Click **Start** to begin!

### Managing Servers

**Console Tab:**
- View real-time server logs
- Type commands directly into the input box
- Press Enter or click Send to execute
- Monitor server startup and operation

**Settings Tab:**
- Change server software or version
- Adjust memory limits
- Modify server port
- Changes take effect on next restart

**Plugins/Mods Tab** (Paper/Fabric only):
- Install .jar files by clicking **+ Add .jar**
- Remove unwanted addons
- Open the addon folder to manage files manually
- Server restart required for changes

## Troubleshooting

### "Couldn't launch Java" error
- Ensure Java is installed: `java -version` in Command Prompt
- Add Java to your PATH environment variable
- Restart the application

### Server won't start
- Check the console for error messages
- Verify you have sufficient disk space
- Try increasing allocated RAM or reducing minimum RAM
- Check if the port is already in use

### Slow performance
- Reduce server max RAM allocation
- Check your system's available resources
- Reduce player slots in server settings
- Disable unnecessary plugins/mods

## Configuration Files

Server profiles and logs are stored in:
```
C:\Users\YourUsername\AppData\Roaming\EasyMinecraftServerr\
```

Each server's files are stored in the path you specify during creation.

## Supported Server Software

- **Vanilla** – Official Minecraft server
- **Paper** – Popular server with plugin support
- **Fabric** – Modern modding platform

More server types may be added in future versions.

## Error Handling & Logging

The application logs all errors to:
```
C:\Users\YourUsername\AppData\Roaming\EasyMinecraftServerr\logs\
```

Check these logs if you encounter issues:
- `application.log` – General application events
- `server-{name}.log` – Per-server operation logs

## License

MIT License – See [LICENSE](LICENSE) for details.

## Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to your fork
5. Open a Pull Request

## Support

For issues, questions, or suggestions, please open an [Issue](https://github.com/OblitR8/Easy-Minecraft-Serverr/issues).
