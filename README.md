This repository aims to provide useful resources related to the [nyzo cryptocurrency](https://tech.nyzo.org).




This repository is structured with a flat top-level folder structure:
- scripts
  - contains standalone scripts which may or may not rely on data from the static folder, bash scripts do not parse arguments passed to it but are expected to be entered as variables within the script - the user is expected to inspect the code while doing so
- dotnet
  - Nyzo.CL
    - a nyzo class library project
      - based on the (micropay oriented) [nyzo chrome extension](https://github.com/construct0/nyzoChromeExtension) javascript code
      - **listed: yes, [nuget.org/packages/Nyzo.CL](https://www.nuget.org/packages/Nyzo.CL/)** 
  - Nyzo.CL.Tests 
    - the test project of Nyzo.CL.Tests
      - to manually run the tests, clone the repository & open the solution (.sln) file in the Nyzo.CL project, your IDE will automatically link it to the Nyzo.CL.Tests project
      - to validate test results on github, navigate to the Actions page
        - this does not provide finegrained insights into what-is and what-is-not tested
      - the version in [the .csproj file](https://github.com/construct0/nyzoResources/blob/main/dotnet.Nyzo.CL.Tests/Nyzo.CL.Tests.csproj) indicates what [version of the Nyzo.CL project](https://github.com/construct0/nyzoResources/blob/main/dotnet.Nyzo.CL/Nyzo.CL.csproj) it belongs to
      - - **listed: excluded from the package**    
- npm
  - Nyzo.CL
    - aims to be the equivalent of dotnet.Nyzo.CL
    - **listed: not yet**
  - Nyzo.CL.Tests
    - aims to be the equivalent of dotnet.Nyzo.CL.Tests
    - **listed: excluded from the package**
- static
  - contains data




### Versioning

Any and all projects within this repository are **not production ready until major version 1.0.0 or higher**.

- dotnet.Nyzo.CL
  - the API will likely change a lot, depending on the needs of construct0, API changes are always accompanied by a new major version
- npm.Nyzo.CL
  - has lower priority than dotnet.Nyzo.CL   
