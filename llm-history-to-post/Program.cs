using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spectre.Console;

namespace LlmHistoryToPost
{
	class Program
	{
		static async Task Main(string[] args)
		{
			try
			{
				string historyFilePath = args.Length > 0 
					? args[0] 
					: Path.Combine(Directory.GetCurrentDirectory(), ".aider.chat.history.md");

				if (!File.Exists(historyFilePath))
				{
					AnsiConsole.MarkupLine($"[red]Error:[/] History file not found at {historyFilePath}");
					return;
				}

				var parser = new ChatHistoryParser();
				var chatHistory = await parser.ParseAsync(historyFilePath);
				
				// Group by day
				var dayGroups = chatHistory.GroupByDay();
				
				if (!dayGroups.Any())
				{
					AnsiConsole.MarkupLine("[yellow]No chat history days found in the file.[/]");
					return;
				}

				// Day selection
				var selectedDay = SelectDay(dayGroups);
				if (selectedDay == null)
				{
					AnsiConsole.MarkupLine("[yellow]No day selected. Exiting.[/]");
					return;
				}

				// Prompt selection
				var selectedPrompts = SelectPrompts(selectedDay);
				if (!selectedPrompts.Any())
				{
					AnsiConsole.MarkupLine("[yellow]No prompts selected. Exiting.[/]");
					return;
				}

				// Collect verdicts
				CollectVerdicts(selectedPrompts);

				// Generate blog post
				var blogPostGenerator = new BlogPostGenerator();
				var outputPath = blogPostGenerator.Generate(selectedDay.Date, selectedPrompts);

				AnsiConsole.MarkupLine($"[green]Blog post generated successfully at:[/] {outputPath}");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
			}
		}

		private static DayGroup SelectDay(List<DayGroup> dayGroups)
		{
			if (dayGroups.Count == 1)
			{
				AnsiConsole.MarkupLine($"[green]Only one day found. Automatically selecting:[/] {dayGroups[0].Date:yyyy-MM-dd}");
				return dayGroups[0];
			}

			var selectedDay = AnsiConsole.Prompt(
				new SelectionPrompt<DayGroup>()
					.Title("Select a day to process:")
					.PageSize(10)
					.UseConverter(day => $"{day.Date:yyyy-MM-dd} ({day.Conversations.Count} conversations)")
					.AddChoices(dayGroups));

			return selectedDay;
		}

		private static List<Conversation> SelectPrompts(DayGroup dayGroup)
		{
			var choices = dayGroup.Conversations
				.Select((conv, index) => new { Index = index, Conversation = conv })
				.ToList();

			var selectedIndices = AnsiConsole.Prompt(
				new MultiSelectionPrompt<int>()
					.Title($"Select prompts from {dayGroup.Date:yyyy-MM-dd}:")
					.PageSize(15)
					.InstructionsText("[grey](Press <space> to select, <enter> to confirm)[/]")
					.UseConverter(index => 
					{
						var prompt = choices[index].Conversation.Prompt;
						return $"{index + 1}. {(prompt.Length > 100 ? prompt.Substring(0, 97) + "..." : prompt)}";
					})
					.AddChoices(choices.Select(c => c.Index)));

			return selectedIndices.Select(index => dayGroup.Conversations[index]).ToList();
		}

		private static void CollectVerdicts(List<Conversation> conversations)
		{
			foreach (var conversation in conversations)
			{
				AnsiConsole.Clear();
				AnsiConsole.MarkupLine("[blue]===== PROMPT =====[/]");
				AnsiConsole.WriteLine(conversation.Prompt);
				
				AnsiConsole.MarkupLine("\n[green]===== RESPONSE =====[/]");
				AnsiConsole.WriteLine(conversation.Response);

				var isSuccess = AnsiConsole.Confirm("Was this a success?", true);
				
				var comment = AnsiConsole.Ask<string>("Enter a comment for the verdict:", "");
				
				conversation.Verdict = new Verdict
				{
					IsSuccess = isSuccess,
					Comment = comment
				};
			}
		}
	}
}
