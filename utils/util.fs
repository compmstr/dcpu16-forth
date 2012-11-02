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

: output-dword-binary ( dword -- )
		32 0 do
				dup 31 i - rshift 0x1 and
				if
						[char] 1 emit
				else
						[char] 0 emit
				then
		loop
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
