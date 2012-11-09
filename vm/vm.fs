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
		s" vm/hw/dumper.fs"
		s" vm/hw/screen.fs"
		s" vm/hw/keyboard.fs"
		4
		add-hw
;

: vm-init ( -- )
		clear-registers
		0 VM_PC w!
		0xFFFF VM_SP w!
		0 VM_EX w!
		0 VM_IA w!
		0 VM_CYCLES !
		0 to VM_START_TIME
		vm-init-hw
		vm-cycles-clear
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

\ limits execution to 100khz in cycles
: vm-throttle
		\ 10 ns per cycle for 100,000 cycles per second
		10 vm_cycles @ * \ expected-time
		utime d>s vm_start_time - \ expected actual
		- dup 0 > if
				wait-ns
		else
				drop
		then
;

\ returns true if able to run code, false if no more code
: vm-step ( -- t/f )
		get-next-code-word
		0 over <> if
				run-word
				vm-throttle
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

: vm-run-file ( loc size -- )
		load-code-from-file
		vm-init
		utime d>s to vm_start_time
		vm-run
		utime d>s
		cr ." ---Run time: " vm_start_time - 1000 / . ." ms---" cr
;

: run-test-file ( -- )
		s" test.dbin" 
		vm-run-file
;

