
string apiUrl = "http://localhost:5057/api"; // API地址
int totalCalls = 10; // 总共的调用次数

using (HttpClient client = new())
{
    for (int callCount = 0; callCount < totalCalls; callCount++)
    {
        HttpResponseMessage response = await client.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            string result = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Call {callCount + 1}: {result}");
        }
        else
        {
            Console.WriteLine($"Call {callCount + 1} failed: {response.StatusCode}");
        }
    }
}

Console.WriteLine("All calls completed.");
Console.ReadKey();
