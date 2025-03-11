import datetime
import pandas as pd
import numpy as np
from scipy import stats
from sklearn.ensemble import IsolationForest

def detect_anomalies(data_for_processing):
    """
    Detect anomalies in birth dates and ages:
    - Out-of-range birth dates
    - Ages over 120 years
    - Duplicate birth dates
    - Z-score based outliers for age detection
    - Isolation Forest model for overall anomaly detection
    """
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
    # Fill NaN values with the mean or a neutral value before applying zscore
    df['Age'] = df['Age'].fillna(df['Age'].mean())  # Rellenar NaN con la media
    z_scores = stats.zscore(df['Age'])
    df['AgeOutlier'] = np.abs(z_scores) > 2  # Consider values with a Z-score > 2 as outliers

    # Isolation Forest for overall anomaly detection
    model = IsolationForest(contamination=0.1)
    df['Isolated'] = model.fit_predict(df[['Age']]) == -1  # Mark -1 as anomaly

    # Combine all anomaly detection criteria
    df['Anomaly'] = df['OutOfRange'] | df['AgeTooHigh'] | df['DuplicateDOB'] | df['AgeOutlier'] | df['Isolated']

    # Filter and return rows with any anomalies
    anomalies = df[df['Anomaly']].copy()

    # Return anomalies as a list of dictionaries
    return anomalies.to_dict(orient='records')


# Example data
data_for_processing = [
    ['John', '1980-05-10'],
    ['Anna', '3000-12-25'],
    ['Peter', '1899-01-15'],
    ['Laura', '1995-10-30'],
    ['Duplicate1', '1980-05-10'],
    ['InvalidFormat', 'invalid-date']
]

# Run anomaly detection
anomalies = detect_anomalies(data_for_processing)

# Print detected anomalies
print("Detected anomalies:")
for entry in anomalies:
    print(entry)
