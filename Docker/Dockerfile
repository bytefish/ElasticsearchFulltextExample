# Get the dotnet Build Environment:
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /source

# COPY Project Files:
COPY *.sln .
COPY ./ElasticsearchFulltextExample.Web/*.csproj ./ElasticsearchFulltextExample.Web/

# And restore the NuGet Packages:
RUN dotnet restore

# COPY 
COPY ElasticsearchFulltextExample.Web/. ./ElasticsearchFulltextExample.Web/
WORKDIR /source/ElasticsearchFulltextExample.Web
RUN dotnet publish -c release -o /app --no-restore

# Build the final image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-focal

# Install Tesseract:
RUN apt-get update && apt-get install -y \
    tesseract-ocr \
    tesseract-ocr-eng

# Copy Artifacts from Build:
WORKDIR /app
COPY --from=build /app ./

# And kick off the Server:
ENTRYPOINT ["dotnet", "ElasticsearchFulltextExample.Web.dll"]