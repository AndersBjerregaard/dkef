using Domain;

string filePath = "data/denero_contacts.csv";

using var reader = new StreamReader(filePath);

// Skip first line since they're just headers
reader.ReadLine();

string? line;
string[] values;

while ((line = reader.ReadLine()) != null) {
	Console.WriteLine(line);
	values = line.Split(';', StringSplitOptions.None);
}

Console.WriteLine("DONE");
