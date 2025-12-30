## API Core

Provides a flexbile core web API (ASP.NET) that pulls in additional API projects that contain a service (recommended) or other class that implements the ```IApiModule``` interface. Currently contains sub-APIs for document analysis and code analysis*. API key verification is performed for deployment environments with hashed keys stored in an attached or remote database through an appropriate implementation of ```IApiKeyStore``` added to app services at build.

Workflow templates allow deployment to Azure whereby the app can either be retained or automatically removed afterwards. Both workflows include endpoint testing.

*See [DocAnalysis](https://github.com/zerihal/DocAnalysis) and [DependencyAnalyser](https://github.com/zerihal/DependencyAnalyser) repos for details of sub-APIs.
