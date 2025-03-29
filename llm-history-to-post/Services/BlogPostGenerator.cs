namespace LlmHistoryToPost.Services;

using System.Text;
using LlmHistoryToPost.Models;

public class BlogPostGenerator
{
	public string GenerateBlogPost(
		DateOnly date, 
		List<PromptResponsePair> selectedPrompts, 
		string introduction, 
		string conclusion,
		int dayNumber)
	{
		var sb = new StringBuilder();
		
		// YAML frontmatter
		sb.AppendLine("---");
		sb.AppendLine($"title: \"HadAI Day {dayNumber}\"");
		sb.AppendLine($"date: {date:yyyy-MM-dd}");
		sb.AppendLine("categories:");
		sb.AppendLine("  - AI");
		sb.AppendLine("tags:");
		sb.AppendLine("  - HadAI");
		sb.AppendLine("  - LLM");
		sb.AppendLine("---");
		sb.AppendLine();
		
		// Introduction
		sb.AppendLine("## Introduction");
		sb.AppendLine();
		sb.AppendLine(introduction);
		sb.AppendLine();
		
		// Prompt-response pairs
		for (int i = 0; i < selectedPrompts.Count; i++)
		{
			var pair = selectedPrompts[i];
			
			sb.AppendLine($"## Prompt {i + 1}");
			sb.AppendLine();
			sb.AppendLine("> **Prompt:**");
			sb.AppendLine(">");
			
			// Format the prompt with proper line breaks
			foreach (var line in pair.Prompt.Split('\n'))
			{
				sb.AppendLine($"> {line}");
			}
			
			sb.AppendLine(">");
			sb.AppendLine("> **Response:**");
			sb.AppendLine(">");
			
			// Format the response with proper line breaks
			foreach (var line in pair.Response.Split('\n'))
			{
				sb.AppendLine($"> {line}");
			}
			
			sb.AppendLine();
			
			// Verdict
			var verdictEmoji = pair.IsSuccess == true ? "✅" : "❌";
			sb.AppendLine($"**Verdict:** {verdictEmoji} {pair.UserComment}");
			sb.AppendLine();
		}
		
		// Conclusion
		sb.AppendLine("## Conclusion");
		sb.AppendLine();
		sb.AppendLine(conclusion);
		
		return sb.ToString();
	}
	
	public string GetOutputFilePath(DateOnly date, int dayNumber)
	{
		var year = date.Year;
		var month = date.Month.ToString("00");
		var day = date.Day.ToString("00");
		
		var directory = Path.Combine("content", "posts", year.ToString(), month);
		
		// Ensure directory exists
		Directory.CreateDirectory(directory);
		
		return Path.Combine(directory, $"{year}-{month}-{day}-hadai-day-{dayNumber}.md");
	}
}
