needs ../utils/util.fs
needs vmvars.fs
needs vmloc.fs
needs vmcode.fs
needs hw.fs

: run-OP_JSR ( a -- ) \ push address of next instruction on stack, then set PC to a
		.d" OP_JSR"
		\ PUSH ++PC
		\ PC has already been incremented while reading this op
		VM_PC-get \ ++PC
		VM_SP_PUSH
		\ PC = A
		vmloc-get \ aval
		VM_PC-set
;
: run-OP_INT ( a -- ) \ trigger software int with message a
		.d" OP_INT"
		vmloc-get
		sw-interrupt
;
: run-OP_IAG ( a -- ) \ get IA(interrupt address)
		VM_IA-get
		swap vmloc-set
;
: run-OP_IAS ( a -- ) \ set IA
		.d" OP_IAS"
		vmloc-get
		VM_IA-set
;
: run-OP_RFI ( a -- ) \ disable interrupt queuing, pop A, then PC from stack
		.d" OP_RFI"
		leave-sw-interrupt
;
: run-OP_IAQ ( a -- ) \ if not zero, interrupts will be queued, if 0, interrupts will be triggered
		.d" OP_IAQ"
		0 = if
				false intq-queue !
		else
				true intq-queue !
		then
;
: run-OP_HWN ( a -- ) \ get number of connected hardware devices
		.d" OP_HWN"
		hw-count @ swap vmloc-set
;
: run-OP_HWQ ( a -- ) \ sets A,B,C,X,Y to info about hw number a
		.d" OP_HWQ"
		vmloc-get 1- \ hw-num
		hw-devs hw-list @ + info-xt @ execute
;
: run-OP_HWI ( a -- ) \ sends interrupt to hardware A
		.d" OP_HWI"
		vmloc-get 1- \ hw-num
		hw-devs hw-list @ + int-xt @ execute
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
