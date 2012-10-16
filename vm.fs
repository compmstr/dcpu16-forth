include shorts.fs
include ops.fs
include util.fs

0x10000 short-array vm_ram

8 short-array vm_registers
\ array indices for registers
0 constant REG_A
1 constant REG_B
2 constant REG_C
3 constant REG_X
4 constant REG_Y
5 constant REG_Z
6 constant REG_I
7 constant REG_J

wvariable VM_PC
wvariable VM_SP
wvariable VM_EX
wvariable VM_IA

: reg-get ( register -- n )
		vm_registers w@
;
: reg-set ( val register -- )
		vm_registers w!
;

: ram-get ( loc -- n)
		vm_ram w@
;
: ram-set ( val loc -- )
		vm_ram w!
;

: get-code-word-op ( word -- op )
		\ bottom 5 bits
		0xF and
;

: get-code-word-a ( word -- a )
		\ top 6 bits
		10 rshift
;

: get-code-word-b ( word -- b )
		\ 2nd 5 bits from bottom
		5 rshift
		0xF and
;

1 constant LOC_REG
2 constant LOC_MEM
3 constant LOC_REG_MEM
4 constant LOC_REG_MEM_OFFSET
5 constant LOC_LITERAL
6 constant LOC_SP
7 constant LOC_PC
8 constant LOC_EX
9 constant LOC_IA

struct
		\ one of the LOC_... constants
		short% field vmloc-type
		\ Register to use
		short% field vmloc-register
		\ Ram location to use
		short% field vmloc-loc
		\ Value for literals
		short% field vmloc-val
end-struct vmloc

\ create <name> vmloc %allot (%alloc does on heap)
\ <name> vmloc-loc \ gets mem address of the loc

: vmloc-store ( val loc -- )
		case dup vmloc-type w@
				LOC_REG of
						vmloc-register w@ \ val register
						reg-set
				endof
		endcase
;
create test-loc vmloc %allot
: test-loc-store
		LOC_REG test-loc vmloc-type w!
		REG_A test-loc vmloc-register w!
		15 REG_A reg-set
		20 test-loc vmloc-store
		REG_A reg-get .
;

create test-code
0x7c01 cw, 0x0030 cw,
0x7fc1 cw, 0x0020 cw, 0x1000 cw,
0x7803 cw, 0x1000 cw,
0xc013 cw,
0x7f80 cw, 0x0020 cw,

variable test-code-len
cw,-len @ test-code-len !

: load-test-code ( -- )
		test-code-len @ 0 do
				test-code i shorts + @
				i ram-set
		loop
;

: vm-init ( -- )
		load-test-code
		0 VM_PC w!
		0xFFFF VM_SP w!
		0 VM_EX w!
		0 VM_IA w!
;

		