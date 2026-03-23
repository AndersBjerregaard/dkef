namespace Domain;

public static class Reader
{
    public static void Read()
    {
        string filePath = "data/dinero_members.csv";

        using var reader = new StreamReader(filePath);

        // Skip first line since they're just headers
        reader.ReadLine();

        string? line;

        int index = 0;

        while ((line = reader.ReadLine()) != null)
        {
            // Console.WriteLine(line);
            string[] values = line.Split(';', StringSplitOptions.None);
            index++;
            if (values.Length != 16)
            {
                Console.WriteLine($"Line {index + 1} has {values.Length} columns, expected 16");
            }
        }

        Console.WriteLine("DONE");
    }
}
