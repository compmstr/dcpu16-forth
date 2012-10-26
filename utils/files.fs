needs strings.fs

256 constant max-line
create file-line-buffer max-line 2 + allot
0 value fd-in
0 value fd-out
variable file-line-pos
0 file-line-pos !

: is-input-open? ( -- t/f )
		0 fd-in <>
;
: is-output-open? ( -- t/f )
		0 fd-out <>
;

: open-input ( addr u -- )
		is-input-open? if
				." Error, file already open"
		else
			r/o open-file throw
			to fd-in
		then
;
: open-input-bin ( addr u -- )
		is-input-open? if
				." Error, file already open"
		else
			r/o bin open-file throw
			to fd-in
		then
;
: open-output-bin ( addr u -- )
		is-input-open? if
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

: read-input-file ( -- buffer size-read )
		fd-in file-size throw
		d>s
		dup allocate throw \ size buffer
		dup rot \ buffer buffer size
		fd-in read-file throw \ buffer size-read
;

: read-input-line ( -- count eof )
		0 file-line-pos !
		file-line-buffer max-line erase
		file-line-buffer max-line fd-in read-line throw
;

: close-input ( -- )
		is-input-open? if
			fd-in close-file throw
			0 to fd-in
		else
				s" Error: File not open"
		then
;

: close-output ( -- )
		is-output-open? if
			fd-out close-file throw
			0 to fd-out
		else
				s" Error: File not open"
		then
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
