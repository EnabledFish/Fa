﻿using fac.AntlrTools;
using fac.ASTs.Exprs;
using fac.ASTs.Exprs.Names;
using fac.ASTs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Stmts {
	public class AstStmt_While: IAstStmt {
		public bool IsDoWhile { init; get; }
		public IAstExpr Condition { get; set; }
		public List<IAstStmt> Contents { get; set; }



		public override void Traversal (int _deep, int _group, Func<IAstExpr, int, int, IAstExpr> _cb) {
			if (!IsDoWhile)
				Condition = _cb (Condition, _deep + 1, 0);
			Contents.Traversal (_deep + 1, 0, _cb);
			if (IsDoWhile)
				Condition = _cb (Condition, _deep + 1, 0);
		}

		public override IAstExpr TraversalCalcType (IAstType _expect_type) {
			if (_expect_type != null)
				throw new Exception ("语句类型不可指定期望类型");
			if (Info.CurrentFunc.ReturnType is AstType_OptionalWrap) {
				try {
					Condition = Condition.TraversalCalcType (IAstType.FromName ("bool"));
				} catch (Exception) {
					Condition = Condition.TraversalCalcType (IAstType.FromName ("bool?"));
				}
			} else {
				Condition = Condition.TraversalCalcType (IAstType.FromName ("bool"));
			}
			Contents.TraversalCalcType ();
			return this;
		}

		public override List<IAstStmt> ExpandStmt ((IAstExprName _var, AstStmt_Label _pos) _cache_err) {
			return ExpandStmtHelper (_cache_err, (_check_cb) => {
				var (_stmts, _expr) = Condition.ExpandExpr (_cache_err, _check_cb);
				Condition = _expr;
				Contents = Contents.ExpandStmts (_cache_err);
				Contents.AddRange (_stmts);
				if (IsDoWhile) {
					return new List<IAstStmt> { this };
				} else {
					_stmts.Add (this);
					return _stmts;
				}
			});
		}

		public override string GenerateCSharp (int _indent) {
			var _sb = new StringBuilder ();
			if (IsDoWhile) {
				_sb.Append ($"{_indent.Indent ()}do {{");
				_sb.AppendStmts (Contents, _indent + 1);
				_sb.Append ($"{_indent.Indent ()}}} while ({Condition.GenerateCSharp (_indent)});");
			} else {
				_sb.Append ($"{_indent.Indent ()}while ({Condition.GenerateCSharp (_indent)}) {{");
				_sb.AppendStmts (Contents, _indent + 1);
				_sb.Append ($"{_indent.Indent ()}}}");
			}
			return _sb.ToString ();
		}
	}
}
