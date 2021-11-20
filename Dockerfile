FROM mcr.microsoft.com/dotnet/sdk:6.0

WORKDIR /app

COPY src src

RUN dotnet build -o ./build ./src

WORKDIR /app/build
CMD dotnet ./AnarchyBot.dll