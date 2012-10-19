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
