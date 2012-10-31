needs vm/vm.fs
needs compiler/compile.fs
needs utils/strings.fs

: wait-for-key
		key drop
;

: any-key-continue
		." Hit any key to continue"
		wait-for-key
;

: command-compile
		." Compile:" cr

		cr ." Enter source file (default: test.dasm): "
		input$
		get-counted-string
		0 over = if
				2drop
				s" test.dasm"
		then

		cr ." Enter output file (default: test.dbin): "
		input$
		get-counted-string
		0 over = if
				2drop
				s" test.dbin"
		then
		cr

		2swap compile-file

		any-key-continue
		page
;

: command-vm-dump
		." VM State: " cr
		
		dump-vm-state cr

		any-key-continue page
;

: command-run
		." Run:" cr

		cr ." Enter file to run (default: test.dbin) "

		input$
		get-counted-string
		0 over = if
				2drop
				s" test.dbin"
		then
		cr

		vm-run-file

		command-vm-dump
;

create commands
' command-compile ,
' command-run ,
' command-vm-dump ,

: menu
		begin
				." DCPU-16:" cr
				." ---------------------------" cr
				." 1) Compile" cr
				." 2) Run" cr
				." 3) Dump VM State" cr
				." q) Quit" cr
				." ---------------------------" cr
				." > " key
				page
				[char] q over = if
						bye
				then
				[char] 1 -
				dup 2 > if
						." *** Invalid choice! ***" cr
				else
					cells commands + @ execute
				then
		again
;

menu
