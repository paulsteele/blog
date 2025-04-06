using LlmHistoryToPost.Models;
using LlmHistoryToPost.Services;
using Spectre.Console;

namespace LlmHistoryToPost;

public static class Program
{
	private const string DefaultHistoryFileName = ".aider.chat.history.md";

	public static void Main(string[] args)
	{
		var console = AnsiConsole.Create(new AnsiConsoleSettings());
		
		try
		{
			ProcessChatHistory(args, console);
		}
		catch (Exception ex)
		{
			console.MarkupLine($"[red]Error: {ex.Message}[/]");
			console.MarkupLine($"[red]Error: {ex.StackTrace}[/]");
		}
	}

	private static void ProcessChatHistory(string[] args, IAnsiConsole console)
	{
		var content = GetInputFileContent(args);
		if (string.IsNullOrEmpty(content))
		{
			console.MarkupLine("[red]Empty chat history. Exiting.[/]");
			return;
		}
		
		var parser = new ChatHistoryParser();
		var history = parser.ParseHistoryContent(content);
		
		var userInteractionService = new UserInteractionService(console);
		
		var selectedDay = userInteractionService.SelectDay(history.PromptsByDay);
		console.MarkupLine($"[green]Selected day: {selectedDay}[/]");
		
		var promptsForDay = history.PromptsByDay[selectedDay];
		var selectedPrompts = userInteractionService.SelectPrompts(promptsForDay);
		
		if (selectedPrompts.Count == 0)
		{
			console.MarkupLine("[red]No prompts selected. Exiting.[/]");
			return;
		}
		
		userInteractionService.CollectVerdicts(selectedPrompts);
		
		var dayNumber = userInteractionService.GetDayNumber();
		GenerateAndSaveBlogPost(console, selectedPrompts, dayNumber);
	}
	
	private static void GenerateAndSaveBlogPost(IAnsiConsole console, List<PromptResponsePair> selectedPrompts, int dayNumber)
	{
		var generator = new BlogPostGenerator();
		var date = DateTimeOffset.Now;
		var blogPostContent = generator.GenerateBlogPost(
			date, 
			selectedPrompts, 
			dayNumber);
		
		var outputFilePath = generator.GetOutputFilePath(date, dayNumber);
		File.WriteAllText(outputFilePath, blogPostContent);
		
		console.MarkupLine($"[green]Blog post generated successfully: {outputFilePath}[/]");
	}
	
	private static string GetInputFileContent(string[] args)
	{
		var filePath = DetermineFilePath(args);
		
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"Chat history file not found: {filePath}");
		}
		
		return File.ReadAllText(filePath);
	}
	
	private static string DetermineFilePath(string[] args)
	{
		if (args.Length > 0)
		{
			return args[0];
		}
		
		var historyFilePath = FilePathUtility.FindFileInDirectoryTree(DefaultHistoryFileName);
		return historyFilePath ?? Path.Combine(Directory.GetCurrentDirectory(), DefaultHistoryFileName);
	}
}
