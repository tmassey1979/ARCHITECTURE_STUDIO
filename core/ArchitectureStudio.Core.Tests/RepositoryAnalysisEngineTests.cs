using System.Text;

namespace ArchitectureStudio.Core.Tests;

public sealed class RepositoryAnalysisEngineTests
{
    [Fact]
    public void Analyzer_detects_required_repository_signals_and_sensitive_data_with_auditable_output()
    {
        var workspacePath = CreateWorkspace(
            ("src/api/Architecture.Api.csproj", """
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
</Project>
"""),
            ("services/orders/pom.xml", """
<project>
  <dependencies>
    <dependency>
      <groupId>org.springframework.boot</groupId>
      <artifactId>spring-boot-starter-web</artifactId>
    </dependency>
  </dependencies>
</project>
"""),
            ("clients/react-app/package.json", """
{
  "name": "react-app",
  "dependencies": {
    "react": "^19.0.0"
  }
}
"""),
            ("clients/angular-app/package.json", """
{
  "name": "angular-app",
  "dependencies": {
    "@angular/core": "^19.0.0"
  }
}
"""),
            ("Dockerfile", """
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY . .
"""),
            ("deploy/app.yaml", """
apiVersion: apps/v1
kind: Deployment
metadata:
  name: architecture-studio
"""),
            (".github/workflows/ci.yml", """
name: ci
on: [push]
jobs:
  build:
    runs-on: ubuntu-latest
"""),
            ("Jenkinsfile", """
pipeline {
  agent any
  stages {
    stage('Build') {
      steps {
        echo 'build'
      }
    }
  }
}
"""),
            ("src/Domain/Orders/Order.cs", "namespace Domain.Orders; public sealed class Order { }"),
            ("src/Application/Orders/PlaceOrder.cs", "namespace Application.Orders; public sealed class PlaceOrder { }"),
            ("src/Infrastructure/Orders/SqlOrderRepository.cs", "namespace Infrastructure.Orders; public sealed class SqlOrderRepository { }"),
            ("src/Web/appsettings.json", """
{
  "customerEmail": "client@example.com",
  "creditCardNumber": "4111111111111111",
  "patientDiagnosis": "hypertension",
  "parentalConsentRequired": true
}
"""));

        try
        {
            var result = new RepositoryAnalysisEngine().Analyze(workspacePath);

            Assert.Contains(result.Signals, signal => signal.Id == "aspnet-core" && signal.Category == RepositorySignalCategory.Framework);
            Assert.Contains(result.Signals, signal => signal.Id == "spring-boot" && signal.Category == RepositorySignalCategory.Framework);
            Assert.Contains(result.Signals, signal => signal.Id == "react" && signal.Category == RepositorySignalCategory.Framework);
            Assert.Contains(result.Signals, signal => signal.Id == "angular" && signal.Category == RepositorySignalCategory.Framework);
            Assert.Contains(result.Signals, signal => signal.Id == "docker" && signal.Category == RepositorySignalCategory.Infrastructure);
            Assert.Contains(result.Signals, signal => signal.Id == "kubernetes" && signal.Category == RepositorySignalCategory.Infrastructure);
            Assert.Contains(result.Signals, signal => signal.Id == "github-actions" && signal.Category == RepositorySignalCategory.CiCd);
            Assert.Contains(result.Signals, signal => signal.Id == "jenkins" && signal.Category == RepositorySignalCategory.CiCd);
            Assert.Contains(result.Signals, signal => signal.Id == "clean-architecture" && signal.Category == RepositorySignalCategory.ArchitecturePattern);
            Assert.Contains(result.Signals, signal => signal.Id == "csharp" && signal.Category == RepositorySignalCategory.Language);
            Assert.Contains(result.Signals, signal => signal.Id == "java" && signal.Category == RepositorySignalCategory.Language);
            Assert.Contains(result.Signals, signal => signal.Id == "typescript" && signal.Category == RepositorySignalCategory.Language);

            Assert.Contains(result.SensitiveData, item => item.Category == SensitiveDataCategory.Personal);
            Assert.Contains(result.SensitiveData, item => item.Category == SensitiveDataCategory.Financial);
            Assert.Contains(result.SensitiveData, item => item.Category == SensitiveDataCategory.Health);
            Assert.Contains(result.SensitiveData, item => item.Category == SensitiveDataCategory.ChildData);

            Assert.All(result.Signals, static signal =>
            {
                Assert.True(signal.Confidence > 0);
                Assert.NotEmpty(signal.Evidence);
                Assert.NotEmpty(signal.AffectedPaths);
            });

            Assert.All(result.SensitiveData, static classification =>
            {
                Assert.True(classification.Confidence > 0);
                Assert.NotEmpty(classification.Evidence);
                Assert.NotEmpty(classification.AffectedPaths);
            });
        }
        finally
        {
            Directory.Delete(workspacePath, recursive: true);
        }
    }

    private static string CreateWorkspace(params (string RelativePath, string Content)[] files)
    {
        var workspacePath = Path.Combine(Path.GetTempPath(), "architecture-studio-analysis-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspacePath);

        foreach (var (relativePath, content) in files)
        {
            var fullPath = Path.Combine(workspacePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, content, Encoding.UTF8);
        }

        return workspacePath;
    }
}
