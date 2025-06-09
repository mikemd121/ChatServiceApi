-------ChatServiceApi-------

This application is developed based on Clean Architecture, with several layers created to maintain code quality and avoid dependencies on the Core domain and Application layers.

The system is built using .NET 8 with dependency injection, aligning with object-oriented programming (OOP) concepts and SOLID principles. Additionally, it follows RESTful architecture to ensure standardization.

Asynchronous programming is applied in specific scenarios to prevent blocking calls and improve performance.

A global exception middleware is implemented to handle exceptions consistently across the application.

Test cases have been covered as much as possible. The following scenarios were specifically tested:

A team of 2 people: 1 Senior (capacity 8) and 1 Junior (capacity 4).
→ 5 chats arrive: 4 are assigned to the Junior, and 1 to the Senior.

A team of 2 Juniors and 1 Mid-level agent.
→ 6 chats arrive: 3 are assigned to the Juniors, and none to the Mid-level agent.
