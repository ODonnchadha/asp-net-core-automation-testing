#### Getting Started

You should set both the `Api` and the `WebApp` projects as startup projects. In Visual
Studio this can be done by "Configure Startup Projects", then choosing "Multiple startup projects",
and finally choosing the `Launch-UI-and-API` profile.

The solution uses two services that it expects to be running locally. The simplest way to
run these services is to use Docker.  The following commands will start the services.

```bash
docker pull rnwood/smpt4dev
docker run --rm -d --name fakemail -p 3000:80 -p 2525:25 rnwood/smtp4dev

docker pull datalust/seq
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq
```

Even without them running you should be able to run the solution.

To see logs, you can navigate to [http://localhost:5341](http://localhost:5341).

To see emails, you can navigate to [http://localhost:3000](http://localhost:3000).

#### Features

This is a simple e-commerce application that has a few features
that we want to explore automated testing strategies for.

Here are the features:

- **API**
  - `GET` based on category (or "all") and by id allow anonymous requests
  - `POST` requires authentication and an `admin` role
  - Validation will be done with [FluentValidation](https://docs.fluentvalidation.net/en/latest/index.html) and errors returned as a `400 Bad Request` with `ProblemDetails`
  - A `GET` with a category of something other than "all", "boots", "equip", or "kayak" will return a `500 internal server error` with `ProblemDetails`
  - Data is refreshed to a known state as the app starts
- Authentication provided by OIDC via the [demo Duende Identity Server](https://demo.duendesoftware.com)
- A custom claims transformer will add the `admin` role to "Bob Smith" and any authentication via Google
- **WebApp**
  - The home page and listing pages will show a subset of products
  - There is a page at `/Admin` that will show a list of products that can be edited or added to
  - If you navigate to `/Admin` without the admin role, you should see an `AccessDenied` page
  - Any validation errors from the API should be displayed on the admin section edit pages
  - Can add items to cart and see a summary of the cart (shows when empty too)
  - Can submit an order or cancel the order and clear the cart
  - A submitted order will send a fake email

"Catch" with a refactoring change:

- change the "admin" role to be "administrator" and see what breaks
(maybe make it a different claim type than role and change to a "policy"??)

For the learner:

- Add edit and (soft) delete to the API and WebApp, then write tests
- More complex "cart edit" functionality
- Be able to apply a "promotion" on the Cart page

#### VS Code Setup

RUnning in VS Code is a totally legitimate use-case for this solution and
repo.

The same instructions above (Getting Started) apply here, but the following
extension should probably be installed (it includes some other extensions):

- [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

Then run the API project and the UI project.

#### Data and EF Core Migrations

The `dotnet ef` tool is used to manage EF Core migrations.  The following command is used to create migrations (from the `CarvedRock.Data` folder).

```bash
dotnet ef migrations add Initial -s ../CarvedRock.Api
```

The initial setup for the application uses SQLite.
The data will be stored in a file called `carvedrock-sample.sqlite` as
defined in the API project's `appsettings.json` file.

The location of the file is in the "local AppData" folder (`Environment.SpecialFolder.LocalApplicationData`):

- Windows: `C:\Users\<username>\AppData\Local\`
- Mac: `/Users/USERNAME/.local/share`

To browse / query the data, you can use some handy extensions:

- In Visual Studio, use the [SQLite and SQL Server Compact Toolbox](https://marketplace.visualstudio.com/items?itemName=ErikEJ.SQLServerCompactSQLiteToolbox) extension
- In VS Code, use the [SQLite Viewer](https://marketplace.visualstudio.com/items?itemName=qwtel.sqlite-viewer) extension

#### Verifiying Emails

The very simple email functionality is done using a template
from [this GitHub repo](https://github.com/leemunroe/responsive-html-email-template)
and the [smtp4dev](https://github.com/rnwood/smtp4dev)
service that can easily run in a Docker container.

There is a UI that you can naviagte to in your browser for
seeing the emails that works great.  If you use the `docker run` command
that I have listed above, the UI is at
[http://localhost:3000](http://localhost:3000).

There is also an API that is part of that service and a couple of quick
API calls call give you the content of the email body that you
want to verify:

```bash
GET http://localhost:3000/api/messages

### find the ID of the message you care about

GET http://localhost:3000/api/messages/<message-guid>/html
```

#### Test Coverage 

To see test coverage for the inner loop tests you've written,
you can use the default [**coverlet**](https://github.com/coverlet-coverage/coverlet) tool that is included with
the `XUnit` test project template.

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Then use the [**reportgenerator**](https://github.com/danielpalme/ReportGenerator) global tool to generate an HTML report.  To install the tool
(you only need to do this once):

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
```

Then run the following command to generate the report:

```bash
reportgenerator -reports:".\Tests\**\TestResults\**\coverage.cobertura.xml" -targetdir:"coverage" -reporttypes:Html
```
To remove any contents from previous runs, use the
following command:

```bash
gci -include TestResults,coverage -recurse | remove-item -force -recurse
```

## AUTOMATION TESTING STRATEGIES WITH ASP.NET CORE by Erik Dahl

- OVERVIEW:
  - Great automated tests that can be run while developing an ASP.NET Core application enables quality, reliability, and even continuous delivery. This course will teach you how to create and execute such a set of tests using .NET technologies.
  - XUnit and WebApplicationFactory. Using real databases with the Testcontainers library.
  - End-to-end tests with Playwright in .NET. NBombner with load tests.
  - Automated pipelines with Azure DevOps and GitHub Actions.

- UNIT AND INTEGRATION TESTS:
  - "interesting" tests covered.
    - Inner Loop testing: Unit and inyegration tests. Test data management. Mocking APIs.
      - Run against source code.
    - End-to-End (E2e) Tests:
    - Performance tests.
    - Execution strategies.
  - Versioning: ASP.NET Core 8. Visual Studio 2022.
  - DEMO: Carved Rock Fitness eCommerce.
    - COnfiguration. Dependency Injection. EF Core for data access. Controller-based APIs.
      - Razor pages for UI. OIDC for sign-in. OAuth2 for API authentication. 
      - Claims transformation for authorization. FluentValidation for object validation.
      - MOTE: Container desktop app installed.
        - Install WSL. (Inclusing LINUX distribution.)
        - INstall a container desktop app. Docker. Rancher. Podman.
  - Test the API first:
    - Arrange. Act. Assert. Use mocking libraries. 
      - And then use [Theory] (xUnit concept) to avoid repetition.
      - Ongoing focus on *readibility.*
      - Limitations on unit tests.
  - NSubstitute: Provides mocks for repositories and databases.
    - Create an instance of the thing that you are going to mock, and then provide an expected behavior.
  - NOTE: `async Task` within unit test method to respect ValidateAsync() method.
  - Use Visual Studio's built-in test runner.
  - Handy trick? Inject an ITestOutputHelper into the tests. It's a part of xUnit abstractions.
  - Copy/paste versus [Fact()] and [Theory()]. 
    - Add [InlineData("", "")]. And update validations.
  - Generating bogus/fake test data. Library: Bogus for .NET: C#, F#, and VB.NET. `Bogus`
      ```csharp
        private readonly Faker _faker = new();
        _faker.Lorem.Letters(51);
      ```
  - Unit tests versus integration tests. Unit test can have some shortcomings.
    - The more you mock, the less meaningful the tests become.
    - Unit tests: Great for business logic, calculations, validations, more.
  - Regarding our API:
    - Most logic deals with response handling, database, and authentication/authorization.
  - Integration tests can be performed locally as well. 
    - Less mocking, but not none. And can check and validate API responses.
  - DEMO: Create and run an integration test.
    - Prerequisites:
      - Test project needs to reference: Microsoft.AspNetCore.Mvc.Testing
      - Test project should be a Web SDK project and not simply SDK.
        ```javascript
          <Project Sdk="Microsoft.NET.Sdk.Web">
        ```
        ```csharp
          public partial class Program { } // Integration testing.

          using Microsoft.AspNetCore.Mvc.Testing;
          using Xunit.Abstractions;
          public class ControllerTests(
            WebApplicationFactory<Program> factory, ITestOutputHelper helper) 
          : IClassFicture<WebApplicationFactory<Program>>
        ```
      - A *good* test ensures proper deserialization.
      - Test and validate both response status code and response payload.
      - Never trust a trust that you have not seen fail.