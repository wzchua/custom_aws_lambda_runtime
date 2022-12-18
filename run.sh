#!/usr/bin/env bash

docker run -v ~/.aws-lambda-rie:/aws-lambda -e WRITE_INDENTED_JSON=true -p 9000:8080 --entrypoint /aws-lambda/aws-lambda-rie  test:latest  /var/task/AwsLambdaRuntimeNative 