using System;
using PandasNet;

namespace AnomalyDetection.PandasNet
{
    public static class Program
    {
        static void Main(string[] args)
        {
            // Create the data as Series (columns)
            var names = new Series(new[] { "John", "Anna", "Peter", "Laura" }, new Column() { DType = typeof(string), Index = 0, Name = "Name" });
            var ages = new Series(new[] { 23, 34, 45, 29 }, new Column() { DType = typeof(int), Index = 1, Name = "Age" });
            var cities = new Series(new[] { "London", "Barcelona", "Russia", "Valencia" }, new Column() { DType = typeof(string), Index = 2, Name = "City" });

            // Add the Series to a List to create the DataFrame
            var data = new List<Series> { names, ages, cities };

            // Create the DataFrame
            var df = new DataFrame(data);

            // Display the original DataFrame
            Console.WriteLine("Original DataFrame:");
            Console.WriteLine(df);

            // Filter the DataFrame to show only people older than 30 years

            // Convertir el arreglo de edades para filtrar > 30
            var ageData = ages.data.Cast<int>(); // Convierte el Array a IEnumerable<int>
            var mask = ageData.Select(age => age > 30).ToArray(); // Crea una máscara booleana

            // Filtrar nombres, edades y ciudades basados en la máscara
            var filteredNames = names.data.Cast<string>().Where((name, index) => mask[index]).ToArray();
            var filteredAges = ageData.Where((age, index) => mask[index]).ToArray();
            var filteredCities = cities.data.Cast<string>().Where((city, index) => mask[index]).ToArray();

            Console.WriteLine("-------------------------");

            // Imprimir los resultados filtrados
            for (int i = 0; i < filteredAges.Length; i++)
            {
                Console.WriteLine($"Name: {filteredNames[i]}, Age: {filteredAges[i]}, City: {filteredCities[i]}");
            }

            // Add a new column 'IsAdult' to the DataFrame
            var isAdult = new Series(new[] { true, true, true, true }, new Column() { DType = typeof(bool), Index = 3, Name = "IsAdult" });
            df["IsAdult"] = isAdult;

            // Display the DataFrame with the new 'IsAdult' column
            Console.WriteLine("\nDataFrame with new 'IsAdult' column:");
            Console.WriteLine(df);
        }
    }

}