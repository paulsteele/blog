namespace LlmHistoryToPost.Services;

using System.Collections.Generic;
using Models;
using Spectre.Console;

public class UserInteractionService(IAnsiConsole console)
{
	public DateOnly SelectDay(Dictionary<DateOnly, List<PromptResponsePair>> promptsByDay)
	{
		var days = promptsByDay.Keys.OrderBy(d => d).ToList();
		
		switch (days.Count)
		{
			case 0:
				throw new InvalidOperationException("No days with conversations found in the history file.");
			case 1:
				console.MarkupLine($"[green]Only one day found: {days[0]}. Automatically selecting it.[/]");
				return days[0];
			default:
				return console.Prompt(
					new SelectionPrompt<DateOnly>()
						.Title("Select a day to process:")
						.PageSize(10)
						.AddChoices(days)
						.UseConverter(d => d.ToString("yyyy-MM-dd"))
				);
		}
	}
	
	public List<PromptResponsePair> SelectPrompts(List<PromptResponsePair> prompts)
	{
		if (prompts.Count == 0)
		{
			throw new InvalidOperationException("No prompts found for the selected day.");
		}
		
		var selectedIndices = console.Prompt(
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
			console.Clear();
			
			console.MarkupLine("[yellow]===== PROMPT =====[/]");
			console.WriteLine(pair.Prompt);
			
			console.MarkupLine("\n[yellow]===== RESPONSE =====[/]");
			console.WriteLine(pair.Response);
			
			pair.IsSuccess = console.Confirm("Was this a success?");
			
			pair.UserComment = console.Ask<string>("Enter your comment for this verdict:");
		}
	}
	
	public int GetDayNumber()
	{
		return console.Ask<int>("Enter the day number for the blog post title:");
	}
}
