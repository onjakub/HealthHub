### Example ElicitationHandler Implementation (C#)

Source: https://modelcontextprotocol.github.io/csharp-sdk/concepts/elicitation/elicitation

This C# code provides an example implementation for an ElicitationHandler. It handles elicitation requests by prompting the user for input based on a provided schema and returning the collected data.

```csharp
async ValueTask<ElicitResult> HandleElicitationAsync(ElicitRequestParams? requestParams, CancellationToken token)
{
    // Bail out if the requestParams is null or if the requested schema has no properties
    if (requestParams?.RequestedSchema?.Properties == null)
    {
        return new ElicitResult();
    }

    // Process the elicitation request
    if (requestParams?.Message is not null)
    {
        Console.WriteLine(requestParams.Message);
    }

    var content = new Dictionary<string, JsonElement>();

    // Loop through requestParams.requestSchema.Properties dictionary requesting values for each property
    foreach (var property in requestParams.RequestedSchema.Properties)
    {
        if (property.Value is ElicitRequestParams.BooleanSchema booleanSchema)
        {
            Console.Write($"{booleanSchema.Description}: ");
            var clientInput = Console.ReadLine();
            bool parsedBool;

            // Try standard boolean parsing first
            if (bool.TryParse(clientInput, out parsedBool))
            {
                content[property.Key] = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(parsedBool));
            }
            // Also accept "yes"/"no" as valid boolean inputs
            else if (string.Equals(clientInput?.Trim(), "yes", StringComparison.OrdinalIgnoreCase))
            {
                content[property.Key] = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(true));
            }
            else if (string.Equals(clientInput?.Trim(), "no", StringComparison.OrdinalIgnoreCase))
            {
                content[property.Key] = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(false));
            }
        }
        else if (property.Value is ElicitRequestParams.NumberSchema numberSchema)
        {
            Console.Write($"{numberSchema.Description}: ");
            var clientInput = Console.ReadLine();
            double parsedNumber;
            if (double.TryParse(clientInput, out parsedNumber))
            {
                content[property.Key] = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(parsedNumber));
            }
        }
        else if (property.Value is ElicitRequestParams.StringSchema stringSchema)
        {
            Console.Write($"{stringSchema.Description}: ");
            var clientInput = Console.ReadLine();
            content[property.Key] = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(clientInput));
        }
    }

    // Return the user's input
    return new ElicitResult
    {
        Action = "accept",
        Content = content
    };
}
```

--------------------------------

### ConfigureSessionOptions Property

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.AspNetCore

An optional asynchronous callback to configure per-session McpServerOptions. It provides access to the HttpContext of the request that initiated the session, allowing for customized session setup.

```csharp
public Func<HttpContext, McpServerOptions, CancellationToken, Task>? ConfigureSessionOptions { get; set; }
```

--------------------------------

### RunSessionHandler Property

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.AspNetCore

An optional asynchronous callback for manually running new MCP sessions. This allows for executing custom logic before a session starts and after it completes.

```csharp
public Func<HttpContext, IMcpServer, CancellationToken, Task>? RunSessionHandler { get; set; }
```

--------------------------------

### ResourceMetadata Property

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.AspNetCore.Authentication

Gets or sets the protected resource metadata for the current request. This property allows setting or retrieving metadata associated with the resource being accessed.

```APIDOC
## ResourceMetadata Property

### Description
Gets or sets the protected resource metadata for the current request.

### Method
Property Access (Get/Set)

### Endpoint
N/A (Class Property)

### Parameters
#### Path Parameters
None

#### Query Parameters
None

#### Request Body
None

### Request Example
```csharp
// Assuming 'requestContext' is an instance of ResourceMetadataRequestContext

// Setting the ResourceMetadata
var metadata = new ProtectedResourceMetadata { /* ... initialize metadata properties ... */ };
requestContext.ResourceMetadata = metadata;

// Getting the ResourceMetadata
ProtectedResourceMetadata currentMetadata = requestContext.ResourceMetadata;
```

### Response
#### Success Response (N/A)
Property access does not produce a distinct API response.

#### Response Example
N/A
```

--------------------------------

### ResourceMetadata Property (C#)

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.AspNetCore.Authentication

Gets or sets the protected resource metadata for the current request. This property allows for the retrieval and modification of metadata associated with the resource being accessed.

```csharp
public ProtectedResourceMetadata? ResourceMetadata { get; set; }
```

--------------------------------

### WithResources (Type)

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds `McpServerResource` instances by discovering methods attributed with `McpServerResourceAttribute` from provided types.

