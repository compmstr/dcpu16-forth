needs constants.fs

variable debug-mode
0 debug-mode !

: type-if-debug ( loc count -- )
		debug-mode @
		if
				type cr
		else
				2drop
		then
;

: output-bits-binary ( n bits -- )
		dup 0 do \ n bits
				2dup 1- i - rshift 0x1 and \ check if bit is set
				if
						[char] 1 emit
				else
						[char] 0 emit
				then
		loop
		2drop
;

: output-dword-binary ( dword -- )
		32 output-bits-binary
;

: output-word-binary ( word -- )
		16 output-bits-binary
;

\ output like .", only if debug-mode is non-0
: .d"
		[char] " parse
		postpone sliteral
		postpone type-if-debug
; immediate

: array ( size -- ; n -- addr )
		create cells \ create n cells
		here over erase \ clear them
		allot
	DOES>
		swap cells + \ index in, address out
;

: not ( t/f -- f/t )
		if
				#0
		else
				#1
		then
;

: struct-size ( align size -- size )
		swap drop
;

: as-bool ( flag -- true/false )
		if
				true
		else
				false
		then
;

: char->reg ( char -- reg_... )
		dup [char] A = if drop REG_A exit then
		dup [char] B = if drop REG_B exit then
		dup [char] C = if drop REG_C exit then
		dup [char] X = if drop REG_X exit then
		dup [char] Y = if drop REG_Y exit then
		dup [char] Z = if drop REG_Z exit then
		dup [char] I = if drop REG_I exit then
		dup [char] J = if drop REG_J exit then
;

: wait-ns ( ns -- )
		utime d>s \ ns start-time
		begin
				utime d>s \ ns start-time now
				over - \ ns start-time diff
				2 pick < while
		repeat
		2drop
;

\ frees a passed in addr if it's not zero
: free-addr-if-nonzero
		0 over <> if
				free throw
		else
				drop
		then
;

\ looks at a variable, if it's not zero, calls free on the value
: free-var-if-nonzero ( addr -- )
		@ 0 over <> if
				free throw
		else
				drop
		then
;

\ signed/unsigned word conversions
: uw>sw ( uw -- sw )
		dup 0x7FFF and \ uw w
		swap 0x8000 and \ w neg?
		if
				negate
		then
;
: sw>uw ( sw -- uw )
		dup 0< if
				negate
				0x8000 or
				0xFFFF and
		else
				0x7FFF and
		then
;
: 2uw>sw ( uw1 uw2 -- sw1 sw2 )
		uw>sw swap uw>sw swap
;
: 2sw>uw ( sw1 sw2 -- uw1 uw2 )
		sw>uw swap sw>uw swap
;
