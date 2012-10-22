needs util.fs

\ copy the string at loc to a count-prefixed string at new-loc
: copy-string ( loc count new-loc -- )
		swap \ loc new-loc count
		2dup swap c! \ loc new-loc count
		swap 1+ \ loc count new-loc+1
		swap \ loc new-loc+1 count
		move
;
\ allocate and copy string over
: save-string ( loc count -- new-loc )
		dup allocate throw
		-rot 2 pick
		copy-string
;


\ turn a counted string (pointer to <loc> <string> ), into a loc count pair
: get-counted-string ( loc -- loc count )
		dup 1+ swap \ loc+1 loc
		c@ \ loc+1 count
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
		\ get count
		dup c@ swap 1+ \ count loc+1
		swap type
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

\ sets all chars in string provided to be upper case
: upper-case ( loc count -- )
		0 do
				i over + \ loc loc+i
				dup c@ \ loc loc+i [loc+i]
				dup [char] a >= \ loc loc+i [loc+i] >='a'
				over [char] z <= and \ loc loc+i char >='a'&<='z'
				if \ loc loc+i char
						upper-diff - \ loc loc+i CHAR
						swap c!
				else
						2drop
				then
		loop
		\ drop loc
		drop
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

s" 0x" save-string constant hex-start
: starts-with-hex-start ( loc -- t/f )
		hex-start get-counted-string rot starts-with
;

\ returns true if string has only digits and/or begins with 0X
: string-number? ( loc count -- t/f )
		over starts-with-hex-start if
				\ eat 2 chars, and reduce count
				2 - swap 2 + swap
		then
		0 do \ loc
				i over + c@ \ loc char
				dup [char] 0 >= \ loc char >=0
				swap [char] 9 <= and
				not if
						drop 0 leave
				then
		loop
;
		