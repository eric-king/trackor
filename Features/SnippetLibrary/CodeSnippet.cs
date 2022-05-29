using System.ComponentModel.DataAnnotations;

namespace Trackor.Features.SnippetLibrary;

public class CodeSnippet
{
    public int Id { get; set; }
    public string Label { get; set; }
    public string Content { get; set; }
    public string Language { get; set; }
    public string SourceUrl { get; set; }

    public List<ValidationException> Validate() 
    {
        var exceptions = new List<ValidationException>();

        if (new UrlAttribute().IsValid(SourceUrl) == false) 
        {
            exceptions.Add(new ValidationException($"SourceUrl is not a valid URL: {SourceUrl}"));
        }

        return exceptions;
    }
}