```APIDOC
## POST /api/resources/types

### Description
Adds McpServerResource instances to the service collection backing `builder` by discovering types with marked methods.

### Method
POST

### Endpoint
/api/resources/types

### Parameters
#### Path Parameters
N/A

#### Query Parameters
N/A

#### Request Body
- **resourceTemplateTypes** (IEnumerable<Type>) - Required - Types with marked methods to add as resources to the server.

### Request Example
```json
[
  "Namespace.Resource1Type, AssemblyName",
  "Namespace.Resource2Type, AssemblyName"
]
```

### Response
#### Success Response (200)
- **builder** (IMcpServerBuilder) - The builder instance provided, now configured with the discovered resources.

#### Response Example
(Returns the builder instance, no specific JSON body)

#### Errors
- **ArgumentNullException** - If `builder` is null.
- **ArgumentNullException** - If `resourceTemplateTypes` is null.
```

--------------------------------

### WithResources (McpServerResource)

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds specific `McpServerResource` instances to the service collection backing the builder.

```APIDOC
## POST /api/resources

### Description
Adds McpServerResource instances to the service collection backing `builder`.

### Method
POST

### Endpoint
/api/resources

### Parameters
#### Path Parameters
N/A

#### Query Parameters
N/A

#### Request Body
- **resourceTemplates** (IEnumerable<McpServerResource>) - Required - The McpServerResource instances to add to the server.

### Request Example
```json
[
  {
    "name": "resource1",
    "path": "/path/to/resource1"
  },
  {
    "name": "resource2",
    "path": "/path/to/resource2"
  }
]
```

### Response
#### Success Response (200)
- **builder** (IMcpServerBuilder) - The builder instance provided, now configured with the resources.

#### Response Example
(Returns the builder instance, no specific JSON body)

#### Errors
- **ArgumentNullException** - If `builder` is null.
- **ArgumentNullException** - If `resourceTemplates` is null.
```

--------------------------------

### Registering Resources via Reflection

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

This method scans the specified assembly for classes marked with McpServerResourceTypeAttribute and their members marked with McpServerResourceAttribute, registering them with the service collection. It supports both static and instance members.

```APIDOC
## GET /websites/modelcontextprotocol_github_io-csharp-sdk

### Description
Scans the specified assembly for classes marked with McpServerResourceTypeAttribute and their members marked with McpServerResourceAttribute, registering them as McpServerResources in the builder's IServiceCollection. Handles both static and instance members. Instance members will have a new instance of the containing class constructed for each invocation.

### Method
GET

### Endpoint
/websites/modelcontextprotocol_github_io-csharp-sdk

### Parameters
#### Path Parameters
- **assembly** (string) - Optional - The assembly to scan. If not provided, the calling assembly is used.

### Request Example
```
GET /websites/modelcontextprotocol_github_io-csharp-sdk
```

### Response
#### Success Response (200)
- **IMcpServerBuilder** (object) - The builder instance with resources registered.

#### Response Example
```json
{
  "builder": "IMcpServerBuilder"
}
```

#### Exceptions
- **ArgumentNullException** - `builder` is null.
```

--------------------------------

### Adding Resources of a Specific Type

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Discovers all instance and static methods (public and non-public) on the specified TResourceType type, where members are attributed as McpServerResourceAttribute, and adds an McpServerResource instance for each.

```APIDOC
## GET /websites/modelcontextprotocol_github_io-csharp-sdk/WithResources

### Description
Discovers and adds McpServerResource instances for all methods on the specified `TResourceType` that are marked with `McpServerResourceAttribute`. Supports both static and instance methods.

### Method
GET

### Endpoint
/websites/modelcontextprotocol_github_io-csharp-sdk/WithResources

### Parameters
#### Path Parameters
- **TResourceType** (string) - Required - The resource type to discover methods from.

### Request Example
```
GET /websites/modelcontextprotocol_github_io-csharp-sdk/WithResources?TResourceType=MyResourceType
```

### Response
#### Success Response (200)
- **IMcpServerBuilder** (object) - The builder instance with resources added.

#### Response Example
```json
{
  "builder": "IMcpServerBuilder"
}
```

#### Exceptions
- **ArgumentNullException** - `builder` is null.
```

--------------------------------

### Adding Standard Input/Output Transport

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds a server transport that uses standard input (stdin) and standard output (stdout) for communication. Suitable for local launches where the server exits when stdin is closed.

```APIDOC
## POST /websites/modelcontextprotocol_github_io-csharp-sdk/StdioServerTransport

### Description
Adds a server transport using standard input and output streams for communication. This is useful for local server launches where the server should terminate upon stdin closure.

### Method
POST

### Endpoint
/websites/modelcontextprotocol_github_io-csharp-sdk/StdioServerTransport

### Parameters
No parameters required for this action.

### Request Example
```json
{
  "action": "addStdioTransport"
}
```

### Response
#### Success Response (200)
- **IMcpServerBuilder** (object) - The builder instance with the stdio transport added.

#### Response Example
```json
{
  "builder": "IMcpServerBuilder"
}
```

#### Exceptions
- **ArgumentNullException** - `builder` is null.
```

