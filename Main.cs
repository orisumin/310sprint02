using System;
using System.IO;

/// <summary>Provides the console application's entry point.</summary>
/// <remarks>C# prohibits a Main method in a class named Main, so the Java Main class becomes Program.</remarks>
public static class Program
{
    public static async Task Main(string[] args)
    {
        await new Options().HandleOptionsAsync();
    }
}
