// C++语法
// https://github.com/antlr/grammars-v4/blob/master/cpp/CPP14Lexer.g4
// https://github.com/antlr/grammars-v4/blob/master/cpp/CPP14Parser.g4

// ANTLR文档
// https://decaf-lang.github.io/minidecaf-tutorial/
// https://wizardforcel.gitbooks.io/antlr4-short-course/content/

grammar Fa;



//
// keyword
//
AImport:					'@import';
ALib:						'@lib';
Break:						'break';
CC__Cdecl:					'__cdecl';
CC__FastCall:				'__fastcall';
CC__StdCall:				'__stdcall';
Continue:					'continue';
Class:						'class';
Const:						'const';
Else:						'else';
FaEntryMain:				'FaEntryMain';
If:							'if';
Internal:					'internal';
Public:						'public';
Protected:					'protected';
Private:					'private';
Return:						'return';
Signed:						'signed';
Static:						'static';
Unsigned:					'unsigned';
Use:						'use';
While:						'while';



//
// element
//

// 赋值运算
Assign:						'=';
addAssign:					AddOp Assign;
subAssign:					SubOp Assign;
starAssign:					StarOp Assign;
divAssign:					DivOp Assign;
modAssign:					ModOp Assign;
orAssign:					OrOp Assign;
andAssign:					AndOp Assign;
xorAssign:					XorOp Assign;
qusQusAssign:				qusQusOp Assign;
starStarAssign:				starStarOp Assign;
andAndAssign:				andAndOp Assign;
orOrAssign:					orOrOp Assign;
shiftLAssign:				shiftLOp Assign;
shiftRAssign:				shiftROp Assign;
allAssign:					Assign | qusQusAssign | addAssign | subAssign | starAssign | starStarAssign | divAssign | modAssign | andAssign | orAssign | xorAssign | andAndAssign | orOrAssign | shiftLAssign | shiftRAssign;

// 一元计算
ReverseOp:					'~';
AddAddOp:					'++';
SubSubOp:					'--';

// 二元计算
PointOp:					'.';
AddOp:						'+';
SubOp:						'-';
StarOp:						'*';
DivOp:						'/';
ModOp:						'%';
OrOp:						'|';
AndOp:						'&';
XorOp:						'^';
qusQusOp:					Qus Qus;
starStarOp:					StarOp StarOp;
andAndOp:					AndOp AndOp;
orOrOp:						OrOp OrOp;
shiftLOp:					QuotJianL QuotJianL;
shiftROp:					QuotJianR QuotJianR;

// 三元或其他
Qus:						'?';
Comma:						',';
ColonColon:					'::';
Colon:						':';
Semi:						';';
Exclam:						'!';

// 括号
QuotFangL:					'[';
QuotFangR:					']';
QuotJianL:					'<';
QuotJianR:					'>';
QuotHuaL:					'{';
QuotHuaR:					'}';
QuotYuanL:					'(';
QuotYuanR:					')';

// 比较   TODO
ltOp:						QuotJianL;
ltEqualOp:					QuotJianL Assign;
gtOp:						QuotJianR;
gtEqualOp:					QuotJianR Assign;
equalOp:					Assign Assign;
notEqualOp:					Exclam Assign;
exprFuncDef:				Assign QuotJianR;


selfOp2:					AddOp | SubOp | StarOp | DivOp | starStarOp | ModOp | AndOp | OrOp | XorOp | andAndOp | orOrOp | shiftLOp | shiftROp;
compareOp2:					ltOp | ltEqualOp | gtOp | gtEqualOp | equalOp | notEqualOp;
changeOp2:					qusQusOp | compareOp2;
allOp2:						selfOp2 | changeOp2;



