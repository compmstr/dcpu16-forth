needs ../vm.fs

: dumper-get-hw-info
		\ ID low word
		0x1337 REG_A reg-set
		\ ID high word
		0xb00b REG_B reg-set
		\ version
		0x0001 REG_C reg-set
		\ Mfgr low word
		0x0001 REG_X reg-set
		\ Mfgr high word
		0x0000 REG_Y reg-set
;
: dumper-hw-int-handler
		." Dumper interrupt" cr
		REG_A reg-get 0 over = if
				drop
				dump-vm-state
		else
				dup ram-get ." RAM at: " swap . ." -- " . cr
		then
;
: dumper-updater
;

' dumper-updater
cr dup ." Dumper Updater: " . cr
' dumper-hw-int-handler
dup ." Dumper Int-handler: " . cr
' dumper-get-hw-info
dup ." Dumper Info: " . cr
