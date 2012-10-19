needs constants.fs
needs types.fs
needs files.fs
needs shorts.fs

struct
		cell% field label-name \ pointer to counted string
		short% field label-pos
		cell% field label-next
end-struct code-label

variable first-label

0x10000 short-array code-buffer
variable code-buffer-pos
0 code-buffer-pos !

variable file-line-pos
0 file-line-pos !

: get-op ( -- op-code )
;
: get-b ( -- vmloc )
;
: get-a ( -- vmloc )
;
\ store encoded op at next spot in code-buffer,
\   increment code-buffer-pos
: encode-op ( op a b -- )
;

: is-line-comment ( -- t/f )
;
: is-line-label ( -- t/f )
;
: store-label ( u2 -- )
		\ store the current line as the first label
		code-label %alloc \ u2 label-loc
		swap \ label-loc u2
		\ create storage for label
		dup allocate throw \ label-loc u2 new-loc 
		swap \ label-loc new-loc u2 
		over \  label-loc new-loc u2 new-loc
		file-line-buffer -rot \ label-loc new-loc buffer u2 new-loc
		copy-string \ label-loc new-loc
		over label-name ! \ label-loc
;

: process-line ( u2 flag -- )
		0 file-line-pos !
		drop \ u2
		is-line-comment if
		else
				is-line-label if
						store-label
				else
						get-op
						get-b
						get-a
						encode-op
				then
		then
;

: compile-file ( string len -- )
		open-input
		begin
				file-line-buffer max-line fd-in read-line throw
		while
						process-line
		repeat
;

