namespace LlmHistoryToPost;

using Services;
using Spectre.Console;
using System.IO;

public static class Program
{
	public static void Main(string[] args)
	{
		try
		{
			// Get the input file path
			var inputFilePath = GetInputFilePath(args);
			
			// Parse the chat history
			var parser = new ChatHistoryParser();
			var history = parser.ParseHistoryFile(inputFilePath);
			
			// User interactions
			var userInteractionService = new UserInteractionService();
			
			// Select a day
			var selectedDay = userInteractionService.SelectDay(history.PromptsByDay);
			AnsiConsole.MarkupLine($"[green]Selected day: {selectedDay}[/]");
			
			// Select prompts for that day
			var promptsForDay = history.PromptsByDay[selectedDay];
			var selectedPrompts = userInteractionService.SelectPrompts(promptsForDay);
			
			if (selectedPrompts.Count == 0)
			{
				AnsiConsole.MarkupLine("[red]No prompts selected. Exiting.[/]");
				return;
			}
			
			// Collect verdicts
			userInteractionService.CollectVerdicts(selectedPrompts);
			
			// Get introduction and conclusion
			var dayNumber = userInteractionService.GetDayNumber();
			
			// Generate blog post
			var generator = new BlogPostGenerator();
			var blogPostContent = generator.GenerateBlogPost(
				selectedDay, 
				selectedPrompts, 
				dayNumber);
			
			// Save to file
			var outputFilePath = generator.GetOutputFilePath(selectedDay, dayNumber);
			File.WriteAllText(outputFilePath, blogPostContent);
			
			AnsiConsole.MarkupLine($"[green]Blog post generated successfully: {outputFilePath}[/]");
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
			AnsiConsole.MarkupLine($"[red]Error: {ex.StackTrace}[/]");
		}
	}
	
	private static string GetInputFilePath(string[] args)
	{
		if (args.Length > 0)
		{
			return args[0];
		}
		
		// Look for .aider.chat.history.md in the directory tree
		var historyFilePath = FilePathUtility.FindFileInDirectoryTree(".aider.chat.history.md");
		
		if (historyFilePath != null)
		{
			return historyFilePath;
		}
		
		// If not found, default to current directory
		return Path.Combine(Directory.GetCurrentDirectory(), ".aider.chat.history.md");
	}
}
