needs ../utils/util.fs
needs vmvars.fs
needs vmloc.fs
needs vmcode.fs
needs specialops.fs

: op-get-vals ( a b -- b-val a-val)
		vmloc-get
		swap vmloc-get
;

: op-get-b-and-vals ( a b -- b b-val a-val )
		swap over \ b a b
		op-get-vals
;

: run-OP_SPECIAL ( a op -- )
		\ b is the new code op
		\ get-next-word doesn't convert it to a vmloc since this was a special op
		special-ops-xt @
		0 over = if
				." Special opcode not implemented" cr
		else
				execute
		then
;
: run-OP_SET ( a b -- ) \ a -> b
		." OP_SET" cr
		swap vmloc-get \ b a-val
		swap vmloc-set \ a-val b vmloc-set
;
: run-OP_ADD ( a b -- ) \ sets b to b+a, sets EX to 1 if overflow, 0 otherwise
		." OP_ADD" cr
		op-get-b-and-vals \ b b-val a-val
		+ \ b b+a
		0xFFFF over > if
				1
		else
				0
		then \ b b+a 0/1
		VM_EX-set \ b b+a
		swap vmloc-set
;
: run-OP_SUB ( a b -- ) \ b-a -> b -- EX is 0xFFFF if underflow, 0 otherwise
		." OP_SUB" cr
		op-get-b-and-vals \ b b-val a-val
		- \ b b-a
		0 over > if
				0xFFFF
		else
				0
		then \ b b-a 0/0xFFFF
		VM_EX-set \ b b-a
		swap vmloc-set
;
: run-OP_MUL ( a b -- ) \ sets b to b*a, sets EX to ((b*a) >> 16) & 0xFFFF (b and a are unsigned)
		." OP_MUL" cr
		op-get-b-and-vals \ b b-val a-val
		* \ b b*a
		dup 16 rshift 0xFFFF and \ b b*a EX
		VM_EX-set \ b b*a
		0xFFFF and \ b b*a (limited to 0xFFFF)
		swap vmloc-set
;
: run-OP_MLI ( a b -- ) \ like MUL, but signed
;
: run-OP_DIV ( a b -- ) \ sets b to b/a, sets EX to ((b<<16)/a)&0xFFFF. if a==0, sets b, EX to 0
		." OP_DIV" cr
		op-get-b-and-vals \ b b-val a-val
		0 over = if
				2drop 0 swap vmloc-set \ ignore b-val/a-val, set b to 0
				0 VM_EX-set
		else
				2dup \ b b-val a-val b-val a-val
				swap 16 lshift \ ... a-val b-val<<16
				swap / \ ... b-val<<16 / a-val
				0xFFFF and VM_EX-set \ b b-val a-val
				/ \ b b/a
				swap vmloc-set
		then
;
: run-OP_DVI ( a b -- ) \ like DIV, but signed
;
: run-OP_MOD ( a b -- ) \ sets b to b%a, if a == 0, sets b to 0 instead
		." OP_MOD" cr
		op-get-b-and-vals \ b b-val a-val
		0 over = if
				2drop 0 swap vmloc-set \ ignore b-val/a-val, set b to 0
		else
				mod \ b <b-val%a-val>
				swap vmloc-set
		then
;
: run-OP_MDI ( a b -- ) \ like mod, but signed (MDI -7, 16 == -7)
;
: run-OP_AND ( a b -- ) \ sets b to b&a
		." OP_AND" cr
		op-get-b-and-vals \ b b-val a-val
		and swap vmloc-set
;
: run-OP_BOR ( a b -- ) \ sets b to b|a
		." OP_BOR" cr
		op-get-b-and-vals \ b b-val a-val
		or swap vmloc-set
;
: run-OP_XOR ( a b -- ) \ sets b to b^a
		." OP_XOR" cr
		op-get-b-and-vals \ b b-val a-val
		xor swap vmloc-set
;
: run-OP_SHR ( a b -- )
		op-get-b-and-vals \ b b-val a-val
		rshift \ b b>>a
		\ set b
		swap vmloc-set
		\ set ex to ((b<<16) >> a)&0xffff
		\ ... b is 16 bits, will always be 0
		0 VM_EX-set
;
: run-OP_ASR ( a b -- )
;
: run-OP_SHL ( a b -- )
		." OP_SHL" cr
		op-get-b-and-vals \ b b-val a-val
		lshift \ b b<<a
		dup 16 rshift 0xFFFF and \ b b<<a ((b<<a)>>16)&0xFFFF
		VM_EX-set \ b b<<a
		swap vmloc-set
;
: run-OP_IFB ( a b -- ) \ skips unless (b&a)!=0
		op-get-vals \ b-val a-val
		and
		0 = if
				vm-skip
		then
;
: run-OP_IFC ( a b -- ) \ skips unless (b&a)==0
		op-get-vals \ b-val a-val
		and
		0 <> if
				vm-skip
		then
;
: run-OP_IFE ( a b -- ) \ run next code only if a == b
		." OP_IFE" cr
		op-get-vals \ b-val a-val
		<> if
				vm-skip
		then
;
: run-OP_IFN ( a b -- ) \ run next code only if a != b
		." OP_IFN" cr
		op-get-vals \ b-val a-val
		= if
				vm-skip
		then
;
: run-OP_IFG ( a b -- ) \ skip unless b>a
		." OP_IFG" cr
		op-get-vals \ b-val a-val
		<= if
				vm-skip
		then
;
: run-OP_IFA ( a b -- ) \ skip unless b>a (signed)
;
: run-OP_IFL ( a b -- ) \ skip unless b<a
		." OP_IFL" cr
		op-get-vals \ b-val a-val
		>= if
				vm-skip
		then
;
: run-OP_IFU ( a b -- ) \ skip unless b<a (signed)
;
: run-OP_ADX ( a b -- )
;
: run-OP_SBX ( a b -- )
;
: run-OP_STI ( a b -- ) \ set b to a, inc I and J
		run-OP_SET
		REG_I reg-get 1+ REG_I reg-set
		REG_J reg-get 1+ REG_J reg-set
;
: run-OP_STD ( a b -- ) \ set b to a, dec I and J
		run-OP_SET
		REG_I reg-get 1- REG_I reg-set
		REG_J reg-get 1- REG_J reg-set
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