--------------------------------

### Configure StdioClientTransportOptions in C#

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Client

Defines the StdioClientTransportOptions class, used to configure the parameters for a StdioClientTransport. This includes setting the command to execute, arguments for the server process, environment variables, working directory, and handling of standard error output.

```csharp
public sealed class StdioClientTransportOptions {
    public IList<string>? Arguments { get; set; }
    public required string Command { get; set; }
    public IDictionary<string, string?>? EnvironmentVariables { get; set; }
    public string? Name { get; set; }
    public TimeSpan ShutdownTimeout { get; set; }
    public Action<string>? StandardErrorLines { get; set; }
    public string? WorkingDirectory { get; set; }
}
```

--------------------------------

### StdioClientTransportOptions Properties

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Client

This section details the properties available for configuring StdioClientTransportOptions.

```APIDOC
## Class StdioClientTransportOptions

### Description
Provides options for configuring StdioClientTransport instances.

### Properties

#### Arguments
**Type:** `IList<string>?`
**Description:** Gets or sets the arguments to pass to the server process when it is started.

#### Command
**Type:** `required string`
**Description:** Gets or sets the command to execute to start the server process.

#### EnvironmentVariables
**Type:** `IDictionary<string, string?>?`
**Description:** Gets or sets environment variables to set for the server process.
**Remarks:** This property allows you to specify environment variables that will be set in the server process's environment. By default, the server process will inherit the current environment's variables. Entries in this dictionary augment and overwrite entries read from the environment. Entries with null values remove the variables.

#### Name
**Type:** `string?`
**Description:** Gets or sets a transport identifier used for logging purposes.

#### ShutdownTimeout
**Type:** `TimeSpan`
**Description:** Gets or sets the timeout to wait for the server to shut down gracefully.
**Remarks:** This property dictates how long the client should wait for the server process to exit cleanly during shutdown before forcibly terminating it. The default is five seconds.

#### StandardErrorLines
**Type:** `Action<string>?`
**Description:** Gets or sets a callback that is invoked for each line of stderr received from the server process.

#### WorkingDirectory
**Type:** `string?`
**Description:** Gets or sets the working directory for the server process.
```

--------------------------------

### WithTools<TToolType> Method

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds McpServerTool instances to the service collection backing the builder. This method discovers and adds tools based on the specified tool type.

```APIDOC
## WithTools<TToolType>

### Description
Adds McpServerTool instances to the service collection backing `builder`. This method discovers all instance and static methods (public and non-public) on the specified `TToolType` type, where the methods are attributed as McpServerToolAttribute, and adds an McpServerTool instance for each. For instance methods, an instance will be constructed for each invocation of the tool.

### Method
`public static IMcpServerBuilder WithTools<TToolType>(this IMcpServerBuilder builder, JsonSerializerOptions? serializerOptions = null)`

### Endpoint
N/A (This is a builder method, not a direct API endpoint)

### Parameters
#### Path Parameters
None

#### Query Parameters
None

#### Request Body
None

### Request Example
None

### Response
#### Success Response (N/A)
N/A

#### Response Example
None

### Type Parameters
`TToolType` - The tool type.

### Remarks
This method discovers all instance and static methods (public and non-public) on the specified `TToolType` type, where the methods are attributed as McpServerToolAttribute, and adds an McpServerTool instance for each. For instance methods, an instance will be constructed for each invocation of the tool.

### Exceptions
- ArgumentNullException: `builder` is null.
```

--------------------------------

### Add McpServerTool Instances from Assembly

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds McpServerTool implementations to the server by scanning a specified assembly for types marked with `McpServerToolTypeAttribute`. This facilitates bulk registration of tools from a compiled library.

```APIDOC
## WithToolsFromAssembly(IMcpServerBuilder, Assembly?, JsonSerializerOptions?) 

### Description
Adds types marked with the `McpServerToolTypeAttribute` attribute from a given assembly as tools to the server. It discovers methods attributed as `McpServerToolAttribute` within these types and registers them.

### Method
`static IMcpServerBuilder`

### Endpoint
N/A (Configuration method)

### Parameters
#### Path Parameters
None

#### Query Parameters
None

#### Request Body
None

### Request Example
```csharp
// Assuming MyToolsAssembly contains types with McpServerToolTypeAttribute
var assemblyToScan = typeof(MyToolClass).Assembly;
var options = new JsonSerializerOptions { WriteIndented = true };
var builder = new McpServerBuilder();
builder.WithToolsFromAssembly(assemblyToScan, options);
```

### Response
#### Success Response (200)
N/A (Configuration method returns builder)

#### Response Example
N/A
```

