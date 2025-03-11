using System;
using System.Collections.Generic;
using Python.Runtime;
using static System.Formats.Asn1.AsnWriter;

class Program
{
    static void Main(string[] args)
    {
        // Inicializar el runtime de Python
        PythonEngine.Initialize();

        using (Py.GIL()) // Adquirir el Global Interpreter Lock (GIL) de Python
        {
            // Aquí está el script completo de Python como texto
            string script = @"
import datetime
import pandas as pd
import numpy as np
from scipy import stats
from sklearn.ensemble import IsolationForest

def detect_anomalies(data_for_processing):
    # Create DataFrame from input data
    df = pd.DataFrame(data_for_processing, columns=['Name', 'DateOfBirth'])

    # Convert 'DateOfBirth' to datetime format, invalid dates become NaT
    df['DateOfBirth'] = pd.to_datetime(df['DateOfBirth'], errors='coerce')

    # Detect birth dates out of range (before 1900 or in the future)
    lower_bound = datetime.datetime(1900, 1, 1)
    upper_bound = datetime.datetime.now()
    df['OutOfRange'] = (df['DateOfBirth'] < lower_bound) | (df['DateOfBirth'] > upper_bound)

    # Calculate age and detect if it's greater than 120 years
    df['Age'] = (datetime.datetime.now() - df['DateOfBirth']).dt.total_seconds() // (365.25 * 24 * 3600)
    df['AgeTooHigh'] = df['Age'] > 120

    # Detect duplicate birth dates
    df['DuplicateDOB'] = df['DateOfBirth'].duplicated(keep=False)

    # Z-Score for outlier detection (based on age)
    df['Age'] = df['Age'].fillna(df['Age'].mean())  # Fill NaN with the mean
    z_scores = stats.zscore(df['Age'])
    df['AgeOutlier'] = np.abs(z_scores) > 2  # Outliers with Z-score > 2

    # Isolation Forest for overall anomaly detection
    model = IsolationForest(contamination=0.1)
    df['Isolated'] = model.fit_predict(df[['Age']]) == -1  # Mark -1 as anomaly

    # Combine all anomaly detection criteria
    df['Anomaly'] = df['OutOfRange'] | df['AgeTooHigh'] | df['DuplicateDOB'] | df['AgeOutlier'] | df['Isolated']

    # Filter and return rows with any anomalies
    anomalies = df[df['Anomaly']].copy()

    # Return anomalies as a list of dictionaries
    return anomalies.to_dict(orient='records')
";

            // Crear los datos en C#
            var dataForProcessing = new[]
            {
                new object[] { "John", "1980-05-10" },
                new object[] { "Anna", "3000-12-25" },
                new object[] { "Peter", "1899-01-15" },
                new object[] { "Laura", "1995-10-30" },
                new object[] { "Duplicate1", "1980-05-10" },
                new object[] { "InvalidFormat", "invalid-date" }
            };

            // Ejecutar el script de Python usando Python.NET
            dynamic anomalies;
            try
            {
                using (var scope = Py.CreateScope())
                {
                    // Inyectar el script completo y definir la función python detect_anomalies
                    scope.Exec(script);

                    // Obtener la función detect_anomalies desde el entorno Python
                    dynamic detectAnomalies = scope.Get("detect_anomalies");

                    // Llamar a la función detect_anomalies pasando los datos desde C#
                    anomalies = detectAnomalies(dataForProcessing);
                }

                // Imprimir las anomalías detectadas
                Console.WriteLine("Anomalías detectadas:");
                foreach (dynamic anomaly in anomalies)
                {
                    Console.WriteLine(anomaly);
                }
            }
            catch (PythonException pyEx)
            {
                Console.WriteLine($"Error en el análisis de Python: {pyEx.Message}");
            }
        }

        // Finalizar el runtime de Python
        PythonEngine.Shutdown();
    }
}