dotnet pack -c Release -o ./Nuget
dotnet tool update --global gratisdns-cli --version 0.1.0 --add-source ./Nuget