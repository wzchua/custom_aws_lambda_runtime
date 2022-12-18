FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
RUN apk add clang build-base zlib-dev

WORKDIR /repo
COPY . .

RUN dotnet restore src/AwsLambdaRuntimeR2R -r linux-musl-x64
RUN dotnet publish src/AwsLambdaRuntimeR2R -c release -r linux-musl-x64 -o /app --self-contained
RUN ls /app -lah

FROM alpine
RUN apk add --no-cache libstdc++
WORKDIR /app
COPY --from=build /app/  /var/task/
ENTRYPOINT ["/var/task/AwsLambdaRuntimeR2R"]