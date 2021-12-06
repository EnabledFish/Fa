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
	public class AstStmt_DefVariable: IAstStmt {
		public IAstType DataType { get; set; }
		public string VarName { init; get; } = Common.GetTempId ();
		public IAstExpr Expr { get; set; }



		public AstExprName_Variable GetRef () => new AstExprName_Variable { Token = Token, ExpectType = DataType, Var = this };

		public static List<IAstStmt> FromCtx (FaParser.DefVarStmtContext _ctx) {
			List<IAstStmt> _stmts = new List<IAstStmt> ();
			var _type = IAstType.FromContext (_ctx.type ());
			foreach (var _var_ctx in _ctx.idAssignExpr ()) {
				var _varstmt = new AstStmt_DefVariable { Token = _ctx.Start, DataType = _type, VarName = _var_ctx.id ().GetText () };
				_varstmt.Expr = FromContext (_var_ctx.expr ());
				if (_varstmt.Expr is AstExpr_Lambda _lambdaexpr)
					_lambdaexpr.InitLambda (_type);
				_stmts.Add (_varstmt);
			}
			return _stmts;
		}

		public override void Traversal (int _deep, int _group, Func<IAstExpr, int, int, IAstExpr> _cb) {
			if (VarName == "_")
				throw new CodeException (Token, "声明的变量名不能为“_”");
			if (Expr != null)
				Expr = _cb (Expr, _deep, _group);
		}

		public override IAstExpr TraversalCalcType (IAstType _expect_type) {
			if (_expect_type != null)
				throw new Exception ("语句类型不可指定期望类型");
			Expr = Expr.TraversalCalcType (DataType);
			return this;
		}

		public override List<IAstStmt> ExpandStmt ((IAstExprName _var, AstStmt_Label _pos) _cache_err) {
#warning TODO: 此处也许不用传递
			return ExpandStmtHelper (_cache_err, (_check_cb) => {
				var _stmts = new List<IAstStmt> { this };
				if (Expr != null && (!Expr.IsSimpleExpr)) {
					var (_stmts2, _expr) = Expr.ExpandExpr (_cache_err, _check_cb);
					Expr = null;
					_stmts.AddRange (_stmts2);
					_stmts.Add (new AstStmt_ExprWrap {
						Token = Token,
						Expr = new AstExpr_Op2 {
							Token = Token,
							Value1 = GetRef (),
							Value2 = _expr,
							Operator = "=",
							ExpectType = ExpectType,
						},
					});
				}
				return _stmts;
			});
		}

		public override string GenerateCSharp (int _indent) {
			string _code = $"{_indent.Indent ()}{DataType.GenerateCSharp (_indent)} {VarName}";
			return $"{_code}{(Expr != null ? $" = {Expr.GenerateCSharp (_indent)}" : "")};\r\n";
		}
	}
}
