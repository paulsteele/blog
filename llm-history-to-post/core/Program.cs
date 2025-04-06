using LlmHistoryToPost.Models;

namespace LlmHistoryToPost;

using Services;
using Spectre.Console;
using System;
using System.IO;

public static class Program
{
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
		// Get the input file content
		var content = GetInputFileContent(args);
		
		// Parse the chat history
		var parser = new ChatHistoryParser();
		var history = parser.ParseHistoryContent(content);
		
		// User interactions
		var userInteractionService = new UserInteractionService(console);
		
		// Select a day
		var selectedDay = userInteractionService.SelectDay(history.PromptsByDay);
		console.MarkupLine($"[green]Selected day: {selectedDay}[/]");
		
		// Select prompts for that day
		var promptsForDay = history.PromptsByDay[selectedDay];
		var selectedPrompts = userInteractionService.SelectPrompts(promptsForDay);
		
		if (selectedPrompts.Count == 0)
		{
			console.MarkupLine("[red]No prompts selected. Exiting.[/]");
			return;
		}
		
		// Collect verdicts
		userInteractionService.CollectVerdicts(selectedPrompts);
		
		// Get day number and generate blog post
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
		
		// Save to file
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
		
		// Look for .aider.chat.history.md in the directory tree
		var historyFilePath = FilePathUtility.FindFileInDirectoryTree(".aider.chat.history.md");

		// If not found, default to current directory
		return historyFilePath ?? Path.Combine(Directory.GetCurrentDirectory(), ".aider.chat.history.md");
	}
}
