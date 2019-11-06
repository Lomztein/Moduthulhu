FROM mcr.microsoft.com/dotnet/core/sdk:2.1

COPY ./Core/ ./src/
RUN dotnet build ./src/Core.csproj -o /build/

ENTRYPOINT dotnet /build/Core.dll
