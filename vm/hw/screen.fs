needs ../../utils/sdl.fs

\ helper function to convert 4 bytes into a double word
: convert-letter ( b b b b -- dw )
		swap 8 shift-left +
		swap 16 shift-left +
		swap 24 shift-left +
; immediate
