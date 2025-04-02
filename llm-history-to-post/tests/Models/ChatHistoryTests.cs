namespace LlmHistoryToPost.Tests.Models;

using LlmHistoryToPost.Models;

[TestFixture]
public class ChatHistoryTests
{
	[Test]
	public void ShouldHaveEmptyCollectionsInInitialState()
	{
		// Arrange & Act
		var history = new ChatHistory();

		// Assert
		Assert.That(history.Sessions, Is.Empty);
		Assert.That(history.PromptsByDay, Is.Empty);
	}

	[Test]
	public void ShouldReturnCorrectlyFormattedDate()
	{
		// Arrange
		var session = new ChatSession
		{
			StartTime = new DateTime(2025, 4, 1, 12, 0, 0)
		};

		// Act
		var result = session.FormattedDate;

		// Assert
		Assert.That(result, Is.EqualTo("2025-04-01"));
	}

	[Test]
	public void ShouldReturnFullPromptWhenPromptIsShort()
	{
		// Arrange
		var pair = new PromptResponsePair
		{
			Prompt = "Short prompt"
		};

		// Act
		var result = pair.GetPromptPreview();

		// Assert
		Assert.That(result, Is.EqualTo("Short prompt"));
	}

	[Test]
	public void ShouldReturnTruncatedPromptWhenPromptIsLong()
	{
		// Arrange
		var longPrompt = new string('A', 150);
		var pair = new PromptResponsePair
		{
			Prompt = longPrompt
		};

		// Act
		var result = pair.GetPromptPreview();

		// Assert
		Assert.That(result.Length, Is.EqualTo(100));
		Assert.That(result, Does.EndWith("..."));
	}

	[Test]
	public void ShouldRespectCustomMaxLengthWhenGettingPromptPreview()
	{
		// Arrange
		var longPrompt = new string('A', 80);
		var pair = new PromptResponsePair
		{
			Prompt = longPrompt
		};

		// Act
		var result = pair.GetPromptPreview(50);

		// Assert
		Assert.That(result.Length, Is.EqualTo(50));
		Assert.That(result, Does.EndWith("..."));
	}
}
