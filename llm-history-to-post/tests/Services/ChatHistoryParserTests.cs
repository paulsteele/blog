namespace LlmHistoryToPost.Tests.Services;

using LlmHistoryToPost.Services;

[TestFixture]
public class ChatHistoryParserTests
{
	private ChatHistoryParser _parser;

	[SetUp]
	public void Setup()
	{
		_parser = new ChatHistoryParser();
	}

	[Test]
	public void ShouldReturnEmptyHistoryWhenContentIsEmpty()
	{
		// Arrange
		var emptyContent = string.Empty;

		// Act
		var result = _parser.ParseHistoryContent(emptyContent);

		// Assert
		Assert.That(result.Sessions, Is.Empty);
		Assert.That(result.PromptsByDay, Is.Empty);
	}

	[Test]
	public void ShouldParseCorrectlyWithSingleSession()
	{
		// Arrange
		var content = @"# aider chat started at 2025-04-01 10:15:30
#### Hello, can you help me with a C# problem?

I'd be happy to help with your C# problem. What specifically are you working on?

#### I need to parse some JSON data.

Sure, you can use System.Text.Json or Newtonsoft.Json for that. Here's a simple example:

```csharp
using System.Text.Json;

var jsonString = ""{\""name\"":\""John\"", \""age\"":30}"";
var person = JsonSerializer.Deserialize<Person>(jsonString);
```

Let me know if you need more specific guidance!
";

		// Act
		var result = _parser.ParseHistoryContent(content);

		// Assert
		Assert.That(result.Sessions, Has.Count.EqualTo(1));
		Assert.That(result.Sessions[0].StartTime, Is.EqualTo(new DateTime(2025, 4, 1, 10, 15, 30)));
		Assert.That(result.Sessions[0].PromptResponsePairs, Has.Count.EqualTo(2));
		Assert.That(result.Sessions[0].PromptResponsePairs[0].Prompt, Is.EqualTo("Hello, can you help me with a C# problem?"));
		Assert.That(result.Sessions[0].PromptResponsePairs[0].Response, Is.EqualTo("I'd be happy to help with your C# problem. What specifically are you working on?"));
		Assert.That(result.Sessions[0].PromptResponsePairs[1].Prompt, Is.EqualTo("I need to parse some JSON data."));
		Assert.That(result.Sessions[0].PromptResponsePairs[1].Response, Contains.Substring("Sure, you can use System.Text.Json or Newtonsoft.Json for that."));
		Assert.That(result.PromptsByDay, Has.Count.EqualTo(1));
		Assert.That(result.PromptsByDay.ContainsKey(new DateOnly(2025, 4, 1)), Is.True);
		Assert.That(result.PromptsByDay[new DateOnly(2025, 4, 1)], Has.Count.EqualTo(2));
	}

	[Test]
	public void ShouldParseCorrectlyWithMultipleSessions()
	{
		// Arrange
		var content = @"# aider chat started at 2025-04-01 10:15:30
#### Hello, can you help me with a C# problem?

I'd be happy to help with your C# problem. What specifically are you working on?

#### I need to parse some JSON data.

Sure, you can use System.Text.Json or Newtonsoft.Json for that.

# aider chat started at 2025-04-01 14:22:45
#### How do I create a unit test in NUnit?

To create a unit test in NUnit, you'll need to:

1. Add the NUnit NuGet package to your test project
2. Create a test class with the [TestFixture] attribute
3. Add test methods with the [Test] attribute

Here's a simple example:

```csharp
using NUnit.Framework;

[TestFixture]
public class CalculatorTests
{
    [Test]
    public void Add_ShouldReturnCorrectSum()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(2, 3);
        
        // Assert
        Assert.That(result, Is.EqualTo(5));
    }
}
```

#### What about test setup and teardown?

NUnit provides [SetUp] and [TearDown] attributes for methods that should run before and after each test:

```csharp
[SetUp]
public void Setup()
{
    // Code that runs before each test
}

[TearDown]
public void Teardown()
{
    // Code that runs after each test
}
```

There are also [OneTimeSetUp] and [OneTimeTearDown] for code that should run once before/after all tests in the fixture.
";

		// Act
		var result = _parser.ParseHistoryContent(content);

		// Assert
		Assert.That(result.Sessions, Has.Count.EqualTo(2));
		
		// First session assertions
		Assert.That(result.Sessions[0].StartTime, Is.EqualTo(new DateTime(2025, 4, 1, 10, 15, 30)));
		Assert.That(result.Sessions[0].PromptResponsePairs, Has.Count.EqualTo(2));
		Assert.That(result.Sessions[0].PromptResponsePairs[0].Prompt, Is.EqualTo("Hello, can you help me with a C# problem?"));
		Assert.That(result.Sessions[0].PromptResponsePairs[1].Prompt, Is.EqualTo("I need to parse some JSON data."));
		
		// Second session assertions
		Assert.That(result.Sessions[1].StartTime, Is.EqualTo(new DateTime(2025, 4, 1, 14, 22, 45)));
		Assert.That(result.Sessions[1].PromptResponsePairs, Has.Count.EqualTo(2));
		Assert.That(result.Sessions[1].PromptResponsePairs[0].Prompt, Is.EqualTo("How do I create a unit test in NUnit?"));
		Assert.That(result.Sessions[1].PromptResponsePairs[1].Prompt, Is.EqualTo("What about test setup and teardown?"));
		
		// PromptsByDay assertions
		Assert.That(result.PromptsByDay, Has.Count.EqualTo(1));
		Assert.That(result.PromptsByDay.ContainsKey(new DateOnly(2025, 4, 1)), Is.True);
		Assert.That(result.PromptsByDay[new DateOnly(2025, 4, 1)], Has.Count.EqualTo(4));
	}

