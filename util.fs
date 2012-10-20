
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

