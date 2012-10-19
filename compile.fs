needs constants.fs
needs types.fs
needs files.fs
needs shorts.fs
needs strings.fs

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

: eat-whitespace ( -- ) \ advance file-line-pos until next non-whitespace
		begin
				file-line-buffer file-line-pos @ \ line-buffer line-pos
				+ c@ dup whitespace? \ current char
		while
						file-line-pos @
						1+ file-line-pos !
		repeat
;

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
		eat-whitespace
		file-line-buffer code-buffer-pos +
		[char] ; =
;
: is-line-label ( -- t/f )
		eat-whitespace
		file-line-buffer code-buffer-pos +
		[char] : = 
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

: process-line ( u2 -- )
		0 file-line-pos !
		." Processing line: "
		file-line-buffer over type cr

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
				read-input-line
		while \ while eats the EOF flag
						process-line
		repeat
;

