needs util.fs
needs vmvars.fs
needs vmloc.fs
needs vmcode.fs

: run-OP_JSR ( a -- ) \ push address of next instruction on stack, then set PC to a
		." run-OP_JSR" cr
		\ PUSH ++PC
		\ PC has already been incremented while reading this op
		VM_PC-get \ ++PC
		VM_SP_PUSH
		\ PC = A
		vmloc-get \ aval
		VM_PC-set
;
: run-OP_INT ( a -- )
;
: run-OP_IAG ( a -- )
;
: run-OP_IAS ( a -- )
;
: run-OP_RFI ( a -- )
;
: run-OP_IAQ ( a -- )
;
: run-OP_HWN ( a -- )
;
: run-OP_HWQ ( a -- )
;
: run-OP_HWI ( a -- )
;

0x20 array special-ops-xt \ operation XT, opcode is index
' run-OP_JSR 0x01 special-ops-xt !
' run-OP_INT 0x08 special-ops-xt !
' run-OP_IAG 0x09 special-ops-xt !
' run-OP_IAS 0x0a special-ops-xt !
' run-OP_RFI 0x0b special-ops-xt !
' run-OP_IAQ 0x0c special-ops-xt !
' run-OP_HWN 0x10 special-ops-xt !
' run-OP_HWQ 0x11 special-ops-xt !
' run-OP_HWI 0x12 special-ops-xt !
