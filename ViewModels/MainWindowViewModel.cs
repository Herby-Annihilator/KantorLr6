using KantorLr6.Infrastructure.Commands;
using KantorLr6.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using KantorLr6.Model.Data;
using org.mariuszgromada.math.mxparser;
using System.IO;
using CompMathLibrary.Interpolation.Polynomials;

namespace KantorLr6.ViewModels
{
	[MarkupExtensionReturnType(typeof(MainWindowViewModel))]
	public class MainWindowViewModel : ViewModel
	{
		private const string LAGRANGE_FILE = "lagrange.dat";
		private const string NEWTON_FILE = "newton.dat";
		private LagrangeInterpolationPolynomial _lagrange;
		private NewtonInterpolationPolynomial _newton;
		public MainWindowViewModel()
		{
			_lagrange = new LagrangeInterpolationPolynomial();
			_newton = new NewtonInterpolationPolynomial();
			CalculateFunctionValueInPointCommand = new LambdaCommand(OnCalculateFunctionValueInPointCommandExecuted, CanCalculateFunctionValueInPointCommandExecute);
			RestoreLagrangeTableCommand = new LambdaCommand(OnRestoreLagrangeTableCommandExecuted, CanRestoreLagrangeTableCommandExecute);
			SaveLagrangeTableCommand = new LambdaCommand(OnSaveLagrangeTableCommandExecuted, CanSaveLagrangeTableCommandCommandExecute);
			RestoreArgumentsTableCommand = new LambdaCommand(OnRestoreArgumentsTableCommandExecuted, CanRestoreArgumentsTableCommandExecute);
			SaveArgumentsTableCommand = new LambdaCommand(OnSaveArgumentsTableCommandExecuted, CanSaveArgumentsTableCommandExecute);
			CalculateNewtonTableCommand = new LambdaCommand(OnCalculateNewtonTableCommandExecuted, CanCalculateNewtonTableCommandExecute);
			AddNewPointCommand = new LambdaCommand(OnAddNewPointCommandExecuted, CanAddNewPointCommandExecute);
			RemoveSelectedPointCommand = new LambdaCommand(OnRemoveSelectedPointCommandExecuted, CanRemoveSelectedPointCommandExecute);
			BuildFunctionGraphicCommand = new LambdaCommand(OnBuildFunctionGraphicCommandExecuted, CanBuildFunctionGraphicCommandExecute);
			BuildPolynomGraphicCommand = new LambdaCommand(OnBuildPolynomGraphicCommandExecuted, CanBuildPolynomGraphicCommandExecute);
			GenerateTableCommand = new LambdaCommand(OnGenerateTableCommandExecuted, CanGenerateTableCommandExecute);
		}

		#region Properties
		private string _title = "Title";
		public string Title { get => _title; set => Set(ref _title, value); }

		private string _status = "Интерполирование многочленами";
		public string Status { get => _status; set => Set(ref _status, value); }

		public ObservableCollection<Point> PointsFunction { get; set; } = new ObservableCollection<Point>();
		public ObservableCollection<Point> PointsPolynom { get; set; } = new ObservableCollection<Point>();

		private string _functionText = "";
		public string FunctionText
		{
			get => _functionText;
			set
			{
				Set(ref _functionText, value);
				GraphTitle = value;
			}
		}

		private string _graphTitle = "График функции -- и соответствующего многочлена";
		public string GraphTitle
		{
			get => _graphTitle;
			set
			{
				if (value is null || string.IsNullOrWhiteSpace(value))
					Set(ref _graphTitle, "График функции -- и соответствующего многочлена");
				else
					Set(ref _graphTitle, $"График функции {value} и соответствующего многочлена");
			}
		}

		private string _pointOfCalculation;
		public string PointOfCalculation { get => _pointOfCalculation; set => Set(ref _pointOfCalculation, value); }

		private double _functionValueInPoint = double.NaN;
		public double FunctionValueInPoint { get => _functionValueInPoint; set => Set(ref _functionValueInPoint, value); }

		private string _argumentsArray;
		public string ArgumentsArray { get => _argumentsArray; set => Set(ref _argumentsArray, value); }

		public ObservableCollection<Point> LagrangeTable { get; set; } = new ObservableCollection<Point>();
		public ObservableCollection<Point> NewtonTable { get; set; } = new ObservableCollection<Point>();

		private Point _selectedPoint;
		public Point SelectedPoint { get => _selectedPoint; set => Set(ref _selectedPoint, value); }

		private string _argumentLeftBoard;
		public string ArgumentLeftBoard { get => _argumentLeftBoard; set => Set(ref _argumentLeftBoard, value); }

		private string _argumentRightBoard;
		public string ArgumentRightBoard { get => _argumentRightBoard; set => Set(ref _argumentRightBoard, value); }

