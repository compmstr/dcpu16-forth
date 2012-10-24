needs strings.fs

256 constant max-line
create file-line-buffer max-line 2 + allot
0 value fd-in
0 value fd-out
variable file-line-pos
0 file-line-pos !

: open-input ( addr u -- )
		r/o open-file throw
		to fd-in
;
: open-output-bin ( addr u -- )
		w/o bin \ bin modifies w\o to make it binary
		create-file throw 
		to fd-out
;
: read-input-line ( -- count eof )
		0 file-line-pos !
		file-line-buffer max-line erase
		file-line-buffer max-line fd-in read-line throw
;

: close-input ( -- )
		fd-in close-file throw
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

