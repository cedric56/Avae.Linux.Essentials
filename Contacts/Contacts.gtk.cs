using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Microsoft.Maui.Devices;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Microsoft.Maui.ApplicationModel.Communication
{
    public interface IAccountPicker
    {
        Task<string?> PickAccountAsync(IEnumerable<string> accounts);
        Task<Contact?> PickContactAsync(IEnumerable<Contact> contacts);
    }

    class AccountPickerImplementation : IAccountPicker
    {
        public Task<string?> PickAccountAsync(IEnumerable<string> accounts)
        {
            var window = new ContactsImplementation.ItemSelectionWindow(accounts);
            return window.ShowDialog<string?>((Avalonia.Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow);
        }
        public Task<Contact?> PickContactAsync(IEnumerable<Contact> contacts)
        {
            var window = new ContactsImplementation.ItemSelectionWindow(contacts);
            return window.ShowDialog<Contact?>((Avalonia.Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow);
        }
    }

    partial class ContactsImplementation : IContacts
    {
        IAccountPicker AccountPicker { get; }

        public ContactsImplementation(IAccountPicker? accountPicker = null)
        {
            AccountPicker = accountPicker ?? new AccountPickerImplementation();
        }

        public async Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            if (DeviceInfo.Current is DeviceInfoImplementation implementation)
            {
                if (implementation.Desktop == Desktop.Gnome ||
                    implementation.Distribution == Distribution.Ubuntu)
                    return await GetGnomeAllAsync(cancellationToken);
                else if (implementation.Desktop == Desktop.KDE)
                    return await GetKdeAllAsync(cancellationToken);
            }

            return Enumerable.Empty<Contact>();
        }

        public async Task<Contact?> PickContactAsync()
        {
            var contacts = await GetAllAsync();
            if (contacts.Any())
                return await AccountPicker.PickContactAsync(contacts);
            return null!;
        }

        static void ExecuteBashCommand(string command)
        {
            // according to: https://stackoverflow.com/a/15262019/637142
            // thans to this we will pass everything as one command
            command = command.Replace("\"", "\"\"");

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"" + command + "\"",
                    //UseShellExecute = false,
                    //RedirectStandardOutput = true,
                    //CreateNoWindow = true
                }
            };
            proc.Start();
        }

        static string GetDisplayName(string configData)
        {
            // Regular expression to match DisplayName
            string pattern = @"DisplayName=([^\n\r]+)";
            Match match = Regex.Match(configData, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return null!; // Return null if DisplayName is not found
        }

        public class ItemSelectionWindow : Window
        {
            public ItemSelectionWindow(IEnumerable<string> addresses)
                : base()
            {
                Title = "Select an account";
                Width = 400;
                Height = 300;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;

                var listBox = new Avalonia.Controls.ListBox
                {
                    ItemsSource = addresses,
                };

                var okButton = new Avalonia.Controls.Button
                {
                    Content = "OK",
                    Margin = new Thickness(0, 0, 5, 0)
                };
                okButton.Click += (_, _) => Close(listBox.SelectedItem);

                var cancelButton = new Avalonia.Controls.Button
                {
                    Content = "Cancel"
                };
                cancelButton.Click += (_, _) => Close();

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Children = { okButton, cancelButton }
                };

                var mainPanel = new Grid
                {
                    RowDefinitions = new RowDefinitions("*, Auto"),
                    ColumnDefinitions = new ColumnDefinitions("*")
                };
                Grid.SetRow(listBox, 0);
                Grid.SetRow(buttonPanel, 1);
                mainPanel.Children.Add(listBox);
                mainPanel.Children.Add(buttonPanel);

                Content = mainPanel;
            }

            public ItemSelectionWindow(IEnumerable<Contact> contacts)
                : base()
            {
                Title = "Select a contact";
                Width = 400;
                Height = 300;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;

                var listBox = new Avalonia.Controls.ListBox
                {
                    ItemsSource = contacts,
                };

                var okButton = new Avalonia.Controls.Button
                {
                    Content = "OK",
                    Margin = new Thickness(0, 0, 5, 0)
                };
                okButton.Click += (_, _) => Close(listBox.SelectedItem);

                var cancelButton = new Avalonia.Controls.Button
                {
                    Content = "Cancel"
                };
                cancelButton.Click += (_, _) => Close();

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Children = { okButton, cancelButton }
                };

                var mainPanel = new Grid
                {
                    RowDefinitions = new RowDefinitions("*, Auto"),
                    ColumnDefinitions = new ColumnDefinitions("*")
                };
                Grid.SetRow(listBox, 0);
                Grid.SetRow(buttonPanel, 1);
                mainPanel.Children.Add(listBox);
                mainPanel.Children.Add(buttonPanel);

                Content = mainPanel;
            }
        }
    }
}
