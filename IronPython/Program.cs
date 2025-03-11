using IronPython.Hosting;

namespace AnomalyDetection.IronPython
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            // Crear el runtime de IronPython
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();

            // Script en Python para detectar anomalías
            string script = @"
import pandas as pd
from datetime import datetime

def detect_anomalies(data):
    # Crear un DataFrame con los datos proporcionados
    df = pd.DataFrame(data, columns=['Name', 'DateOfBirth'])
    
    # Convertir las fechas de nacimiento al formato datetime
    try:
        df['DateOfBirth'] = pd.to_datetime(df['DateOfBirth'], errors='coerce')  # Coerción de errores (formato inválido -> NaT)
    except Exception as e:
        return {'Error': str(e)}
    
    # Detectar fechas fuera de rango razonable
    lower_bound = datetime(1900, 1, 1)
    upper_bound = datetime.now()
    df['OutOfRange'] = (df['DateOfBirth'] < lower_bound) | (df['DateOfBirth'] > upper_bound)
    
    # Detectar edades mayores a 120 años
    df['Age'] = (datetime.now() - df['DateOfBirth']).dt.total_seconds() // (365.25 * 24 * 3600)
    df['AgeTooHigh'] = df['Age'] > 120
    
    # Detectar fechas de nacimiento duplicadas
    df['DuplicateDOB'] = df['DateOfBirth'].duplicated(keep=False)
    
    # Filtrar registros con anomalías
    anomalies = df[df['OutOfRange'] | df['AgeTooHigh'] | df['DuplicateDOB']].copy()
    
    # Retornar el DataFrame de anomalías como diccionario para usar en .NET
    return anomalies.to_dict(orient='records')
";

            // Crear los datos en C#
            var data = new[]
            {
            new object[] { "John", "1980-05-10" },
            new object[] { "Anna", "3000-12-25" }, // Fecha en el futuro
            new object[] { "Peter", "1899-01-15" }, // Antes de 1900
            new object[] { "Laura", "1995-10-30" },
            new object[] { "Duplicate1", "1980-05-10" }, // Fecha duplicada
            new object[] { "InvalidFormat", "invalid-date" }, // Formato incorrecto,
            new object[] { "ViciestoIncorrecto", "1981-02-29" }
        };

            // Convertir los datos de C# a un formato adecuado para Python (lista de listas)
            var pythonData = data.Select(d => new List<object> { d[0], d[1] }).ToList();

            try
            {
                // Ejecutar el script en el entorno de Python
                scope.SetVariable("data", pythonData);
                engine.Execute(script, scope);

                // Obtener la función detect_anomalies y ejecutarla
                dynamic detect_anomalies = scope.GetVariable("detect_anomalies");
                dynamic analysis = detect_anomalies(pythonData);

                // Procesar los resultados
                Console.WriteLine("Anomalías detectadas:");
                foreach (var entry in analysis)
                {
                    Console.WriteLine(entry);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrió un error en el análisis de Python: {ex.Message}");
            }
        }
    }
}