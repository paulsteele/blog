namespace LlmHistoryToPost.Tests.Services;

using LlmHistoryToPost.Models;
using LlmHistoryToPost.Services;
using Moq;

[TestFixture]
public class BlogPostGeneratorTests
{
	private BlogPostGenerator _generator;
	private List<PromptResponsePair> _testPrompts;
	private DateTimeOffset _testDate;
	private int _testDayNumber;

	[SetUp]
	public void Setup()
	{
		_generator = new BlogPostGenerator();
		_testDate = new DateTimeOffset(2025, 4, 1, 12, 0, 0, TimeSpan.FromHours(5));
		_testDayNumber = 7;
		
		_testPrompts =
		[
			new PromptResponsePair()
			{
				Prompt = "Test prompt 1",
				Response = "Test response 1",
				Title = "First Test Prompt",
				IsSuccess = true,
				UserComment = "This worked great"
			},

			new PromptResponsePair()
			{
				Prompt = "Test prompt 2",
				Response = "Test response 2",
				Title = "Second Test Prompt",
				IsSuccess = false,
				UserComment = "This didn't work"
			}
		];
	}

	[Test]
	public void ShouldGenerateCorrectMarkdownWithValidInputs()
	{
		var result = _generator.GenerateBlogPost(_testDate, _testPrompts, _testDayNumber);

		Assert.That(result, Is.Not.Null);
		Assert.That(result, Does.Contain($"title: \"Hour a Day: AI - Day {_testDayNumber} - \""));
		Assert.That(result, Does.Contain("date: 2025-04-01T12:00:00+05:00"));
		Assert.That(result, Does.Contain("## Introduction"));
		Assert.That(result, Does.Contain("## First Test Prompt"));
		Assert.That(result, Does.Contain("## Second Test Prompt"));
		Assert.That(result, Does.Contain("## Conclusion"));
		Assert.That(result, Does.Contain("✅ This worked great"));
		Assert.That(result, Does.Contain("❌ This didn't work"));
	}

	[Test]
	public void ShouldGenerateMinimalMarkdownWithEmptyPromptList()
	{
		var emptyPrompts = new List<PromptResponsePair>();

		var result = _generator.GenerateBlogPost(_testDate, emptyPrompts, _testDayNumber);

		Assert.That(result, Is.Not.Null);
		Assert.That(result, Does.Contain($"title: \"Hour a Day: AI - Day {_testDayNumber} - \""));
		Assert.That(result, Does.Contain("date: 2025-04-01T12:00:00+05:00"));
		Assert.That(result, Does.Contain("## Introduction"));
		Assert.That(result, Does.Contain("## Conclusion"));
		Assert.That(result, Does.Not.Contain("## Prompt"));
	}

	[Test]
	public void ShouldFormatMultilinePromptAndResponseCorrectly()
	{
		// Arrange
		var multilinePrompts = new List<PromptResponsePair>
		{
			new()
			{
				Prompt = "Line 1\nLine 2\nLine 3",
				Response = "Response 1\nResponse 2",
				Title = "Multiline Test",
				IsSuccess = true,
				UserComment = "Comment"
			}
		};

		// Act
		var result = _generator.GenerateBlogPost(_testDate, multilinePrompts, _testDayNumber);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result, Does.Contain("> Line 1"));
		Assert.That(result, Does.Contain("> Line 2"));
		Assert.That(result, Does.Contain("> Line 3"));
		Assert.That(result, Does.Contain("{{< details \"**Response:** (click to expand)\" >}}"));
		Assert.That(result, Does.Contain("> Response 1"));
		Assert.That(result, Does.Contain("> Response 2"));
		Assert.That(result, Does.Contain("{{< /details >}}"));
	}

	[Test]
	public void ShouldReturnCorrectOutputFilePath()
	{
		var result = _generator.GetOutputFilePath(_testDate, _testDayNumber);

		Assert.That(result, Is.Not.Null);
		Assert.That(result, Does.EndWith($"2025-04-01-hadai-day-7-temp.md"));
	}
	
	[Test]
	public void ShouldRemoveMarkdownHeadersFromResponse()
	{
		// Arrange
		var promptsWithHeaders = new List<PromptResponsePair>
		{
			new()
			{
				Prompt = "Test prompt",
				Response = "# Header 1\nNormal text\n  # Indented header\n   #Multiple hashes\nNo header line\n  ##Test#Case",
				Title = "Markdown Header Test",
				IsSuccess = true,
				UserComment = "Testing header removal"
			}
		};

		// Act
		var result = _generator.GenerateBlogPost(_testDate, promptsWithHeaders, _testDayNumber);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result, Does.Contain("> Header 1"));  // # removed
		Assert.That(result, Does.Contain("> Normal text"));  // unchanged
		Assert.That(result, Does.Contain(">   Indented header"));  // # removed, indentation preserved
		Assert.That(result, Does.Contain(">    Multiple hashes"));  // multiple # removed
		Assert.That(result, Does.Contain("> No header line"));  // unchanged
		Assert.That(result, Does.Contain(">   Test#Case"));  // ## at start removed, # in middle preserved
		Assert.That(result, Does.Not.Contain("> # "));  // No # characters should remain at start of lines
	}
}
