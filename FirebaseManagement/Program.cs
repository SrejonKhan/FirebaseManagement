using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FirebaseManagement
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Init();
            await SelectOptionsAsync();
        }

        static void Init()
        {
            Console.Write("Enter Service Key File File Path - "); 
            string fileLocation = Console.ReadLine();

            if (!File.Exists(fileLocation))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("File does not exist. \n\n");
                Console.ResetColor();
                Init();
            }
            else
            {
                if (Path.GetExtension(fileLocation) != ".json")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Key file is not .json file. \n\n");
                    Console.ResetColor();
                    Init();
                }
            }

            try
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(fileLocation),
                });
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Encountered error - {ex.Message}. \n\n");
                Console.ResetColor();
                Init();
            }
        }

        static async Task SelectOptionsAsync()
        {
            Console.WriteLine("Firebase Management");

            string options = "\n" +
                "1. Delete all Anonymous Users \n";
            Console.WriteLine(options);

            Console.Write("Select Option - ");

            if (Int32.TryParse(Console.ReadLine(), out int selectedOption))
            {
                switch (selectedOption-1)
                {
                    case 0:
                        await DeleteAllAnonymousUserAsync();
                        break;
                    default:
                        await SelectOptionsAsync();
                        break;
                }
            }
            else
            {
                Console.WriteLine("Wrong Input!");
                Thread.Sleep(1500);
                Console.Clear();
                await SelectOptionsAsync();
            }
        }

        static async Task DeleteAllAnonymousUserAsync()
        {
            Console.WriteLine("\nPlease wait, it may take some time to delete all users. \n");

            List<string> anonUID = new List<string>();
            // Start listing users from the beginning, 1000 at a time.
            var pagedEnumerable = FirebaseAuth.DefaultInstance.ListUsersAsync(null);
            var responses = pagedEnumerable.AsRawResponses().GetAsyncEnumerator();

            while (await responses.MoveNextAsync())
            {
                ExportedUserRecords response = responses.Current;
                foreach (ExportedUserRecord user in response.Users)
                {
                    if (user.ProviderData.Length == 0)
                        anonUID.Add(user.Uid);
                }
            }

            DeleteUsersResult result = await FirebaseAuth.DefaultInstance.DeleteUsersAsync(anonUID);

            Console.WriteLine($"Successfully deleted {result.SuccessCount} users.");
            Console.WriteLine($"Failed to delete {result.FailureCount} users.");

            foreach (ErrorInfo err in result.Errors)
            {
                Console.WriteLine($"Error #{err.Index}, reason: {err.Reason}");
            }

            Console.ReadKey();
        }
    }
}
