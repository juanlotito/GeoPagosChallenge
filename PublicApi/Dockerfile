# Utiliza una imagen base oficial, por ejemplo, para .NET:
FROM mcr.microsoft.com/dotnet/aspnet:6.0

# Establece el directorio de trabajo
WORKDIR /app

# Copia los archivos binarios precompilados al contenedor
COPY ./bin/Release/net6.0/publish/ .

# Expone el puerto en el que tu API estará escuchando
EXPOSE 5000

# Comando para ejecutar la aplicación
ENTRYPOINT ["dotnet", "PublicApi.dll"]