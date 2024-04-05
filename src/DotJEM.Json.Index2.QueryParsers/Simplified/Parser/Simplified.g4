grammar Simplified;

/* Inspired by: https://github.com/lrowe/lucenequery and JIRA */
/*
 * Parser Rules
 */

query  : WS? clause = defaultClause  (WS order = orderingClause)? WS? EOF;

/*
 This implements all clauses grouped into batches of the same type.
 The order implements precedence (important).
*/

defaultClause : orClause (WS? orClause)*;
orClause      : andClause (orOperator andClause)*;
andClause     : notClause (andOperator notClause)*;
notClause     : basicClause (notOperator basicClause)*;
basicClause   :
  WS? LPA defaultClause WS? RPA
  | WS? atom
  ;

atom : pureValue | wildcardValue | field | inClause | notInClause;

anyClause: STAR WS? COLON WS? STAR;

// Range: field : [A TO B]
rangeClause :
    fieldName = name
    WS? COLON WS?
    LSBR WS?
    from = rangeValue
    WS TO WS
    to = rangeValue WS?
    RSBR;
rangeValue : starValue 
           | pureValue 
           | offsetValue 
           ;

// In: field IN (A, B, C)  |  field NOT IN (A, B, C)
inClause       : TERM WS IN WS? LPA WS? pureValue ( WS? COMMA WS? pureValue )* WS? RPA;
notInClause    : TERM WS NOT WS IN WS? LPA WS? pureValue ( WS? COMMA WS? pureValue )* WS? RPA;

// Order: ORDER BY field:DESC, field2, field3:ASC
orderingClause    : WS? ORDER WS BY WS orderingField ( WS? COMMA WS? orderingField )* WS?;
orderingField     : WS? fieldName = name (WS direction = orderingDirection)?;
orderingDirection : (ASC | DESC);

field       : TERM WS? operator WS? ( wildcardValue | starValue | pureValue );
name        : TERM;

wildcardValue : WILDCARD_TERM       #Wildcard
              ;

starValue     : STAR                #MatchAll
              ;

pureValue     : TERM                #Term
              | DATE                #Date
              | DATE_TIME           #DateTime
              | INTEGER             #Integer
              | DECIMAL             #Decimal
              | PHRASE              #Phrase
              ;

offsetValue   : SIMPLE_DATE_OFFSET  #SimpleDateOffset
              | COMPLEX_DATE_OFFSET #ComplexDateOffset
              ;



andOperator : WS? AND;
orOperator  : WS? OR;
notOperator : WS? (AND WS)? NOT;

operator : EQ       #Equals
         | COLON    #Equals
		 | NEQ		#NotEquals
         | GT	    #GreaterThan
		 | GTE      #GreaterThanOrEquals
		 | LT       #LessThan
		 | LTE      #LessThanOrEquals
		 | SIM      #Similar
		 | NSIM     #NotSimilar
		 ;



/*
 * Lexer Rules
 */
 
LPA   : '(';
RPA   : ')';
STAR  : '*';
QMARK : '?';
COMMA : ',';
PLUS  : '+';
MINUS : '-';
DOT   : '.';
COLON : ':';

AND        : A N D      ;
OR         : O R        ;
NOT        : N O T      ;
IN         : I N        ;
ORDER      : O R D E R  ;
BY		   : B Y        ;
ASC        : A S C      ;
DESC       : D E S C    ;
TO         : T O        ;

DAYS       : D A Y S    ;

EQ   : '='       ;
NEQ  : '!='      ;
GT   : '>'       ;
GTE  : '>='      ;
LT   : '<'       ;
LTE  : '<='      ;
SIM  : '~'       ;
NSIM : '!~'      ;

WS  : (' '|'\t'|'\r'|'\n'|'\u3000')+;
SS  : ' ';

fragment INT        : '0' .. '9';
fragment ESC        : '\\' .;

INTEGER  : MINUS? INT+;
DECIMAL  : MINUS? INT+ ('.' INT+)?;

// Special Date Handling:
//updated > 2018-03-04T14:41:23+00:00
fragment TIMEOFFSET  : ( MINUS | PLUS ) INT INT ( ':' INT INT );
TIME        : INT INT ':' INT INT ( ':' INT INT )? TIMEOFFSET?;
DATE        : INT INT INT INT MINUS INT INT MINUS INT INT;
DATE_TIME   : DATE 'T' TIME;

