# ChartTools
ChartTools is a .NET 8 library with the purpose of modeling song files for plastic guitar video games like Guitar Hero, Rock Band and Clone Hero. It currently supports reading of .chart and .ini files, with .mid support currently in development.

If you find any bugs, you can report them in the [Issues section](https://github.com/TheBoxyBear/ChartTools/issues) of the repository. Make sure to use the "bug" label.

As this project is in development, it should only be used with charts with a backup available. **I am not responsible for damages to charts!**

## Getting Started
For an overview on installation and taking your first steps with ChartTools, see [Articles](articles/getting-started.md). A GitHub Pages website is available with detailed articles and API documentation.

## Contributing
If you like to contribute to the development of ChartTools, feel free to comment on an issue, submit a pull request or submit your own issues. To test your code, create a project named `Debug` and it will be automatically excluded from commits.

### Documentation
The solution includes a `Docs` project that can be executed to build and deploy locally on port 8080. Remember to terminate the local server with `Ctrl+C` before closing as it can prevent later executions from using the port. If this occurs, run

```bash
netstat -aof | findstr :8080
taskkill /f /pid <PID>
```

where `PID` is the right-most ID in the output of `netstat`.

## License and Attribution
This project is licensed under the GNU General Public License 3.0. See [LICENSE](https://github.com/TheBoxyBear/charttools/blob/stable/LICENSE) for details.

This project makes use of one or more third-party libraries to aid in functionality, see [attribution.txt](https://github.com/TheBoxyBear/charttools/blob/stable/attribution.txt) for details.

## Special Thanks
- [FireFox](https://github.com/FireFox2000000) for making the Moonscraper editor open-source
- [TheNathannator](https://github.com/TheNathannator) for their direct contributions.
- [Matthew Sitton](https://github.com/mdsitton), lead developer of Clone Hero for sharing their in-depth knowledge and general programming wisdom.
- Members of the [Clone Hero Discord](https://discord.gg/clonehero) and [Moonscraper Discord](https://discord.gg/wdnD83APhE), including but not limited to DarkAngel2096, drumbs (TheNathannator), FireFox, Kanske, mdsitton, Spachi, and XEntombmentX for their help in researching.
