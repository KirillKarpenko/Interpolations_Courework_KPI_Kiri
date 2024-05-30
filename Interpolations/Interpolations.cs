using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpolations
{
	public class Interpolate
	{
		public static int complexity = 0;
		public static double[] ComputeCoeffs(List<List<double>> values)
		{
			List<double> xValues = new List<double>();
			for (int i = 0; i < values.Count; i++)
			{
				xValues.Add(values[i][0]);
			}

			List<double> yValues = new List<double>();
			for (int i = 0; i < values.Count; i++)
			{
				yValues.Add(values[i][1]);
			}

			int n = xValues.Count;
			double[] f = new double[n];

			yValues.CopyTo(f);

			for (int i = 1; i < n; i++)
			{
				for (int j = n - 1; j >= i; j--)
				{
					f[j] = (f[j] - f[j - 1]) / (xValues[j] - xValues[j - i]);
					complexity++;
				}
			}

			for (int i = 0; i < f.Length; i++)
			{
				f[i] = Math.Round(f[i], 5, MidpointRounding.ToEven);
			}

			return f;
		}

		public static double Newton(double x, List<List<double>> values)
		{
			List<double> xValues = new List<double>();
			for (int i = 0; i < values.Count; i++)
			{
				xValues.Add(values[i][0]);
			}

			double[] f = ComputeCoeffs(values);

			double result = f[0];
			double temp = 1;

			for (int i = 1; i < xValues.Count; i++)
			{
				temp *= x - xValues[i - 1];
				result += f[i] * temp;
				complexity++;
			}

			return result;
		}

		public static double Linear(double x, List<List<double>> values)
		{
			List<double> xValues = new List<double>();
			for (int i = 0; i < values.Count; i++)
			{
				xValues.Add(values[i][0]);
			}

			List<double> yValues = new List<double>();
			for (int i = 0; i < values.Count; i++)
			{
				yValues.Add(values[i][1]);
			}

			int index = 0;
			while (index < xValues.Count - 2 && x > xValues[index + 1])
			{
				index++;
				complexity++;
			}

			double x0 = xValues[index];
			double y0 = yValues[index];
			double x1 = xValues[index + 1];
			double y1 = yValues[index + 1];

			double interpolatedValue = y0 + (y1 - y0) * (x - x0) / (x1 - x0);

			complexity++;

			return interpolatedValue;
		}

		public static string BuildNewtonEquation(List<List<double>> values)
		{
			double[] f = Interpolate.ComputeCoeffs(values);

			List<double> x = new List<double>();
			for (int i = 0; i < values.Count; i++)
			{
				x.Add(values[i][0]);
			}

			for (int i = 0; i < f.Length; i++)
			{
				f[i] = (double)Math.Round(f[i], 3, MidpointRounding.ToEven);
			}

			string equation = "y = ";

			for (int i = 0; i < x.Count; i++)
			{
				string term = "";

				if (i > 0)
				{
					term += "(";

					for (int j = 0; j < i; j++)
					{
						string signX = x[j] >= 0 ? "-" : "+";
						term += $"(x {signX} {Math.Abs(x[j])})";
						if (j < i - 1)
							term += "*";
					}

					term += ")";
				}

				string sign = f[i] >= 0 ? "+" : "-";
				equation += $" {sign} {Math.Abs(f[i])}{term}";


			}

			return equation;
		}

		public static string BuildLinearEquation(double xValue, List<List<double>> values)
		{
			List<double> xValues = new List<double>();
			for (int i = 0; i < values.Count; i++)
			{
				xValues.Add(values[i][0]);
			}

			List<double> yValues = new List<double>();
			for (int i = 0; i < values.Count; i++)
			{
				yValues.Add(values[i][1]);
			}

			int index = 0;
			while (index < xValues.Count - 2 && xValue > xValues[index + 1])
			{
				index++;
			}

			double x0 = xValues[index];
			double y0 = yValues[index];
			double x1 = xValues[index + 1];
			double y1 = yValues[index + 1];

			string signX0 = x0 >= 0 ? "-" : "+";
			string signX1 = x1 >= 0 ? "-" : "+";
			string signy0 = y0 >= 0 ? "-" : "+";

			string equation = $"y = {y0} + ((x {signX1} {Math.Abs(x1)}) * ({y1} {signy0} {Math.Abs(y0)})) / ({x1} {signX0} {Math.Abs(x0)})";

			return equation;
		}
	}
}