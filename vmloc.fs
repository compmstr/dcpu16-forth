needs shorts.fs
needs util.fs
needs vm.fs
needs constants.fs
needs vmvars.fs

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
		case dup \ loc 5/6bit ( case takes off duplicate )
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
				\ TODO This part doesn't work
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
		." vmloc-set"
		.s
		cr
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
		." end-vmloc-set"
		.s
		cr
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
