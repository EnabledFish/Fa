﻿using fac.AntlrTools;
using fac.ASTs.Exprs.Names;
using fac.ASTs.Stmts;
using fac.ASTs.Types;
using fac.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Exprs {
	public class AstExprTypeCast: IAstExpr {
		public IAstExpr Value { get; set; }



		private AstExprTypeCast () { }

		public static IAstExpr Make (IAstExpr _dest, IAstType _to_type) {
			if (_dest.ExpectType == null) {
				if (_dest is AstExprName_BuildIn)
					return _dest;
				throw new Exception ("应识别类型后做转换处理");
			} else if (_to_type == null || _to_type is AstType_Any || _dest.ExpectType.IsSame (_to_type)) {
				return _dest;
			} else if (
				TypeFuncs.AllowTypeCast (_dest.ExpectType, _to_type)
				//|| (_dest.ExpectType is AstType_OptionalWrap _owrap && _owrap.ItemType.IsSame (_to_type))
				|| (_to_type is AstType_OptionalWrap _owrap2 && _dest.ExpectType.IsSame (_owrap2.ItemType))
				) {
				return new AstExprTypeCast { Token = _dest.Token, ExpectType = _to_type, Value = _dest };
			} else if (_dest.ExpectType is AstType_OptionalWrap _otype1 && _to_type is AstType_OptionalWrap _otype2 && TypeFuncs.AllowTypeCast (_otype1.ItemType, _otype2.ItemType)) {
				return new AstExprTypeCast { Token = _dest.Token, ExpectType = _to_type, Value = _dest };
			} else {
				throw new CodeException (_dest.Token, $"类型 {_dest.ExpectType} 无法转为类型 {_to_type}");
			}
		}

		public static IAstExpr ForceMake (IAstExpr _dest, IAstType _to_type) {
			return new AstExprTypeCast { Token = _dest.Token, Value = _dest, ExpectType = _to_type };
		}

		public override void Traversal (int _deep, int _group, Func<IAstExpr, int, int, IAstExpr> _cb) => Value = _cb (Value, _deep, _group);

		public override IAstExpr TraversalCalcType (IAstType _expect_type) {
			// 只有一种情况会调用到，提前构造好转换，也就是ExpectType设置好之后
			if (ExpectType == null)
				throw new NotImplementedException ();
			Value.TraversalCalcType (null);
			return this;
		}

		public override IAstType GuessType () => Value.GuessType ();

		public override (List<IAstStmt>, IAstExpr) ExpandExpr () {
			var (_stmts, _expr) = Value.ExpandExpr ();
			Value = _expr;
			return (_stmts, this);
		}

		public override string GenerateCSharp (int _indent) {
			string _val = Value.GenerateCSharp (_indent);
			if (Value.ExpectType is AstType_OptionalWrap _owrap && ExpectType.IsSame (_owrap.ItemType)) {
				return $"{_val}.GetValue ()";
			} else {
				string _type = ExpectType.GenerateCSharp (_indent);
				if (ExpectType is AstType_OptionalWrap _owrap1 && Value.ExpectType.IsSame (_owrap1.ItemType)) {
					return $"{_type}.FromValue ({_val})";
				} else {
					return $"({_type}) {_val}";
				}
			}
		}

		public override bool AllowAssign () => false;
	}
}
