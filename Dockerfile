FROM mcr.microsoft.com/dotnet/core/sdk:2.1

COPY ./Core/ ./Core/
COPY ./Plugins/ ./Plugins/

RUN dotnet publish -c Release -o /build/ ./Core/Core.csproj 
RUN dotnet publish -c Release -o /plugins/ ./Plugins/Plugins.csproj

RUN mkdir -p /build/Data/Plugins/

RUN cp -v /plugins/Plugins.dll /build/Data/Plugins/Plugins.dll

ENTRYPOINT dotnet /build/Core.dll
