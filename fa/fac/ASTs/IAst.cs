﻿using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs {
	abstract class IAst {
		public IToken Token { init; get; }

		public abstract string GenerateCSharp (int _indent);
	}
}
