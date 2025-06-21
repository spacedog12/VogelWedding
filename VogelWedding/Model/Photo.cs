public class Photo : BaseModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public string? Comment { get; set; }
    public string Url { get; set; } = string.Empty;
}