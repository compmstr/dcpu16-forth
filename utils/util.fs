needs constants.fs
\ 0 constant [struct \ -- 0 | 0 is default size of structure

\ : field \ offset n -- offset+n ; addr -- addr + offset
		( when defining a structure, a field of size n starts at the given offset,
		  returning the next offset. at runtime, the offset is added to the base addres )
		\ create
		\ over , + \ copy offset, add n, allocate n cells
	\ does>
		\ @ +
\ ;

\ : struct] \ offset -- ; -- size ;
		\ \ end a structure defiintion
		\ constant
\ ;

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

