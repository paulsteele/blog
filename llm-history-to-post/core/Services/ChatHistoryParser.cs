namespace LlmHistoryToPost.Services;

using System.Text.RegularExpressions;
using LlmHistoryToPost.Models;

public partial class ChatHistoryParser
{
	private static readonly Regex SessionStartRegex = AiderChatRegex();
	private static readonly Regex UserPromptRegex = UserRegex();
	
	public ChatHistory ParseHistoryContent(string content)
	{
		var history = new ChatHistory();
		
		// Parse sessions
		var sessionMatches = SessionStartRegex.Matches(content);
		history.Sessions.AddRange(ParseSessions(content, sessionMatches));
		
		// Group by day
		foreach (var session in history.Sessions)
		{
			var dateOnly = DateOnly.FromDateTime(session.StartTime);
			
			if (!history.PromptsByDay.TryGetValue(dateOnly, out var value))
			{
				value = []; 
				history.PromptsByDay[dateOnly] = value;
			}

            value.AddRange(session.PromptResponsePairs);
		}
		
		return history;
	}
	
	private List<PromptResponsePair> ParsePromptResponsePairs(string sessionContent)
	{
		var pairs = new List<PromptResponsePair>();
		var promptMatches = UserPromptRegex.Matches(sessionContent);
		
		if (promptMatches.Count == 0)
		{
			return pairs;
		}
		
		var currentPrompts = new List<string>();
		
		for (var i = 0; i < promptMatches.Count; i++)
		{
			var currentMatch = promptMatches[i];
			var promptText = currentMatch.Groups[1].Value.Trim();
			
			// Check if this is the start of a new group (not consecutive with previous)
			var isNewGroup = currentPrompts.Count > 0 && i > 0 && !IsConsecutivePrompt(sessionContent, promptMatches[i-1], currentMatch);
			
			// If we're starting a new group, process the previous group first
			if (isNewGroup)
			{
				var response = ExtractResponse(promptMatches, sessionContent, i - 1);
				pairs.Add(new PromptResponsePair
				{
					Prompt = string.Join("\n", currentPrompts),
					Response = response
				});
				
				// Reset for new group
				currentPrompts = [];
			}
			
			// Add current prompt to the group
			currentPrompts.Add(promptText);
			
			// If this is the last prompt, process the final group
			if (i == promptMatches.Count - 1)
			{
				var response = ExtractResponse(promptMatches, sessionContent, i);
				pairs.Add(new PromptResponsePair
				{
					Prompt = string.Join("\n", currentPrompts),
					Response = response
				});
			}
		}
		
		return pairs;
	}
	
	private static string ExtractResponse(MatchCollection promptMatches, string sessionContent, int lastPromptIndex)
	{
		var lastPromptMatch = promptMatches[lastPromptIndex];
		var responseStartIndex = lastPromptMatch.Index + lastPromptMatch.Length;
		
		var responseEndIndex = lastPromptIndex < promptMatches.Count - 1 
			? promptMatches[lastPromptIndex + 1].Index 
			: sessionContent.Length;
		
		return sessionContent[responseStartIndex..responseEndIndex].Trim();
	}
	
	private static bool IsConsecutivePrompt(string content, Match current, Match next)
	{
		// Calculate the text between the end of the current match and the start of the next match
		var endOfCurrentLine = current.Index + current.Length;
		var startOfNextMatch = next.Index;
		
		// Extract the text between the two matches
		var textBetween = content[endOfCurrentLine..startOfNextMatch];
		
		// If there are only newlines and whitespace between matches, they're consecutive
		return textBetween.Trim().Length == 0;
	}

	private List<ChatSession> ParseSessions(string content, MatchCollection sessionMatches)
	{
		var sessions = new List<ChatSession>();
		
		for (var i = 0; i < sessionMatches.Count; i++)
		{
			var sessionMatch = sessionMatches[i];
			var startTimeStr = sessionMatch.Groups[1].Value;
			var startTime = DateTime.Parse(startTimeStr);
			
			var session = new ChatSession
			{
				StartTime = startTime
			};
			
			// Determine the content of this session
			var startIndex = sessionMatch.Index + sessionMatch.Length;
			var endIndex = (i < sessionMatches.Count - 1) 
				? sessionMatches[i + 1].Index 
				: content.Length;
			
			var sessionContent = content.Substring(startIndex, endIndex - startIndex);
			
			// Parse prompt-response pairs
			session.PromptResponsePairs.AddRange(ParsePromptResponsePairs(sessionContent));
			
			sessions.Add(session);
		}
		
		return sessions;
	}
	
	[GeneratedRegex(@"# aider chat started at (\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})", RegexOptions.Compiled)] 
	private static partial Regex AiderChatRegex(); 
	[GeneratedRegex(@"^####\s+(.+)$", RegexOptions.Multiline | RegexOptions.Compiled)] 
	private static partial Regex UserRegex();
}
