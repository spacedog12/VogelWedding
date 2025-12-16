using Microsoft.AspNetCore.Components.Forms;

namespace VogelWedding.Interfaces;

public interface ISupabasePhotosService
{
	Task<List<string>> UploadFilesAsync(IReadOnlyList<IBrowserFile> files, string folder);
	Task<List<string>> GetImageUrlsAsync(string folder);
}
