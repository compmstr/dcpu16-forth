needs shorts.fs
needs ops.fs
needs util.fs

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
		0x1F and
;

: get-code-word-a ( word -- a )
		\ top 6 bits
		10 rshift
;

: get-code-word-b ( word -- b )
		\ 2nd 5 bits from bottom
		5 rshift
		0x1F and
;

: get-next-word ( -- n )
		\ [PC++]
		VM_PC w@ dup \ PC PC
		ram-get swap \ [PC] PC
		1+ VM_PC w! \ increment PC
;

1  constant LOC_REG
2  constant LOC_MEM
3  constant LOC_REG_MEM
4  constant LOC_REG_MEM_OFFSET
5  constant LOC_LITERAL
6  constant LOC_SP
7  constant LOC_PC
8  constant LOC_EX
9  constant LOC_IA
10 constant LOC_PUSHPOP
11 constant LOC_PEEK
12 constant LOC_PICK

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

: vmloc-from-bits ( <5/6bit code> -- vmloc )
		vmloc %allot \ 5/6bit loc
		swap \ loc 5/6bit
		\ REGISTER
		dup 0x07 <= if \ 5/6bit 0x07 <=
				\ loc 5/6bit
				over vmloc-type LOC_REG swap w!
				over vmloc-register w! \ loc 5/6bit loc.register w!
				exit \ RETURN
		then
		\ [REGISTER]
		dup 0x0f <= if \ 5/6bit 0x0F <=
				\ loc 5/6bit
				over vmloc-type LOC_REG_MEM swap w!
				0x08 mod \ loc register
				over vmloc-register w!
				exit \ RETURN
		then
		\ [REGISTER + next word]
		dup 0x017 <= if
				\ loc 5/6bit
				over vmloc-type LOC_REG_MEM_OFFSET swap w!
				0x08 mod \ loc register
				over vmloc-register w! \ loc
				get-next-word \ loc <next word>
				over vmloc-loc w! \ loc
				exit \ RETURN
		then
		case dup
				0x18 of \ PUSH [--SP] if b (dest), POP [SP++] if a (src)
						over vmloc-type LOC_PUSHPOP swap w!
						drop
				endof
				0x19 of \ [SP] PEEK
						over vmloc-type LOC_PEEK swap w!
						drop
				endof
				0x1a of \ [SP + next word] / PICK n
						over vmloc-type LOC_PICK swap w!
						drop
						get-next-word
						over vmloc-loc w!
				endof
				0x1b of \ SP
						over vmloc-type LOC_SP swap w!
						drop
				endof
				0x1c of \ PC
						over vmloc-type LOC_PC swap w!
						drop
				endof
				0x1d of \ EX
						over vmloc-type LOC_EX swap w!
						drop
				endof
				0x1e of \ [next word]
						over vmloc-type LOC_MEM swap w!
						drop
						get-next-word
						over vmloc-loc w!
				endof
				0x1f of \ next word literal
						over vmloc-type LOC_LITERAL swap w!
						drop
						get-next-word
						over vmloc-val w!
				endof
				dup 0x20 >= if \ 5/6bit 0x20 >= has to have 6th bit set
						over vmloc-type LOC_LITERAL swap w!
						0x20 -
						over vmloc-val w!
				then
		endcase
;

: dump-vmloc ( addr -- addr )
		cr
		." Type " dup vmloc-type w@ .
		cr
		." Register " dup vmloc-register w@ .
		cr
		." Loc " dup vmloc-loc w@ .
		cr
		." Val " dup vmloc-val w@ .
		cr
;

\ create <name> vmloc %allot (%alloc does on heap)
\ <name> vmloc-loc \ gets mem address of the loc

: vmloc-set ( val loc -- )
		case dup vmloc-type w@
				LOC_REG of
						vmloc-register w@ \ val register
						reg-set
				endof
				LOC_MEM of
						vmloc-loc w@ \ val loc
						ram-set
				endof
				LOC_REG_MEM of
						vmloc-register w@ \ val register
						reg-get \ val loc
						ram-set
				endof
				LOC_REG_MEM_OFFSET of
						dup \ val loc loc
						vmloc-register w@ \ val loc register
						reg-get \ val loc regval
						swap \ val regval loc
						vmloc-loc w@ \ val regval ramloc
						ram-get + \ val regval+ramloc
						ram-set
				endof
				LOC_LITERAL of
						\ fail silently
				endof
				LOC_SP of
						drop \ val
						VM_SP w!
				endof
				LOC_PC of
						drop \ val
						VM_PC w!
				endof
				LOC_EX of
						drop \ val
						VM_EX w!
				endof
				LOC_IA of
						drop \ val
						VM_IA w!
				endof
		endcase
