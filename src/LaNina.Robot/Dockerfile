FROM microsoft/dotnet-nightly:2.0-sdk-jessie AS build-env
WORKDIR /app

# define environment variables
ENV ENVIRONMENT=local

# copy csproj and restore as distinct layers
COPY nuget.config ./
COPY *.csproj ./
RUN dotnet restore

# copy everything else and build
COPY . ./
RUN dotnet publish -c Release -r linux-x64 -o out

# build runtime image
FROM microsoft/dotnet-nightly:2.0-runtime-deps-jessie

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        gnupg2 \
        apt-utils \
        uuid-runtime \
        wget \
        jq \
        ssh \
        bc \
        curl \
        libxml2-utils
        
RUN wget https://github.com/stedolan/jq/releases/download/jq-1.5/jq-linux64
RUN chmod +x jq-linux64
RUN mv jq-linux64 $(which jq)

# install azure cli packages
RUN echo "deb [arch=amd64] https://packages.microsoft.com/repos/azure-cli/ wheezy main" | \
         tee /etc/apt/sources.list.d/azure-cli.list

RUN apt-key adv --keyserver packages.microsoft.com --recv-keys 417A0893
RUN apt-get --assume-yes install apt-transport-https
RUN apt-get update && apt-get install azure-cli

RUN az --version

# SSH keys
RUN mkdir -p /root/.ssh
RUN ssh-keygen -q -t rsa -N '' -f /root/.ssh/id_rsa

WORKDIR /app
COPY --from=build-env /app/out ./
COPY Engine/Scripts ./
ENTRYPOINT ["./LaNina.Robot"]