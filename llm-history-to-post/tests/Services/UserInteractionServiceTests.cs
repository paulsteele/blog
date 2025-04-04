namespace LlmHistoryToPost.Tests.Services;

using LlmHistoryToPost.Models;
using LlmHistoryToPost.Services;
using Spectre.Console;
using Spectre.Console.Testing;

[TestFixture]
public class UserInteractionServiceTests
{
	private UserInteractionService _service;
	private TestConsole _testConsole;
	private Dictionary<DateOnly, List<PromptResponsePair>> _testPromptsByDay;
	private List<PromptResponsePair> _testPrompts;

	[SetUp]
	public void Setup()
	{
		_testConsole = new TestConsole();
		_service = new UserInteractionService(_testConsole);
		
		// Initialize test data
		_testPromptsByDay = new Dictionary<DateOnly, List<PromptResponsePair>>
		{
			{ new DateOnly(2025, 4, 1), [new PromptResponsePair { Prompt = "Test prompt 1" }] },
			{ new DateOnly(2025, 4, 2), [new PromptResponsePair { Prompt = "Test prompt 2" }] }
		};
		
		_testPrompts =
		[
			new PromptResponsePair { Prompt = "Test prompt 1", Response = "Test response 1" },
			new PromptResponsePair { Prompt = "Test prompt 2", Response = "Test response 2" }
		];
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
			{ new DateOnly(2025, 4, 3), [] }
		};
		
		// Set up the test console to select the second option
		_testConsole.Input.PushKey(ConsoleKey.DownArrow);
		_testConsole.Input.PushKey(ConsoleKey.Enter);

		var result = _service.SelectDay(dict);
		
		Assert.That(result, Is.EqualTo(new DateOnly(2025, 4, 2)));
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
		// Setup in SetUp method

		// Act & Assert
		// This test will depend on how you want to handle the console interaction
		// Would need to mock or use a testing framework for Spectre.Console
	}

	[Test]
	public void ShouldSetVerdictAndCommentWhenCollectingVerdicts()
	{
		// Arrange
		// Setup in SetUp method

		// Act & Assert
		// This test will depend on how you want to handle the console interaction
		// Would need to mock or use a testing framework for Spectre.Console
	}

	[Test]
	public void ShouldReturnEnteredNumberWhenGettingDayNumber()
	{
		// Arrange
		// Setup in SetUp method

		// Act & Assert
		// This test will depend on how you want to handle the console interaction
		// Would need to mock or use a testing framework for Spectre.Console
	}
}
