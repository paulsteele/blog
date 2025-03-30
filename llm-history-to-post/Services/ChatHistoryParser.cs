namespace LlmHistoryToPost.Services;

using System.Text.RegularExpressions;
using LlmHistoryToPost.Models;

public partial class ChatHistoryParser
{
	private static readonly Regex SessionStartRegex = AiderChatRegex();
	private static readonly Regex UserPromptRegex = UserRegex();
	private static readonly Regex ConsecutivePromptLinesRegex = ConsecutivePromptRegex();
	
	public ChatHistory ParseHistoryFile(string filePath)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"Chat history file not found: {filePath}");
		}
		
		var content = File.ReadAllText(filePath);
		var history = new ChatHistory();
		
		// Parse sessions
		var sessionMatches = SessionStartRegex.Matches(content);
		for (int i = 0; i < sessionMatches.Count; i++)
		{
			var sessionMatch = sessionMatches[i];
			var startTimeStr = sessionMatch.Groups[1].Value;
			var startTime = DateTime.Parse(startTimeStr);
			
			var session = new ChatSession
			{
				StartTime = startTime
			};
			
			// Determine the content of this session
			int startIndex = sessionMatch.Index + sessionMatch.Length;
			int endIndex = (i < sessionMatches.Count - 1) 
				? sessionMatches[i + 1].Index 
				: content.Length;
			
			var sessionContent = content.Substring(startIndex, endIndex - startIndex);
			
			// Parse prompt-response pairs
			ParsePromptResponsePairs(sessionContent, session);
			
			history.Sessions.Add(session);
		}
		
		// Group by day
		foreach (var session in history.Sessions)
		{
			var dateOnly = DateOnly.FromDateTime(session.StartTime);
			
			if (!history.PromptsByDay.ContainsKey(dateOnly))
			{
				history.PromptsByDay[dateOnly] = new List<PromptResponsePair>();
			}
			
			history.PromptsByDay[dateOnly].AddRange(session.PromptResponsePairs);
		}
		
		return history;
	}
	
	private void ParsePromptResponsePairs(string sessionContent, ChatSession session)
	{
		var promptMatches = UserPromptRegex.Matches(sessionContent);
		
		for (int i = 0; i < promptMatches.Count; i++)
		{
			// Start of a new prompt group
			var startMatch = promptMatches[i];
			var combinedPrompt = new List<string> { startMatch.Groups[1].Value.Trim() };
			
			// Find the end of this prompt group (all consecutive #### lines)
			int lastPromptIndex = i;
			for (int j = i + 1; j < promptMatches.Count; j++)
			{
				// Check if there's only whitespace between this prompt and the previous one
				if (IsConsecutivePrompt(sessionContent, promptMatches[j-1], promptMatches[j]))
				{
					combinedPrompt.Add(promptMatches[j].Groups[1].Value.Trim());
					lastPromptIndex = j;
				}
				else
				{
					break;
				}
			}
			
			// Determine the response (text between the last prompt line and the next prompt group)
			var lastPromptMatch = promptMatches[lastPromptIndex];
			int responseStartIndex = lastPromptMatch.Index + lastPromptMatch.Length;
			int responseEndIndex = (lastPromptIndex < promptMatches.Count - 1) 
				? promptMatches[lastPromptIndex + 1].Index 
				: sessionContent.Length;
			
			var response = sessionContent.Substring(responseStartIndex, responseEndIndex - responseStartIndex).Trim();
			
			var pair = new PromptResponsePair
			{
				Prompt = string.Join("\n", combinedPrompt),
				Response = response
			};
			
			session.PromptResponsePairs.Add(pair);
			
			// Skip to the end of this prompt group
			i = lastPromptIndex;
		}
	}
	
	private bool IsConsecutivePrompt(string content, Match current, Match next)
	{
		// Calculate the text between the end of the current match and the start of the next match
		int endOfCurrentLine = current.Index + current.Length;
		int startOfNextMatch = next.Index;
		
		// Extract the text between the two matches
		string textBetween = content.Substring(endOfCurrentLine, startOfNextMatch - endOfCurrentLine);
		
		// If there are only newlines and whitespace between matches, they're consecutive
		return textBetween.Trim().Length == 0;
	}

    [GeneratedRegex(@"# aider chat started at (\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})", RegexOptions.Compiled)]
    private static partial Regex AiderChatRegex();
    [GeneratedRegex(@"^####\s+(.+)$", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex UserRegex();
    [GeneratedRegex(@"(^####\s+.+$\n)+", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex ConsecutivePromptRegex();
}
