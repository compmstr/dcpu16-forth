needs ../../utils/sdl.fs
needs ../../utils/util.fs
needs ../../utils/shorts.fs
needs ../vm.fs

: keyboard-updater
;
: keyboard-hw-int-handler
;
: keyboard-get-hw-info
		\ ID low/high word
		0x7406 REG_A reg-set
		0x30CF REG_B reg-set
		\ Version
		0x0001 REG_C reg-set
		\ Mfgr low/high word
		0x1337 REG_X reg-set
		0xB00B REG_Y reg-set
;

' keyboard-updater
cr dup ." Keyboard Updater: " . cr
' keyboard-hw-int-handler
dup ." Keyboard Int-handler: " . cr
' keyboard-get-hw-info
dup ." Keyboard Info: " . cr
