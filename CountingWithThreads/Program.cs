using System.Collections;
using System.Text;

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
        public static void ThreadProcess(int thread_number, string file_path, int start, long totalChunkSize, int buffer_size = 1028)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            FileStream thread_file = File.Create($"{thread_number}.txt");

            using (FileStream fs = File.OpenRead(file_path))
            {
                int bytes = 0;

                fs.Seek(start, SeekOrigin.Begin);

                byte[] buffer = new byte[totalChunkSize];
                UTF8Encoding temp = new UTF8Encoding(true);

                long bytes_read = 0;
                while (bytes_read < totalChunkSize && (bytes = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    thread_file.Write(buffer, 0 , bytes);
                    bytes_read += bytes;
                }
            }

            thread_file.Close();
            watch.Stop();

            Console.WriteLine($"{thread_number} finished in {watch.Elapsed}");
            Thread.Sleep(5000);
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

            Dictionary<string, ArrayList> data = new Dictionary<string, ArrayList>()
            {
                { "words", new ArrayList() }
            };


            long bytes_per_thread = fi.Length / number_of_threads;

            Console.WriteLine($"total sizes (bytes): {fi.Length}\n");
            Console.Write($"Threads: {number_of_threads}\n");
            Console.WriteLine($"bytes per thread: {bytes_per_thread}");

            int thread_number = 0;
            for(long i = 0; (fi.Length - (i + bytes_per_thread)) >= 0 ; i = i + bytes_per_thread)
            {
                var start = i;
                var end = i + bytes_per_thread - 1;
                Console.WriteLine($"{start} {end}");
                Thread t = new Thread(() => ThreadProcess(thread_number += 1, file_path, (int)start, bytes_per_thread));
                threads.Add(t);
            }

            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());
        }
    }
}
