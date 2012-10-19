needs constants.fs
needs types.fs
needs files.fs
needs shorts.fs
needs strings.fs
needs util.fs

struct
		cell% field label-name \ pointer to counted string
		short% field label-pos
		cell% field label-next
end-struct code-label

variable labels-head
0 labels-head !

0x10000 short-array code-buffer
variable code-buffer-pos
0 code-buffer-pos !

variable file-line-pos
0 file-line-pos !

: eat-whitespace ( -- ) \ advance file-line-pos until next non-whitespace
		begin
				file-line-buffer file-line-pos @ \ line-buffer line-pos
				+ c@ \ current char
				whitespace?
		while
						file-line-pos @
						1+ file-line-pos !
		repeat
;

: get-next-token ( -- loc count ) \ find the next whitespace delimited token in the file line
		eat-whitespace \ clear out any leading whitespace
		\ store the loc and the initial size
		file-line-pos @ file-line-buffer + \ file-line-buffer+pos
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
		file-line-pos @ over + file-line-pos ! \ loc count
;

: get-op ( loc len -- op-code ) \ token string -> op-code
;
: get-b ( -- vmloc )
;
: get-a ( -- vmloc )
;
\ store encoded op at next spot in code-buffer,
\   increment code-buffer-pos
: encode-op ( op a b -- )
;

: is-token-comment ( loc size -- loc size t/f )
		over c@
		[char] ; =
;
: is-token-label ( loc size -- loc size t/f )
		over c@
		[char] : = 
;
: store-label ( loc size -- )
		\ strip off the :
		swap 1+ swap 1- \ loc+1 size-1
		\ store the next token as a label
		dup allocate throw \ loc size new-loc
		-rot 2 pick \ new-loc loc size new-loc
		copy-string \ new-loc

		code-label %alloc \ new-loc label-loc
		dup label-name \ new-loc label-loc label-name
		rot \ label-loc label-name new-loc
		swap ! \ label-loc

		code-buffer-pos @ \ label-loc code-pos
		over label-pos ! \ label-loc

		labels-head @ \ label-loc labels-head
		over label-next ! \ label-loc
		labels-head !
;

: process-line ( u2 -- )
		0 file-line-pos !
		." Processing line: "
		file-line-buffer over type cr
		get-next-token \ loc size
		is-token-comment if
		else
				is-token-label if
						store-label
				else
						get-op \ expects a token
						get-b
						get-a
						encode-op
				then
		then
;

: compile-file ( string len -- )
		open-input
		begin
				read-input-line
		while \ while eats the EOF flag
						process-line
		repeat
;

