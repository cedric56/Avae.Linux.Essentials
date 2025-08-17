using Microsoft.Maui.Devices;
using System.Diagnostics;
using Tmds.DBus.Protocol;
using Tmds.DBus.SourceGenerator;
using VCardParser.Helpers;

namespace Microsoft.Maui.ApplicationModel.Communication
{
    public partial class ContactsImplementation
	{
		public async Task<IEnumerable<Contact>> GetGnomeAllAsync(CancellationToken cancellationToken = default)
		{
			using var connection = new Connection(Address.Session);

			try
			{
				await connection.ConnectAsync();
				var activatables = await connection.ListActivatableServicesAsync();

				if (DeviceInfo.Current is DeviceInfoImplementation implementation)
				{
					if (implementation.Desktop == Desktop.WSL)
					{
						if (Process.GetProcessesByName("evolution").Length == 0)
						{
							// If Evolution is not running, we need to start it
							ExecuteBashCommand("E_BOOK_DEBUG=1 evolution");
						}
					}

					var activatable = activatables.FirstOrDefault(a => a.Contains("org.gnome.evolution.dataserver.Source"));

					var uids = new Dictionary<string, string>();
					var objectManager = new OrgFreedesktopDBusObjectManagerProxy(connection, activatable, "/org/gnome/evolution/dataserver/SourceManager");
					var sources = await objectManager.GetManagedObjectsAsync();
					foreach (var entry in sources)
					{
						var objectPath = entry.Key;
						var interfaces = entry.Value;

						if (interfaces.ContainsKey("org.gnome.evolution.dataserver.Source"))
						{
							var source = new OrgGnomeEvolutionDataserverSourceProxy(connection, activatable, objectPath);

							try
							{
								var uid = await source.GetUIDPropertyAsync();

								var data = await source.GetDataPropertyAsync();
								if (data.Contains("[Address Book]"))
								{
									var displayName = GetDisplayName(data);
									uids.Add(uid, displayName);
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine($"Failed to get UID for {objectPath}: {ex.Message}");
							}
						}
					}


					activatable = activatables.FirstOrDefault(a => a.Contains("org.gnome.evolution.dataserver.AddressBook"));


					var dictionary = new Dictionary<string, IEnumerable<Contact>>();

					foreach (var uid in uids)
					{
						try
						{
							var contacts = await GetGnomeContacts(connection, activatable, uid.Key, cancellationToken);
							dictionary.Add(uid.Value, contacts);
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Failed to get contacts: {ex.Message}");
						}
					}
					if (!cancellationToken.IsCancellationRequested)
					{
						if (dictionary.Count > 1)
						{
							var account = await AccountPicker.PickAccountAsync(dictionary.Keys);
							if (account != null && dictionary.ContainsKey(account))
							{
								return dictionary[account];
							}
						}
						else if (dictionary.Count == 1)
						{
							var contact = dictionary.First();
							return contact.Value;
						}
						else
						{
							return await GetGnomeContacts(connection, activatable, "system-address-book", cancellationToken);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			return Enumerable.Empty<Contact>();
		}

		private async Task<IEnumerable<Contact>> GetGnomeContacts(Connection connection, string activatable, string uid, CancellationToken cancellation)
		{
			while (!cancellation.IsCancellationRequested)
			{
				try
				{
					var factory = new OrgGnomeEvolutionDataserverAddressBookFactoryProxy(connection,
								activatable,
								"/org/gnome/evolution/dataserver/AddressBookFactory");

					var token = new CancellationTokenSource();
					token.CancelAfter(TimeSpan.FromSeconds(1));
					var book = await factory.OpenAddressBookAsync(uid).WaitAsync(token.Token);

					var contactsService = new OrgGnomeEvolutionDataserverAddressBookProxy(connection, activatable, book.ObjectPath);
					token = new CancellationTokenSource();
					token.CancelAfter(TimeSpan.FromSeconds(2));
					var list = await contactsService.GetContactListAsync("").WaitAsync(token.Token);
					var vcards = new List<VCardParser.Models.Contact>();
					foreach (var contact in list)
					{
						vcards.Add(contact.DecodeVCard());
					}
					return vcards.Select(c =>
					{
						return new Contact(c.Title, "", c.FormattedName, "", c.LastName, "",
							c.Phones.Select(p => new ContactPhone(p.Number)),
							c.Emails.Select(e => new ContactEmail(e.Address)),
							c.FormattedName);
					});
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Failed to get book: {ex.Message}");
					await Task.Delay(1000); // Wait before retrying
				}
			}
			return Enumerable.Empty<Contact>();
		}
	}
}
