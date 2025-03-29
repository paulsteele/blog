namespace LlmHistoryToPost.Models;

public class ChatSession
{
	public DateTime Date { get; set; }
	public List<PromptResponsePair> Conversations { get; set; } = new();
	
	public string FormattedDate => Date.ToString("yyyy-MM-dd");
}

public class PromptResponsePair
{
	public string Prompt { get; set; } = string.Empty;
	public string Response { get; set; } = string.Empty;
	public bool? IsSuccess { get; set; }
	public string UserComment { get; set; } = string.Empty;
	
	public string PromptPreview => Prompt.Length <= 100 
		? Prompt 
		: Prompt.Substring(0, 97) + "...";
}
