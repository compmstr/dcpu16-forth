needs ../utils/strings.fs
needs ../utils/util.fs
needs ../utils/files.fs

struct
		cell% field macro-name
		cell% field macro-content
		cell% field macro-prev
end-struct macro%

variable macro-list
0 macro-list !

: print-macro ( addr -- )
		>r
		." Macro: " r@ macro-name @ print-string cr
		." Content: " r@ macro-content @ print-string cr
		rdrop
;

\ add macro to the list
: add-macro ( addr -- )
		macro-list @ over macro-prev !
		macro-list !
;

\ allocate and fill in a new macro
: new-macro ( nloc ncount cloc ccount -- loc )
		macro% %alloc >r
		save-string r@ macro-content !
		save-string r@ macro-name !
		0 r@ macro-prev !
		r>
;

\ parses a macro definition from the current file/line
: parse-macro ( -- )
;

: macro-find ( loc count -- addr/0 )
		macro-list @ >r \ loc count
		begin r@ while
						r@ macro-name @ get-counted-string
						2over string= if
								2drop
								r>
								exit
						then
						r> macro-prev @ >r
		repeat
		r> \ this will be 0 here
;

\ find the number of commas not preceded by \
: num-commas ( loc count -- n )
		0 swap
		0 ?do \ loc accum
				over dup i + c@ [char] , = \ loc accum loc [loc+i]
				swap i + 1- c@ [char] \ <> and
				if
						1+
				then
		loop
;

: macro-inv-name ( loc count -- loc' count' )
		\ get from ( to the first ,
		2dup [char] ( string-first-instance 1+ \ loc count (-idx
		swap over - \ loc i count-i
		-rot + \ count-i loc+i
		dup rot \ loc loc count
		[char] , string-first-instance
;

\ replaces a macro invocation inside the provided string
\ expects a macro invocation in loc count
\ ex: (FOO,A,B) --
\     this will set the name to FOO, and \1 to A, and \2 to B
: macro-replace ( loc count -- loc' count' )
;

: macro-test
		s" NEXT" s" CONTINUE..." new-macro
		add-macro
		s" EXIT" s" GO AWAY!" new-macro
		add-macro
		macro-list @ print-macro
;
