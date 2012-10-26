needs ../utils/util.fs

variable hw-list
0 hw-list !
variable hw-count
0 hw-count !

struct
		cell% field info-xt
		cell% field int-xt
		cell% field update-xt
end-struct hw-dev

: hw-devs ( num -- num*sizeof<hw-dev> )
		hw-dev struct-size *
;

\ takes a list of files to load as hardware devices
: add-hw ( loc count ... num_devs -- )
		hw-list @ 0 <> if
				hw-list @ free
		then
		\ use included ( loc count -- ) to include a file
		\ allocate space for hardware
		dup hw-devs allocate throw \ loc count ... num_devs new-loc
		hw-list !
		dup hw-count !
		0 do
				2dup ." Including hardware: " type cr
				included \ int-xt info-xt update-xt
				\ r/o open-file throw include-file
				hw-list @ i hw-devs +
				swap over info-xt ! \ info-xt int-xt loc
				swap over int-xt ! \ info-xt loc
				update-xt !
		loop
;
