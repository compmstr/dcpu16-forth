
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

\ copy the string at loc to a count-prefixed string at new-loc
: copy-string ( loc count new-loc -- )
		swap \ loc new-loc count
		2dup swap c! \ loc new-loc count
		swap 1+ \ loc count new-loc+1
		swap \ loc new-loc+1 count
		move
;
( s" hello there"
dup allocate throw
-rot 2 pick \ new-loc loc size new-loc
copy-string
)
\ prints a <count> <string> string
: print-string ( loc -- )
		\ get count
		dup c@ swap 1+ \ count loc+1
		swap type
;

