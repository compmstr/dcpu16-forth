needs ../utils/shorts.fs
needs ../utils/util.fs
needs ../utils/types.fs

0x10000 short-array vm_ram

8 short-array vm_registers

wvariable VM_PC
wvariable VM_SP
wvariable VM_EX
wvariable VM_IA

\ interrupt queue
\ are we currently queueing interrupts?
variable intq-queue
false intq-queue !
\ front of queue
variable intq-front
0 intq-front !
\ back of queue
variable intq-end
0 intq-end !
variable intq-size
0 intq-size !

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
  VM_PC-get
  1+ VM_PC-set
;
: VM_PC-
  VM_PC-get
  1- VM_PC-set
;

: VM_SP+1 \ ( -- ) pop
	VM_SP-get
  1+ VM_SP w!
;
: VM_SP-1 \ ( -- ) push
  VM_SP-get
  1- VM_SP w!
;
: VM_SP_PUSH \ ( val -- )
		VM_SP-1
		VM_SP-get ram-set
;
: VM_SP_POP \ ( -- val )
		VM_SP-get ram-get
		VM_SP+1
;

: get-next-word ( -- n )
		\ [PC++]
		VM_PC-get \ PC
		ram-get \ [PC]
		VM_PC+ \ increment PC
;

: alloc-intq ( -- loc )
		intq-entry %alloc
		dup intq-entry struct-size erase
;

: enqueue-intq ( message -- )
		alloc-intq >r
		\ set the message
		r@ intq-entry-message w!
		\ set next to 0
		0 r@ intq-entry-next !
		\ if there's no front, set this to the front
		0 intq-front @ = if
				r@ intq-front !
		else
				\ set current end's next to this
				r@ intq-end @ intq-entry-next !
		then
		r@ intq-end !
		r> drop
;

: intq-empty? ( -- t/f )
		intq-front @ 0 =
;

: dequeue-intq ( -- message )
		intq-empty? if
				." Error: no interrupts enqueued"
				abort
		then
		intq-front @
		dup intq-entry-next @ >r \ intq-front
		intq-entry-message w@ \ message
		r@ intq-front !
		\ if this is the last item, set the end to 0 too
		r@ 0 = if
				0 intq-end !
		else
				\ set the new front's prev to 0
				0 r@ intq-entry-prev !
		then
		r> drop
;
		