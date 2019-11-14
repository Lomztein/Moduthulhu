FROM mcr.microsoft.com/dotnet/core/sdk:2.1

RUN dotnet publish Core.csproj -c Release -o /build/

ENTRYPOINT dotnet /build/Core.dll