		private string _step;
		public string Step { get => _step; set => Set(ref _step, value); }

		private string _generateTableLeftBoard;
		public string GenerateTableLeftBoard { get => _generateTableLeftBoard; set => Set(ref _generateTableLeftBoard, value); }

		private string _generateTableRightBoard;
		public string GenerateTableRightBoard { get => _generateTableRightBoard; set => Set(ref _generateTableRightBoard, value); }

		private string _generateTableStep;
		public string GenerateTableStep { get => _generateTableStep; set => Set(ref _generateTableStep, value); }
		#endregion

		#region Commands

		public ICommand CalculateFunctionValueInPointCommand { get; }
		private void OnCalculateFunctionValueInPointCommandExecuted(object p)
		{
			try
			{
				FunctionValueInPoint = double.NaN;
				double point = Convert.ToDouble(PointOfCalculation);
				double[] args = new double[LagrangeTable.Count];
				double[] values = new double[LagrangeTable.Count];
				for (int i = 0; i < LagrangeTable.Count; i++)
				{
					args[i] = LagrangeTable[i].X;
					values[i] = LagrangeTable[i].Y;
				}
				_lagrange.Arguments = (double[])args.Clone();
				_lagrange.Values = (double[])values.Clone();
				FunctionValueInPoint = _lagrange.GetFunctionValueIn(point);
				Status = "Успешное выполнение";
			}
			catch(Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanCalculateFunctionValueInPointCommandExecute(object p)
		{
			return !(LagrangeTable.Count == 0 || string.IsNullOrWhiteSpace(PointOfCalculation));
		}

		public ICommand RestoreLagrangeTableCommand { get; }
		private void OnRestoreLagrangeTableCommandExecuted(object p)
		{
			try
			{
				LagrangeTable.Clear();
				StreamReader reader = new StreamReader(LAGRANGE_FILE);
				string[] points = reader.ReadToEnd().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
				string[] coords;
				for (int i = 0; i < points.Length; i++)
				{
					coords = points[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);
					LagrangeTable.Add(new Point(Convert.ToDouble(coords[0]), Convert.ToDouble(coords[1])));
				}
				reader.Close();
				Status = "Данные восстановлены";
			}
			catch(Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}			
		}
		private bool CanRestoreLagrangeTableCommandExecute(object p)
		{
			return File.Exists(LAGRANGE_FILE);
		}

		public ICommand SaveLagrangeTableCommand { get; }
		private void OnSaveLagrangeTableCommandExecuted(object p)
		{
			try
			{
				StreamWriter writer = new StreamWriter(LAGRANGE_FILE);
				for (int i = 0; i < LagrangeTable.Count; i++)
				{
					writer.WriteLine($"{LagrangeTable[i].X} {LagrangeTable[i].Y}");
				}
				writer.Close();
				Status = $"Данные записаны в файл {LAGRANGE_FILE}";
			}
			catch(Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}		
		}
		private bool CanSaveLagrangeTableCommandCommandExecute(object p)
		{
			return LagrangeTable.Count > 0;
		}

		public ICommand RestoreArgumentsTableCommand { get; }
		private void OnRestoreArgumentsTableCommandExecuted(object p)
		{
			try
			{
				StreamReader reader = new StreamReader(NEWTON_FILE);
				ArgumentsArray = reader.ReadToEnd().Replace("\r\n", " ");
				reader.Close();
				Status = $"Данные восстановлены";
			}
			catch (Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanRestoreArgumentsTableCommandExecute(object p)
		{
			return File.Exists(NEWTON_FILE);
		}

		public ICommand SaveArgumentsTableCommand { get; }
		private void OnSaveArgumentsTableCommandExecuted(object p)
		{
			try
			{
				ArgumentsArray = ArgumentsArray.Replace("\r\n", " ");
				StreamWriter writer = new StreamWriter(NEWTON_FILE);
				writer.Write(ArgumentsArray);
				writer.Close();
				Status = $"Данные записаны в файл {NEWTON_FILE}";
			}
			catch(Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}			
		}
		private bool CanSaveArgumentsTableCommandExecute(object p)
		{
			return !string.IsNullOrWhiteSpace(ArgumentsArray);
		}

		public ICommand CalculateNewtonTableCommand { get; }
		private void OnCalculateNewtonTableCommandExecuted(object p)
		{
			try
			{
				NewtonTable.Clear();
				Function f = new Function(FunctionText);
				Expression expression;
				string[] numbers = ArgumentsArray.Split(" ", StringSplitOptions.RemoveEmptyEntries);
				double[] args = new double[numbers.Length];
				for (int i = 0; i < numbers.Length; i++)
				{
					args[i] = Convert.ToDouble(numbers[i]);
					expression = new Expression($"f({args[i].ToString().Replace(",", ".")})", f);
					NewtonTable.Add(new Point(args[i], expression.calculate()));
				}
				Status = $"Успешное выполнение";
			}
			catch (Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanCalculateNewtonTableCommandExecute(object p)
		{
			return !string.IsNullOrWhiteSpace(ArgumentsArray) && !string.IsNullOrWhiteSpace(FunctionText);
		}

		public ICommand AddNewPointCommand { get; }
		private void OnAddNewPointCommandExecuted(object p)
		{
			try
			{

				LagrangeTable.Add(new Point(0, 0));
				Status = $"Строка добавлена";
			}
			catch (Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanAddNewPointCommandExecute(object p)
		{
			return true;
		}

		public ICommand RemoveSelectedPointCommand { get; }
		private void OnRemoveSelectedPointCommandExecuted(object p)
		{
			try
			{
				LagrangeTable.Remove(SelectedPoint);
				SelectedPoint = null;
				Status = $"Строка удалена";
			}
			catch (Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanRemoveSelectedPointCommandExecute(object p)
		{
			return SelectedPoint != null;
		}

		public ICommand BuildFunctionGraphicCommand { get; }
		private void OnBuildFunctionGraphicCommandExecuted(object p)
		{
			try
			{
				Function f = new Function(FunctionText);
				Expression expression;
				PointsFunction.Clear();
				double left = Convert.ToDouble(ArgumentLeftBoard);
				double right = Convert.ToDouble(ArgumentRightBoard);
				double step = Convert.ToDouble(Step);
				Status = "Строю...";
				for (double i = left; i < right; i += step)
				{
					expression = new Expression($"f({i.ToString().Replace(",", ".")})", f);
					PointsFunction.Add(new Point(i, expression.calculate()));
				}
				Status = "График функции построен";
			}
			catch (Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanBuildFunctionGraphicCommandExecute(object p)
		{
			return !(string.IsNullOrWhiteSpace(ArgumentLeftBoard) || string.IsNullOrWhiteSpace(ArgumentRightBoard) || string.IsNullOrWhiteSpace(Step) || string.IsNullOrWhiteSpace(FunctionText));
		}

		public ICommand BuildPolynomGraphicCommand { get; }
		private void OnBuildPolynomGraphicCommandExecuted(object p)
		{
			try
			{
				PointsPolynom.Clear();
				Status = "Строю...";
				double[] args;
				double[] values;
				double left = Convert.ToDouble(ArgumentLeftBoard);
				double right = Convert.ToDouble(ArgumentRightBoard);
				double step = Convert.ToDouble(Step);
				if (NewtonTable.Count > 0)
				{
					args = new double[NewtonTable.Count];
					values = new double[NewtonTable.Count];
					for (int i = 0; i < args.Length; i++)
					{
						args[i] = NewtonTable[i].X;
						values[i] = NewtonTable[i].Y;
					}
					_newton.Arguments = (double[])args.Clone();
					_newton.Values = (double[])values.Clone();
					List<double> otherArgs = new List<double>();
					for (double i = left; i < right; i += step)
					{
						otherArgs.Add(i);
					}
					double[] polynomValues = _newton.GetFunctionValuesInPoints(otherArgs.ToArray());
					for (int i = 0; i < polynomValues.Length; i++)
					{
						PointsPolynom.Add(new Point(otherArgs[i], polynomValues[i]));
					}
					Status = "График многочлена построен";
				}	
				else
					Status = "График длжна быть построена таблица";
			}
			catch (Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanBuildPolynomGraphicCommandExecute(object p)
		{
			return NewtonTable.Count > 0 && CanBuildFunctionGraphicCommandExecute(p);
		}

		public ICommand GenerateTableCommand { get; }
		private void OnGenerateTableCommandExecuted(object p)
		{
			try
			{
				double left = Convert.ToDouble(GenerateTableLeftBoard);
				double right = Convert.ToDouble(GenerateTableRightBoard);
				double step = Convert.ToDouble(GenerateTableStep);
				LagrangeTable.Clear();
				SelectedPoint = null;
				Function f = new Function(FunctionText);
				Expression expression;
				for (double i = left; i < right; i += step)
				{
					expression = new Expression($"f({i.ToString().Replace(",", ".")})", f);
					LagrangeTable.Add(new Point(i, expression.calculate()));
				}
				Status = $"Таблица сгенерирована";
			}
			catch (Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanGenerateTableCommandExecute(object p)
		{
			return !(string.IsNullOrWhiteSpace(GenerateTableLeftBoard) || string.IsNullOrWhiteSpace(GenerateTableRightBoard) || string.IsNullOrWhiteSpace(GenerateTableStep) || string.IsNullOrWhiteSpace(FunctionText));
		}
		#endregion
	}
}
