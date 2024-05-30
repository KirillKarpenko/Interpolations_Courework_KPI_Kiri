using Interpolations;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Windows.Forms;
using System.Drawing;
using ZedGraph;
using System.IO;

namespace Coursendze
{
	public partial class MainWindow : Form
	{
		private Random random = new Random();
		private object rawMethod;
		public MainWindow()
		{
			InitializeComponent();
		}

		private void VertexesAmount_ValueChanged(object sender, EventArgs e)
		{
			int visibleFieldsCount = (int)VertexesAmount.Value;

			foreach (Control control in vertList.Controls)
			{
				control.Visible = true;
			}

			for (int i = visibleFieldsCount; i < vertList.Controls.Count; i++)
			{
				vertList.Controls[i].Visible = false;
			}

			foreach (Control control in vertList.Controls)
			{
				if (!control.Visible)
				{
					foreach (NumericUpDown upNDown in control.Controls.OfType<NumericUpDown>())
					{
						upNDown.Value = 0;
					}
				}
			}
		}

		private void action_Click_1(object sender, EventArgs e)
		{
			Interpolate.complexity = 0;
			double xValue = (double)Math.Round(searchedX.Value, 2, MidpointRounding.ToEven);
			int vertAmount = (int)VertexesAmount.Value;

			List<List<double>> values = new List<List<double>>();
			foreach (Control control in vertList.Controls)
			{
				if (control.Visible)
				{
					List<double> point = new List<double>();
					foreach (NumericUpDown upNDown in control.Controls.OfType<NumericUpDown>())
					{
						point.Add((double)upNDown.Value);
					}
					values.Add(point);
				}
			}

			values.Sort((x, y) => x[0].CompareTo(y[0]));

			this.rawMethod = interpolationMethod.SelectedItem;

			if (rawMethod != null)
			{
				interpolationMethod.BackColor = Color.White;
				string method = rawMethod.ToString();

				if (method == "Лінійна Інтерполяція")
				{
					double result = Interpolate.Linear(xValue, values);

					result = (double)Math.Round(result, 2, MidpointRounding.ToEven);

					answerX.Text = xValue.ToString();
					answerY.Text = result.ToString();
					answerBox.Visible = true;

					summary.Text = Interpolate.BuildLinearEquation(xValue, values);

					double[] answerValues = { xValue, result };

					DrawGraphLinear(values, answerValues);
				}
				else
				{
					double result = Interpolate.Newton(xValue, values);

					if (result is double.NaN)
					{
						MessageBox.Show("Неможливо інтерполювати даний Х використовуючи Метод Ньютона", "Помилка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
						answerBox.Visible = false;
						summaryBox.Visible = false;
						zedGraphControl1.Visible = false;
						return;
					}

					result = (double)Math.Round(result, 2, MidpointRounding.ToEven);


					answerX.Text = xValue.ToString();
					answerY.Text = result.ToString();
					answerBox.Visible = true;

					string equation = Interpolate.BuildNewtonEquation(values);

					summary.Text = equation;

					double[] answerValues = { xValue, result };

					if (!DrawGraphNewton(values, answerValues))
					{
						return;
					}
				}

				complexityValueGui.Text = Interpolate.complexity.ToString();
				complexityBox.Visible = true;
				summaryBox.Visible = true;
				writefile.Visible = true;
			}
			else
			{
				MessageBox.Show("Оберіть Метод Інтерполяції", "Помилка!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				interpolationMethod.BackColor = Color.OrangeRed;
			}
		}

		private void interpolationMethod_SelectedIndexChanged(object sender, EventArgs e)
		{
			interpolationMethod.BackColor = Color.White;
		}

		private void generate_Click(object sender, EventArgs e)
		{
			foreach (Control control in vertList.Controls)
			{
				if (control.Visible)
				{
					foreach (NumericUpDown upNDown in control.Controls.OfType<NumericUpDown>())
					{
						upNDown.Value = random.Next(-99, 100);
					}
				}
			}

			this.ActiveControl = action;
		}

		private bool DrawGraphNewton(List<List<double>> points, double[] answer)
		{
			GraphPane graphPane = zedGraphControl1.GraphPane;
			graphPane.Title.Text = "Графік Інтерполяції за Методом Ньютона";
			graphPane.XAxis.Title.Text = "X";
			graphPane.YAxis.Title.Text = "Y";
			zedGraphControl1.Visible = true;

			List<double> answerViaList = new List<double>
			{
				answer[0],
				answer[1]
			};
			points.Add(answerViaList);

			points.Sort((x, y) => x[0].CompareTo(y[0]));

			PointPairList pointPairs = new PointPairList();
			for (double x = points[0][0]; x <= points[points.Count - 1][0]; x += 0.1)
			{
				double y = Interpolate.Newton(x, points);
				if (y is double.NaN)
				{
					MessageBox.Show("Неможливо інтерполювати дані вершини використовуючи Метод Ньютона", "Помилка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
					answerBox.Visible = false;
					summaryBox.Visible = false;
					zedGraphControl1.Visible = false;
					return false;
				}
				pointPairs.Add(x, y);
			}

			PointPairList answerPoint = new PointPairList
			{
				{ answer[0], answer[1] }
			};

			pointPairs.Add(answerPoint);
			pointPairs.Sort(SortType.XValues);

			graphPane.CurveList.Clear();
			graphPane.GraphObjList.Clear();

			LineItem point = graphPane.AddCurve("Інтерпольована Точка", answerPoint, Color.Red, SymbolType.Circle);
			LineItem curve = graphPane.AddCurve("Графік", pointPairs, Color.Blue, SymbolType.None);

			curve.Line.Width = 2.0f;
			point.Symbol.Size = 10.0f;
			point.Symbol.Fill.Type = FillType.Solid;

			zedGraphControl1.AxisChange();
			zedGraphControl1.Invalidate();
			return true;
		}

		private void DrawGraphLinear(List<List<double>> points, double[] answer)
		{
			GraphPane graphPane = zedGraphControl1.GraphPane;
			graphPane.Title.Text = "Графік Лінійної Інтерполяції";
			graphPane.XAxis.Title.Text = "X";
			graphPane.YAxis.Title.Text = "Y";
			zedGraphControl1.Visible = true;

			PointPairList pointPairs = new PointPairList();
			for (int i = 0; i < points.Count; i++)
			{
				pointPairs.Add(points[i][0], points[i][1]);
			}

			PointPairList answerPoint = new PointPairList
			{
				{ answer[0], answer[1] }
			};

			pointPairs.Add(answer[0], Interpolate.Linear(answer[0], points));

			pointPairs.Sort(SortType.XValues);

			graphPane.CurveList.Clear();
			graphPane.GraphObjList.Clear();

			LineItem point = graphPane.AddCurve("Інтерпольована Точка", answerPoint, Color.Red, SymbolType.Circle);
			LineItem curve = graphPane.AddCurve("Графік", pointPairs, Color.Blue, SymbolType.None);

			curve.Line.Width = 2.0f;
			point.Symbol.Size = 10.0f;
			point.Symbol.Fill.Type = FillType.Solid;

			zedGraphControl1.AxisChange();
			zedGraphControl1.Invalidate();
		}

		private void writefile_Click(object sender, EventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();

			saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
			saveFileDialog.FileName = "result";
			saveFileDialog.FilterIndex = 1;
			saveFileDialog.RestoreDirectory = true;

			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				string fileName = saveFileDialog.FileName;

				try
				{
					using (StreamWriter writer = new StreamWriter(fileName))
					{
						writer.WriteLine("Спосіб Інтерполяції:");
						writer.WriteLine(this.rawMethod.ToString());
						writer.WriteLine("\nІнтерпольована вершина:");
						writer.WriteLine($"x: {answerX.Text}  y: {answerY.Text}");
						writer.WriteLine("\nЗведений вигляд:");
						writer.WriteLine(summary.Text);
						writer.WriteLine("\nЗагальна Складність:");
						writer.WriteLine(complexityValueGui.Text);
					}

					MessageBox.Show("Дані було успішно записано у файл " + fileName);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Помилка: " + ex.Message);
				}
			}
		}
	}
}