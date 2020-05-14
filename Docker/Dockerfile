# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY ./ElasticsearchFulltextExample.Web/*.csproj ./ElasticsearchFulltextExample.Web/
RUN dotnet restore

# copy everything else and build app
COPY ElasticsearchFulltextExample.Web/. ./ElasticsearchFulltextExample.Web/
WORKDIR /source/ElasticsearchFulltextExample.Web
RUN dotnet publish -c release -o /app --no-restore

# Build the final image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-focal

# Install Elasticsearch and Tesseract:
RUN  apt-get update && apt-get install -y \
    wget \
    apt-transport-https

RUN wget -qO - https://artifacts.elastic.co/GPG-KEY-elasticsearch | sudo apt-key add -
RUN echo "deb https://artifacts.elastic.co/packages/oss-7.x/apt stable main" | tee  /etc/apt/sources.list.d/elastic-7.x.list

# Install Tesseract:
RUN apt-get update && apt-get install -y \
    openjdk-11-jre \ 
    tesseract-ocr \
    tesseract-ocr-eng \
    elasticsearch-oss

# Copy Artifacts from Build:
WORKDIR /app
COPY --from=build /app ./

# And kick off the Server:
ENTRYPOINT ["dotnet", "ElasticsearchFulltextExample.Web.dll"]