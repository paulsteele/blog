namespace LlmHistoryToPost.Services;

using System.Collections.Generic;
using LlmHistoryToPost.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

public class UserInteractionService
{
	private readonly IAnsiConsole _console;

	public UserInteractionService(IAnsiConsole console)
	{
		_console = console ?? throw new ArgumentNullException(nameof(console));
	}
	public DateOnly SelectDay(Dictionary<DateOnly, List<PromptResponsePair>> promptsByDay)
	{
		var days = promptsByDay.Keys.OrderBy(d => d).ToList();
		
		if (days.Count == 0)
		{
			throw new InvalidOperationException("No days with conversations found in the history file.");
		}
		
		if (days.Count == 1)
		{
			_console.MarkupLine($"[green]Only one day found: {days[0]}. Automatically selecting it.[/]");
			return days[0];
		}
		
		return _console.Prompt(
			new SelectionPrompt<DateOnly>()
				.Title("Select a day to process:")
				.PageSize(10)
				.AddChoices(days)
				.UseConverter(d => d.ToString("yyyy-MM-dd"))
		);
	}
	
	public List<PromptResponsePair> SelectPrompts(List<PromptResponsePair> prompts)
	{
		if (prompts.Count == 0)
		{
			throw new InvalidOperationException("No prompts found for the selected day.");
		}
		
		var selectedIndices = _console.Prompt(
			new MultiSelectionPrompt<int>()
				.Title("Select prompts to include in the blog post:")
				.PageSize(15)
				.InstructionsText("[grey](Press [blue]<space>[/] to toggle selection, [green]<enter>[/] to confirm)[/]")
				.AddChoices(Enumerable.Range(0, prompts.Count))
				.UseConverter(i => $"{i + 1}. {Markup.Escape(prompts[i].GetPromptPreview())}")
		);
		
		return selectedIndices.Select(i => prompts[i]).ToList();
	}
	
	public void CollectVerdicts(List<PromptResponsePair> selectedPrompts)
	{
		foreach (var pair in selectedPrompts)
		{
			_console.Clear();
			
			_console.MarkupLine("[yellow]===== PROMPT =====[/]");
			_console.WriteLine(pair.Prompt);
			
			_console.MarkupLine("\n[yellow]===== RESPONSE =====[/]");
			_console.WriteLine(pair.Response);
			
			pair.IsSuccess = _console.Confirm("Was this a success?");
			
			pair.UserComment = _console.Ask<string>("Enter your comment for this verdict:");
		}
	}
	
	public int GetDayNumber()
	{
		return _console.Ask<int>("Enter the day number for the blog post title:");
	}
}
