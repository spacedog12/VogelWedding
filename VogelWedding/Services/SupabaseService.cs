using Microsoft.AspNetCore.Components.Forms;
using VogelWedding.Model;
using Newtonsoft.Json;
using VogelWedding.Pages.Wishlist;

public class SupabaseService
{
	private readonly Supabase.Client _client;
	private readonly AccessService _accessService;

	public SupabaseService(Supabase.Client client, AccessService accessService)
	{
		_client = client;
		_accessService = accessService;
	}

	public Supabase.Client Client => _client;

	public async Task SubmitRsvpAsync(RsvpEntry entry)
	{
		try
		{
			// Console.WriteLine($"Submitting RSVP to Supabase...");
			// Console.WriteLine($"Payload: {JsonConvert.SerializeObject(entry)}");


			var response = await _client
				.From<RsvpEntry>()
				.Insert(entry);

			if (response == null || !response.Models.Any())
			{
				Console.WriteLine("No response received from Supabase");
				throw new Exception("No response received from Supabase");
			}
		}
		catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
		{
			// Log detailed errors for connectivity or serialization issues
			Console.WriteLine($"Error occurred: {ex.Message}");
			Console.WriteLine($"Stack trace: {ex.StackTrace}");
			throw new Exception("Failed to connect to Supabase or process the response. See inner exception for details.", ex);
		}
		catch (Exception ex)
		{
			// Generic error handling for unknown issues
			Console.WriteLine($"An unexpected error occurred: {ex.Message}");
			throw new Exception("An error occurred while submitting the RSVP. Please try again later.", ex);
		}

	}

	public async Task<List<RsvpEntry>> GetRsvpEntriesAsync()
	{
		var res = await _client.From<RsvpEntry>().Get();
		return res.Models;
	}

	public async Task<string> UploadPhotoAsync(IBrowserFile file, string? name, string? comment)
	{
		using var stream = file.OpenReadStream();
		using var memoryStream = new MemoryStream();
		await stream.CopyToAsync(memoryStream);
		// var stream = file.OpenReadStream();
		var bytes = memoryStream.ToArray();

		var path = $"photos/{Guid.NewGuid()}_{file.Name}";

		await _client.Storage.From("public").Upload(bytes, path);
		var url = _client.Storage.From("public").GetPublicUrl(path);

		await _client.From<Photo>().Insert(new Photo
		{
			Name = name, Comment = comment, Url = url
		});

		return url;
	}

	public async Task<List<Photo>> GetPhotosAsync()
	{
		try
		{
			var res = await _client.From<Photo>().Get();
			return res?.Models ?? new List<Photo>();
		}
		catch (Exception ex)
		{
			// Log the exception here if you have a logging system
			throw new Exception("Failed to retrieve photos from the database", ex);
		}
	}

	public async Task<bool> LoginAdminAsync(string? email, string? password)
	{
		try
		{
			await _client.Auth.SignIn(email: email, password: password);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public async Task<AppSettings?> GetSettingsAsync()
	{
		try
		{
			Console.WriteLine("Fetching settings from Supabase...");
			var response = await _client
				.From<AppSettings>()
				.Get();

			var settings = response?.Models.FirstOrDefault();

			return settings;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error getting settings: {ex.Message}");
			return new AppSettings
			{
				Id = Guid.NewGuid(), SiteTitle = "Our Wedding", RsvpEnabled = true, NotificationEmail = null
			};
		}
	}

	public async Task UpdateSettingsAsync(AppSettings settings)
	{
		try
		{
			if (settings.Id == Guid.Empty)
			{
				settings.Id = Guid.NewGuid();
			}

			await _client
				.From<AppSettings>()
				.Where(x => x.Id == settings.Id)
				.Update(settings);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error updating settings: {ex.Message}");
			throw;
		}
	}

	public async Task<List<WishlistItem>> GetWishlistItemsAsync()
	{
		var result = await _client.From<WishlistItem>().Get();
		return result.Models;
	}

	// public async Task<List<WishlistImages>> GetWishlistImagesAsync()
	// {
	// 	var result = await _client.From<WishlistImages>().Get();
	// 	return result.Models;
	// }

	public async Task<List<AboutImages>> GetAboutImagesAsync()
	{
		var result = await _client.From<AboutImages>().Get();
		return result.Models;
	}
	
	public async Task<List<FactoryImages>> GetFactoryImagesAsync()
	{
		var result = await _client.From<FactoryImages>().Get();
		return result.Models;
	}

	public async Task<List<InformationImages>> GetInformationImagesAsync()
	{
		var result = await _client.From<InformationImages>().Get();
		return result.Models;
	}

	public async Task SetPurchaseAsync(WishlistPurchase purchase)
	{
		try
		{
			await _client
				.From<WishlistPurchase>()
				.Insert(purchase);

			var itemResponse = await _client
				.From<WishlistItem>()
				.Where(x => x.ID == purchase.WishlistItemId)
				.Single();

			if (itemResponse == null)
			{
				throw new Exception("Whislist item not found");
			}


			if (itemResponse.Quantity == null)
			{
				if (itemResponse.Price.HasValue)
				{
					// itemResponse.Price -= purchase.PaidAmount;
					itemResponse.PaidAmount += purchase.PaidAmount;
					itemResponse.NumberPaidUsers += 1;
					
					await _client
						.From<WishlistItem>()
						.Upsert(itemResponse);
				}
				else
				{
					itemResponse.PaidAmount += purchase.PaidAmount;
					itemResponse.NumberPaidUsers += 1;

					await _client
						.From<WishlistItem>()
						.Upsert(itemResponse);
				}
			}
			else if (itemResponse.Price.HasValue)
			{
				// itemResponse.Quantity -= purchase.Quantity;
				itemResponse.Quantity -= 1;
				itemResponse.PaidAmount += purchase.PaidAmount;
				itemResponse.NumberPaidUsers += 1;

				await _client
					.From<WishlistItem>()
					.Upsert(itemResponse);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Faild to save purchase to database: {ex.Message}");
		}
	}
}
