FROM mcr.microsoft.com/dotnet/sdk:6.0

WORKDIR /tests

COPY . .

RUN dotnet restore

CMD ["dotnet", "test"]
