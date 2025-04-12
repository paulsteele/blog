namespace LlmHistoryToPost.Tests.Services;

using LlmHistoryToPost.Models;
using LlmHistoryToPost.Services;
using Spectre.Console.Testing;

[TestFixture]
public class UserInteractionServiceTests
{
	private UserInteractionService _service;
	private TestConsole _testConsole;

	[SetUp]
	public void Setup()
	{
		_testConsole = new TestConsole();
		_testConsole.Profile.Capabilities.Interactive = true;
		
		_service = new UserInteractionService(_testConsole);
	}

	[Test]
	public void ShouldReturnDayWhenOnlyOneDayExists()
	{
		var dict = new Dictionary<DateOnly, List<PromptResponsePair>>
		{
			{ new DateOnly(2025, 4, 1), [] }
		};

		var result = _service.SelectDay(dict);
		
		Assert.That(result, Is.EqualTo(new DateOnly(2025, 4, 1)));
	}

	[Test]
	public void ShouldPromptForSelectionWhenMultipleDaysExist()
	{
		var dict = new Dictionary<DateOnly, List<PromptResponsePair>>
		{
			{ new DateOnly(2025, 4, 1), [] },
			{ new DateOnly(2025, 4, 2), [] },
			{ new DateOnly(2025, 4, 3), [] },
			{ new DateOnly(2025, 4, 4), [] }
		};
		
		// Set up the test console to select the second option
		_testConsole.Input.PushKey(ConsoleKey.DownArrow);
		_testConsole.Input.PushKey(ConsoleKey.Enter);

		var result = _service.SelectDay(dict);
		
		Assert.That(result, Is.EqualTo(new DateOnly(2025, 4, 3)));
	}

	[Test]
	public void ShouldThrowExceptionWhenNoDaysExist()
	{
		// Arrange
		var emptyDict = new Dictionary<DateOnly, List<PromptResponsePair>>();

		// Act & Assert
		Assert.That(() => _service.SelectDay(emptyDict), 
			Throws.TypeOf<InvalidOperationException>());
	}

	[Test]
	public void ShouldThrowExceptionWhenNoPromptsExist()
	{
		// Arrange
		var emptyList = new List<PromptResponsePair>();

		// Act & Assert
		Assert.That(() => _service.SelectPrompts(emptyList), 
			Throws.TypeOf<InvalidOperationException>());
	}

	[Test]
	public void ShouldReturnSelectedPromptsWhenPromptsExist()
	{
		// Arrange
		var testPrompts = new List<PromptResponsePair>();
		for (var i = 1; i <= 10; i++)
		{
			testPrompts.Add(new PromptResponsePair 
			{ 
				Prompt = $"Test prompt {i}", 
				Response = $"Test response {i}" 
			});
		}
		
		// Select prompts 2, 4, 7, and 9
		// Navigate to prompt 2 and select it
		_testConsole.Input.PushKey(ConsoleKey.DownArrow);
		_testConsole.Input.PushKey(ConsoleKey.Spacebar);
		
		// Navigate to prompt 4 and select it
		_testConsole.Input.PushKey(ConsoleKey.DownArrow);
		_testConsole.Input.PushKey(ConsoleKey.DownArrow);
		_testConsole.Input.PushKey(ConsoleKey.Spacebar);
		
		// Navigate to prompt 7 and select it
		_testConsole.Input.PushKey(ConsoleKey.DownArrow);
		_testConsole.Input.PushKey(ConsoleKey.DownArrow);
		_testConsole.Input.PushKey(ConsoleKey.DownArrow);
		_testConsole.Input.PushKey(ConsoleKey.Spacebar);
		
		// Navigate to prompt 9 and select it
		_testConsole.Input.PushKey(ConsoleKey.DownArrow);
		_testConsole.Input.PushKey(ConsoleKey.DownArrow);
		_testConsole.Input.PushKey(ConsoleKey.Spacebar);
		
		_testConsole.Input.PushKey(ConsoleKey.Enter);

		var result = _service.SelectPrompts(testPrompts);
		
		Assert.That(result, Has.Count.EqualTo(4));
		Assert.That(result, Contains.Item(testPrompts[1])); 
		Assert.That(result, Contains.Item(testPrompts[3]));
		Assert.That(result, Contains.Item(testPrompts[6]));
		Assert.That(result, Contains.Item(testPrompts[8]));
	}

	[Test]
	public void ShouldSetVerdictAndCommentWhenCollectingPromptMetadata()
	{
		// Arrange
		var selectedPrompts = new List<PromptResponsePair>
		{
			new() { Prompt = "Test prompt 1", Response = "Test response 1" },
			new() { Prompt = "Test prompt 2", Response = "Test response 2" }
		};
		
		// Simulate user selecting "Good" for first prompt with comment
		_testConsole.Input.PushTextWithEnter("Y");
		_testConsole.Input.PushTextWithEnter("This is a good prompt");
		
		// Simulate user selecting "Bad" for second prompt with comment
		_testConsole.Input.PushTextWithEnter("N");
		_testConsole.Input.PushTextWithEnter("This is a bad prompt");
		
		// Act
		_service.CollectPromptMetadata(selectedPrompts);
		
		// Assert
		Assert.Multiple(() =>
		{
			// Check first prompt
			Assert.That(selectedPrompts[0].IsSuccess, Is.True);
			Assert.That(selectedPrompts[0].UserComment, Is.EqualTo("This is a good prompt"));
			
			// Check second prompt
			Assert.That(selectedPrompts[1].IsSuccess, Is.False);
			Assert.That(selectedPrompts[1].UserComment, Is.EqualTo("This is a bad prompt"));
		});
	}

	[Test]
	public void ShouldReturnEnteredNumberWhenGettingDayNumber()
	{
		// Arrange
		_testConsole.Input.PushTextWithEnter("10");

		// Act
		var result = _service.GetDayNumber();

		// Assert
		Assert.That(result, Is.EqualTo(10));
	}

	[Test]
	public void ShouldPromptAgainWhenInvalidNumberIsEntered()
	{
		_testConsole.Input.PushTextWithEnter("invalid");
		_testConsole.Input.PushTextWithEnter("15");  // Valid number

		var result = _service.GetDayNumber();

		Assert.That(result, Is.EqualTo(15));
		
		var output = _testConsole.Output;
		var promptCount = output.Split("Enter the day number").Length - 1;
		Assert.That(promptCount, Is.EqualTo(2));
	}
}
