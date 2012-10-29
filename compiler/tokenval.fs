needs ../utils/util.fs
needs ../utils/files.fs

struct
		\ one of the LOC_... constants
		short% field tokenval-type
		\ Register to use
		short% field tokenval-reg
		\ Ram location to use (or offset)
		short% field tokenval-loc
		\ Literal value to use
		short% field tokenval-val
		\ String to use to look up label
		cell% field tokenval-labelname
end-struct tokenval

: dump-tokenval ( tokenval -- dokenval )
		dup tokenval swap drop dump
;

: clear-tokenval ( tokenval -- tokenval )
		dup tokenval swap drop erase
;

\ All of these: ( loc count -- t/f )
: tokenvalue-LOC_REG?
		\ ." LOC_REG?: " 2dup type cr
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
		\ ." LOC_MEM? " 2dup type cr
		2dup square-bracketed?
		-rot
		2 pick not if 2drop exit then
		string-without-ends
		string-number? and
;
: tokenvalue-LOC_REG_MEM?
		\ ." LOC_REG_MEM? " 2dup type cr
		2dup square-bracketed?
		-rot
		string-without-ends
		tokenvalue-LOC_REG? and
;
: tokenvalue-LOC_REG_MEM_OFFSET?
		\ ." LOC_REG_MEM_OFFSET? " 2dup type cr
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
: tokenvalue-LOC_LABEL_MEM?
		square-bracketed?
		\ since this is after all of the other memory location checks,
		\   we already know they're not the others
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
' TOKENVALUE-LOC_LABEL_MEM? , LOC_LABEL_MEM ,
0 ,

\ takes a token, and returns a LOC_ constant for the type of value
\ returns -1 on no type found
: tokenvalue-type ( loc size -- LOC_... )
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


\ all of these: ( a-mode tokenval -- size )
: tokenval-size-LOC_REG 2drop 0 ;
: tokenval-size-LOC_MEM 2drop 1 ;
: tokenval-size-LOC_REG_MEM 2drop 0 ;
: tokenval-size-LOC_REG_MEM_OFFSET 2drop 1 ;
: tokenval-size-LOC_LITERAL
		( dup tokenval-val w@ 0x20 < \ a-mode tokenval val<0x20
		\ and a-mode
		-rot and if
				drop 0
		else
				drop 1
		then )
		2drop 1
;
: tokenval-size-LOC_SP 2drop 0 ;
: tokenval-size-LOC_PC 2drop 0 ;
: tokenval-size-LOC_EX 2drop 0 ;
: tokenval-size-LOC_PUSHPOP 2drop 0 ;
: tokenval-size-LOC_PEEK 2drop 0 ;
: tokenval-size-LOC_PICK 2drop 1 ;
: tokenval-size-LOC_LABEL 2drop 1 ;
: tokenval-size-LOC_LABEL_MEM 2drop 1 ;

create tokenval-sizers
' tokenval-size-LOC_REG ,
' tokenval-size-LOC_MEM ,
' tokenval-size-LOC_REG_MEM ,
' tokenval-size-LOC_REG_MEM_OFFSET ,
' tokenval-size-LOC_LITERAL ,
' tokenval-size-LOC_SP ,
' tokenval-size-LOC_PC ,
' tokenval-size-LOC_EX ,
' tokenval-size-LOC_PUSHPOP ,
' tokenval-size-LOC_PEEK ,
' tokenval-size-LOC_PICK ,
' tokenval-size-LOC_LABEL ,
' tokenval-size-LOC_LABEL_MEM ,
0 ,

\ returns the size in words of a token val
\   either 0 or 1
: get-tokenval-size ( tokenval -- size )
		." get-tokenval-size" cr
		dup tokenval-type w@ 1- cells \ tokenval (type-1 cells)
		tokenval-sizers + @
		0 over <> if
				execute
		else
				." Invalid token type (no size function)"
				abort
		then
;

\ all of these ( a-mode tokenval -- word [word] count )
\ count is either 0 or 1, if there's an extra word
: tokenval-encode-LOC_REG
		." encode-loc_reg" cr
		swap drop tokenval-reg w@ 0
;
: tokenval-encode-LOC_MEM
		." encode-loc_mem" cr
		swap drop tokenval-loc w@ \ loc
		0x1e swap \ 0x1e loc
		1
;
: tokenval-encode-LOC_REG_MEM
		." encode-loc_reg_mem" cr
		swap drop tokenval-reg w@ 0x08 + 0
;
: tokenval-encode-LOC_REG_MEM_OFFSET
		." encode-loc_reg_mem_offset" cr
		swap drop dup \ loc loc
		tokenval-reg w@ 0x10 + \ loc 0x10+reg
		swap tokenval-loc w@ \ reg loc
		1
