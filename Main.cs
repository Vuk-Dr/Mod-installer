﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

class Program
{
    public class FileInfo
    {
        public string url { get; set; }
        public string fileName { get; set; }
    }

    static async Task Main()
    {
        string jsonUrl = "https://github.com/Vuk-Dr/Mod-installer/raw/master/files.json";
        string jsonPath = AppDomain.CurrentDomain.BaseDirectory + "files.json";
        

        Console.WriteLine("Unesi putanju do Minecraft root foldera: ");
        string? UserPath = Console.ReadLine();

        while (!Directory.Exists(UserPath + "/versions"))
        {
            Console.WriteLine("Nepravilna putanja, pokušaj ponovo: ");
            UserPath = Console.ReadLine();
        }
        if (!Directory.Exists(UserPath + "/mods")) Directory.CreateDirectory(UserPath + "/mods");

        await DownloadFileWithProgressAsync(jsonUrl, jsonPath);
        string jsonString = File.ReadAllText(jsonPath);
        List<FileInfo> files = JsonSerializer.Deserialize<List<FileInfo>>(jsonString);

        foreach (var file in files) 
        {
            string filePath = UserPath + "/mods/" + file.fileName;
            await DownloadFileWithProgressAsync(file.url, filePath);
        }

        Console.WriteLine("Svi fajlovi uspešno preuzeti.\nSkinuti verziju Minecrafta: Forge 1.20.1");
        Console.ReadLine();
    }



    static async Task DownloadFileWithProgressAsync(string url, string filePath)
    {
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;
            double totalMb = Math.Round((double)totalBytes / 1024 / 1024, 2);

            using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                          fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                var buffer = new byte[8192];  // Buffer size (8 KB)
                long totalRead = 0;
                double totalMbRead;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;
                    totalMbRead = Math.Round((double)totalRead / 1024 / 1024, 2);

                    if (totalBytes.HasValue)
                    {
                        Console.Write($"\rPreuzimanje {Path.GetFileName(filePath)}: {totalMbRead} od {totalMb} Mb. ({(totalRead * 100.0 / totalBytes.Value):0.00}%)");
                    }
                    else
                    {
                        Console.Write($"\rPreuzimanje {Path.GetFileName(filePath)}: {totalMbRead} Mb");
                    }
                }

                Console.WriteLine();
            }
        }
    }
}
