needs ../utils/util.fs
needs ../utils/strings.fs
needs ../utils/constants.fs

struct
		cell% field op-name
		cell% field op-code
end-struct op-convert

: add-op ( loc string val -- loc-next-index )
		2 pick op-code ! \ loc string
		over op-name ! \ loc
		op-convert struct-size + \ loc (next index)
;

\ ex: s" SET" op-table find-op
: find-op ( loc len table -- code/0 )
		@ -rot \ table loc count
		begin
				2 pick op-name @ \ table loc count op-name
				0 over = if
						2drop 2drop 0
						exit
				then
				get-counted-string
				2over \ table loc count op-loc op-count loc count
				compare \ table loc count (t/f op == cur)
		while \ table loc count
						rot op-convert struct-size + -rot
		repeat
		2drop \ table
		op-code @ \ op-code
;

\ Set up space for op tables
variable op-table
op-convert struct-size 0x20 * allocate throw
op-table !
op-table @ op-convert struct-size 0x20 * erase

variable special-op-table
op-convert struct-size 0x20 * allocate throw
special-op-table !
special-op-table @ op-convert struct-size 0x20 * erase


\ Standard opcodes
\ string val
op-table @
s" SET" save-string 0x01 add-op
s" ADD" save-string 0x02 add-op
s" SUB" save-string 0x03 add-op
s" MUL" save-string 0x04 add-op
s" MLI" save-string 0x05 add-op
s" DIV" save-string 0x06 add-op
s" DVI" save-string 0x07 add-op
s" MOD" save-string 0x08 add-op
s" MDI" save-string 0x09 add-op
s" AND" save-string 0x0a add-op
s" BOR" save-string 0x0b add-op
s" XOR" save-string 0x0c add-op
s" SHR" save-string 0x0d add-op
s" ASR" save-string 0x0e add-op
s" SHL" save-string 0x0f add-op
s" IFB" save-string 0x10 add-op
s" IFC" save-string 0x11 add-op
s" IFE" save-string 0x12 add-op
s" IFN" save-string 0x13 add-op
s" IFG" save-string 0x14 add-op
s" IFA" save-string 0x15 add-op
s" IFL" save-string 0x16 add-op
s" IFU" save-string 0x17 add-op
s" ADX" save-string 0x1a add-op
s" SBX" save-string 0x1b add-op
s" STI" save-string 0x1e add-op
s" STD" save-string 0x1f add-op
drop

special-op-table @
\ Special opcodes
s" JSR" save-string 0x01 add-op
s" INT" save-string 0x08 add-op
s" IAG" save-string 0x09 add-op
s" IAS" save-string 0x0a add-op
s" RFI" save-string 0x0b add-op
s" IAQ" save-string 0x0c add-op
s" HWN" save-string 0x10 add-op
s" HWQ" save-string 0x11 add-op
s" HWI" save-string 0x12 add-op
drop
