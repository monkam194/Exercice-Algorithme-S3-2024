using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Current Directory: " + Directory.GetCurrentDirectory());

        string[] filePaths = {
            "../../../../data/0e303ef7354aab60637e09a5fae96fa8.txt",
            "../../../../data/08b49b5f34fdc2c28191b45749c365a8.txt",
            "../../../../data/9264a7c4e7251fc75910c4155e5c489a.txt",
            "../../../../data/e001e8d4317c1db1d6a0f3d3f6d03d25.txt"
        };

        // Vérifiez que les fichiers existent
        foreach (var filePath in filePaths)
        {
            if (File.Exists(filePath))
            {
                Console.WriteLine($"File exists: {filePath}");
            }
            else
            {
                Console.WriteLine($"File not found: {filePath}");
            }
        }

        var algorithms = new Dictionary<string, Func<List<string>, List<string>>>()
        {
            { "Selection Sort", SelectionSort },
            { "Bubble Sort", BubbleSort },
            { "Insertion Sort", InsertionSort },
            { "Quicksort", Quicksort }
        };

        var results = ProcessFiles(filePaths, algorithms);
        WriteCsv(results);
    }

    static List<string> LoadFile(string filePath)
    {
        return File.ReadAllLines(filePath).ToList();
    }

    static (List<string>, double) MeasureTime(Func<List<string>, List<string>> algorithm, List<string> data)
    {
        var stopwatch = Stopwatch.StartNew();
        var sortedData = algorithm(new List<string>(data));
        stopwatch.Stop();
        return (sortedData, stopwatch.Elapsed.TotalSeconds);
    }

    static List<(string FileName, string Algorithm, double Duration)> ProcessFiles(string[] filePaths, Dictionary<string, Func<List<string>, List<string>>> algorithms)
    {
        var results = new List<(string, string, double)>();
        foreach (var filePath in filePaths)
        {
            var data = LoadFile(filePath);
            foreach (var kvp in algorithms)
            {
                var algorithmName = kvp.Key;
                var algorithm = kvp.Value;
                var (sortedData, duration) = MeasureTime(algorithm, data);
                var fileName = Path.GetFileName(filePath);
                results.Add((fileName, algorithmName, duration));
            }
        }
        return results;
    }

    static void WriteCsv(List<(string FileName, string Algorithm, double Duration)> results, string outputFile = "benchmark_results.csv")
    {
        var algorithmNames = results.Select(r => r.Algorithm).Distinct().ToList();
        var fileNames = results.Select(r => r.FileName).Distinct().ToList();

        using (var writer = new StreamWriter(outputFile))
        {
            // Écrire l'en-tête
            writer.Write("Nom des fichiers");
            foreach (var algo in algorithmNames)
            {
                writer.Write($",{algo}");
            }
            writer.WriteLine();

            // Écrire les durées
            foreach (var file in fileNames)
            {
                writer.Write(file);
                foreach (var algo in algorithmNames)
                {
                    var result = results.FirstOrDefault(r => r.FileName == file && r.Algorithm == algo);
                    writer.Write(result != default ? $",{result.Duration}" : ",");
                }
                writer.WriteLine();
            }
        }
    }

    static List<string> SelectionSort(List<string> arr)
    {
        int n = arr.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int minIdx = i;
            for (int j = i + 1; j < n; j++)
            {
                if (string.Compare(arr[j], arr[minIdx], StringComparison.Ordinal) < 0)
                {
                    minIdx = j;
                }
            }
            var temp = arr[minIdx];
            arr[minIdx] = arr[i];
            arr[i] = temp;
        }
        return arr;
    }

    static List<string> BubbleSort(List<string> arr)
    {
        int n = arr.Count;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                if (string.Compare(arr[j], arr[j + 1], StringComparison.Ordinal) > 0)
                {
                    var temp = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = temp;
                }
            }
        }
        return arr;
    }

    static List<string> InsertionSort(List<string> arr)
    {
        int n = arr.Count;
        for (int i = 1; i < n; ++i)
        {
            var key = arr[i];
            int j = i - 1;
            while (j >= 0 && string.Compare(arr[j], key, StringComparison.Ordinal) > 0)
            {
                arr[j + 1] = arr[j];
                j = j - 1;
            }
            arr[j + 1] = key;
        }
        return arr;
    }

    static List<string> Quicksort(List<string> arr)
    {
        if (arr.Count <= 1)
        {
            return arr;
        }
        var pivot = arr[arr.Count / 2];
        var left = arr.Where(x => string.Compare(x, pivot, StringComparison.Ordinal) < 0).ToList();
        var middle = arr.Where(x => x == pivot).ToList();
        var right = arr.Where(x => string.Compare(x, pivot, StringComparison.Ordinal) > 0).ToList();
        return Quicksort(left).Concat(middle).Concat(Quicksort(right)).ToList();
    }
}
