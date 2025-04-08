namespace LlmHistoryToPost.Models;

public record PromptResponsePair
{
	public string Prompt { get; init; } = string.Empty;
	public string Response { get; init; } = string.Empty;
	public bool? IsSuccess { get; set; }
	public string UserComment { get; set; } = string.Empty;
	
	public string GetPromptPreview(int maxLength = 100)
	{
		if (maxLength <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length must be positive");
		}
		
		if (Prompt.Length <= maxLength)
		{
			return Prompt;
		}
		
		return Prompt[..(maxLength - 3)] + "...";
	}
}
