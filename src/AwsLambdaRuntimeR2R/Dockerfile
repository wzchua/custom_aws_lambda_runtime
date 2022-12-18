FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build

WORKDIR /repo
COPY . .

RUN dotnet restore src/AwsLambdaRuntimeR2R -r linux-musl-x64
RUN dotnet publish src/AwsLambdaRuntimeR2R -c release -r linux-musl-x64 -o /app --no-self-contained
RUN ls /app -lah

FROM mcr.microsoft.com/dotnet/runtime:7.0-alpine
WORKDIR /app
COPY --from=build /app/  /var/task/
ENTRYPOINT ["/var/task/AwsLambdaRuntimeR2R"]