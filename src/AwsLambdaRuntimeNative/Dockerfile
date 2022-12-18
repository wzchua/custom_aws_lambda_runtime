FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
RUN apk add clang build-base zlib-dev

WORKDIR /repo
COPY . .

RUN dotnet restore src/AwsLambdaRuntimeNative -r linux-musl-x64
RUN dotnet publish src/AwsLambdaRuntimeNative -c release -r linux-musl-x64 -o /app
RUN ls /app -lah

FROM alpine
RUN apk add --no-cache libstdc++
WORKDIR /app
COPY --from=build /app/AwsLambdaRuntimeNative  /var/task/AwsLambdaRuntimeNative
ENTRYPOINT ["/var/task/AwsLambdaRuntimeNative"]