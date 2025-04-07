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
		Assert.That(result.Sessions[0].PromptResponsePairs[1].Prompt, Is.EqualTo("I need to parse some JSON data."));
		Assert.That(result.PromptsByDay, Has.Count.EqualTo(1));
		Assert.That(result.PromptsByDay.ContainsKey(new DateOnly(2025, 4, 1)), Is.True);
		Assert.That(result.PromptsByDay[new DateOnly(2025, 4, 1)], Has.Count.EqualTo(2));
	}

	[Test]
	public void ShouldParseCorrectlyWithMultipleSessions()
	{
		// Arrange
		// TODO: Create test file content with multiple sessions

		// Act
		// TODO: Parse the file

		// Assert
		// TODO: Verify all sessions were parsed correctly
	}

	[Test]
	public void ShouldGroupSessionsByDayCorrectly()
	{
		// Arrange
		// TODO: Create test file content with sessions from different days

		// Act
		// TODO: Parse the file

		// Assert
		// TODO: Verify sessions are grouped by day correctly
	}

	[Test]
	public void ShouldParseSinglePromptResponsePairCorrectly()
	{
		// Arrange
		// TODO: Create session content with a single prompt-response pair

		// Act
		// TODO: Parse the prompt-response pairs

		// Assert
		// TODO: Verify the prompt-response pair was parsed correctly
	}

	[Test]
	public void ShouldParseMultiplePromptResponsePairsCorrectly()
	{
		// Arrange
		// TODO: Create session content with multiple prompt-response pairs

		// Act
		// TODO: Parse the prompt-response pairs

		// Assert
		// TODO: Verify all prompt-response pairs were parsed correctly
	}

	[Test]
	public void ShouldCombineConsecutivePromptsCorrectly()
	{
		// Arrange
		// TODO: Create session content with consecutive prompts

		// Act
		// TODO: Parse the prompt-response pairs

		// Assert
		// TODO: Verify consecutive prompts are combined correctly
	}
}
