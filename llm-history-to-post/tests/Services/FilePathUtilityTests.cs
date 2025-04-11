using LlmHistoryToPost.Services;
using System.Runtime.InteropServices;

namespace LlmHistoryToPost.Tests.Services;

[TestFixture]
public class FilePathUtilityTests
{
	private string _testDirectory;
	private string _originalDirectory;
	
	// Helper method to normalize paths for comparison on macOS
	private static bool PathsAreEqual(string path1, string path2)
	{
		// Normalize paths to handle macOS /private prefix
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			// Remove "/private" prefix if it exists
			path1 = path1.Replace("/private/var", "/var");
			path2 = path2.Replace("/private/var", "/var");
		}
		
		return Path.GetFullPath(path1) == Path.GetFullPath(path2);
	}

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
		Assert.That(PathsAreEqual(result!, testFilePath), Is.True, 
			$"Paths don't match: '{result}' vs '{testFilePath}'");
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
		var result = FilePathUtility.FindOrCreateBlogPostDirectory(2025, 4);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(PathsAreEqual(result!, monthDir), Is.True,
			$"Paths don't match: '{result}' vs '{monthDir}'");
	}

	[Test]
	public void ShouldThrowExceptionWhenContentDirectoryDoesNotExist()
	{
		// Arrange - don't create any directories
		
		// Act & Assert
		var exception = Assert.Throws<DirectoryNotFoundException>(() => 
			FilePathUtility.FindOrCreateBlogPostDirectory(2025, 4));
		
		Assert.That(exception.Message, Does.Contain("content"));
	}
	
	[Test]
	public void ShouldCreateDateFoldersWhenContentDirectoryExists()
	{
		// Arrange
		// Create only the content directory but not the year/month structure
		var contentDir = Path.Combine(_testDirectory, "content");
		Directory.CreateDirectory(contentDir);
		
		// Expected path that should be created
		var expectedPath = Path.Combine(contentDir, "post", "2025", "04");
		
		// Act
		var result = FilePathUtility.FindOrCreateBlogPostDirectory(2025, 4);
		
		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(Directory.Exists(result), Is.True);
		Assert.That(PathsAreEqual(result!, expectedPath), Is.True,
			$"Paths don't match: '{result}' vs '{expectedPath}'");
	}
	
	[Test]
	public void ShouldFindContentDirectoryInParentDirectory()
	{
		// Arrange
		// Create a nested directory structure with content directory at the top
		var contentDir = Path.Combine(_testDirectory, "content");
		Directory.CreateDirectory(contentDir);
		
		// Create a nested subdirectory structure
		var subDir1 = Path.Combine(_testDirectory, "subdir1");
		var subDir2 = Path.Combine(subDir1, "subdir2");
		Directory.CreateDirectory(subDir2);
		
		// Change current directory to the nested subdirectory
		var originalDir = Environment.CurrentDirectory;
		Environment.CurrentDirectory = subDir2;
		
		try
		{
			// Expected path that should be found and created
			var expectedPath = Path.Combine(contentDir, "post", "2025", "04");
			
			// Act
			var result = FilePathUtility.FindOrCreateBlogPostDirectory(2025, 4);
			
			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(Directory.Exists(result), Is.True);
			Assert.That(PathsAreEqual(result!, expectedPath), Is.True,
				$"Paths don't match: '{result}' vs '{expectedPath}'");
		}
		finally
		{
			// Restore the current directory even if the test fails
			Environment.CurrentDirectory = originalDir;
		}
	}
}
