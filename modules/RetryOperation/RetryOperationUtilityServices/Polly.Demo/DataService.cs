using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Polly.Demo
{
    public class DataService
    {
        public async Task<string> GetDataAsync()
        {
            Console.WriteLine($"Attempting to retrieve data at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}...");
            Stopwatch sw = Stopwatch.StartNew();
            // Simulate a transient failure
            Random rand = new Random();
            if(rand.Next(0 , 1000) != 0)
            {
                Console.WriteLine("Data retrieval failed. Retrying...");
                await Task.Delay(rand.Next(100 , 1000)); // Simulate some async work
                sw.Stop();
                Console.WriteLine($"It takes {sw.ElapsedMilliseconds} ms");
                throw new Exception("Transient error occurred.");
            }
            await Task.Delay(rand.Next(100, 1000)); // Simulate some async work
            sw.Stop();
            Console.WriteLine($"It takes {sw.ElapsedMilliseconds} ms");
            return "Data retrieved successfully.";
        }
    }
}
