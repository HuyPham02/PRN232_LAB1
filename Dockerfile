# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY PRN232.LMS.Repositories/PRN232.LMS.Repositories.csproj PRN232.LMS.Repositories/
COPY PRN232.LMS.Services/PRN232.LMS.Services.csproj PRN232.LMS.Services/
COPY PRN232.LMS.API/PRN232.LMS.API.csproj PRN232.LMS.API/
RUN dotnet restore PRN232.LMS.API/PRN232.LMS.API.csproj

# Copy everything and build
COPY . .
RUN dotnet publish PRN232.LMS.API/PRN232.LMS.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PRN232.LMS.API.dll"]
