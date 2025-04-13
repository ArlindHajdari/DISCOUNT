# DISCOUNT Service

The DISCOUNT solution provides a gRPC-based service for generating and managing discount codes. It consists of a server application (`DISCOUNT.Server`) and a client application (`DISCOUNT.Client`).

## Features

- Generate unique discount codes with customizable length and count.
- Validate and use discount codes.
- Redis integration for storing and managing discount codes.
- gRPC communication between the client and server.
- DISCOUNT codes must remain between service restarts. (Redis)
- The length of the DISCOUNT code is 7-8 characters during generation.
- DISCOUNT code must be generated randomly and cannot repeat (Sets).
- Generation could be repeated as many times as desired.
- Maximum of 2 thousand DISCOUNT codes can be generated with single request.
- System must be capable of processing multiple requests in parallel.

---

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/) (recommended, for containerized deployment)

---

## Project Structure

- **DISCOUNT.Server**: The gRPC server that handles discount code generation and code usage storing them in Redis following this structure: <code><discount-codes:"generated-code"></code> with the value of "used" or "unused".
- **DISCOUNT.Client**: The gRPC client which acts as a TCP Socket server that communicates with the gRPC server.
- **DISCOUNT.Tests**: Unit tests for the solution. (Not implemented)

---

## Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/ArlindHajdari/DISCOUNT
cd DISCOUNT
```

### 2. Make sure that Docker is running and run the infrastructure
```bash
docker-compose up --build
```

### 3. Use any preferred tool to reach the TCP Socket server, in my case netcat
```bash
nc 127.0.0.1 6000
```

### 4. Now the server waits for the raw data to be pasted.
#### TCP server understands the following JSON structure:
```json
{
    "MethodName": "UseDiscountCode",
    "Payload": "{\"Code\": \"GEWDG66Q\"}"
}
```

#### Where MethodName can be one of: UseDiscountCode or GenerateDiscountCode. And the Payload can also be:
```json
{
    "MethodName": "GenerateDiscountCode",
    "Payload": "{\"Length\": 8, \"Count\": 8}"
}
```

### Paste the JSON and the response will be received in the same window terminal
```bash
# Command
nc 127.0.0.1 6000
{"MethodName": "GenerateDiscountCode", "Payload": "{\"Length\": 8, \"Count\": 8}"}

# Output
{"Result":true}
```

### OR
```bash
# Command
nc 127.0.0.1 6000
{"MethodName": "UseDiscountCode", "Payload": "{\"Code\": \"GEWDG66Q\"}"}

# Output
{"Result":2}
```

### The responses may differ, current results are generated in the time this README has been created.
#### The result of UseDiscountCode may be confusing but it follows the enum:

```csharp
public enum UseCodeResult : byte
{
    Success = 0,
    CodeNotFound = 1,
    CodeAlreadyUsed = 2
}
```

---

## Redis
### 1. After running the necessary containers, check the Redis ContainerID/Name and open Its cli:

```bash
# Check containers
docker ps
```

```bash
# Run redis cli
docker exec -it <my-redis-container-name> redis-cli
```

```bash
# Check keys
KEYS discount-codes* 
```

```bash
# Check valu for a specific key
GET <paste key here> 
```

#### The value of the keys in Redis will be: used or unused.
