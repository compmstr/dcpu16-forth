needs strings.fs

256 constant max-line
struct
		cell% field input-file-line-buffer
		cell% field input-file-line-buffer-len
		cell% field input-file-fd
		cell% field input-file-line-pos
		cell% field input-file-next
end-struct input-file

variable input-file-stack
0 input-file-stack !

: push-input-file ( addr u -- )
		\ open file
		." push-input-file: " 2dup type cr
		r/o open-file throw \ fd
		input-file %alloc \ fd mem
		swap over \ mem fd mem
		input-file-fd ! \ mem
		max-line 2 + allocate throw \ mem buffer
		over input-file-line-buffer !
		0 over input-file-line-pos !
		\ get current top of stack
		input-file-stack @
		over input-file-next !
		\ set this new one as top of stack
		input-file-stack !
;

: pop-input-file ( -- )
		input-file-stack @
		0 over = if
				abort" Error: no more files on stack"
		then
		\ close the file on the top of the stack
		dup input-file-fd @ close-file throw
		\ free the buffer
		dup input-file-line-buffer free throw
		\ get the 2nd in stack
		input-file-next @
		input-file-stack !
;

\ returns true if there is a file open on the top of the file stack
: is-input-file-open? ( -- t/f )
		input-file-stack @ 0 <> if
				true
		else
				false
		then
;

0 value fd-out

: is-output-open? ( -- t/f )
		0 fd-out <>
;

: close-input ( -- )
		pop-input-file
;

: close-output ( -- )
		is-output-open? if
			fd-out close-file throw
			0 to fd-out
		else
				." Error: File not open" cr
		then
;


: open-input ( addr u -- )
		push-input-file
;

: open-output-bin ( addr u -- )
		is-output-open? if
				." Error, file already open"
		else
			w/o bin \ bin modifies w\o to make it binary
			create-file throw 
			to fd-out
		then
;
: write-output-bin ( addr count -- )
		fd-out write-file throw
;

: read-bin-file ( addr u -- buffer size-read )
		r/o bin open-file throw >r
		r@ file-size throw
		d>s
		dup allocate throw \ size buffer
		dup rot \ buffer buffer size
		r@ read-file throw \ buffer size-read
		r> close-file throw
;

: print-current-line-buffer
		input-file-stack @ input-file-line-buffer @
		input-file-stack @ input-file-line-buffer-len @
		type
;

: uppercase-input-buffer
		\ convert the input into upper case
		input-file-stack @ input-file-line-buffer @
		input-file-stack @ input-file-line-buffer-len @
		upper-case
;

: read-input-line ( -- count not-eof )
		input-file-stack @ >r
		0 r@ input-file-line-pos !
		r@ input-file-line-buffer @ max-line erase
		r@ input-file-line-buffer @ max-line
		r@ input-file-fd @ read-line throw
		over r> input-file-line-buffer-len !
;

: eat-whitespace ( -- ) \ advance file-line-pos until next non-whitespace
		input-file-stack @ >r
		begin
				r@ input-file-line-buffer @
				r@ input-file-line-pos @ \ line-buffer line-pos
				+ c@ \ current char
				whitespace?
		while
						1 r@ input-file-line-pos +!
		repeat
		r> drop
;

: get-next-token ( -- loc count ) \ find the next whitespace/null delimited token in the file line
		eat-whitespace \ clear out any leading whitespace
		input-file-stack @ >r
		\ store the loc and the initial size
		r@ input-file-line-pos @
		r@ input-file-line-buffer @ + \ file-line-buffer+pos
		0 
		begin
				2dup + c@ \ loc count [loc+count]
				dup \ loc count char char
				whitespace? not \ loc count char t/f
				swap null? not and \ loc count t/f
		while
						\ increment size of token
						1+ \ loc size
		repeat
		\ update the file-line pos
		\ file-line-pos @ over + file-line-pos ! \ loc count
		dup r> input-file-line-pos +!
;
