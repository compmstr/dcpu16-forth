
variable hw-list
0 hw-list !
variable hw-count
0 hw-count !

struct
		cell% field info-xt
		cell% field int-xt
end-struct hw-dev

: hw-devs ( num -- num*sizeof<hw-dev> )
		hw-dev struct-size *
;

\ takes a list of files to load as hardware devices
: add-hw ( loc count ... num_devs -- )
		\ use included ( loc count -- ) to include a file
		\ allocate space for hardware
		dup hw-devs allocate throw
		dup hw-count !
		0 do
				included
				hw-list i hw-devs + -rot \ loc int-xt info-xt
				2 pick info-xt ! \ loc int-xt
				swap int-xt !
		loop
;