;
: tokenval-encode-LOC_LITERAL
		." encode-loc_literal" cr
		( tokenval-val w@ \ a-mode val
		swap over 0x20 < \ val a-mode val<0x20
		and if \ val
				0x20 + 0 \ val+0x20 0
		else
				0x1f swap 1 \ 0x1f val 1
		then )
		swap drop tokenval-val w@ \ val
		0x1f swap 1
;
: tokenval-encode-LOC_SP
		." encode-loc_sp" cr
		2drop 0x1b 0
;
: tokenval-encode-LOC_PC
		." encode-loc_pc" cr
		2drop 0x1c 0
;
: tokenval-encode-LOC_EX
		2drop 0x1d 0
;
: tokenval-encode-LOC_PUSHPOP
		2drop 0x18 0
;
: tokenval-encode-LOC_PEEK
		2drop 0x19 0
;
: tokenval-encode-LOC_PICK
		swap drop \ loc
		tokenval-loc w@ \ loc
		0x1a swap 1 \ 0x1a loc 1
;
: tokenval-encode-LOC_LABEL
		." encode-loc_label" cr
		swap drop
		0x1f swap tokenval-val w@ \ 0x1f(literal) loc
		1
;
: tokenval-encode-LOC_LABEL_MEM
		." encode-loc_label_mem" cr
		swap drop
		0x1e swap tokenval-val w@ \ 0x1e(memory) loc
		1
;
create tokenval-encoders
' tokenval-encode-LOC_REG ,
' tokenval-encode-LOC_MEM ,
' tokenval-encode-LOC_REG_MEM ,
' tokenval-encode-LOC_REG_MEM_OFFSET ,
' tokenval-encode-LOC_LITERAL ,
' tokenval-encode-LOC_SP ,
' tokenval-encode-LOC_PC ,
' tokenval-encode-LOC_EX ,
' tokenval-encode-LOC_PUSHPOP ,
' tokenval-encode-LOC_PEEK ,
' tokenval-encode-LOC_PICK ,
' tokenval-encode-LOC_LABEL ,
' tokenval-encode-LOC_LABEL_MEM ,

: encode-tokenval ( a-mode tokenval -- word [word] count[0/1] )
		dup tokenval-type w@ 1- cells
		tokenval-encoders + @
		execute
;

\ All of these: ( a-mode tokenval loc count -- tokenvalue )
: tokenvalue-get-LOC_REG
		2 pick tokenval-type LOC_REG swap w!
		drop c@ char->reg
		over tokenval-reg w!
;
: tokenvalue-get-LOC_MEM
		2 pick tokenval-type LOC_MEM swap w!
		string-without-ends string->number
		over tokenval-loc w!
;
: tokenvalue-get-LOC_REG_MEM
		2 pick tokenval-type LOC_REG_MEM swap w!
		string-without-ends drop c@ char->reg
		over tokenval-reg w!

;
: tokenvalue-get-LOC_REG_MEM_OFFSET
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
				\ top of stack is number
				string->number \ tokenval loc count offset
				-rot
				drop c@ char->reg \ tokenval offset reg
				swap
		then \ tokenval reg offset
		2 pick tokenval-loc w!
		over tokenval-reg w!
;
: tokenvalue-get-LOC_LITERAL
		2 pick tokenval-type LOC_LITERAL swap w!
		string->number
		over tokenval-val w!
;
: tokenvalue-get-LOC_SP
		2drop LOC_SP over tokenval-type w!
;
: tokenvalue-get-LOC_PC
		2drop \ tokenval
		LOC_PC over tokenval-type w!
;
: tokenvalue-get-LOC_EX
		2drop LOC_EX over tokenval-type w!
;
: tokenvalue-get-LOC_PUSHPOP
		2drop LOC_PUSHPOP over tokenval-type w!
;
: tokenvalue-get-LOC_PEEK
		2drop LOC_PEEK over tokenval-type w!
;
: tokenvalue-get-LOC_PICK
		2 pick tokenval-type LOC_PICK swap w!
		\ get the next token (pick's n)
		get-next-token string->number
		over tokenval-loc w!
;
: tokenvalue-get-LOC_LABEL
		2 pick tokenval-type LOC_LABEL swap w!
		save-string
		over tokenval-labelname !
;
: tokenvalue-get-LOC_LABEL_MEM
		2 pick tokenval-type LOC_LABEL_MEM swap w!
		string-without-ends
		save-string
		over tokenval-labelname !
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
' TOKENVALUE-GET-LOC_LABEL ,
' TOKENVALUE-GET-LOC_LABEL_MEM ,

\ returns the a or b of a line of code (consumes next token)
: get-token-value ( tokenval loc size -- tokenval )
		." get-token-value: " 2dup type cr
		rot clear-tokenval -rot
		remove-trailing-comma
		2dup tokenvalue-type \ loc size type
		tokenvalue-getters swap 1- \ loc size getters type-1
		cells + @
		0 over <> if
				execute \ getters[type-1] execute
		else
				." Invalid token type (no getter)"
				abort
		then
;
