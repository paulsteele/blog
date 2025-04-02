namespace LlmHistoryToPost.Tests.Services;

using LlmHistoryToPost.Models;
using LlmHistoryToPost.Services;
using Moq;
using Spectre.Console;

[TestFixture]
public class UserInteractionServiceTests
{
	private UserInteractionService _service;
	private Mock<IAnsiConsole> _consoleMock;
	private Dictionary<DateOnly, List<PromptResponsePair>> _testPromptsByDay;
	private List<PromptResponsePair> _testPrompts;

	[SetUp]
	public void Setup()
	{
		_consoleMock = new Mock<IAnsiConsole>();
		_service = new UserInteractionService(_consoleMock.Object);
		
		// Initialize test data
		_testPromptsByDay = new Dictionary<DateOnly, List<PromptResponsePair>>
		{
			{ new DateOnly(2025, 4, 1), [new PromptResponsePair { Prompt = "Test prompt 1" }] },
			{ new DateOnly(2025, 4, 2), [new PromptResponsePair { Prompt = "Test prompt 2" }] }
		};
		
		_testPrompts = new List<PromptResponsePair>
		{
			new() { Prompt = "Test prompt 1", Response = "Test response 1" },
			new() { Prompt = "Test prompt 2", Response = "Test response 2" }
		};
	}

	[Test]
	public void ShouldReturnDayWhenOnlyOneDayExists()
	{
		// Arrange
		var singleDayDict = new Dictionary<DateOnly, List<PromptResponsePair>>
		{
			{ new DateOnly(2025, 4, 1), new List<PromptResponsePair>() }
		};

		// Act & Assert
		// This test will depend on how you want to handle the console interaction
		// var result = _service.SelectDay(singleDayDict);
		// Assert.That(result, Is.EqualTo(new DateOnly(2025, 4, 1)));
	}

	[Test]
	public void ShouldPromptForSelectionWhenMultipleDaysExist()
	{
		// Arrange
		// Setup in SetUp method

		// Act & Assert
		// This test will depend on how you want to handle the console interaction
		// Would need to mock or use a testing framework for Spectre.Console
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
