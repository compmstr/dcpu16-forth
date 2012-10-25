needs ../utils/constants.fs
needs ../utils/types.fs
needs ../utils/files.fs
needs ../utils/shorts.fs
needs ../utils/strings.fs
needs ../utils/util.fs
needs op-convert-table.fs
needs tokenval.fs
needs codelistentry.fs

variable code-buffer
0 code-buffer !
variable code-buffer-pos
0 code-buffer-pos !
: free-code-buffer
		." Freeing code buffer" cr
		0 code-buffer @ <> if
				code-buffer @ free throw
				0 code-buffer !
				0 code-buffer-pos !
		then
;
: alloc-code-buffer ( size -- )
		." Allocating code buffer of size: " dup . cr
		shorts allocate throw code-buffer !
		0 code-buffer-pos !
;
: append-to-code-buffer ( short -- )
		code-buffer @ code-buffer-pos @ shorts + !
		1 code-buffer-pos +!
;

variable code-labels
0 code-labels !

variable code-list
0 code-list !
variable code-list-end
0 code-list-end !

struct
		cell% field label-name \ pointer to counted string
		short% field label-pos
		cell% field label-next
end-struct code-label

: empty-codelist ( -- )
		\ only do this if list isn't empty
		code-list @ >r
		begin
				0 r@ = not while
						r@ empty-codelistentry
						r> dup codelistentry-next @ >r
						free throw
		repeat

		r> drop
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
		drop \ size
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
		label-pos w@ \ val label-pos
		swap \ label-pos val
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
		." Processing op: " 2dup type cr
		2dup op-table find-op \ loc size op
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
				-rot 2drop \ get rid of op name, don't need it
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

: encode-codelist ( -- )
		\ TODO dictionay gets corrupted somewhere in here
		0 code-buffer-pos !
		code-list @ \ first thing of code
		>r \ store it in the return stack
		begin
				r@ 0 <> while
						r@ encode-codelistentry \ cle [word] [word] [word] count/-1
						-1 over = if
								drop
						else
								1+ 0 do \ [word] [word] word
										append-to-code-buffer
								loop
						then
						r> codelistentry-next @ >r
		repeat

		\ don't need the code-list pointer anymore
		r> drop
		2 allocate throw drop
;

: compile-file ( fout-filename len fin-filename len -- )
		." Compiling file: " 2dup type cr
		." To file: " 2over type cr
		\ clear out the code list if needed
		empty-codelist
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
		free-code-buffer
		1+ alloc-code-buffer
		encode-codelist

		\ code is compiled, write to output
		open-output-bin

		code-buffer @
		code-buffer-pos @
		shorts write-output-bin

		close-output
;

: test-compile
		s" test.dbin"
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

: dump-code-buffer
		code-buffer @ code-buffer-pos @ shorts dump
;