--------------------------------

### Add McpServerTool Instances (by type)

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds McpServerTool implementations to the server's service collection by specifying their types. This method allows for dynamic discovery and registration of tools based on attributed methods.

```APIDOC
## WithTools(IMcpServerBuilder, IEnumerable<Type>, JsonSerializerOptions?) 

### Description
Adds McpServerTool instances to the service collection backing the builder by providing a collection of types. This method scans the provided types for methods marked with `McpServerToolAttribute` and registers them as tools.

### Method
`static IMcpServerBuilder`

### Endpoint
N/A (Configuration method)

### Parameters
#### Path Parameters
None

#### Query Parameters
None

#### Request Body
None

### Request Example
```csharp
// Assuming MyToolClass contains methods with McpServerToolAttribute
var toolTypes = new List<Type> { typeof(MyToolClass) };
var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var builder = new McpServerBuilder();
builder.WithTools(toolTypes, options);
```

### Response
#### Success Response (200)
N/A (Configuration method returns builder)

#### Response Example
N/A
```

--------------------------------

### Add MCP Tools to Server Builder (C#)

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds McpServerTool instances to the service collection backing the builder. It discovers methods attributed with McpServerToolAttribute on a specified tool type and registers them. For instance methods, a new instance is created per invocation.

```csharp
public static IMcpServerBuilder WithTools<TToolType>(this IMcpServerBuilder builder, JsonSerializerOptions? serializerOptions = null)
```

--------------------------------

### ResourceMetadataRequestContext Constructor (C#)

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.AspNetCore.Authentication

Initializes a new instance of the ResourceMetadataRequestContext class. This constructor requires HttpContext, AuthenticationScheme, and McpAuthenticationOptions as parameters to set up the request context.

```csharp
public ResourceMetadataRequestContext(HttpContext context, AuthenticationScheme scheme, McpAuthenticationOptions options)
```

--------------------------------

### Add McpServerResource instances - C#

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds McpServerResource instances to the service collection backing the builder. These resources represent data or functionality exposed by the Model Context Protocol server.

```csharp
public static IMcpServerBuilder WithResources(this IMcpServerBuilder builder, IEnumerable<McpServerResource> resourceTemplates)
```

--------------------------------

### Add McpServerTool Instances by Instance (C#)

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds McpServerTool instances to the service collection backing the builder. This overload directly accepts a collection of McpServerTool instances for server integration. ArgumentNullException is thrown if builder or tools are null.

```csharp
public static IMcpServerBuilder WithTools(this IMcpServerBuilder builder, IEnumerable<McpServerTool> tools)
```

--------------------------------

### Configure Read Resource Handler - C#

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Configures a handler for reading resource data from the Model Context Protocol server. This is often used in conjunction with a list handler to provide a complete resource implementation.

```csharp
public static IMcpServerBuilder WithReadResourceHandler(this IMcpServerBuilder builder, Func<RequestContext<ReadResourceRequestParams>, CancellationToken, ValueTask<ReadResourceResult>> handler)
```

--------------------------------

### ResourceSigningAlgValuesSupported Property

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Authentication

Lists the JWS signing algorithms supported by the protected resource for signing resource responses. The value 'none' must not be used. This property is optional.

```csharp
[JsonPropertyName("resource_signing_alg_values_supported")]
public List<string>? ResourceSigningAlgValuesSupported { get; set; }
```

--------------------------------

### Configure Resource Subscription Handler

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Configures a handler to manage client requests for subscribing to resource updates. This allows clients to receive real-time notifications when resources change, eliminating the need for constant polling.

```APIDOC
## WithSubscribeToResourcesHandler(IMcpServerBuilder, Func<RequestContext<SubscribeRequestParams>, CancellationToken, ValueTask<EmptyResult>>) 

### Description
Configures a handler for resource subscription requests. This handler is responsible for registering client interest in specific resources, enabling the server to notify subscribed clients about resource changes.

### Method
`static IMcpServerBuilder`

### Endpoint
N/A (Configuration method)

### Parameters
#### Path Parameters
None

#### Query Parameters
None

#### Request Body
None

### Request Example
```csharp
// Example usage within an MCP server setup
var builder = new McpServerBuilder();
builder.WithSubscribeToResourcesHandler((requestContext, cancellationToken) => {
    // Your subscription handling logic here
    Console.WriteLine($"Received subscription request for resources: {requestContext.Params.Resources}");
    return ValueTask.FromResult(new EmptyResult());
});
```

### Response
#### Success Response (200)
N/A (Configuration method returns builder)

#### Response Example
N/A
```

--------------------------------

