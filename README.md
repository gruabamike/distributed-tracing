# Distributed Tracing with OpenTelemetry in .NET
## Introduction
// TODO

## Architecture
// TODO

## Tracing

## Setup & Deployment

### Prerequisites
### Setup Local NuGet Feed (BaGet via Docker)
https://loic-sharma.github.io/BaGet/installation/docker/

Make sure that you've installed nuget.exe
check via dotnet nuget
Download: https://www.nuget.org/downloads

Setup BaGet in any folder (e.g. C:\src\BaGet)

create baget.env
add folder baget-data
run docker command in same folder as .env file


docker run --rm --name nuget-server -p 5555:80 --env-file baget.env -v "$(pwd)/baget-data:/var/baget" loicsharma/baget:latest

### dotnet pack

### dotnet ef
dotnet ef can be installed as either a global or local tool. Most developers prefer installing dotnet ef as a global tool using the following command:
dotnet tool install --global dotnet-ef

Update the tool using the following command:
dotnet tool update --global dotnet-ef

### Publish OpenTelemetry.Instrumentation.ActiveMQ Packages
Open opentelemetry-dotnet-contrib solution
switch to branch

switch to src folder "OpenTelemetry.Instrumentation.ActiveMQ"
dotnet pack

navigate to specified output folder where the packages and symbols have been created

dotnet nuget push -k ${{NUGET-API-KEY}} -s http://localhost:5555/v3/index.json OpenTelemetry.Instrumentation.ActiveMQ.0.0.0-alpha.0.559.nupkg
dotnet nuget push -k ${{NUGET-API-KEY}} -s http://localhost:5555/v3/index.json OpenTelemetry.Instrumentation.ActiveMQ.0.0.0-alpha.0.559.snupkg

### Run DistributedTracing via Docker-Compose
navigate to root project folder
double check if the references nuget package version in the libraries matches the pushed ones
docker-compose up

initial database migration
Users.Api, Orders.Api
dotnet ef database update


example api call to make an order
POST /orders HTTP/1.1
Content-Type: application/json
User-Agent: PostmanRuntime/7.30.0
Accept: */*
Postman-Token: c0f043d2-dbad-45c8-832b-cf64667eebe3
Host: localhost:9001
Accept-Encoding: gzip, deflate, br
Connection: keep-alive
Content-Length: 136
 
{
  "userId": "00000000-0000-0000-0000-000000000001",
  "productId": "00000000-0000-0000-0000-000000000001",
  "quantity": 2
}


## Services
BaGet: http://localhost:5555
ActiveMQ: http://localhost:8161 (user and password: admin)
Zipkin: http://localhost:9411/zipkin
Prometheus: http://localhost:9090
Jaeger: http://localhost:16686