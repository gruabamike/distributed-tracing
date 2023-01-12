# distributed-tracing




# Local NuGet Feed
docker run --rm --name nuget-server -p 5555:80 --env-file baget.env -v "$(pwd)/baget-data:/var/baget" loicsharma/baget:latest

# Push NuGet Package and Symbols to BaGet
dotnet nuget push -k ${{NUGET-API-KEY}} -s http://localhost:5555/v3/index.json OpenTelemetry.Instrumentation.ActiveMQ.0.0.0-alpha.0.559.nupkg
dotnet nuget push -k ${{NUGET-API-KEY}} -s http://localhost:5555/v3/index.json OpenTelemetry.Instrumentation.ActiveMQ.0.0.0-alpha.0.559.snupkg
