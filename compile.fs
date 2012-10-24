needs constants.fs
needs types.fs
needs files.fs
needs shorts.fs
needs strings.fs
needs util.fs
needs op-convert-table.fs

0x0 constant CODELISTENTRY-TYPE_OP
0x1 constant CODELISTENTRY-TYPE_SPECIAL-OP
0x2 constant CODELISTENTRY-TYPE_LABEL
0x3 constant CODELISTENTRY-TYPE_DATA

0x10000 short-array code-buffer
variable code-buffer-pos
0 code-buffer-pos !

variable file-line-pos
0 file-line-pos !

variable code-labels
0 code-labels !

struct
		cell% field codelistentry-type
		cell% field codelistentry-op
		cell% field codelistentry-bval
		cell% field codelistentry-aval
		\ pointer to entry in code-labels list
		cell% field codelistentry-label
		\ counted array of data to copy over verbatim
		cell% field codelistentry-data
		\ how many words this code entry is into the code stream
		cell% field codelistentry-codeloc
		cell% field codelistentry-next
end-struct codelistentry

variable code-list
0 code-list !
variable code-list-end
0 code-list-end !

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

struct
		cell% field label-name \ pointer to counted string
		short% field label-pos
		cell% field label-next
end-struct code-label

: dump-tokenval ( tokenval -- dokenval )
		dup tokenval swap drop dump
;

: clear-tokenval ( tokenval -- tokenval )
		dup tokenval swap drop erase
;

\ all of these: ( a-mode tokenval -- size )
: tokenval-size-LOC_REG 2drop 0 ;
: tokenval-size-LOC_MEM 2drop 1 ;
: tokenval-size-LOC_REG_MEM 2drop 0 ;
: tokenval-size-LOC_REG_MEM_OFFSET 2drop 1 ;
: tokenval-size-LOC_LITERAL
		dup tokenval-val w@ 0x20 <
		\ and a-mode
		and if
				drop 0
		else
				drop 1
		then
;
: tokenval-size-LOC_SP 2drop 0 ;
: tokenval-size-LOC_PC 2drop 0 ;
: tokenval-size-LOC_EX 2drop 0 ;
: tokenval-size-LOC_PUSHPOP 2drop 0 ;
: tokenval-size-LOC_PEEK 2drop 0 ;
: tokenval-size-LOC_PICK 2drop 1 ;
: tokenval-size-LOC_LABEL 2drop 1 ;

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

\ all of these ( a-mode tokenval -- word [word] count )
\ count is either 0 or 1, if there's an extra word
: tokenval-encode-LOC_REG
		swap drop tokenval-reg w@ 0
;
: tokenval-encode-LOC_MEM
		swap drop tokenval-loc w@ \ loc
		0x1e swap \ 0x1e loc
		1
;
: tokenval-encode-LOC_REG_MEM
		swap drop tokenval-reg w@ 0x08 + 0
;
: tokenval-encode-LOC_REG_MEM_OFFSET
		swap drop dup \ loc loc
		tokenval-reg w@ 0x10 + \ loc 0x10+reg
		swap tokenval-loc w@ \ reg loc
		1
;
: tokenval-encode-LOC_LITERAL
		tokenval-val w@ \ a-mode val
		swap over 0x20 < and if \ val
				0x20 + 0 \ val+0x20 0
		else
				0x1f swap 1 \ 0x1f val 1
		then
;
: tokenval-encode-LOC_SP
		2drop 0x1b 0
;
: tokenval-encode-LOC_PC
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
		swap drop
		tokenval-loc w@ 0x1f swap \ 0x1f(literal) loc
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

: encode-tokenval ( tokenval -- word [word] count[0/1] )
		dup tokenval-type w@ cells
		tokenval-encoders + @
		execute
;

\ returns the size in words of a token val
\   either 0 or 1
: get-tokenval-size ( tokenval -- size )
		dup tokenval-type w@ 1- cells \ tokenval (type-1 cells)
		tokenval-sizers + @
		0 over <> if
				execute
		else
				." Invalid token type (no size function"
				abort
		then
;

\ gets the size of a code list entry, for ops, size is 1-3, for labels, 0
\   for data, 0+
: get-codelistentry-size ( codelistentry -- size )
		case dup codelistentry-type @
				codelistentry-type_op of
						true over codelistentry-aval @ get-tokenval-size
						swap false swap codelistentry-bval @ get-tokenval-size
						+ 1+
				endof
				codelistentry-type_special-op of
						true swap codelistentry-aval @ get-tokenval-size
						1+
				endof
				codelistentry-type_label of
						drop 0
				endof
				codelistentry-type_data of
						\ TODO
				endof
		endcase
;

