needs util.fs

\ copy the string at loc to a count-prefixed string at new-loc
: copy-string ( loc count new-loc -- )
		swap \ loc new-loc count
		2dup swap ! \ loc new-loc count
		swap cell+ \ loc count new-loc+1
		swap \ loc new-loc+1 count
		cmove
;
\ allocate and copy string over
: save-string ( loc count -- new-loc )
		dup cell+ allocate throw
		-rot 2 pick
		copy-string
;


\ turn a counted string (pointer to <loc> <string> ), into a loc count pair
: get-counted-string ( loc -- loc count )
		dup cell+ swap \ loc+1 loc
		@ \ loc+1 count
;

\ generates a hash for a string
\ djb2 algorithm:
(
	hash_i = hash_i-1 * 33 ^ str[i]
)
: hash-string ( loc len -- hash )
		#5381 \ loc len init
		swap 0 do \ loc init len 0 do
				\ loc hash_i-1
				over i + c@ \ loc hash_i-1 [loc+i]
				swap dup \ loc c hash hash
				5 lshift \ loc c hash hash<<5
				+ + \ loc hash hash<<5 + c +
				0xFFFF and \ limit to a word
		loop
		\ drop location
		swap drop
;

\ prints a <count> <string> string
: print-string ( loc -- )
		get-counted-string
		type
;

\ tab
#9 constant TAB-char
#10 constant newline-char
\ bl is space

\ check if character is tab, newline, space
: whitespace? ( c -- t/f )
		dup tab-char = \ c t/f
		over newline-char = or \ c t/f
		over bl = or \ c t/f
		swap drop \ t/f
;

: null? ( c -- t/f )
		0 =
;

: upper-diff
		[char] a [char] A -
;

: upper-case-char ( char -- CHAR )
		\ could also use non-standard 'toupper'
		dup [char] a >= \ char >=a
		over [char] z <= and \ char lower?
		if
				upper-diff -
		then
;

: lower-case-char ( CHAR -- char )
		dup [char] A >=
		over [char] Z <= and
		if
				upper-diff +
		then
;

: map-string ( xt loc count -- )
    dup 0 = if
        2drop drop
        exit
    then
		0 do
				i over + \ xt loc loc+i
				dup c@ \ xt loc loc+i [loc+i]
				3 pick execute \ xt loc loc+i CHAR
				swap c! \ xt loc
		loop
		\ drop xt and loc
		2drop
;

\ sets all chars in string provided to be upper case
: upper-case ( loc count -- )
		['] upper-case-char -rot
		map-string
;

: lower-case ( loc count -- )
		['] lower-case-char -rot
		map-string
;

: first-char ( loc count -- c )
		drop c@
;

: last-char ( loc count -- c)
		1- + c@
;
\ returns true if string is surrounded by [ and ]
: square-bracketed? ( loc count -- t/f )
		2dup first-char [char] [ =
		-rot
		last-char [char] ] = and
;
\ returns true if string is surrounded by " and "
: quoted? ( loc count -- t/f )
		2dup first-char [char] [ =
		-rot
		last-char [char] ] = and
;

: starts-with ( loc-n count-n loc-h -- t/f)
		swap 0 do \ loc-n loc-h
				over i + c@ \ loc-n loc-h c-n
				over i + c@ \ loc-n loc-h c-n c-h
				<> if
						2drop 0 leave
				then
		loop
		\ if it's not false, set to true
		0 over <> if
			2drop 1
		then
;

: starts-with-cstring ( needle haystack -- t/f )
		get-counted-string drop \ needle h-loc
		swap get-counted-string rot \ n-loc n-count h-loc
		starts-with
;

: starts-with-hex-start? ( loc -- t/f )
		dup c@ [char] 0 = swap \ =0 loc
		1+ \ =0 loc+1
		c@ \ =0 char
		[char] x over = \ =0 char =x
		swap [char] X = or
		and
;

: decimal-digit? ( char -- t/f )
		dup [char] 0 >=
		swap [char] 9 <= and
;
: hex-digit? ( char -- t/f )
		dup decimal-digit?
		swap upper-case-char \ 0-9? CHAR
		dup [char] A >=
		swap [char] F <= and \ 0-9? A-F?
		or
;


\ returns true if string has only digits and/or begins with 0X
: string-number? ( loc count -- t/f )
		over starts-with-hex-start? dup if \ loc count hex?
				-rot \ hex? loc count
				\ eat 2 chars, and reduce count
				2 - swap 2 + swap
		else
				-rot \ hex? loc count
		then
		0 do \ hex? loc
				i over + c@ \ hex? loc char
				\ if this is a hex number, check for a hex digit
				2 pick if \ hex? loc char
						hex-digit? \ hex? loc hex-digit?
				else
						decimal-digit? \ hex? loc digit?
				then
				not if
						drop 0 leave
				then
		loop
		swap drop as-bool
;

: string->number ( loc count -- number )
		\ evaluate
		\ try to convert the number
		s>number? drop \ drop flag, it will just return 0
		d>s
;

\ takes off the first and last chars of a string
: string-strip-ends ( loc count -- loc count )
		2 - \ loc count-2
		2dup over \ loc count loc count loc
		1+ -rot \ loc count loc+1 loc count
		cmove
;

\ gets a string with the first and last char not seen
: string-without-ends ( loc count -- loc count )
		2 - swap 1 + swap
;

\ finds first instance of the char passed in in a string
\   returns -1 if not found
: string-first-instance ( loc count char -- idx )
		-1 swap \ loc count -1(flag) char
		rot 0 do \ loc flag char
				2 pick i + c@ \ loc flag char [loc+i]
				over = if
						swap drop i swap \ loc i char
						leave
				then
		loop
		drop \ drop off char
		swap drop \ drop off loc
;

: string-contains? ( loc count char -- t/f )
		string-first-instance -1 <>
;

\ returns 2 strings, before delim, and after delim
\   if char not found, returns empty (length 0) string for 2nd string
\ s" i+1000" 2dup char + string-split
\   cr type bl emit type cr -> 1000 i
: string-split ( loc count char -- loc0 count0 loc1 count1 )
		2 pick 2 pick rot \ loc count loc count char
		string-first-instance \ loc count idx
		dup -1 = if \ loc count idx
				\ character not found...
				drop
				2dup + 0 \ loc count loc+count 0
		else
				\ copy loc/count to return stack
				-rot 2dup 2>r rot \ loc count idx
				swap drop dup \ loc count0(idx) idx
				2r> \ loc count0 idx loc count
				rot 1+ \ .. loc count idx+1
				swap over - \ .. loc idx count-idx
				-rot + swap \ .. loc+idx count-idx
		then
;

: remove-trailing-comma ( loc count -- loc count )
		2dup last-char [char] , = if
				1-
		then
;

\ returns counted string
: input$ ( -- addr$ )
		256 allocate throw
		dup cell+ 255 accept
		over !
;