// Special Timespan Handling:
fragment NOW                      : N O W;
fragment TODAY                    : T O D A Y;

fragment SIMPLE_TIMESPAN          : (INT+ '.')? INT INT ':' INT INT ( ':' INT INT ('.' INT INT))?;
SIMPLE_DATE_OFFSET                : ( ( NOW | TODAY ) SS? )? ( PLUS | MINUS ) SIMPLE_TIMESPAN;

fragment COMPLEX_TIME_SPAN_DAY    : INT+ SS? ( D | D A Y | DAYS );
fragment COMPLEX_TIME_SPAN_HOUR   : INT+ SS? ( H | H O U R | H O U R S );
fragment COMPLEX_TIME_SPAN_MIN    : INT+ SS? ( M | M I N | M I N U T E | M I N U T E S );
fragment COMPLEX_TIME_SPAN_SEC    : INT+ SS? ( S | S E C | S E C O N D | S E C O N D S );
fragment COMPLEX_TIMESPAN
    : COMPLEX_TIME_SPAN_DAY
    | COMPLEX_TIME_SPAN_DAY SS? COMPLEX_TIME_SPAN_HOUR
    | COMPLEX_TIME_SPAN_DAY SS? COMPLEX_TIME_SPAN_HOUR SS? COMPLEX_TIME_SPAN_MIN
    | COMPLEX_TIME_SPAN_DAY SS? COMPLEX_TIME_SPAN_HOUR SS? COMPLEX_TIME_SPAN_MIN SS? COMPLEX_TIME_SPAN_SEC
    | COMPLEX_TIME_SPAN_DAY SS? COMPLEX_TIME_SPAN_MIN
    | COMPLEX_TIME_SPAN_DAY SS? COMPLEX_TIME_SPAN_MIN SS? COMPLEX_TIME_SPAN_SEC
    | COMPLEX_TIME_SPAN_DAY SS? COMPLEX_TIME_SPAN_SEC

    | COMPLEX_TIME_SPAN_HOUR
    | COMPLEX_TIME_SPAN_HOUR SS? COMPLEX_TIME_SPAN_MIN
    | COMPLEX_TIME_SPAN_HOUR SS? COMPLEX_TIME_SPAN_MIN SS? COMPLEX_TIME_SPAN_SEC
    | COMPLEX_TIME_SPAN_HOUR SS? COMPLEX_TIME_SPAN_SEC

    | COMPLEX_TIME_SPAN_MIN
    | COMPLEX_TIME_SPAN_MIN SS? COMPLEX_TIME_SPAN_SEC

    | COMPLEX_TIME_SPAN_SEC
    ;
COMPLEX_DATE_OFFSET               : ( ( NOW | TODAY ) SS? )? ( PLUS | MINUS ) COMPLEX_TIMESPAN;


fragment TERM_CHAR  : (~( ' ' | '\t' | '\n' | '\r' | '\u3000' | '\'' | '\"' 
                        | '(' | ')'  | '['  | ']'  | '{'      | '}'  
						| '!' | ':'  | '~'  | '>'  | '='      | '<'
						| '?' | '*'
				        | '\\'| ',' )| ESC );

fragment WILDCARD_CHAR : (~( ' ' | '\t' | '\n' | '\r' | '\u3000' | '\'' | '\"' 
                        | '(' | ')'  | '['  | ']'  | '{'      | '}' 
						| '!' | ':'  | '~'  | '>'  | '='      | '<'
				        | '\\'| ',' )| ESC ); 


TERM   : TERM_CHAR+ ;
WILDCARD_TERM  : WILDCARD_CHAR+;

PHRASE : '\"' ( ESC | ~('\"'|'\\'))+ '\"';

fragment A : [aA];
fragment B : [bB];
fragment C : [cC];
fragment D : [dD];
fragment E : [eE];
fragment F : [fF];
fragment G : [gG];
fragment H : [hH];
fragment I : [iI];
fragment J : [jJ];
fragment K : [kK];
fragment L : [lL];
fragment M : [mM];
fragment N : [nN];
fragment O : [oO];
fragment P : [pP];
fragment Q : [qQ];
fragment R : [rR];
fragment S : [sS];
fragment T : [tT];
fragment U : [uU];
fragment V : [vV];
fragment W : [wW];
fragment X : [xX];
fragment Y : [yY];
fragment Z : [zZ];