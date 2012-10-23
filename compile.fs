needs constants.fs
needs types.fs
needs files.fs
needs shorts.fs
needs strings.fs
needs util.fs
needs op-convert-table.fs

struct
		cell% field label-name \ pointer to counted string
		short% field label-pos
		cell% field label-next
end-struct code-label

struct
		\ one of the LOC_... constants
		short% field tokenval-type
		\ Register to use
		short% field tokenval-register
		\ Ram location to use (or offset)
		short% field tokenval-loc
		\ Literal value to use
		short% field tokenval-val
		\ String to use to look up label
		cell% field tokenval-label
end-struct tokenval

struct
		cell% field codelist-op
		cell% field codelist-bval
		cell% field codelist-aval
		cell% field codelist-next
end-struct codelist

struct
		cell% field codelist-special-op
		cell% field codelist-special-aval
		cell% field codelist-special-next
end-struct codelist-special

: clear-tokenval ( tokenval -- tokenval )
		dup tokenval swap drop erase
;

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

\ All of these: ( loc count -- t/f )
: tokenvalue-LOC_REG?
		\ check if it's one char
		1 = if
				c@ false \ char false
				over [char] A = or
				over [char] B = or
				over [char] C = or
				over [char] X = or
				over [char] Y = or
				over [char] Z = or
				over [char] I = or
				over [char] J = or
		else
				false
		then
		swap drop
;
: tokenvalue-LOC_MEM?
		2dup square-bracketed?
		-rot
		2 pick not if 2drop exit then
		string-without-ends
		string-number? and
;
: tokenvalue-LOC_REG_MEM?
		2dup square-bracketed?
		-rot
		string-without-ends
		tokenvalue-LOC_REG? and
;
: tokenvalue-LOC_REG_MEM_OFFSET?
		dup 2 <= if 2drop false exit then
		2dup square-bracketed?
		-rot
		string-without-ends
		2dup
		[char] + string-contains?
		-rot \ brackets? contains? loc count
		[char] + string-split
		dup 1 = if
				\ if first one is register (1 char) ...
				tokenvalue-LOC_REG? -rot
				string-number?
		else
				string-number? -rot
				tokenvalue-LOC_REG?
		then \ brackets? contains? num? reg? <or reg? num?>
		and and and
;
: tokenvalue-LOC_LITERAL?
		string-number?
;
: tokenvalue-LOC_SP?
		s" SP" compare 0 =
;
: tokenvalue-LOC_PC?
		s" PC" compare 0 =
;
: tokenvalue-LOC_EX?
		s" EX" compare 0 =
;
: tokenvalue-LOC_PUSHPOP?
		2dup
		s" PUSH" compare 0 =
		-rot
		s" POP" compare 0 = or
;
: tokenvalue-LOC_PEEK?
		s" PEEK" compare 0 =
;
: tokenvalue-LOC_PICK?
		s" PICK" compare 0 =
;
create tokentype-checks
' TOKENVALUE-LOC_REG? , LOC_REG ,
' TOKENVALUE-LOC_MEM? , LOC_MEM ,
' TOKENVALUE-LOC_REG_MEM? , LOC_REG_MEM ,
' TOKENVALUE-LOC_REG_MEM_OFFSET? , LOC_REG_MEM_OFFSET ,
' TOKENVALUE-LOC_LITERAL? , LOC_LITERAL ,
' TOKENVALUE-LOC_SP? , LOC_SP ,
' TOKENVALUE-LOC_PC? , LOC_PC ,
' TOKENVALUE-LOC_EX? , LOC_EX ,
' TOKENVALUE-LOC_PUSHPOP? , LOC_PUSHPOP ,
' TOKENVALUE-LOC_PEEK? , LOC_PEEK ,
' TOKENVALUE-LOC_PICK? , LOC_PICK ,
0 ,

\ takes a token, and returns a LOC_ constant for the type of value
\ returns -1 on no type found
: tokenvalue-type ( loc size -- LOC_... )
		\ if ends with comma, drop comma
		2dup last-char [char] , = if
				1-
		then
		tokentype-checks -rot \ checks loc size
		begin
				2dup 4 pick \ checks loc size(x2) checks
				@ execute \ checks loc size type?
				if
						2drop 1 cells + @
						\ flag to leave loop
						true
				else
						rot 2 cells + \ loc size next_entry
						dup @ 0 = if \ if the next entry = 0
								drop 2drop
								LOC_LABEL true \ exit loop, leaving LOC_LABEL on stack
						else
								-rot
								false \ continue loop
						then
				then
		until
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

