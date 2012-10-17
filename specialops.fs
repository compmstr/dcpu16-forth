needs util.fs
needs vmvars.fs
needs vmloc.fs
needs vmcode.fs

: run-OP_JSR ( a op -- )
;
: run-OP_INT ( a op -- )
;
: run-OP_IAG ( a op -- )
;
: run-OP_IAS ( a op -- )
;
: run-OP_RFI ( a op -- )
;
: run-OP_IAQ ( a op -- )
;
: run-OP_HWN ( a op -- )
;
: run-OP_HWQ ( a op -- )
;
: run-OP_HWI ( a op -- )
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
