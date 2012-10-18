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
		#15 REG_A reg-set
		#20 test-loc vmloc-set
		REG_A reg-get .
;

create test-code
0x7c01 cw, 0x0030 cw, \ set A, 0x30
0x7fc1 cw, 0x0020 cw, 0x1000 cw, \ SET [0x1000], 0x20
0x7803 cw, 0x1000 cw, \ SUB A, [0x1000]
0xc013 cw, \ IFN A, 0x10
0x7f80 cw, 0x0020 cw, \ SET PC, end(0x20)
\ loop
0xa8c1 cw, \ SET I, 10
0x7c01 cw, 0x2000 cw, \ SET A, 0x2000
\ :loop -- 0x0D
0x22c1 cw, 0x2000 cw, \ SET [0x2000+I], [A]
0x84c3 cw, \ SUB I, 1
0x80d3 cw, \ IFN I, 0
0xb781 cw, \ SET PC, loop
\ test ops
0xc001 cw, \ set A, 0x10
0x7c21 cw, 0x0020 cw, \ set B, 0x20
0x0022 cw, \ ADD B, A --> 0x30
0x0026 cw, \ DIV B, A --> 0x3
0x0024 cw, \ MUL B, A --> 0x30

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
		clear-registers
		0 VM_PC w!
		0xFFFF VM_SP w!
		0 VM_EX w!
		0 VM_IA w!
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

: vm-run ( -- ) \ runs until get-next-code-word returns 0
		begin
				get-next-code-word dup while
						run-word
		repeat
		." Done" cr
		drop \ drop the 0 left over from get-next-code-word
;

: encode-word ( a b op -- val )
		swap #5 lshift + \ a b/op
		swap #10 lshift + \ a/b/op
;

