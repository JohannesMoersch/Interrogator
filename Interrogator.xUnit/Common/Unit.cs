﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Interrogator.xUnit.Common
{
	public struct Unit : IEquatable<Unit>
	{
		public static Unit Value { get; } = new Unit();

		public bool Equals(Unit other)
			=> true;

		public override int GetHashCode()
			=> 0;

		public override bool Equals(object obj)
			=> obj is Unit;

		public override string ToString()
			=> "Unit";
	}
}
