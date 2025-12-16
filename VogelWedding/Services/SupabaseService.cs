using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Supabase.Gotrue;
using VogelWedding.Model;
using Client = Supabase.Client;

public class SupabaseService(Client client, NavigationManager navigationManager)
{

	public Client Client => client;

	public async Task SubmitRsvpAsync(RsvpEntry entry)
	{
		try
		{
			// Console.WriteLine($"Submitting RSVP to Supabase...");
			// Console.WriteLine($"Payload: {JsonConvert.SerializeObject(entry)}");


			var response = await client
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

	public async Task<List<T>> GetEntriesAsync<T>(CancellationToken cancellationToken = default) where T : BaseModel, new()
	{
		try
		{
			var res = await client.From<T>().Get(cancellationToken);
			return res?.Models ?? new List<T>();
		}
		catch (Exception ex)
		{
			// Log the error or handle it appropriately
			throw new Exception($"Failed to fetch entries of type {typeof(T).Name}", ex);
		}
	}

	public async Task<InvoiceModel?> GetInvoiceByIdAsync(int id, CancellationToken cancellationToken = default)
	{
		try
		{
			var response = await client
				.From<InvoiceModel>()
				.Where(x => x.ID == id)
				.Single(cancellationToken);

			return response;
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to fetch entries of Invoice: {ex}");
			throw;
		}
	}

	public async Task<string> UploadPhotoAsync(IBrowserFile file, string? name, string? comment)
	{
		using var stream = file.OpenReadStream();
		using var memoryStream = new MemoryStream();
		await stream.CopyToAsync(memoryStream);
		// var stream = file.OpenReadStream();
		var bytes = memoryStream.ToArray();

		var path = $"photos/{Guid.NewGuid()}_{file.Name}";

		await client.Storage.From("public").Upload(bytes, path);
		var url = client.Storage.From("public").GetPublicUrl(path);

		await client.From<Photo>().Insert(new Photo
		{
			Name = name, Comment = comment, Url = url
		});

		return url;
	}

	public async Task<List<Photo>> GetPhotosAsync()
	{
		try
		{
			var res = await client.From<Photo>().Get();
			return res?.Models ?? new List<Photo>();
		}
		catch (Exception ex)
		{
			// Log the exception here if you have a logging system
			throw new Exception("Failed to retrieve photos from the database", ex);
		}
	}

	public async Task<AccessLevel> LoginUserAsync(string password)
	{
		const string guestEmail = "guest@example.com";
		const string invitedEmail = "invited@example.com";
		const string testUserAllEmail = "testall@example.com";
		const string testUserEmail = "test@example.com";

		try
		{
			var maybeSessionGuest = await client.Auth.SignIn(email: guestEmail, password: password);

			if (maybeSessionGuest != null) return AccessLevel.GuestAll;
		}
		catch(Exception ex)
		{
			Console.WriteLine($"Could not log in with guest, trying invited now: {ex.Message}");

		}
		
		try
		{
			var maybeSessionInvited = await client.Auth.SignIn(email: testUserAllEmail, password: password);

			if (maybeSessionInvited != null) return AccessLevel.TestUserAll;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Could not log in with TestUser, trying invited now: {ex.Message}");
		}

		try
		{
			var maybeSessionInvited = await client.Auth.SignIn(email: testUserEmail, password: password);

			if (maybeSessionInvited != null) return AccessLevel.TestUserInvited;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Could not log in with TestUserInvited, trying invited now: {ex.Message}");
		}

		try
		{
			var maybeSessionInvited = await client.Auth.SignIn(email: invitedEmail, password: password);

			if (maybeSessionInvited != null) return AccessLevel.GuestInvited;
		}
		catch
		{
			Console.WriteLine($"None of the users were able to log in.");
			return AccessLevel.None;
		}

		return AccessLevel.None;
	}
	
	public async Task<AccessLevel> Logout()
	{
		try
		{
			await client.Auth.SignOut();
			navigationManager.NavigateTo("/", true);
		}
		catch(Exception ex)
		{
			await Console.Error.WriteLineAsync($"Logging out failed: {ex.Message}");
		}
		
		return AccessLevel.None;    
	}


	public async Task<bool> LoginAdminAsync(string? email, string? password)
	{
		try
		{
			await client.Auth.SignIn(email: email, password: password);
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
			var response = await client
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

			await client
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

	public async Task<WishlistItem?> GetWishlistItemAsync(Guid id)
	{
		return await client
			.From<WishlistItem>()
			.Where(x => x.ID == id)
			.Single();
	}

	public async Task SetPurchaseAsync(WishlistPurchase purchase)
	{
		try
		{
			if (purchase.PurchasedAt == default)
			{
				purchase.PurchasedAt = DateTimeOffset.UtcNow;
			}

			await client
				.From<WishlistPurchase>()
				.Insert(purchase);

			var itemResponse = await client
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

					await client
						.From<WishlistItem>()
						.Upsert(itemResponse);
				}
				else
				{
					itemResponse.PaidAmount += purchase.PaidAmount;
					itemResponse.NumberPaidUsers += 1;

					await client
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

				await client
					.From<WishlistItem>()
					.Upsert(itemResponse);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Faild to save purchase to database: {ex.Message}");
		}
	}

	public async Task UpdatePurchaseAsync(WishlistPurchase purchase)
	{
		if (purchase == null || purchase.ID == Guid.Empty)
			throw new ArgumentException("Purchase or Purchase.ID is invalid");

		try
		{
			if (!purchase.EmailSent && purchase.MoneyReceived)
			{
				purchase.MoneyReceived = false;
				purchase.MoneyReceivedDate = null;
			}

			if (purchase.MoneyReceived)
			{
				purchase.MoneyReceivedDate ??= DateTimeOffset.Now;
			}
			else
			{
				purchase.MoneyReceivedDate = null;
			}

			await client
				.From<WishlistPurchase>()
				.Where(x => x.ID == purchase.ID)
				.Update(purchase);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to update WishlistPurchase {purchase.ID}: {ex.Message}");
			throw;
		}
	}
}
