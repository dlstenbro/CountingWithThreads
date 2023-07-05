using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CountingWithThreads.Models;
using Microsoft.VisualBasic;

namespace CountingWithThreads
{

    /*
     * Threading Example 1
     * Objective: Read through a source of test for count of each word
     * Instead of using brute force method (multiple loops to count words), use threads for speed.
     * 
     */
    public static class Program
    {
        public static WordList wordlist = new WordList();
        public static void CountWords(string content)
        {
            // clean up the unicode chunks by only accepting characters
            // under https://www.ssec.wisc.edu/~tomw/java/unicode.html
            // if they don't fit between the range, replace with a space character.
            content = Regex.Replace(content, @"[^\u0041-\u005A\u0061-\u007A]+", " ");

            foreach (string w in content.Split(" ", StringSplitOptions.TrimEntries))
            {
                if (!wordlist.Entries.TryAdd(w, 1))
                {
                    wordlist.Entries[w] += 1;
                }
            }
        }

        public static void Run(int thread_number, string file_path, int start, long totalChunkSize, int buffer_size = 1028)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            FileStream thread_file = File.Create($"{thread_number}.txt");

            Console.WriteLine($"running thread # {thread_number} : {start} {totalChunkSize}");

            using (FileStream fs = File.OpenRead(file_path))
            {
                int bytes = 0;
                long bytes_read = 0;

                fs.Seek(start, SeekOrigin.Begin);

                byte[] buffer = new byte[totalChunkSize];
                UTF8Encoding temp = new UTF8Encoding(true);

                while (bytes_read < totalChunkSize && (bytes = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string content = Encoding.UTF8.GetString(buffer);

                    CountWords(content);

                    thread_file.Write(buffer, 0 , bytes);
                    bytes_read += bytes;
                }
            }

            thread_file.Close();
            watch.Stop();

            Console.WriteLine($"{thread_number} finished in {watch.Elapsed}");
            
        }

        public static void Main()
        {
            var watch = new System.Diagnostics.Stopwatch();
            List<Thread> threads = new List<Thread>();

            string file_name = "sample-book.txt";
            string file_path = Path.Combine(Environment.CurrentDirectory, file_name);
            int number_of_threads = 1;

            FileInfo fi = new FileInfo(file_name);
            var message = $"Reading in text file: {fi.FullName} {fi.Length}\n\nEnter in the number of threads to use";
            Console.WriteLine(message);

            number_of_threads = int.Parse(Console.ReadLine() ?? "1");

            long bytes_per_thread = fi.Length / number_of_threads;

            Console.WriteLine($"total sizes (bytes): {fi.Length}\n");
            Console.Write($"Threads: {number_of_threads}\n");
            Console.WriteLine($"bytes per thread: {bytes_per_thread}");

            int thread_number = 0;
            for(long i = 0; (fi.Length - (i + bytes_per_thread)) >= 0 ; i = i + bytes_per_thread)
            {
                var start = i;
                Thread t = new Thread(() => Run(thread_number += 1, file_path, (int)start, bytes_per_thread));
                threads.Add(t);
            }

            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

            //convert wordlist to a list with dictionary entries
            // the output will look friendly converting to json
            var entryList = wordlist.Entries.Select(kvp => new Entry { word = kvp.Key, count = kvp.Value }).ToList();
            Dictionary<string, List<Entry>> cleanoutput = new Dictionary<string, List<Entry>>()
            {
                {  "words", entryList }
            };

            JsonDocument js = JsonDocument.Parse(JsonSerializer.Serialize(cleanoutput));

            string output = Path.Combine(Environment.CurrentDirectory, "sample-book-words.json");
            File.WriteAllText(output, js.RootElement.GetRawText().Replace(Environment.NewLine, ""));
        }
    }
}
