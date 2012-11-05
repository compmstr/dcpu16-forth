needs vm/hw/screen.fs

init-screen
2drop drop

\ 0xF 0x0 0x0 char A generate-screen-char 0 ram-set
\ 0x1 0x0 0x0 char B generate-screen-char 1 ram-set
\ 0x0 0xF 0x0 char C generate-screen-char 2 ram-set

: print-chars
	127 0 do
	  i 0xF mod dup 0xF swap - 0x0 i generate-screen-char i ram-set
	loop
;
print-chars

utime d>s 0x10000000 + refresh-display
