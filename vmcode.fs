needs vmvars.fs
needs util.fs
needs ops.fs

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
\ leaves 0 on the stack if no more code
: get-next-code-word ( -- a b word )
		get-next-word \ word
		dup 0x0000 <> if
			dup get-code-word-a \ word a-code
			vmloc-from-bits \ word a-loc
			\ ." A: " dump-vmloc cr
			over dup get-code-word-op \ word a-loc word op
			swap get-code-word-b \ word a-loc op b-code
			swap OP_SPECIAL <> if \ if op isn't special/0
					vmloc-from-bits \ get b, otherwise it's the special op
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