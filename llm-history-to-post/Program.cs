namespace LlmHistoryToPost;

using LlmHistoryToPost.Services;
using Spectre.Console;

public class Program
{
	public static void Main(string[] args)
	{
		try
		{
			// Get the input file path
			string inputFilePath = GetInputFilePath(args);
			
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
			var introduction = userInteractionService.GetIntroduction();
			var conclusion = userInteractionService.GetConclusion();
			var dayNumber = userInteractionService.GetDayNumber();
			
			// Generate blog post
			var generator = new BlogPostGenerator();
			var blogPostContent = generator.GenerateBlogPost(
				selectedDay, 
				selectedPrompts, 
				introduction, 
				conclusion,
				dayNumber);
			
			// Save to file
			var outputFilePath = generator.GetOutputFilePath(selectedDay, dayNumber);
			File.WriteAllText(outputFilePath, blogPostContent);
			
			AnsiConsole.MarkupLine($"[green]Blog post generated successfully: {outputFilePath}[/]");
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
			if (ex.InnerException != null)
			{
				AnsiConsole.MarkupLine($"[red]Inner error: {ex.InnerException.Message}[/]");
			}
		}
	}
	
	private static string GetInputFilePath(string[] args)
	{
		if (args.Length > 0)
		{
			return args[0];
		}
		
		// Default to the current directory
		return Path.Combine(Directory.GetCurrentDirectory(), ".aider.chat.history.md");
	}
}
