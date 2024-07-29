rm -f ./Adriva.DevTools.Cli/bin/Debug/*.nupkg
dotnet pack ./Adriva.DevTools.Cli/Adriva.DevTools.Cli.csproj
dotnet tool install --global --add-source ./Adriva.DevTools.Cli/bin/Debug/ Adriva.DevTools.Cli