;

: vmloc-get ( loc -- val )
		case dup vmloc-type w@
				LOC_REG of
						vmloc-register w@
						reg-get
				endof
				LOC_MEM of
						vmloc-loc w@
						ram-get
				endof
				LOC_REG_MEM of
						vmloc-register w@
						reg-get
						ram-get
				endof
				LOC_REG_MEM_OFFSET of
						dup \ loc loc
						vmloc-register w@
						reg-get \ loc regloc
						swap
						vmloc-loc w@ \ regloc ramloc
						+ ram-get
				endof
				LOC_LITERAL of
						vmloc-val w@
				endof
				LOC_SP of
						drop
						VM_SP w@
				endof
				LOC_PC of
						drop
						VM_PC w@
				endof
				LOC_EX of
						drop
						VM_EX w@
				endof
				LOC_IA of
						drop
						VM_IA w@
				endof
		endcase
;


create test-loc vmloc %allot drop
: test-loc-store
		LOC_REG test-loc vmloc-type w!
		REG_A test-loc vmloc-register w!
		15 REG_A reg-set
		20 test-loc vmloc-set
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

: vm-skip ( -- ) \ skips next code word
		get-next-word \ word
		dup get-code-word-a \ word a-bits
		vmloc-from-bits drop \ word
		\ TODO if op == 0, don't get B
		get-code-word-b \ b-bits
		vmloc-from-bits drop \ --
;

: load-test-code ( -- )
		test-code-len @ 0 do
				test-code i shorts + w@
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

: run-special-word
\ Special opcodes
0x01 constant OP_JSR
0x08 constant OP_INT
0x09 constant OP_IAG
0x0a constant OP_IAS
0x0b constant OP_RFI
0x0c constant OP_IAQ
0x10 constant OP_HWN
0x11 constant OP_HWQ
0x12 constant OP_HWI
;

\ Standard opcodes
: run-word ( a b word -- )
		case get-code-word-op \ a b
				OP_SPECIAL of
				endof
				OP_SET of
						swap vmloc-get \ b a-val
						swap vmloc-set \ a-val b vmloc-set
				endof
				OP_ADD of
				endof
				OP_SUB of \ b-a -> b
						swap over vmloc-get \ b a b-val
						swap vmloc-get \ b b-val a-val
						- \ b b-a
						swap vmloc-set
				endof
				OP_MUL of
				endof
				OP_MLI of
				endof
				OP_DIV of
				endof
				OP_DVI of
				endof
				OP_MOD of
				endof
				OP_MDI of
				endof
				OP_AND of
				endof
				OP_BOR of
				endof
				OP_XOR of
				endof
				OP_SHR of
				endof
				OP_ASR of
				endof
				OP_SHL of
				endof
				OP_IFB of
				endof
				OP_IFC of
				endof
				OP_IFE of
				endof
				OP_IFN of \ run next code only if a != b
				endof
				OP_IFG of
				endof
				OP_IFA of
				endof
				OP_IFL of
				endof
				OP_IFU of
				endof
				OP_ADX of
				endof
				OP_SBX of
				endof
				OP_STI of
				endof
				OP_STD of
				endof
		endcase
;

: vm-step ( -- )
		get-next-word \ word
		dup get-code-word-a \ word a-code
		vmloc-from-bits \ word a-loc
		." A: "
		dump-vmloc
		cr
		over get-code-word-b \ word a-loc b-code
		vmloc-from-bits \ word a-loc b-loc
		." B: "
		dump-vmloc
		cr
		rot \ a-loc b-loc word
		." Op: " dup get-code-word-op .
		cr
		run-word
;

: encode-word ( a b op -- val )
		swap 5 lshift + \ a b/op
		swap 10 lshift + \ a/b/op
;

