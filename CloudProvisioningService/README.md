# Cloud Provisioning Service

This project implements a microservice that interacts with AWS to automate the provisioning of compute resources. It leverages the Open Service Broker API (OSB API) for creating and managing service instances and bindings.

## Design Choices

1. **Separation of Concerns**: The microservice separates the concerns of credential management, provisioning services, and binding services. This is achieved by using interfaces and dependency injection.
2. **Security**: The microservice uses AWS Security Token Service (STS) to generate temporary credentials for enhanced security. Each service interaction requires validation of these credentials.
3. **Extensibility**: The design allows for easy addition of new services by adhering to the OSB API specifications and using a plugin architecture.
4. **State Management**: Active bindings and their credentials are managed in-memory, ensuring quick access and easy invalidation.

## Technologies Used

- **ASP.NET Core**: For building the web API.
- **Amazon EC2 SDK**: For interacting with AWS EC2 services.
- **Amazon Security Token Service (STS)**: For generating temporary security credentials.
- **Swagger**: For API documentation and testing.
- **Dependency Injection**: For managing dependencies and enhancing testability.

## Plugin Architecture

The microservice uses a plugin architecture, making it agnostic of the cloud provider. It is designed to load and manage plugins dynamically. In this case, the AWS plugin is used for interacting with AWS services. To add a plugin, simply copy the plugin DLL (e.g., AWS plugin) into the `plugins` subfolder. The microservice will automatically detect and manage the plugin.

## How to Run the Microservice

1. **Clone the Repository**:
    ```sh
    git clone https://github.com/marangiv/CloudProvisioningService.git
    cd CloudProvisioningService
    ```

2. **Restore Dependencies**:
    ```sh
    dotnet restore
    ```

3. **Build the Project**:
    ```sh
    dotnet build
    ```

4. **Run the Microservice**:
    ```sh
    dotnet run --project CloudProvisioningService
    ```

## Assumptions Made

- The microservice assumes that AWS STS (Security Token Service) is configured and accessible. The service generates temporary credentials using STS for each service interaction.
- The service bindings and provisioning operations are managed in-memory for simplicity. In a production environment, a persistent storage solution should be considered.
- The microservice is designed to work with AWS regions and assumes the region provided is valid and supported by AWS.
- The microservice assumes that there is a default VPC in the provided region.

## API Framework Chosen

- **ASP.NET Core**: Chosen for its robustness, performance, and rich ecosystem. It provides a solid foundation for building scalable web APIs.

## API Specification

- **OSB API (Open Service Broker API)**: The microservice adheres to OSB API v2.15 specifications for creating and managing service instances and bindings.

## Instructions on How to Interact with OSB API Endpoints

### 1. Bind a Service Instance
- **Endpoint**: `PUT /v2/service_instances/{instance_id}/service_bindings/{binding_id}`
- **Description**: Binds a service instance to a binding, generating temporary credentials.
- **Parameters**:
  - `instance_id`: The ID of the service instance.
  - `binding_id`: The ID of the binding.
  - `region_id`: The AWS region (query parameter).
- **Request Body**:
  ```json
  {
    "serviceId": "aws-compute-service-id",
    "planId": "basic-plan",
    "parameters": "{}"
  }
- **Example**:
  ```bash
  curl -X 'PUT' \
  'http://localhost:5000/v2/service_instances/my-instance-123/service_bindings/my-binding-456?region_id=us-east-1' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "serviceId": "aws-compute-service-id",
  "planId": "basic-plan",
  "parameters": "{}"
### 2. Unbind a Service Instance
- **Endpoint**: `DELETE /v2/service_instances/{instance_id}/service_bindings/{binding_id}`
- **Description**: Unbinds a service instance from a binding, invalidating the temporary credentials.
- **Parameters**:
  - `instance_id`: The ID of the service instance.
  - `binding_id`: The ID of the binding.


### 3. Provision a Service Instance
- **Endpoint**: `PUT /v2/instances/{instance_id}`
- **Description**: Provisions a new service instance.
- **Parameters**:
  - `instance_id`: The ID of the instance to be provisioned.
- **Request Body**:
  ```json
  {
    "imageId": "ami-0346fd83e3383dcb4",
    "instanceType": "t2.micro",
    "instanceId": "i-1234567890abcdef0",
    "bindingId": "my-binding-456"
  }
### 4. Deprovision a Service Instance
- **Endpoint**: `DELETE /v2/instances/{instance_id}`
- **Description**: Deprovisions an existing service instance.
- **Parameters**:
  - `instance_id`: The ID of the instance to be deprovisioned.
- **Request Body**:
  ```json
  {
    "instanceId": "i-1234567890abcdef0",
    "bindingId": "my-binding-456"
  }
### 5. Start a Service Instance

- **Endpoint**: `POST /v2/instances/{instance_id}/start`
- **Description**: Starts an existing service instance.
- **Parameters**:
  - `instance_id`: The ID of the instance to be started.
- **Request Body**:
  ```json
  {
    "instanceId": "i-1234567890abcdef0",
    "bindingId": "my-binding-456"
  }
- **Example**:
  ```bash
  curl -X 'POST' \
  'http://localhost:5000/v2/instances/i-1234567890abcdef0/start' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "instanceId": "i-1234567890abcdef0",
  "bindingId": "my-binding-456"

### 6. Stop a Service Instance

- **Endpoint**: `POST /v2/instances/{instance_id}/stop`
- **Description**: Stops an existing service instance.
- **Parameters**:
  - `instance_id`: The ID of the instance to be stopped.
- **Request Body**:
  ```json
  {
    "instanceId": "i-1234567890abcdef0",
    "bindingId": "my-binding-456"
  }
- **Example**:
  ```bash
  curl -X 'POST' \
  'http://localhost:5000/v2/instances/i-1234567890abcdef0/stop' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "instanceId": "i-1234567890abcdef0",
  "bindingId": "my-binding-456"

### 7. Restart a Service Instance
- **Endpoint**: `POST /v2/instances/{instance_id}/restart`
- **Description**: Restarts an existing service instance.
- **Parameters**:
  - `instance_id`: The ID of the instance to be restarted.
- **Request Body**:
  ```json
  {
    "instanceId": "i-1234567890abcdef0",
    "bindingId": "my-binding-456"
  }
- **Example**:
  ```bash
  curl -X 'POST' \
  'http://localhost:5000/v2/instances/i-1234567890abcdef0/restart' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "instanceId": "i-1234567890abcdef0",
  "bindingId": "my-binding-456"

### License
- This project is licensed under the MIT License.