### Add McpServerTool Instances (by instance)

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds specific McpServerTool instances to the server's service collection, allowing them to be discovered and invoked by clients.

```APIDOC
## WithTools(IMcpServerBuilder, IEnumerable<McpServerTool>) 

### Description
Adds McpServerTool instances to the service collection backing the builder. These tools represent specific functionalities that the server can expose to clients.

### Method
`static IMcpServerBuilder`

### Endpoint
N/A (Configuration method)

### Parameters
#### Path Parameters
None

#### Query Parameters
None

#### Request Body
None

### Request Example
```csharp
// Assuming McpServerTool is a defined class implementing the tool interface
var myTool = new MyCustomTool(); 
var builder = new McpServerBuilder();
builder.WithTools(new List<McpServerTool> { myTool });
```

### Response
#### Success Response (200)
N/A (Configuration method returns builder)

#### Response Example
N/A
```

--------------------------------

### HttpServerTransportOptions Configuration

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.AspNetCore

Configuration options for the Model Context Protocol's Streaming HTTP transport.

```APIDOC
## Class HttpServerTransportOptions

**Namespace:** ModelContextProtocol.AspNetCore
**Assembly:** ModelContextProtocol.AspNetCore.dll

Configuration options for McpEndpointRouteBuilderExtensions.MapMcp, which implements the Streaming HTTP transport for the Model Context Protocol.

### Properties

- **ConfigureSessionOptions** (Func<HttpContext, McpServerOptions, CancellationToken, Task>?) - Gets or sets an optional asynchronous callback to configure per-session McpServerOptions with access to the HttpContext of the request that initiated the session.
- **IdleTimeout** (TimeSpan) - Gets or sets the duration of time the server will wait between any active requests before timing out an MCP session. Defaults to 2 hours. This is checked in the background every 5 seconds. A client trying to resume a session will receive a 404 status code and should restart their session. A client can keep their session open by keeping a GET request open.
- **MaxIdleSessionCount** (int) - Gets or sets the maximum number of idle sessions to track in memory. This is used to limit the number of sessions that can be idle at once. Defaults to 10,000 sessions. Past this limit, the server will log a critical error and terminate the oldest idle sessions even if they have not reached their IdleTimeout until the idle session count is below this limit. Clients that keep their session open by keeping a GET request open will not count towards this limit.
- **PerSessionExecutionContext** (bool) - Gets or sets whether the server should use a single execution context for the entire session. If false, handlers like tools get called with the ExecutionContext belonging to the corresponding HTTP request which can change throughout the MCP session. If true, handlers will get called with the same ExecutionContext used to call ConfigureSessionOptions and RunSessionHandler. Defaults to false. Enabling a per-session ExecutionContext can be useful for setting AsyncLocal<T> variables that persist for the entire session, but it prevents you from using IHttpContextAccessor in handlers.
- **RunSessionHandler** (Func<HttpContext, IMcpServer, CancellationToken, Task>?) - Gets or sets an optional asynchronous callback for running new MCP sessions manually. This is useful for running logic before a session starts and after it completes.
- **Stateless** (bool) - Gets or sets whether the server should run in a stateless mode that does not require all requests for a given session to arrive at the same ASP.NET Core application process. Defaults to false. If true, the "/sse" endpoint will be disabled, and client information will be round-tripped as part of the "MCP-Session-Id" header instead of stored in memory. Unsolicited server-to-client messages and all server-to-client requests are also unsupported, because any responses may arrive at another ASP.NET Core application process. Client sampling and roots capabilities are also disabled in stateless mode, because the server cannot make requests.
- **TimeProvider** (TimeProvider) - Used for testing the IdleTimeout.
```

--------------------------------

### Configure MCP Client Options with ElicitationHandler (C#)

Source: https://modelcontextprotocol.github.io/csharp-sdk/concepts/elicitation/elicitation

This snippet demonstrates how to configure the McpClientOptions in the MCP C# SDK to support elicitation. It involves setting client information and defining an ElicitationHandler delegate.

```csharp
McpClientOptions options = new()
{
    ClientInfo = new()
    {
        Name = "ElicitationClient",
        Version = "1.0.0"
    },
    Capabilities = new()
    {
        Elicitation = new()
        {
            ElicitationHandler = HandleElicitationAsync
        }
    }
};

await using var mcpClient = await McpClientFactory.CreateAsync(clientTransport, options);
```

--------------------------------

### WithReadResourceHandler

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Configures a handler for reading resource content from the Model Context Protocol server. This is typically used in conjunction with a listing handler to provide full resource functionality.

