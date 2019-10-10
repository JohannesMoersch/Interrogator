using System;

namespace Interrogator.xUnit.Common
{
	public class InvalidGroupNameException : Exception
	{
		public string GroupName { get; }

		public InvalidGroupNameException(string groupName)
		{
			GroupName = groupName;
		}
	}
}