﻿using Antlr4.Runtime;
using fac.ASTs.Stmts;
using fac.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Exprs.Names {
	public abstract class IAstExprName: IAstExpr {
		public static IAstExprName FindClass (IToken _token, string _str_class) {
			var _classes = Info.GetClassFromName (_str_class);
			if (_classes.Count == 0) {
				return null;
			} else if (_classes.Count == 1) {
				return new AstExprName_Class { Token = _token, Class = _classes[0] };
			} else {
				throw new CodeException (_token, $"不明确的符号 {_str_class}。可能为{string.Join ('、', from p in _classes select p.FullName)}");
			}
		}

		public override void Traversal ((int _deep, int _group, int _loop, Func<IAstExpr, int, int, int, IAstExpr> _cb) _trav) { }

		public override (List<IAstStmt>, IAstExpr) ExpandExprAssign (IAstExpr _rval, (IAstExprName _var, AstStmt_Label _pos)? _cache_err) {
			var (_stmts, _val) = ExpandExpr (_cache_err);
			_stmts.Add (AstStmt_ExprWrap.MakeAssign (_val, _rval));
			return (_stmts, _val);
		}

		public override (List<IAstStmt>, IAstExpr) ExpandExpr ((IAstExprName _var, AstStmt_Label _pos)? _cache_err) => (new List<IAstStmt> (), this);
	}
}