```APIDOC
## POST /api/resources/{resourceId}

### Description
Configures a handler for reading a resource available from the Model Context Protocol server.

### Method
POST

### Endpoint
/api/resources/{resourceId}

### Parameters
#### Path Parameters
- **resourceId** (string) - Required - The identifier of the resource to read.

#### Query Parameters
- **handler** (Func<RequestContext<ReadResourceRequestParams>, CancellationToken, ValueTask<ReadResourceResult>>) - Required - The handler function that processes resource read requests.

### Request Example
```json
{
  "requestContext": {
    "parameters": {
      "resourceId": "exampleResourceId"
    }
  }
}
```

### Response
#### Success Response (200)
- **handlerResult** (ValueTask<ReadResourceResult>) - The result of the resource read operation.

#### Response Example
```json
{
  "handlerResult": {
    "content": "resource content here"
  }
}
```

#### Errors
- **ArgumentNullException** - If the builder is null.
```

--------------------------------

### WithResourcesFromAssembly

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds types marked with `McpServerResourceTypeAttribute` from a specified assembly as resources to the server.

```APIDOC
## POST /api/resources/assembly

### Description
Adds types marked with the McpServerResourceTypeAttribute attribute from the given assembly as resources to the server.

### Method
POST

### Endpoint
/api/resources/assembly

### Parameters
#### Path Parameters
N/A

#### Query Parameters
- **resourceAssembly** (string) - Optional - The name of the assembly to load the types from. If null, the calling assembly will be used.

### Request Example
```json
{
  "resourceAssembly": "MyResourceAssembly.dll"
}
```

### Response
#### Success Response (200)
- **builder** (IMcpServerBuilder) - The builder instance provided, now configured with the discovered resources.

#### Response Example
(Returns the builder instance, no specific JSON body)

#### Errors
- **ArgumentNullException** - If `builder` is null.
```

--------------------------------

### Add Stdio Server Transport (C#)

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Configures the server to use standard input (stdin) and standard output (stdout) for communication. This is typical for local execution where the server exits when the stdin stream is closed, acting as a single-session service.

```csharp
public static IMcpServerBuilder WithStdioServerTransport(this IMcpServerBuilder builder)
```

--------------------------------

### Configure Subscribe Handler for Resources (C#)

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Configures a handler for resource subscription requests. This handler is crucial for managing client interest in specific resources and enabling real-time notifications. It's often used with an unsubscribe handler for a complete subscription system. Dependencies include RequestContext, SubscribeRequestParams, CancellationToken, and EmptyResult.

```csharp
public static IMcpServerBuilder WithSubscribeToResourcesHandler(this IMcpServerBuilder builder, Func<RequestContext<SubscribeRequestParams>, CancellationToken, ValueTask<EmptyResult>> handler)
```

--------------------------------

### ResourcePolicyUri Property

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Authentication

A URL to a page containing human-readable information about the protected resource's requirements and how clients can use the provided data. This property is optional.

```csharp
[JsonPropertyName("resource_policy_uri")]
public Uri? ResourcePolicyUri { get; set; }
```

--------------------------------

### WithUnsubscribeFromResourcesHandler Method

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Configures a handler for resource unsubscription requests. This method sets up the logic to process requests for clients to stop receiving resource notifications.

```APIDOC
## WithUnsubscribeFromResourcesHandler

### Description
Configures a handler for resource unsubscription requests. This method sets up the logic to process requests for clients to stop receiving resource notifications.

### Method
`public static IMcpServerBuilder WithUnsubscribeFromResourcesHandler(this IMcpServerBuilder builder, Func<RequestContext<UnsubscribeRequestParams>, CancellationToken, ValueTask<EmptyResult>> handler)`

### Endpoint
N/A (This is a builder method, not a direct API endpoint)

### Parameters
#### Path Parameters
None

#### Query Parameters
None

#### Request Body
None

### Request Example
None

### Response
#### Success Response (N/A)
N/A

#### Response Example
None

### Remarks
The unsubscribe handler is responsible for removing client interest in specific resources. When a client no longer needs to receive notifications about resource changes, it can send an unsubscribe request. This handler is typically paired with WithSubscribeToResourcesHandler(IMcpServerBuilder, Func<RequestContext<SubscribeRequestParams>, CancellationToken, ValueTask<EmptyResult>>) to provide a complete subscription management system. The unsubscribe operation is idempotent, meaning it can be called multiple times for the same resource without causing errors, even if there is no active subscription. After removing a subscription, the server should stop sending notifications to the client about changes to the specified resource.

### Exceptions
- ArgumentNullException: `builder` is null.
```

--------------------------------

### Add McpServerResource types - C#

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds McpServerResource instances to the service collection by specifying types that contain marked methods. The SDK discovers and registers these methods as resources.

```csharp
public static IMcpServerBuilder WithResources(this IMcpServerBuilder builder, IEnumerable<Type> resourceTemplateTypes)
```

--------------------------------

