namespace LlmHistoryToPost.Services;

using System.Text;
using LlmHistoryToPost.Models;

public partial class BlogPostGenerator
{
	public string GenerateBlogPost(
		DateTimeOffset date, 
		List<PromptResponsePair> selectedPrompts, 
		int dayNumber)
	{
		var sb = new StringBuilder();
		
		// YAML frontmatter
		sb.AppendLine("---");
		sb.AppendLine($"title: \"Hour a Day: AI - Day {dayNumber} - \"");
		sb.AppendLine($"date: {date:yyyy'-'MM'-'dd'T'HH':'mm':'ssK}");
		sb.AppendLine("categories:");
		sb.AppendLine("  - \"Hour a Day: AI\"");
		sb.AppendLine("tags:");
		sb.AppendLine("  - ai");
		sb.AppendLine("---");
		sb.AppendLine();
		
		// Introduction
		sb.AppendLine("## Introduction");
		sb.AppendLine();
		
		// Prompt-response pairs
		for (int i = 0; i < selectedPrompts.Count; i++)
		{
			var pair = selectedPrompts[i];
			
			// Use the title exactly as provided by the user
			sb.AppendLine($"## {pair.Title}");
			sb.AppendLine();
			sb.AppendLine("> **Prompt:**");
			sb.AppendLine(">");
			
			// Format the prompt with proper line breaks
			foreach (var line in pair.Prompt.Split('\n'))
			{
				sb.AppendLine($"> {line}");
			}
			
			sb.AppendLine(">");
			sb.AppendLine();
			sb.AppendLine("{{< details \"**Response:** (click to expand)\" >}}");
			
			foreach (var line in pair.Response.Split('\n'))
			{
				// Use regex to replace leading # characters while preserving indentation
				var processedLine = StartOfLineRegex().Replace(line, "$1");
				
				sb.AppendLine($"> {processedLine}");
			}
			
			sb.AppendLine("{{< /details >}}");
			
			sb.AppendLine();
			
			// Verdict
			var verdictEmoji = pair.IsSuccess == true ? "✅" : "❌";
			sb.AppendLine($"**Verdict:** {verdictEmoji} {pair.UserComment}");
			sb.AppendLine();
		}
		
		// Conclusion
		sb.AppendLine("## Conclusion");
		sb.AppendLine();
		
		return sb.ToString();
	}
	
	public string GetOutputFilePath(DateTimeOffset date, int dayNumber)
	{
		var year = date.Year;
		var month = date.Month;
		var day = date.Day.ToString("00");
		
		var directory = FilePathUtility.FindOrCreateBlogPostDirectory(year, month);
		
		// Format month as two digits for the filename
		var monthStr = month.ToString("00");
		
		return Path.Combine(directory, $"{year}-{monthStr}-{day}-hadai-day-{dayNumber}-temp.md");
	}

    [System.Text.RegularExpressions.GeneratedRegex(@"^(\s*)#+\s*")]
    private static partial System.Text.RegularExpressions.Regex StartOfLineRegex();
}
