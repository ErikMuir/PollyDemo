# POLLY
Polly is a .NET resilience and transient-fault-handling library that allows developers to express policies in a fluent and thread-safe manner.

## What is a Resilience Framework?
- Recovery if possible
- Graceful degradation if no other option

## Reactive Strategies 
- **Fallback** - returns a default value
- **Retry** - retries immediately
    - X times
    - Forever (until succeeds)
    - Delegates
        - onRetry
- **Wait and Retry** - wait before resending request
    - X times
    - Forever (until succeeds)
    - Ability to customize wait time
    - Delegates
        - onRetry
- **Circuit Breaker** - stops all requests to a faulty service
    - Normal - simple sum of failures, no minimum throughput
    - Advanced - percentage of failures over time, has minimum throughput
    - Circuit states
        - Closed
        - Open
        - Half-Open
    - Delegates
        - onBreak
        - onHalfOpen
        - onReset

## Proactive Strategies
- **Timeout** - end a request when you want
    - Optimistic - with cancellation token
    - Pessemistic - without cancellation token
- **Bulkhead Isolation** - prevents faults from bringing the whole system down
    - Execution slots
    - Queue slots
    - Delegates
        - onBulkheadRejected
- **Caching** - store a previous response using a memory cache provider
- **PolicyWrap** - define a combined policy strategy, built of previously-defined policies
- **NoOp** - basically a "no policy" policy, great for unit testing
- **PolicyRegistry** - provides a registry for storing configured policy instances and retrieving them later for use. It promotes separation of policy definition and usage, enabling common patterns such as defining policies centrally on start-up, and accessing them at point-of-use as an injected dependency.
