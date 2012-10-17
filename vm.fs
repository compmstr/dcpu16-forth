needs shorts.fs
needs constants.fs
needs ops.fs
needs util.fs
needs vmloc.fs
needs vmvars.fs

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
		get-code-word-op \ a b op
		ops-xt @ \ <op xt or 0>
		0 over = if
				." Opcode not implemented"
		else
				execute
		then
;

: vm-step ( -- )
		get-next-code-word
		0 over <> if
				run-word
		else
				." No more code"
		then
;

: encode-word ( a b op -- val )
		swap 5 lshift + \ a b/op
		swap 10 lshift + \ a/b/op
;

