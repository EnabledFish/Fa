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
	public class AstExpr_Switch: IAstExpr {
		public IAstExpr Condition { get; set; }
		public List<IAstExpr> CaseValues { get; set; }
		public List<IAstExpr> CaseWhen { get; set; }
		public List<(List<IAstStmt> _stmts, IAstExpr _expr)> CaseCodes { get; set; }



		public override void Traversal (int _deep, int _group, Func<IAstExpr, int, int, IAstExpr> _cb) {
			if (Condition != null)
				Condition = _cb (Condition, _deep, _group);
			if (CaseValues != null)
				CaseValues.Traversal (_deep, _group, _cb);
			CaseWhen.Traversal (_deep, _group, _cb);
			for (int i = 0; i < CaseCodes.Count; ++i) {
				var _temp_int = Common.GetRandomInt ();
				for (int j = 0; j < CaseCodes[i]._stmts.Count; ++j)
					CaseCodes[i]._stmts[j] = _cb (CaseCodes[i]._stmts[j],_deep + 1, _temp_int) as IAstStmt;
				CaseCodes[i] = (_stmts: CaseCodes[i]._stmts, _expr: _cb (CaseCodes[i]._expr, _deep + 1, _temp_int));
			}
		}

		public override IAstExpr TraversalCalcType (IAstType _expect_type) {
			if (Condition != null)
				Condition = Condition.TraversalCalcType (null);
			if (CaseValues != null)
				CaseValues.TraversalCalcType (Condition.ExpectType);
			CaseWhen.TraversalCalcType (IAstType.FromName ("bool"));
			if (_expect_type == null)
				_expect_type = TypeFuncs.GetCompatibleType (true, (from p in CaseCodes select p._expr.GuessType ()).ToArray ());
			for (int i = 0; i < CaseCodes.Count; ++i) {
				for (int j = 0; j < CaseCodes[i]._stmts.Count; ++j)
					CaseCodes[i]._stmts[j] = CaseCodes[i]._stmts[j].TraversalCalcType (null) as IAstStmt;
				CaseCodes[i] = (CaseCodes[i]._stmts, CaseCodes[i]._expr.TraversalCalcType (_expect_type));
			}
			ExpectType = _expect_type;
			return AstExprTypeCast.Make (this, _expect_type);
		}

		public override IAstType GuessType () {
			return TypeFuncs.GetCompatibleType (true, (from p in CaseCodes select p._expr.GuessType ()).ToArray ());
		}

		public override (List<IAstStmt>, IAstExpr) ExpandExpr ((IAstExprName _var, AstStmt_Label _pos)? _cache_err) {
			var _temp_id = Common.GetTempId ();
			var _defvar_stmt = new AstStmt_DefVariable { Token = Token, DataType = ExpectType, VarName = _temp_id };
			var _switch_stmt = new AstStmt_Switch { Token = Token, Condition = Condition, CaseValues = CaseValues, CaseWhen = CaseWhen, CaseCodes = new List<IAstStmt> () };
			foreach (var (_stmts1, _expr1) in CaseCodes) {
				_stmts1.Add (AstStmt_ExprWrap.MakeAssign (_defvar_stmt.GetRef (), _expr1));
				_switch_stmt.CaseCodes.Add (_stmts1.Count == 1 ? _stmts1[0] : new AstStmt_HuaQuotWrap { Token = _expr1.Token, Stmts = _stmts1 });
			}
			var _stmts = new List<IAstStmt> { _defvar_stmt };
			_stmts.AddRange (_switch_stmt.ExpandStmt (_cache_err));
			return (_stmts, _defvar_stmt.GetRef ());
		}

		public override string GenerateCSharp (int _indent) => throw new Exception ("不应执行此处代码");

		public override bool AllowAssign () => false;
	}
}
