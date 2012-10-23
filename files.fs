256 constant max-line
create file-line-buffer max-line 2 + allot
0 value fd-in
0 value fd-out
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
		file-line-buffer max-line erase
		file-line-buffer max-line fd-in read-line throw
;

: close-input ( -- )
		fd-in close-file throw
;


