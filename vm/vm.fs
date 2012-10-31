needs ../utils/shorts.fs
needs ../utils/constants.fs
needs ../utils/util.fs
needs ../utils/files.fs
needs vmvars.fs
needs ops.fs
needs vmloc.fs
needs hw.fs

: load-code-from-file ( string count -- )
		read-bin-file \ buffer size-read
		2 / \ convert size to shorts
		0 do
				dup i shorts + w@
				i ram-set
		loop
		free drop \ don't need buffer anymore
;

: vm-init-hw ( -- )
		s" vm/hw/clock.fs"
		s" vm/hw/clock.fs"
		s" vm/hw/dumper.fs"
		3
		add-hw
;

: vm-init ( -- )
		clear-registers
		0 VM_PC w!
		0xFFFF VM_SP w!
		0 VM_EX w!
		0 VM_IA w!
		vm-init-hw
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

\ update all of the hardware installed
: update-hw ( -- )
		hw-count @ 0 do
				i hw-devs hw-list @ +
				update-xt @ execute
		loop
;

\ returns true if able to run code, false if no more code
: vm-step ( -- t/f )
		get-next-code-word
		0 over <> if
				run-word
				true
		else
				." No more code" cr
				\ 0 already on the stack
		then
		\ try to run an interrupt
		update-hw
		run-sw-interrupt
;

: vm-run ( -- ) \ runs until get-next-code-word returns 0
		begin
				vm-step while
		repeat
		." Done" cr
;

: encode-word ( a b op -- val )
		swap #5 lshift + \ a b/op
		swap #10 lshift + \ a/b/op
;

create test-code
0x7c01 cw, 0x0030 cw, \ set A, 0x30
0x7fc1 cw, 0x0020 cw, 0x1000 cw, \ SET [0x1000], 0x20
0x7803 cw, 0x1000 cw, \ SUB A, [0x1000]
0xc013 cw, \ IFN A, 0x10
0x7f81 cw, 0x0020 cw, \ SET PC, end(0x20)
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

: vm-run-file ( loc size -- )
		load-code-from-file
		vm-init
		utime d>s
		vm-run
		utime d>s
		cr ." ---Run time: " swap - 1000 / . ." ms---" cr
;

: run-test-file ( -- )
		s" test.dbin" 
		vm-run-file
;

