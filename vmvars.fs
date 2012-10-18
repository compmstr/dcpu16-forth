needs shorts.fs
needs util.fs
needs types.fs

0x10000 short-array vm_ram

8 short-array vm_registers

wvariable VM_PC
wvariable VM_SP
wvariable VM_EX
wvariable VM_IA

create vmloc_a vmloc %allot drop
create vmloc_b vmloc %allot drop

: clear-registers ( -- ) \ set all registers to 0
		0 vm_registers 8 shorts erase
;

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

: VM_EX-set ( val -- )
  VM_EX w!
;
: VM_EX-get ( -- val )
  VM_EX w@
;
: VM_PC-set ( val -- )
  VM_PC w!
;
: VM_PC-get ( -- val )
  VM_PC w@
;
: VM_SP-set ( val -- )
  VM_SP w!
;
: VM_SP-get ( -- val )
  VM_SP w@
;
: VM_IA-set ( val -- )
  VM_IA w!
;
: VM_IA-get ( -- val )
  VM_IA w@
;

: VM_PC+
  VM_PC w@
  1+ VM_PC w!
;
: VM_PC-
  VM_PC w@
  1- VM_PC w!
;

: VM_SP+
  VM_SP w@
  1+ VM_SP w!
;
: VM_SP-
  VM_SP w@
  1- VM_SP w!
;

: get-next-word ( -- n )
		\ [PC++]
		VM_PC w@ \ PC
		ram-get \ [PC]
		VM_PC+ \ increment PC
;

