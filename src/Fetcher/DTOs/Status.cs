namespace Fetcher.DTOs;

public sealed record Status(Guid Id, string Path, bool WasSuccess, DateTime CreationDate)
{
    public string? ErrorMessage { get; set; }
}
