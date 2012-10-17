needs util.fs
needs vmvars.fs
needs vmloc.fs
needs vmcode.fs

: run-OP_SPECIAL ( a b -- )
;
: run-OP_SET ( a b -- ) \ a -> b
		." OP_SET"
		swap vmloc-get \ b a-val
		swap vmloc-set \ a-val b vmloc-set
;
: run-OP_ADD ( a b -- )
;
: run-OP_SUB ( a b -- ) \ b-a -> b -- EX is 0xFFFF if underflow, 0 otherwise
		." OP_SUB cr
		swap over vmloc-get \ b a b-val
		swap vmloc-get \ b b-val a-val
		- \ b b-a
		0 over < if then
		swap vmloc-set
;
: run-OP_MUL ( a b -- )
;
: run-OP_MLI ( a b -- )
;
: run-OP_DIV ( a b -- )
;
: run-OP_DVI ( a b -- )
;
: run-OP_MOD ( a b -- )
;
: run-OP_MDI ( a b -- )
;
: run-OP_AND ( a b -- )
;
: run-OP_BOR ( a b -- )
;
: run-OP_XOR ( a b -- )
;
: run-OP_SHR ( a b -- )
;
: run-OP_ASR ( a b -- )
;
: run-OP_SHL ( a b -- )
;
: run-OP_IFB ( a b -- )
;
: run-OP_IFC ( a b -- )
;
: run-OP_IFE ( a b -- ) \ run next code only if a == b
		." OP_IFE" cr
		<> if
				vm-skip
		then
;
: run-OP_IFN ( a b -- ) \ run next code only if a != b
		." OP_IFN" cr
		= if
				vm-skip
		then
;
: run-OP_IFG ( a b -- )
;
: run-OP_IFA ( a b -- )
;
: run-OP_IFL ( a b -- )
;
: run-OP_IFU ( a b -- )
;
: run-OP_ADX ( a b -- )
;
: run-OP_SBX ( a b -- )
;
: run-OP_STI ( a b -- )
;
: run-OP_STD ( a b -- )
;

0x20 array ops-xt \ operation XT, opcode is index
\ Fill the ops array
' run-OP_SPECIAL 0x00 ops-xt !
' run-OP_SET 0x01 ops-xt !
' run-OP_ADD 0x02 ops-xt !
' run-OP_SUB 0x03 ops-xt !
' run-OP_MUL 0x04 ops-xt !
' run-OP_MLI 0x05 ops-xt !
' run-OP_DIV 0x06 ops-xt !
' run-OP_DVI 0x07 ops-xt !
' run-OP_MOD 0x08 ops-xt !
' run-OP_MDI 0x09 ops-xt !
' run-OP_AND 0x0a ops-xt !
' run-OP_BOR 0x0b ops-xt !
' run-OP_XOR 0x0c ops-xt !
' run-OP_SHR 0x0d ops-xt !
' run-OP_ASR 0x0e ops-xt !
' run-OP_SHL 0x0f ops-xt !
' run-OP_IFB 0x10 ops-xt !
' run-OP_IFC 0x11 ops-xt !
' run-OP_IFE 0x12 ops-xt !
' run-OP_IFN 0x13 ops-xt !
' run-OP_IFG 0x14 ops-xt !
' run-OP_IFA 0x15 ops-xt !
' run-OP_IFL 0x16 ops-xt !
' run-OP_IFU 0x17 ops-xt !
' run-OP_ADX 0x1a ops-xt !
' run-OP_SBX 0x1b ops-xt !
' run-OP_STI 0x1e ops-xt !
' run-OP_STD 0x1f ops-xt !

