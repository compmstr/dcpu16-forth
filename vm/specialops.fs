needs ../utils/util.fs
needs vmvars.fs
needs vmloc.fs
needs vmcode.fs

: run-OP_JSR ( a -- ) \ push address of next instruction on stack, then set PC to a
		." OP_JSR" cr
		\ PUSH ++PC
		\ PC has already been incremented while reading this op
		VM_PC-get \ ++PC
		VM_SP_PUSH
		\ PC = A
		vmloc-get \ aval
		VM_PC-set
;
: run-OP_INT ( a -- ) \ trigger software int with message a
		." OP_INT" cr
		vmloc-get
		sw-interrupt
;
: run-OP_IAG ( a -- ) \ get IA(interrupt address)
		VM_IA-get
		swap vmloc-set
;
: run-OP_IAS ( a -- ) \ set IA
		vmloc-get
		VM_IA-set
;
: run-OP_RFI ( a -- ) \ disable interrupt queuing, pop A, then PC from stack
		." OP_RFI" cr
		leave-sw-interrupt
;
: run-OP_IAQ ( a -- ) \ if not zero, interrupts will be queued, if 0, interrupts will be triggered
		0 = if
				false intq-queue !
		else
				true intq-queue !
		then
;
: run-OP_HWN ( a -- ) \ get number of connected hardware devices
;
: run-OP_HWQ ( a -- ) \ sets A,B,C,X,Y to info about hw number a
;
: run-OP_HWI ( a -- ) \ sends interrupt to hardware A
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
