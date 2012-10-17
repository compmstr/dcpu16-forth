needs shorts.fs
needs util.fs

0x10000 short-array vm_ram

8 short-array vm_registers

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

: get-next-word ( -- n )
		\ [PC++]
		VM_PC w@ dup \ PC PC
		ram-get swap \ [PC] PC
		1+ VM_PC w! \ increment PC
;

