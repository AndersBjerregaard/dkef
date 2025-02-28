string filePath = "data/denero_contacts.csv";

using var reader = new StreamReader(filePath);

string? line;

while ((line = reader.ReadLine()) != null) {
	Console.WriteLine(line);
}

Console.WriteLine("DONE");
