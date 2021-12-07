﻿using fac.AntlrTools;
using fac.ASTs.Exprs;
using fac.ASTs.Exprs.Names;
using fac.ASTs.Types;
using fac.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Stmts {
	public class AstStmt_ExprWrap: IAstStmt {
		public IAstExpr Expr { get; set; }



		public static AstStmt_ExprWrap MakeAssign (IAstExprName _dest, IAstExpr _src) {
			return new AstStmt_ExprWrap { Token = _src.Token, Expr = new AstExpr_Op2 { Token = _src.Token, Value1 = _dest, Value2 = _src, Operator = "=", ExpectType = _dest.ExpectType } };
		}

		public static AstStmt_ExprWrap MakeOp2 (IAstExpr _val1, string _op, IAstExpr _val2, IAstType _expect_type) {
			return new AstStmt_ExprWrap { Token = _val1.Token, Expr = AstExpr_Op2.MakeOp2 (_val1, _op, _val2, _expect_type) };
		}

		public static AstStmt_ExprWrap MakeCondition (IAstExpr _val1, string _op, IAstExpr _val2) {
			return new AstStmt_ExprWrap { Token = _val1.Token, Expr = AstExpr_Op2.MakeCondition (_val1, _op, _val2) };
		}

		public override void Traversal (int _deep, int _group, Func<IAstExpr, int, int, IAstExpr> _cb) {
			if (Expr != null)
				Expr = _cb (Expr, _deep, _group);
		}

		public override IAstExpr TraversalCalcType (IAstType _expect_type) {
			if (_expect_type != null)
				throw new Exception ("语句类型不可指定期望类型");
			if (Expr != null) {
				Expr = Expr.TraversalCalcType (null);

				// 异常强制处理
				if ((!(Expr is AstExpr_Op2 _op2expr && _op2expr.Operator == "=")) && Expr.ExpectType is AstType_OptionalWrap)
					throw new CodeException (Token, "此处未处理异常必须处理");
			}
			return this;
		}

		public override List<IAstStmt> ExpandStmt ((IAstExprName _var, AstStmt_Label _pos) _cache_err) {
			return ExpandStmtHelper (_cache_err, (_check_cb) => {
				var (_stmts, _expr) = Expr.ExpandExpr (_cache_err, _check_cb);
				Expr = _expr;
				_stmts.Add (this);
				return _stmts;
			});
		}

		public override string GenerateCSharp (int _indent) => $"{_indent.Indent ()}{Expr.GenerateCSharp (_indent)};\r\n";
	}
}
