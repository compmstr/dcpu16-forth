
2 constant short

\ Calculates number of shorts for offsets (like cells)
: shorts ( n -- n )
		short * ;

: short-array ( size "name" -- loc ; index -- loc )
		create shorts
		here over erase
		allot
	DOES> 
		swap shorts + ;

\ support for shorts in gforth's structs
1 shorts 1 shorts 2constant short%

\ support using shorts within a create
: w, ( n -- )
		here w! \ store the short at here
		short allot
;
\ create <name> <short> sh, <short> sh, ...

\ Storage for the size (in shorts) of the last create <name> <val> cw, ...
variable cw,-len
0 cw,-len !
: cw, ( n -- )
		1 cw,-len +!
		w,
;
\ To use:
\ create test-code 0x7c01 cw, 0x0030 cw,
\ cw,-len @ --> (size of test-code)

: wvariable ( "name" -- addr )
		\ creates a 2-byte variable
		create short allot
;