### Adding Stream Server Transport

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds a server transport that uses the specified input and output streams for communication. Allows custom stream-based communication.

```APIDOC
## POST /websites/modelcontextprotocol_github_io-csharp-sdk/StreamServerTransport

### Description
Adds a server transport using the provided input and output streams for communication, enabling custom stream-based data transfer.

### Method
POST

### Endpoint
/websites/modelcontextprotocol_github_io-csharp-sdk/StreamServerTransport

### Parameters
#### Request Body
- **inputStream** (Stream) - Required - The input stream to use for communication.
- **outputStream** (Stream) - Required - The output stream to use for communication.

### Request Example
```json
{
  "inputStream": "<base64_encoded_stream_data>",
  "outputStream": "<base64_encoded_stream_data>"
}
```

### Response
#### Success Response (200)
- **IMcpServerBuilder** (object) - The builder instance with the stream transport added.

#### Response Example
```json
{
  "builder": "IMcpServerBuilder"
}
```

#### Exceptions
- **ArgumentNullException** - `builder` is null.
- **ArgumentNullException** - `inputStream` is null.
- **ArgumentNullException** - `outputStream` is null.
```

--------------------------------

### Add McpServerTool Instances by Type (C#)

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds McpServerTool instances to the service collection backing the builder. This overload accepts a collection of types, discovering methods attributed with McpServerToolAttribute for tool registration. It supports both static and instance methods, with instances being constructed per invocation for instance methods. JsonSerializerOptions can be provided for parameter marshalling. ArgumentNullException is thrown if builder or toolTypes are null.

```csharp
public static IMcpServerBuilder WithTools(this IMcpServerBuilder builder, IEnumerable<Type> toolTypes, JsonSerializerOptions? serializerOptions = null)
```

--------------------------------

### Register Server Resources with Attributes (C#)

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Scans an assembly for classes marked with McpServerResourceTypeAttribute and registers members marked with McpServerResourceAttribute as resources. It handles both static and instance members, automatically instantiating classes for instance members. Note: This method uses runtime reflection and is not compatible with Native AOT; use WithResources<TResourceType> for Native AOT compatibility.

```csharp
public static IMcpServerBuilder WithResources<TResourceType>(this IMcpServerBuilder builder)
```

--------------------------------

### Configuring Logging Level Handler

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Configures a handler for processing logging level change requests from clients. This handler is invoked when a client sends a `logging/setLevel` request.

```APIDOC
## POST /websites/modelcontextprotocol_github_io-csharp-sdk/SetLoggingLevelHandler

### Description
Configures a handler to process client requests for changing the logging level. The server should also update its internal LoggingLevel property.

### Method
POST

### Endpoint
/websites/modelcontextprotocol_github_io-csharp-sdk/SetLoggingLevelHandler

### Parameters
#### Request Body
- **handler** (Func<RequestContext<SetLevelRequestParams>, CancellationToken, ValueTask<EmptyResult>>) - Required - The handler that processes requests to change the logging level.

### Request Example
```json
{
  "handler": "async (context, cancellationToken) => { /* handle level change */ return EmptyResult.Instance; }"
}
```

### Response
#### Success Response (200)
- **IMcpServerBuilder** (object) - The builder instance with the logging level handler configured.

#### Response Example
```json
{
  "builder": "IMcpServerBuilder"
}
```

#### Exceptions
- **ArgumentNullException** - `builder` is null.
```

--------------------------------

### Stateless Property

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.AspNetCore

Configures the server to run in a stateless mode, meaning requests for a session do not need to arrive at the same application process. This disables the "/sse" endpoint and requires client information to be sent via headers.

```csharp
public bool Stateless { get; set; }
```

--------------------------------

### C# Server Request for Boolean Elicitation

Source: https://modelcontextprotocol.github.io/csharp-sdk/concepts/elicitation/elicitation

Demonstrates how a C# server using the ModelContextProtocol SDK can request a boolean response from a user. It checks client capabilities for elicitation, defines a schema for a boolean input named 'Answer', and handles the user's response.

```csharp
[McpServerTool, Description("A simple game where the user has to guess a number between 1 and 10.")]
public async Task<string> GuessTheNumber(
    IMcpServer server, // Get the McpServer from DI container
    CancellationToken token
)
{
    // Check if the client supports elicitation
    if (server.ClientCapabilities?.Elicitation == null)
    {
        // fail the tool call
        throw new McpException("Client does not support elicitation");
    }

    // First ask the user if they want to play
    var playSchema = new RequestSchema
    {
        Properties =
        {
            ["Answer"] = new BooleanSchema()
        }
    };

    var playResponse = await server.ElicitAsync(new ElicitRequestParams
    {
        Message = "Do you want to play a game?",
        RequestedSchema = playSchema
    }, token);

    // Check if user wants to play
    if (playResponse.Action != "accept" || playResponse.Content?["Answer"].ValueKind != JsonValueKind.True)
    {
        return "Maybe next time!";
    }

```

