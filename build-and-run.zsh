#!/bin/zsh

docker-compose -f docker-compose-camunda.yaml up -d
docker-compose -f docker-compose-infra.yaml up -d
docker-compose -f docker-compose.yaml up