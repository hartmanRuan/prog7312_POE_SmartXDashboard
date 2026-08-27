using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartXDashboard.Services
{
    public class TelemetryBatchProcessor
    {
        /// <summary>
        /// Processes a jagged array where each inner array represents readings for a single sensor node.
        /// Returns an array of average values per node.
        /// </summary>
        public double[] CalculateNodeAverages(double[][] batchReadings)
        {
            if (batchReadings == null || batchReadings.Length == 0)
                return Array.Empty<double>();

            double[] averages = new double[batchReadings.Length];

            for (int i = 0; i < batchReadings.Length; i++)
            {
                if (batchReadings[i] == null || batchReadings[i].Length == 0)
                {
                    averages[i] = 0.0;
                    continue;
                }

                double sum = 0.0;
                for (int j = 0; j < batchReadings[i].Length; j++)
                {
                    sum += batchReadings[i][j];
                }

                averages[i] = sum / batchReadings[i].Length;
            }

            return averages;
        }

        /// <summary>
        /// Processes a 2D multi-dimensional matrix [sensors, timestamps] and identifies 
        /// the peak (highest) reading across the entire dataset.
        /// </summary>
        public double FindGlobalPeakReading(double[,] telemetryMatrix)
        {
            if (telemetryMatrix == null || telemetryMatrix.Length == 0)
                return 0.0;

            int rows = telemetryMatrix.GetLength(0);
            int cols = telemetryMatrix.GetLength(1);

            double peak = double.MinValue;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (telemetryMatrix[r, c] > peak)
                    {
                        peak = telemetryMatrix[r, c];
                    }
                }
            }

            return peak == double.MinValue ? 0.0 : peak;
        }
    }
}