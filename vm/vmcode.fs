needs vmvars.fs
needs ../utils/util.fs
needs ops.fs

: get-code-word-op ( word -- op )
		\ bottom 5 bits
		0x1F and
;

: get-code-word-a ( word -- a )
		\ top 6 bits
		#10 rshift
;

: get-code-word-b ( word -- b )
		\ 2nd 5 bits from bottom
		#5 rshift
		0x1F and
;

\ get the next code word, as well as a/b if needed
\   can also be used to skip an instruction
\ leaves 0 on the stack if no more code
: get-next-code-word ( -- a b word )
		get-next-word \ word
		dup 0x0000 <> if
			dup get-code-word-a \ word a-code
			vmloc_a vmloc-from-bits \ word a-loc
			\ ." A: " dump-vmloc cr
			over dup get-code-word-op \ word a-loc word op
			swap get-code-word-b \ word a-loc op b-code
			swap OP_SPECIAL <> if \ if op isn't special/0
					vmloc_b vmloc-from-bits \ get b, otherwise it's the special op
					\ ." B: " dump-vmloc cr
			then
			\ word a b
			rot \ a b word
			\ dup ." Op: " get-code-word-op . cr
	else
		VM_PC- \ set PC back one
	then
;

: vm-skip ( -- ) \ skips next code word
		get-next-code-word
		2drop drop \ drop a, b, and word
;

: sw-interrupt ( message -- )
		VM_IA-get 0 over <> if
				enqueue-intq
		else
				drop \ do nothing
		then
;

: leave-sw-interrupt ( -- )
		\ turn off interrupt queueing
		false intq-queue !
		\ pop A
		VM_SP_POP REG_A reg-set
		\ pop PC
		VM_SP_POP VM_PC-set
;

: run-sw-interrupt ( -- )
		false intq-empty? =
		false intq-queue @ =
		and
		0 VM_IA-get <>
		and if
				true intq-queue !
				\ push PC and A to stack
				VM_PC-get VM_SP_PUSH
				REG_A reg-get VM_SP_PUSH
				\ put the message in A
				dequeue-intq \ message
				REG_A reg-set
				\ set PC to IA
				VM_IA-get VM_PC-set
		then
;

: dump-vm-state ( -- )
		cr ." Registers -- " cr
		." A: " REG_A reg-get . space
		." B: " REG_B reg-get . space
		." C: " REG_C reg-get . cr
		." X: " REG_X reg-get . space
		." Y: " REG_Y reg-get . space
		." Z: " REG_Z reg-get . cr
		." I: " REG_I reg-get . space
		." J: " REG_J reg-get . cr
		." Misc -- " cr
		." PC: " VM_PC-get . space
		." EX: " VM_EX-get . space
		." IA: " VM_IA-get . space
		." SP: " VM_SP-get . cr
;

