using System;

namespace RoboSharp
{
	public class GeneralOutputEventArgs : EventArgs
	{
		public string GeneralOutput
		{
			get;
			set;
		}

		public GeneralOutputEventArgs(string file)
		{
			GeneralOutput = file;
		}
	}
}