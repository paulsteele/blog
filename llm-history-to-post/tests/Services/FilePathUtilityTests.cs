using LlmHistoryToPost.Services;

namespace LlmHistoryToPost.Tests.Services;

[TestFixture]
public class FilePathUtilityTests
{
	private string _testDirectory;
	private string _originalDirectory;

	[SetUp]
	public void Setup()
	{
		// Create a temporary directory structure for testing
		_testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(_testDirectory);
		
		// Store original directory and set current directory to test directory
		_originalDirectory = Environment.CurrentDirectory;
		Environment.CurrentDirectory = _testDirectory;
	}

	[TearDown]
	public void TearDown()
	{
		// Restore original directory
		Environment.CurrentDirectory = _originalDirectory;
		
		// Clean up test directory
		if (Directory.Exists(_testDirectory))
		{
			Directory.Delete(_testDirectory, true);
		}
	}

	[Test]
	public void ShouldReturnFilePathWhenFileExists()
	{
		// Arrange
		var testFileName = "test.txt";
		var testFilePath = Path.Combine(_testDirectory, testFileName);
		File.WriteAllText(testFilePath, "Test content");
		
		// Act
		var result = FilePathUtility.FindFileInDirectoryTree(testFileName);
		
		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result, Is.EqualTo(testFilePath));
	}

	[Test]
	public void ShouldReturnNullWhenFileDoesNotExist()
	{
		// Arrange
		var nonExistentFileName = "nonexistent.txt";
		
		// Act
		var result = FilePathUtility.FindFileInDirectoryTree(nonExistentFileName);
		
		// Assert
		Assert.That(result, Is.Null);
	}

	[Test]
	public void ShouldReturnPathWhenDirectoryExists()
	{
		// Arrange
		// Create a content/post directory structure
		var contentDir = Path.Combine(_testDirectory, "content");
		var postDir = Path.Combine(contentDir, "post");
		var yearDir = Path.Combine(postDir, "2025");
		var monthDir = Path.Combine(yearDir, "04");
		Directory.CreateDirectory(monthDir);

		// Act
		var result = FilePathUtility.FindOrCreateBlogPostDirectory(2025, "04");

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result, Is.EqualTo(monthDir));
	}

	[Test]
	public void ShouldCreateAndReturnPathWhenDirectoryDoesNotExist()
	{
		// Arrange
		// Create only the content directory
		var contentDir = Path.Combine(_testDirectory, "content");
		Directory.CreateDirectory(contentDir);
		
		// Expected path that should be created
		var expectedPath = Path.Combine(_testDirectory, "content", "post", "2025", "04");
		
		// Act
		var result = FilePathUtility.FindOrCreateBlogPostDirectory(2025, "04");
		
		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(Directory.Exists(result), Is.True);
		Assert.That(result, Is.EqualTo(expectedPath));
	}
}
