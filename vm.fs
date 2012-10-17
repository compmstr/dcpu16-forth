needs shorts.fs
needs constants.fs
needs util.fs
needs vmloc.fs
needs vmvars.fs

: get-code-word-op ( word -- op )
		\ bottom 5 bits
		0x1F and
;

: get-code-word-a ( word -- a )
		\ top 6 bits
		10 rshift
;

: get-code-word-b ( word -- b )
		\ 2nd 5 bits from bottom
		5 rshift
		0x1F and
;

\ get the next code word, as well as a/b if needed
\   can also be used to skip an instruction
: get-next-code-word ( -- a b word )
		get-next-word \ word
		dup get-code-word-a \ word a-code
		vmloc-from-bits \ word a-loc
		." A: " dump-vmloc cr
		over dup get-code-word-op \ word a-loc word op
		swap get-code-word-b \ word a-loc op b-code
		swap OP_SPECIAL <> if \ if op isn't special/0
				vmloc-from-bits \ get b, otherwise it's the special op
				." B: " dump-vmloc cr
		then
		." Op: " dup get-code-word-op .
		cr
;

create test-loc vmloc %allot drop
: test-loc-store
		LOC_REG test-loc vmloc-type w!
		REG_A test-loc vmloc-register w!
		15 REG_A reg-set
		20 test-loc vmloc-set
		REG_A reg-get .
;

create test-code
0x7c01 cw, 0x0030 cw,
0x7fc1 cw, 0x0020 cw, 0x1000 cw,
0x7803 cw, 0x1000 cw,
0xc013 cw,
0x7f80 cw, 0x0020 cw,

variable test-code-len
cw,-len @ test-code-len !

: vm-skip ( -- ) \ skips next code word
		." SKIP"
		get-next-code-word
;

: load-test-code ( -- )
		test-code-len @ 0 do
				test-code i shorts + w@
				i ram-set
		loop
;

: vm-init ( -- )
		load-test-code
		0 VM_PC w!
		0xFFFF VM_SP w!
		0 VM_EX w!
		0 VM_IA w!
;

: run-special-word ( a op -- )
		case
				OP_JSR of
				endof
				OP_INT of
				endof
				OP_IAG of
				endof
				OP_IAS of
				endof
				OP_RFI of
				endof
				OP_IAQ of
				endof
				OP_HWN of
				endof
				OP_HWQ of
				endof
				OP_HWI of
				endof
		endcase
;

\ Standard opcodes
: run-word ( a b word -- )
		case get-code-word-op \ a b
				OP_SPECIAL of
						drop run-special-word
				endof
				OP_SET of
						swap vmloc-get \ b a-val
						swap vmloc-set \ a-val b vmloc-set
				endof
				OP_ADD of
				endof
				OP_SUB of \ b-a -> b
						swap over vmloc-get \ b a b-val
						swap vmloc-get \ b b-val a-val
						- \ b b-a
						swap vmloc-set
				endof
				OP_MUL of
				endof
				OP_MLI of
				endof
				OP_DIV of
				endof
				OP_DVI of
				endof
				OP_MOD of
				endof
				OP_MDI of
				endof
				OP_AND of
				endof
				OP_BOR of
				endof
				OP_XOR of
				endof
				OP_SHR of
				endof
				OP_ASR of
				endof
				OP_SHL of
				endof
				OP_IFB of
				endof
				OP_IFC of
				endof
				OP_IFE of
				endof
				OP_IFN of \ run next code only if a != b
						= if
								vm-skip
						then
				endof
				OP_IFG of
				endof
				OP_IFA of
				endof
				OP_IFL of
				endof
				OP_IFU of
				endof
				OP_ADX of
				endof
				OP_SBX of
				endof
				OP_STI of
				endof
				OP_STD of
				endof
		endcase
;

: vm-step ( -- )
		get-next-code-word
		run-word
;

: encode-word ( a b op -- val )
		swap 5 lshift + \ a b/op
		swap 10 lshift + \ a/b/op
;

