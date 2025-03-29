using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LlmHistoryToPost
{
	public class BlogPostGenerator
	{
		public string Generate(DateTime date, List<Conversation> conversations)
		{
			// Create the output directory if it doesn't exist
			string year = date.Year.ToString();
			string month = date.Month.ToString("00");
			string day = date.Day.ToString("00");
			
			string directoryPath = Path.Combine("content", "posts", year, month);
			Directory.CreateDirectory(directoryPath);
			
			// Determine the day number (this is a simplification - in a real app you might want to track this better)
			int dayNumber = 1; // Default to 1 if we can't determine
			
			string fileName = $"{year}-{month}-{day}-hadai-day-{dayNumber}.md";
			string outputPath = Path.Combine(directoryPath, fileName);
			
			var sb = new StringBuilder();
			
			// Add YAML frontmatter
			sb.AppendLine("---");
			sb.AppendLine($"title: \"HADAI Day {dayNumber}\"");
			sb.AppendLine($"date: {year}-{month}-{day}");
			sb.AppendLine("categories:");
			sb.AppendLine("  - HADAI");
			sb.AppendLine("tags:");
			sb.AppendLine("  - AI");
			sb.AppendLine("  - LLM");
			sb.AppendLine("---");
			sb.AppendLine();
			
			// Introduction
			sb.AppendLine("## Introduction");
			sb.AppendLine();
			sb.AppendLine("Today I worked with an AI assistant on various tasks. Here's a summary of our interactions.");
			sb.AppendLine();
			
			// Conversations
			foreach (var conversation in conversations)
			{
				sb.AppendLine("## Prompt and Response");
				sb.AppendLine();
				sb.AppendLine("> Prompt:");
				sb.AppendLine(">");
				
				// Format the prompt with proper indentation for blockquote
				foreach (var line in conversation.Prompt.Split('\n'))
				{
					sb.AppendLine($"> {line}");
				}
				
				sb.AppendLine(">");
				sb.AppendLine("> Response:");
				sb.AppendLine(">");
				
				// Format the response with proper indentation for blockquote
				foreach (var line in conversation.Response.Split('\n'))
				{
					sb.AppendLine($"> {line}");
				}
				
				sb.AppendLine();
				
				// Add verdict
				if (conversation.Verdict != null)
				{
					sb.AppendLine(conversation.Verdict.ToString());
					sb.AppendLine();
				}
			}
			
			// Conclusion
			sb.AppendLine("## Conclusion");
			sb.AppendLine();
			sb.AppendLine("This concludes today's interactions with the AI assistant.");
			
			// Write to file
			File.WriteAllText(outputPath, sb.ToString());
			
			return outputPath;
		}
	}
}
