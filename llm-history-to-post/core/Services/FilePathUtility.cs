namespace LlmHistoryToPost.Services;

public static class FilePathUtility
{
	/// <summary>
	/// Searches for a file by starting in the current directory and moving up through parent directories
	/// until either the file is found or we reach the user's home directory.
	/// </summary>
	/// <param name="fileName">The name of the file to find</param>
	/// <returns>The full path to the file if found, or null if not found</returns>
	public static string? FindFileInDirectoryTree(string fileName)
	{
		var currentDir = Directory.GetCurrentDirectory();
		var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		
		while (!string.IsNullOrEmpty(currentDir) && currentDir.Length >= homeDir.Length)
		{
			var filePath = Path.Combine(currentDir, fileName);
			if (File.Exists(filePath))
			{
				return filePath;
			}
			
			// Move up to parent directory
			var parentDir = Directory.GetParent(currentDir);
			if (parentDir == null)
			{
				break;
			}
			
			currentDir = parentDir.FullName;
		}
		
		return null;
	}
	
	/// <summary>
	/// Finds or creates a directory for blog posts based on the date
	/// </summary>
	/// <param name="year">Year</param>
	/// <param name="month">Month (1-12)</param>
	/// <returns>The full path to the directory</returns>
	public static string FindOrCreateBlogPostDirectory(int year, int month)
	{
		// Start with current directory and look for a content/post directory structure
		var currentDir = Directory.GetCurrentDirectory();
		var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		string? contentDir = null;
		
		while (!string.IsNullOrEmpty(currentDir) && currentDir.Length >= homeDir.Length)
		{
			var possibleContentDir = Path.Combine(currentDir, "content");
			if (Directory.Exists(possibleContentDir))
			{
				contentDir = possibleContentDir;
				break;
			}
			
			// Move up to parent directory
			var parentDir = Directory.GetParent(currentDir);
			if (parentDir == null)
			{
				break;
			}
			
			currentDir = parentDir.FullName;
		}
		
		// If we didn't find a content directory, throw an exception
		if (contentDir == null)
		{
			throw new DirectoryNotFoundException("Could not find 'content' directory in the directory tree.");
		}
		
		// Create the year/month directory structure
		var postDir = Path.Combine(contentDir, "post", year.ToString(), month.ToString("D2"));
		Directory.CreateDirectory(postDir);
		
		return postDir;
	}
}