	[Test]
	public void ShouldGroupSessionsByDayCorrectly()
	{
		// Arrange
		var content = @"# aider chat started at 2025-04-01 10:15:30
#### Hello, can you help me with a C# problem?

I'd be happy to help with your C# problem. What specifically are you working on?

# aider chat started at 2025-04-02 09:30:15
#### How do I implement dependency injection?

Dependency injection in C# is typically implemented using a DI container. Here's how you can set it up.

# aider chat started at 2025-04-02 14:45:22
#### What's the difference between interfaces and abstract classes?

Interfaces and abstract classes serve different purposes in C#:

1. Interfaces only declare functionality without implementation
2. Abstract classes can provide partial implementation

# aider chat started at 2025-04-03 11:20:45
#### How do I use LINQ?

LINQ (Language Integrated Query) provides a consistent way to query data from different sources.
";

		// Act
		var result = _parser.ParseHistoryContent(content);

		// Assert
		Assert.That(result.Sessions, Has.Count.EqualTo(4));
		Assert.That(result.PromptsByDay, Has.Count.EqualTo(3));
		
		// Check April 1st
		var april1 = new DateOnly(2025, 4, 1);
		Assert.That(result.PromptsByDay.ContainsKey(april1), Is.True);
		Assert.That(result.PromptsByDay[april1], Has.Count.EqualTo(1));
		Assert.That(result.PromptsByDay[april1][0].Prompt, Is.EqualTo("Hello, can you help me with a C# problem?"));
		
		// Check April 2nd
		var april2 = new DateOnly(2025, 4, 2);
		Assert.That(result.PromptsByDay.ContainsKey(april2), Is.True);
		Assert.That(result.PromptsByDay[april2], Has.Count.EqualTo(2));
		Assert.That(result.PromptsByDay[april2][0].Prompt, Is.EqualTo("How do I implement dependency injection?"));
		Assert.That(result.PromptsByDay[april2][1].Prompt, Is.EqualTo("What's the difference between interfaces and abstract classes?"));
		
		// Check April 3rd
		var april3 = new DateOnly(2025, 4, 3);
		Assert.That(result.PromptsByDay.ContainsKey(april3), Is.True);
		Assert.That(result.PromptsByDay[april3], Has.Count.EqualTo(1));
		Assert.That(result.PromptsByDay[april3][0].Prompt, Is.EqualTo("How do I use LINQ?"));
	}


	[Test]
	public void ShouldCombineConsecutivePromptsCorrectly()
	{
		// Arrange
		var content = @"# aider chat started at 2025-04-01 10:15:30
#### First part of a question

#### And here's the second part

Here's my response to your complete question.

#### A new separate question

And here's the answer to your new question.
";

		// Act
		var result = _parser.ParseHistoryContent(content);

		// Assert
		Assert.That(result.Sessions, Has.Count.EqualTo(1));
		Assert.That(result.Sessions[0].PromptResponsePairs, Has.Count.EqualTo(2));
		
		// Check that consecutive prompts were combined
		Assert.That(result.Sessions[0].PromptResponsePairs[0].Prompt, Is.EqualTo("First part of a question\nAnd here's the second part"));
		Assert.That(result.Sessions[0].PromptResponsePairs[0].Response, Is.EqualTo("Here's my response to your complete question."));
		
		// Check the second prompt-response pair
		Assert.That(result.Sessions[0].PromptResponsePairs[1].Prompt, Is.EqualTo("A new separate question"));
		Assert.That(result.Sessions[0].PromptResponsePairs[1].Response, Is.EqualTo("And here's the answer to your new question."));
	}
}