: get-codelistentry ( -- loc )
		codelistentry %alloc
		dup codelistentry swap drop erase
;

: dump-codelistentry ( loc -- loc )
		dup codelistentry swap drop dump
;

: dump-codelistentry-op ( loc -- loc )
		cr ." Code List Entry: " cr
		dump-codelistentry cr
		." B: " cr
		dup codelistentry-bval @ dump-tokenval drop cr
		." A: " cr
		dup codelistentry-aval @ dump-tokenval drop cr
;

\ free the memory allocated for a codelistentry
: empty-codelistentry ( entry -- )
		case dup codelistentry-type
				CODELISTENTRY-TYPE_OP of
						\ free a, b
						dup codelistentry-aval free
						dup codelistentry-bval free
				endof
				CODELISTENTRY-TYPE_SPECIAL-OP of
						\ free a
						dup codelistentry-aval free
				endof
				CODELISTENTRY-TYPE_DATA of
						\ free data
						dup codelistentry-data free
				endof
		endcase
;

: empty-codelist ( -- )
		\ only do this if list isn't empty
		0 code-list = not if
				code-list @
				dup empty-codelistentry
				dup codelistentry-next @ \ cur next
				swap free \ next
		then
		0 code-list !
		0 code-list-end !
;

: add-to-codelist ( codelistentry -- )
		\ first entry being added
		code-list @ 0 = if
				dup code-list !
				code-list-end !
		else
				dup \ entry entry
				\ get the current last item
				code-list-end @ \ entry entry last
				codelistentry-next !
				\ update the end of the list
				code-list-end !
		then
;

: store-label ( loc size -- codelistentry )
		\ strip off the :
		swap 1+ swap 1- \ loc+1 size-1
		save-string \ string

		code-label %alloc \ string label-loc
		dup label-name \ string label-loc label-name
		rot \ label-loc label-name string
		swap ! \ label-loc

		\ to start with, the label position will be 0
		0 over label-pos ! \ label-loc

		code-labels @ \ label-loc code-labels
		over label-next ! \ label-loc
		dup code-labels ! \ label-loc
		\ TODO: Create codelistentry
		get-codelistentry >r \ label-loc codelistentry
		CODELISTENTRY-TYPE_LABEL r@ codelistentry-type !
		r@ codelistentry-label !
		r>
;

: get-next-label ( label -- <next label> ) label-next @ ;

\ looks for a label by name, returns -1 if none found
: find-label ( search-string -- label-loc )
		get-counted-string \ search-loc search-size
		code-labels @ \ sloc ssize label
		begin 0 over <> while
						dup >r
						label-name @ get-counted-string \ sloc ssize lloc lsize
						2over
						compare 0= if \ sloc ssize
								2drop r> exit
						then
						r> get-next-label
		repeat
		2drop drop -1
;

: get-next-code-entry ( codelistentry -- <next codelistentry> ) codelistentry-next @ ;

: set-code-entry-label-pos ( code-pos code-entry -- code-pos code-entry )
		\ only do something if this is a label
		CODELISTENTRY-TYPE_LABEL over codelistentry-type @ = if
				\ get the label
				dup codelistentry-label @ \ pos entry label
				0 over <> if
						2 pick swap label-pos w! \ pos entry
				else
						drop
				then
		then
;

\ runs through the code list, and sets the codeloc for each entry
: set-codelist-codelocs ( -- size ) 
		0 code-list @ \ <size> <first code entry>
		begin
				0 over <> while
						\ set entry's loc
						2dup codelistentry-codeloc !
						set-code-entry-label-pos
						\ add entry's size
						dup get-codelistentry-size \ accum entry size
						rot + swap \ accum+size entry
						\ get next entry
						get-next-code-entry
		repeat
		swap drop \ size
;

\ set a label type tokenval to a literal one
: replace-tokenval-label ( tokenval -- )
		dup tokenval-type @
		LOC_LABEL <> if
				drop exit
		then

		dup tokenval-labelname @ \ val labelname
		find-label \ val label
		-1 over = if
				." Error, label not found: "
				dup tokenval-labelname @ print-string cr
				abort
		then
		swap \ label val
		\ leave the type as label, just set the value
		tokenval-val w!
;

: replace-loc_labels  ( -- )
		code-list @ \ <first code entry>
		begin
				0 over <> while
						\ get entry's type
						dup codelistentry-type w@ \ entry type
						case
								CODELISTENTRY-TYPE_OP of
										\ check if aval is label
										dup codelistentry-aval @
										replace-tokenval-label
										\ check if bval is label
										dup codelistentry-bval @
										replace-tokenval-label
								endof
								CODELISTENTRY-TYPE_SPECIAL-OP of
										\ check if aval is label
										dup codelistentry-aval @
										replace-tokenval-label
								endof
						endcase
						get-next-code-entry
		repeat
		drop
