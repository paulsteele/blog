namespace LlmHistoryToPost.Services;

using System.Collections.Generic;
using LlmHistoryToPost.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

public class UserInteractionService
{
	public DateOnly SelectDay(Dictionary<DateOnly, List<PromptResponsePair>> promptsByDay)
	{
		var days = promptsByDay.Keys.OrderBy(d => d).ToList();
		
		if (days.Count == 0)
		{
			throw new InvalidOperationException("No days with conversations found in the history file.");
		}
		
		if (days.Count == 1)
		{
			AnsiConsole.MarkupLine($"[green]Only one day found: {days[0]}. Automatically selecting it.[/]");
			return days[0];
		}
		
		return AnsiConsole.Prompt(
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
		
		var selectedIndices = AnsiConsole.Prompt(
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
			AnsiConsole.Clear();
			
			AnsiConsole.MarkupLine("[yellow]===== PROMPT =====[/]");
			AnsiConsole.WriteLine(pair.Prompt);
			
			AnsiConsole.MarkupLine("\n[yellow]===== RESPONSE =====[/]");
			AnsiConsole.WriteLine(pair.Response);
			
			pair.IsSuccess = AnsiConsole.Confirm("Was this a success?");
			
			pair.UserComment = AnsiConsole.Ask<string>("Enter your comment for this verdict:");
		}
	}
	
	public int GetDayNumber()
	{
		return AnsiConsole.Ask<int>("Enter the day number for the blog post title:");
	}
}