\ All of these: ( tokenval loc count -- tokenvalue )
: tokenvalue-get-LOC_REG
		." LOC_REG GET" cr
		2 pick tokenval-type LOC_REG swap w!
		drop c@ char->reg
		over tokenval-register w!
;
: tokenvalue-get-LOC_MEM
		." LOC_MEM GET" cr
		2 pick tokenval-type LOC_MEM swap w!
		string-without-ends string->number
		over tokenval-loc w!
;
: tokenvalue-get-LOC_REG_MEM
		." LOC_REG_MEM GET" cr
		2 pick tokenval-type LOC_REG_MEM swap w!
		string-without-ends drop c@ char->reg
		over tokenval-register w!
		
;
: tokenvalue-get-LOC_REG_MEM_OFFSET
		." LOC_REG_MEM_OFFSET GET" cr
		2 pick tokenval-type LOC_REG_MEM_OFFSET swap w!
		string-without-ends
		[char] + string-split
		dup 1 = if
				\ top of stack is register (1 char):
				drop c@ char->reg
				-rot
				\ get offset
				string->number \ tokenval reg offset
		else
				string->number \ tokenval loc count offset
				-rot
				drop c@ char->reg \ tokenval offset reg
				swap
		then \ tokenval reg offset
		2 pick tokenval-loc w! 
		over tokenval-register w!
;
: tokenvalue-get-LOC_LITERAL
		." LOC_LITERAL GET" cr
		2 pick tokenval-type LOC_LITERAL swap w!
		string->number
		over tokenval-val w!
;
: tokenvalue-get-LOC_SP
		." LOC_SP GET" cr
		2 pick tokenval-type LOC_SP swap w!
;
: tokenvalue-get-LOC_PC
		." LOC_PC GET" cr
		2 pick tokenval-type LOC_PC swap w!
;
: tokenvalue-get-LOC_EX
		." LOC_EX GET" cr
		2 pick tokenval-type LOC_EX swap w!
;
: tokenvalue-get-LOC_PUSHPOP
		." LOC_PUSHPOP GET" cr
		2 pick tokenval-type LOC_PUSHPOP swap w!
;
: tokenvalue-get-LOC_PEEK
		." LOC_PEEK GET" cr
		2 pick tokenval-type LOC_PEEK swap w!
;
: tokenvalue-get-LOC_PICK
		." LOC_PICK GET" cr
		2 pick tokenval-type LOC_PICK swap w!
;
: tokenvalue-get-LOC_LABEL
		." LOC_LABEL GET" cr
		2 pick tokenval-type LOC_LABEL swap w!
;
create tokenvalue-getters
' TOKENVALUE-GET-LOC_REG ,
' TOKENVALUE-GET-LOC_MEM ,
' TOKENVALUE-GET-LOC_REG_MEM ,
' TOKENVALUE-GET-LOC_REG_MEM_OFFSET ,
' TOKENVALUE-GET-LOC_LITERAL ,
' TOKENVALUE-GET-LOC_SP ,
' TOKENVALUE-GET-LOC_PC ,
' TOKENVALUE-GET-LOC_EX ,
' TOKENVALUE-GET-LOC_PUSHPOP ,
' TOKENVALUE-GET-LOC_PEEK ,
' TOKENVALUE-GET-LOC_PICK ,

\ returns the a or b of a line of code (consumes next token)
: get-token-value ( tokenval loc size -- tokenval )
		rot clear-tokenval -rot
		2dup tokenvalue-type \ loc size type
		tokenvalue-getters swap 1- \ loc size getters type-1
		cells + @ execute \ getters[type-1] execute
;
\ store encoded op at next spot in code-buffer,
\   increment code-buffer-pos
: encode-op ( op a b -- )
;
: encode-special-op ( op a -- )
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
						2dup
						op-table find-op \ loc size op
						0 over = if
								\ special op
								drop \ loc size
								2dup special-op-table find-op
								-rot \ spc-op loc size
								get-token-value \ spc-op a
								encode-special-op \ hex-code
						else
								\ standard op
								-rot \ op loc size
								2dup
								get-token-value
								-rot \ op b loc size
								get-token-value
								-rot \ op b a
								encode-op \ hex-code
						then
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