;

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

: get-next-token ( -- loc count ) \ find the next whitespace/null delimited token in the file line
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
		2 pick tokenval-type LOC_SP swap w!
;
: tokenvalue-get-LOC_PC
		2drop \ tokenval
		LOC_PC over tokenval-type w!
;
: tokenvalue-get-LOC_EX
		2 pick tokenval-type LOC_EX swap w!
;
: tokenvalue-get-LOC_PUSHPOP
		2 pick tokenval-type LOC_PUSHPOP swap w!
;
: tokenvalue-get-LOC_PEEK
		2 pick tokenval-type LOC_PEEK swap w!
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

\ returns the a or b of a line of code (consumes next token)
: get-token-value ( tokenval loc size -- tokenval )
		." get-token-value: " 2dup type cr
		rot clear-tokenval -rot
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
\ store encoded op at next spot in code-buffer,
\   increment code-buffer-pos
: encode-op ( op a b -- )
;
: encode-special-op ( op a -- )
;

: is-line-blank ( loc size -- loc size t/f )
		\ if the size is 0, line is blank
		0 over = if
				true exit
		then
		2dup
		true -rot \ loc size true loc size
		0 do \ true loc
				dup i + c@
				whitespace? not if
						swap drop false swap
						leave
				then
		loop
		drop \ drop location
;

: is-line-comment ( loc size -- loc size t/f )
		over c@
		[char] ; =
;
: is-line-label ( loc size -- loc size t/f )
		over c@
		[char] : = 
;
: is-line-dat ( loc size -- loc size t/f )
		2dup drop
		s" DAT" rot
		starts-with
;

: process-op ( loc size -- codelistentry )
		\ break:
		op-table find-op \ op
		0 over = if
				\ special op
				drop \ loc size
				special-op-table find-op \ spc-op
				get-next-token \ spc-op loc size
				tokenval %alloc -rot \ spc-op tokenval loc size
				get-token-value \ spc-op a
				get-codelistentry \ spc-op a codelistentry
				\ store a copy in the return stack
				dup >r \ spc-op a codelistentry
				CODELISTENTRY-TYPE_SPECIAL-OP over codelistentry-type !
				codelistentry-aval ! \ spc-op
				\ copy loc off return stack
				r@ \ spc-op codelistentry
				codelistentry-op !
				r>
		else
				\ standard op
				get-next-token \ op loc size
				tokenval %alloc -rot \ op tokenval(b) loc size
				get-token-value \ op b
				get-next-token \ op b loc size
				tokenval %alloc -rot \ op b tokenval(a) loc size
				get-token-value \ op b a
				get-codelistentry \ op b a codelistentry(cle)
				CODELISTENTRY-TYPE_OP over codelistentry-type !
				dup >r \ op b a cle
				codelistentry-aval !
				r@
				codelistentry-bval !
				r@
				codelistentry-op !
				r>
		then
;

: process-dat ( loc size -- codelistentry )
		\ TODO
;

: process-line ( u2 -- <codelistentry or 0> )
		0 file-line-pos !
		." Processing line: "
		file-line-buffer over type cr
		drop \ don't need the length after this
		get-next-token \ loc size
		is-line-comment >r \ loc size
		is-line-blank r> \ loc size blank? comment?
		or if
				\ skip
				2drop 0
		else
				is-line-label if
						store-label
				else
						is-line-dat if
								process-dat
						else
								process-op
						then
				then
		then
;

: codelistentry-encode-OP
;
: codelistentry-encode-SPECIAL-OP
;
: codelistentry-encode-LABEL
;
: codelistentry-encode-DATA
;
create codelistentry-encoders
' codelistentry-encode-OP ,
' codelistentry-encode-SPECIAL-OP ,
' codelistentry-encode-LABEL ,
' codelistentry-encode-DATA ,

: encode-codelist ( buffer -- buffer )
		code-list @ \ first thing of code
;

: compile-file ( string len -- )
		open-input
		begin
				read-input-line
		while \ while eats the EOF flag
						process-line
						\ if we have a codelistentry to add, add it
						0 over <> if
								add-to-codelist
						else
								drop \ get rid of the 0
						then
		repeat
		drop \ drop last 0
		close-input
		\ calculate the code locations
		set-codelist-codelocs \ code-size
		\ replace all of the locations set to labels with the label's position
		replace-loc_labels
		\ allocate space for the code
		shorts allocate throw \ code-buffer
		encode-codelist
;

: test-compile
		s" test.dasm"
		compile-file
;

: test-by-line
		s" test.dasm"
		open-input
		read-input-line drop
		process-line
;

: test-next-line
		read-input-line drop
		process-line
;