//
// Literal
//
fragment SimpleEscape:		'\\\'' | '\\"' | '\\\\' | '\\n' | '\\r' | ('\\' ('\r' '\n'? | '\n')) | '\\t';
fragment HexEscape:			'\\x' HEX HEX;
fragment UniEscape:			'\\u' HEX HEX HEX HEX;
fragment Schar:				SimpleEscape | HexEscape | UniEscape | ~["\\\r\n];
//
BoolLiteral:				'true' | 'false';
IntLiteral:					NUM+;
FloatLiteral:				NUM+ PointOp NUM+;
String1Literal:				'"' Schar* '"';
literal:					BoolLiteral | IntLiteral | FloatLiteral | String1Literal;

fragment NUM:				[0-9];
fragment HEX:				NUM | [a-fA-F];
fragment ID_BEGIN:			[a-zA-Z_] | ('\\u' HEX HEX HEX HEX);
fragment ID_AFTER:			NUM | [a-zA-Z_] | ('\\u' HEX HEX HEX HEX);
Id:							ID_BEGIN ID_AFTER*;
ids:						Id (PointOp Id)*;



//
// type
//
typeAfter:					(QuotFangL QuotFangR) | AndOp | Qus;
type:						Const? ( Id
								| (Id QuotJianL type (Comma type)* QuotJianR)	// Lazy<int>
								| (QuotYuanL type (Comma type)+ QuotYuanR)		// (int, string)
							) typeAfter*;										// int[]&
eTypeAfter:					(QuotFangL QuotFangR) | AndOp | StarOp;
eSign:						Signed | Unsigned;
eType:						Const? eSign? Id eTypeAfter*;					// int[]&



//
// list
//
typeVar:					type Id?;
typeVarList:				typeVar (Comma typeVar)*;
eTypeVar:					eType Id?;
eTypeVarList:				eTypeVar (Comma eTypeVar)*;



//
// if
//
quotStmtPart:				QuotHuaL stmt* QuotHuaR;
quotStmtExpr:				QuotHuaL stmt* expr QuotHuaR;
ifStmt:						If expr quotStmtPart (Else If expr quotStmtPart)* (Else quotStmtPart)?;
ifExpr:						If expr quotStmtExpr (Else If expr quotStmtExpr)* Else quotStmtExpr;



//
// while
//
whileStmt:					While QuotYuanL expr QuotYuanR QuotHuaL stmt* QuotHuaR;



//
// expr
//
quotExpr:					QuotYuanL expr QuotYuanR;
exprOpt:					expr?;
strongExprBase:				ids | (ColonColon Id) | literal | ifExpr | quotExpr;
strongExprPrefix:			SubOp | AddAddOp | SubSubOp | ReverseOp;										// 前缀 - ++ -- ~
strongExprSuffix			: AddAddOp | SubSubOp															// 后缀 ++ --
							| (QuotYuanL (exprOpt (Comma exprOpt)*) QuotYuanR)								//     Write ("")
							| (QuotFangL (exprOpt (Colon exprOpt)*) QuotFangR)								//     list [12]
							| (PointOp Id)																	//     wnd.Name
							;
strongExpr:					strongExprPrefix* strongExprBase strongExprSuffix*;
middleExpr:					strongExpr (allOp2 strongExpr)*;												//      a == 24    a + b - c
expr:						middleExpr (allAssign middleExpr)*;



//
// define variable
//
tmpAssignExpr:				Assign expr Semi;
defVarStmt:					type Id Assign expr Semi;



//
// stmt
//
useStmt:					Use ids Semi;
normalStmt:					((Return? expr?) | Break | Continue) Semi;
stmt:						normalStmt | ifStmt | whileStmt | defVarStmt;



//
// class
//
publicLevel:				Public | Internal | Protected | Private;
classParent:				Colon ids (Comma ids)*;
classStmt:					publicLevel? Class Id classParent? QuotHuaL (classVar | classFunc)* QuotHuaR;
//
classVarExtFunc:			publicLevel? Id (Semi | classFuncBody);
classVarExt:				QuotHuaL classVarExtFunc+ QuotHuaR tmpAssignExpr?;
classVar:					publicLevel? Static? type Id (Semi | tmpAssignExpr | classVarExt);
//
classFuncName:				Id | (QuotFangL QuotFangR) | allOp2 | allAssign;
classFuncBody:				(exprFuncDef expr Semi) | (QuotHuaL stmt* QuotHuaR);
classFunc:					publicLevel? Static? type classFuncName QuotYuanL typeVarList? QuotYuanR classFuncBody;



//
// import
//
callConvention:				CC__Cdecl | CC__FastCall | CC__StdCall;
importStmt:					AImport eType callConvention Id QuotYuanL eTypeVarList QuotYuanR Semi;
libStmt:					ALib String1Literal Semi;
importBlock:				(importStmt | libStmt)+;



//
// fa_entry_main
//
faEntryMainFuncBlock:		Static type FaEntryMain QuotYuanL QuotYuanR QuotHuaL stmt* QuotHuaR;
program:					useStmt* importBlock? classStmt* faEntryMainFuncBlock?;



//
// skips
//
Comment1:					'/*' .*? '*/' -> channel (HIDDEN);
Comment2:					'//' ~ [\r\n]* -> channel (HIDDEN);
WS:							[ \t\r\n]+ -> channel (HIDDEN);
