FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ShareYourRecipe.csproj", "./"]
RUN dotnet restore "./ShareYourRecipe.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ShareYourRecipe.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShareYourRecipe.csproj" -c Release -o /app/publish

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShareYourRecipe.dll"]