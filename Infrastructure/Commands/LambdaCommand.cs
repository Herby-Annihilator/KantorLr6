using KantorLr6.Infrastructure.Commands.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace KantorLr6.Infrastructure.Commands
{
	public class LambdaCommand : Command
	{
		private Action<object> _execute;
		private Func<object, bool> _canExecute;

		public LambdaCommand(Action<object> execute, Func<object, bool> canExecute)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		public override bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

		public override void Execute(object parameter)
		{
			if (!CanExecute(parameter)) return;
			_execute.Invoke(parameter);
		}
	}
}
