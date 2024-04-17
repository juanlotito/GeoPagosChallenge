# Utiliza una imagen base de SDK para poder ejecutar los tests
FROM mcr.microsoft.com/dotnet/sdk:6.0

# Directorio de trabajo
WORKDIR /tests

# Copia los archivos del proyecto
COPY . .

# Restaura las dependencias y herramientas del proyecto
RUN dotnet restore

# Ejecuta los tests
CMD ["dotnet", "test"]