--------------------------------

### ResourceMetadataRequestContext Constructor

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.AspNetCore.Authentication

Initializes a new instance of the ResourceMetadataRequestContext class. This constructor is used internally by the authentication middleware.

```APIDOC
## ResourceMetadataRequestContext Constructor

### Description
Initializes a new instance of the ResourceMetadataRequestContext class.

### Method
Constructor

### Parameters
#### Path Parameters
None

#### Query Parameters
None

#### Request Body
None

### Request Example
```csharp
// Example usage within an ASP.NET Core authentication handler
var context = HttpContext; // Assuming HttpContext is available
var scheme = new AuthenticationScheme("TestScheme", null, typeof(TestSchemeHandler));
var options = new McpAuthenticationOptions(); // Instantiate your options

var requestContext = new ResourceMetadataRequestContext(context, scheme, options);
```

### Response
#### Success Response (N/A)
This is a constructor and does not return a response in the traditional sense.

#### Response Example
N/A
```

--------------------------------

### ResourceDocumentation Property

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Authentication

A URL pointing to a page with human-readable information for developers about the protected resource. This property is optional.

```csharp
[JsonPropertyName("resource_documentation")]
public Uri? ResourceDocumentation { get; set; }
```

--------------------------------

### Add resources from assembly - C#

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds types marked with McpServerResourceTypeAttribute from a specified assembly as resources to the server. If no assembly is provided, the calling assembly is used.

```csharp
public static IMcpServerBuilder WithResourcesFromAssembly(this IMcpServerBuilder builder, Assembly? resourceAssembly = null)
```

--------------------------------

### AuthorizationServers Property

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Authentication

Provides a list of authorization server URIs, which are the issuer identifiers for OAuth authorization servers that can be used with this protected resource. This property is optional.

```csharp
[JsonPropertyName("authorization_servers")]
public List<Uri> AuthorizationServers { get; set; }
```

--------------------------------

### DpopSigningAlgValuesSupported Property

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Authentication

Lists the JWS algorithm values supported by the resource server for validating Demonstrating Proof of Possession (DPoP) proof JWTs. This property is optional.

```csharp
[JsonPropertyName("dpop_signing_alg_values_supported")]
public List<string>? DpopSigningAlgValuesSupported { get; set; }
```

--------------------------------

### Configure Resource Unsubscribe Handler (C#)

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Configures a handler for resource unsubscription requests. This function processes requests to remove client interest in specific resources and is typically paired with a subscribe handler.

```csharp
public static IMcpServerBuilder WithUnsubscribeFromResourcesHandler(this IMcpServerBuilder builder, Func<RequestContext<UnsubscribeRequestParams>, CancellationToken, ValueTask<EmptyResult>> handler)
```

--------------------------------

### BearerMethodsSupported Property

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Authentication

Indicates the supported methods for sending an OAuth 2.0 bearer token to the protected resource. Valid values include 'header', 'body', or 'query'. This property is optional.

```csharp
[JsonPropertyName("bearer_methods_supported")]
public List<string> BearerMethodsSupported { get; set; }
```

--------------------------------

### Add Stream Server Transport (C#)

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Adds a server transport mechanism that utilizes specified input and output streams for communication. This allows for custom stream-based communication channels.

```csharp
public static IMcpServerBuilder WithStreamServerTransport(this IMcpServerBuilder builder, Stream inputStream, Stream outputStream)
```

--------------------------------

### JwksUri Property

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Authentication

The URL of the protected resource's JSON Web Key (JWK) Set document, which contains public keys used for signing resource responses. This URL must use the 'https' scheme. This property is optional.

```csharp
[JsonPropertyName("jwks_uri")]
public Uri? JwksUri { get; set; }
```

--------------------------------

### Configure Logging Level Handler (C#)

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/Microsoft.Extensions.DependencyInjection

Sets up a handler to process client requests for changing the server's logging level. The provided handler is invoked when a 'logging/setLevel' request is received. The server should also update its internal LoggingLevel property.

```csharp
public static IMcpServerBuilder WithSetLoggingLevelHandler(this IMcpServerBuilder builder, Func<RequestContext<SetLevelRequestParams>, CancellationToken, ValueTask<EmptyResult>> handler)
```

--------------------------------

### ProtectedResourceMetadata Class Definition

Source: https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.Authentication

Defines the ProtectedResourceMetadata class, representing resource metadata for OAuth authorization according to RFC 9396 and RFC 9728. It includes various properties for configuring authorization details, supported methods, and security algorithms.

```csharp
public sealed class ProtectedResourceMetadata
```