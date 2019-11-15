FROM mcr.microsoft.com/dotnet/core/sdk:2.1

RUN dotnet publish ./Core/Core.csproj -c Release -o /build/
RUN dotnet publish ./Plugin/Plugins.csproj -c Release -o /plugins/

RUN mkdir /build/Data/
RUN mkdir /build/Data/Plugins/

RUN cp /plugins/Plugins.dll /build/Data/Plugins/

ENTRYPOINT dotnet /build/Core.dll
